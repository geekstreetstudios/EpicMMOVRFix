using HarmonyLib;
using System;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;

namespace EpicMMO
{
    public static class EpicMMOVRHUD
    {
        private static FieldInfo _characterLevelField;

        // Fast delegates (NO reflection at runtime)
        private static Func<object, Character, object> _getEnemyHudFast;
        private static Func<object, object> _getNameComponentFast;
        private static Func<object, string> _getTextFast;
        private static Action<object, string> _setTextFast;

        // EpicMMO
        private static MethodInfo _containsMethod;
        private static MethodInfo _getLevelMethod;

        private static PropertyInfo _levelSystemInstance;
        private static MethodInfo _getPlayerLevelMethod;

        public static void Initialize(Harmony harmony)
        {
            try
            {
                _characterLevelField = typeof(Character).GetField("m_level", BindingFlags.NonPublic | BindingFlags.Instance);

                var enemyHudManagerType = AccessTools.TypeByName("ValheimVRMod.VRCore.UI.EnemyHudManager");
                var hudDataType = AccessTools.TypeByName("ValheimVRMod.VRCore.UI.EnemyHudManager+HudData");

                if (enemyHudManagerType == null || hudDataType == null)
                    return;

                var updateNameMethod = AccessTools.Method(enemyHudManagerType, "UpdateName");
                var getHudMethod = AccessTools.Method(enemyHudManagerType, "getEnemyHud");

                var nameField = AccessTools.Field(hudDataType, "name");

                // --- BUILD FAST ACCESSORS ---

                // getEnemyHud delegate
                {
                    var instanceParam = Expression.Parameter(typeof(object), "instance");
                    var charParam = Expression.Parameter(typeof(Character), "c");

                    var castInstance = Expression.Convert(instanceParam, enemyHudManagerType);
                    var call = Expression.Call(castInstance, getHudMethod, charParam);

                    var convertResult = Expression.Convert(call, typeof(object));

                    _getEnemyHudFast = Expression.Lambda<Func<object, Character, object>>(
                        convertResult, instanceParam, charParam).Compile();
                }

                // get name component delegate
                {
                    var hudParam = Expression.Parameter(typeof(object), "hud");
                    var castHud = Expression.Convert(hudParam, hudDataType);

                    var field = Expression.Field(castHud, nameField);
                    var convert = Expression.Convert(field, typeof(object));

                    _getNameComponentFast = Expression.Lambda<Func<object, object>>(convert, hudParam).Compile();
                }

                // text property access
                var textProp = nameField.FieldType.GetProperty("text");

                {
                    var objParam = Expression.Parameter(typeof(object), "obj");
                    var cast = Expression.Convert(objParam, nameField.FieldType);
                    var prop = Expression.Property(cast, textProp);

                    _getTextFast = Expression.Lambda<Func<object, string>>(prop, objParam).Compile();
                }

                {
                    var objParam = Expression.Parameter(typeof(object), "obj");
                    var valueParam = Expression.Parameter(typeof(string), "val");

                    var cast = Expression.Convert(objParam, nameField.FieldType);
                    var prop = Expression.Property(cast, textProp);

                    var assign = Expression.Assign(prop, valueParam);

                    _setTextFast = Expression.Lambda<Action<object, string>>(assign, objParam, valueParam).Compile();
                }

                // --- EpicMMO ---
                var dataMonstersType = AccessTools.TypeByName("EpicMMOSystem.DataMonsters");
                if (dataMonstersType != null)
                {
                    _containsMethod = AccessTools.Method(dataMonstersType, "contains");
                    _getLevelMethod = AccessTools.Method(dataMonstersType, "getLevel");
                }

                var levelSystemType = AccessTools.TypeByName("EpicMMOSystem.LevelSystem");
                if (levelSystemType != null)
                {
                    _levelSystemInstance = AccessTools.Property(levelSystemType, "Instance");
                    _getPlayerLevelMethod = AccessTools.Method(levelSystemType, "getLevel");
                }

                harmony.Patch(updateNameMethod,
                    postfix: new HarmonyMethod(typeof(EpicMMOVRHUD), nameof(OnUpdateNamePostfix))
                    {
                        priority = Priority.High
                    });
            }
            catch (Exception e)
            {
                Debug.LogError($"EpicMMO VR HUD init error: {e}");
            }
        }

        private static void OnUpdateNamePostfix(object __instance, Character c)
        {
            if (!EpicMMOVRUIPatch.ConfigEnableMod.Value) return;
            if (c == null) return;

            try
            {
                var hud = _getEnemyHudFast(__instance, c);
                if (hud == null) return;

                var nameComponent = _getNameComponentFast(hud);
                if (nameComponent == null) return;

                string currentText = _getTextFast(nameComponent);
                if (string.IsNullOrEmpty(currentText)) return;

                // Prevent duplicates
                if (currentText.Contains("[") && currentText.Contains("]"))
                    return;

                int characterLevel = _characterLevelField != null ? (int)_characterLevelField.GetValue(c) : 1;

                int monsterLevel = GetMonsterLevel(c, characterLevel);
                if (monsterLevel <= 0) return;

                string color = GetLevelColor(monsterLevel);
                string formatted = $"<color={color}>[{monsterLevel}]</color>";

                _setTextFast(nameComponent, $"{currentText} {formatted}");
            }
            catch (Exception e)
            {
                Debug.LogError($"EpicMMO VR HUD Error: {e}");
            }
        }

        private static int GetMonsterLevel(Character c, int characterLevel)
        {
            try
            {
                if (_containsMethod == null || _getLevelMethod == null)
                    return 0;

                bool contains = (bool)_containsMethod.Invoke(null, new object[] { c.gameObject.name });
                if (!contains) return 0;

                int level = (int)_getLevelMethod.Invoke(null, new object[] { c.gameObject.name });

                return level + (characterLevel - 1);
            }
            catch
            {
                return 0;
            }
        }

        private static string GetLevelColor(int monsterLevel)
        {
            int playerLevel = 1;

            try
            {
                var instance = _levelSystemInstance?.GetValue(null);
                if (instance != null && _getPlayerLevelMethod != null)
                {
                    playerLevel = (int)_getPlayerLevelMethod.Invoke(instance, null);
                }
            }
            catch { }

            int max = playerLevel + 5;
            int min = playerLevel - 5;

            if (monsterLevel > max)
                return "#FF0000";
            else if (monsterLevel < min)
                return "#2FFFDC";
            else
                return "#FFFFFF";
        }
    }
}