using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EpicMMO
{
    public static class EpicMMOVRHUD
    {
        private static bool _isInitialized = false;
        private static Harmony _harmony;

        // Caches
        private static Dictionary<string, string> _nameCache = new Dictionary<string, string>();
        private static Dictionary<string, int> _monsterLevelCache = new Dictionary<string, int>();

        // EpicMMOSystem type caches
        private static Type _epicMMOSystemType;
        private static Type _dataMonstersType;

        // Method caches
        private static MethodInfo _containsMethod;
        private static MethodInfo _getLevelMethod;

        // Config field caches
        private static bool? _enabledLevelControl;
        private static bool? _mobLvlPerStar;
        private static string _mobLVLChars;
        private static int _maxLevelExp;
        private static int _minLevelExp;

        // Reflection field for private m_level
        private static FieldInfo _characterLevelField;

        public static void Initialize(Harmony harmony)
        {
            if (_isInitialized) return;
            _harmony = harmony;

            // Cache reflection fields
            _characterLevelField = typeof(Character).GetField("m_level", BindingFlags.NonPublic | BindingFlags.Instance);

            // Cache EpicMMOSystem types
            _epicMMOSystemType = AccessTools.TypeByName("EpicMMOSystem.EpicMMOSystem");
            _dataMonstersType = AccessTools.TypeByName("EpicMMOSystem.DataMonsters");

            if (_dataMonstersType != null)
            {
                _containsMethod = AccessTools.Method(_dataMonstersType, "contains");
                _getLevelMethod = AccessTools.Method(_dataMonstersType, "getLevel");
            }

            // Cache config values
            CacheConfigValues();

            ApplyVHVRPatches();
            _isInitialized = true;
        }

        private static void CacheConfigValues()
        {
            try
            {
                if (_epicMMOSystemType != null)
                {
                    // Cache config fields using Traverse for safety
                    var enabledLevelControlTraverse = Traverse.Create(_epicMMOSystemType).Field("enabledLevelControl");
                    var mobLvlPerStarTraverse = Traverse.Create(_epicMMOSystemType).Field("mobLvlPerStar");
                    var mobLVLCharsTraverse = Traverse.Create(_epicMMOSystemType).Field("MobLVLChars");
                    var maxLevelExpTraverse = Traverse.Create(_epicMMOSystemType).Field("maxLevelExp");
                    var minLevelExpTraverse = Traverse.Create(_epicMMOSystemType).Field("minLevelExp");

                    // Use GetValue() without generic type to avoid casting issues
                    _enabledLevelControl = enabledLevelControlTraverse.GetValue() as bool? ?? true;
                    _mobLvlPerStar = mobLvlPerStarTraverse.GetValue() as bool? ?? false;
                    _mobLVLChars = mobLVLCharsTraverse.GetValue() as string ?? "[@]";
                    _maxLevelExp = maxLevelExpTraverse.GetValue() as int? ?? 5;
                    _minLevelExp = minLevelExpTraverse.GetValue() as int? ?? 5;
                }
            }
            catch (Exception e)
            {
                EpicMMOVRUIPatch.LogError($"Error caching config values: {e}");

                // Set defaults if caching fails
                _enabledLevelControl = true;
                _mobLvlPerStar = false;
                _mobLVLChars = "[@]";
                _maxLevelExp = 5;
                _minLevelExp = 5;
            }
        }

        private static void ApplyVHVRPatches()
        {
            try
            {
                var enemyHudManagerType = AccessTools.TypeByName("ValheimVRMod.VRCore.UI.EnemyHudManager");
                if (enemyHudManagerType == null) return;

                var updateNameMethod = AccessTools.Method(enemyHudManagerType, "UpdateName");
                if (updateNameMethod != null)
                {
                    _harmony.Patch(updateNameMethod,
                        prefix: new HarmonyMethod(typeof(EpicMMOVRHUD), nameof(OnUpdateNamePrefix)));
                }
            }
            catch (Exception e)
            {
                EpicMMOVRUIPatch.LogError($"Error applying VHVR patches: {e}");
            }
        }

        private static void OnUpdateNamePrefix(object __instance, Character c, ref string name)
        {
            if (!EpicMMOVRUIPatch.ConfigEnableMod.Value) return;
            if (c == null) return;
            if (_enabledLevelControl == false) return;

            try
            {
                // Get character level using reflection
                int characterLevel = _characterLevelField != null ? (int)_characterLevelField.GetValue(c) : 1;

                // Use cache key to avoid repeated processing
                string cacheKey = $"{c.gameObject.name}|{name}|{characterLevel}";

                if (_nameCache.TryGetValue(cacheKey, out string cachedName))
                {
                    name = cachedName;
                    return;
                }

                var modifiedName = GetModifiedNameWithLevel(c, name, characterLevel);
                if (modifiedName != name)
                {
                    _nameCache[cacheKey] = modifiedName;
                    name = modifiedName;
                }
            }
            catch
            {
                // Silently fail to avoid performance impact
            }
        }

        private static string GetModifiedNameWithLevel(Character c, string originalName, int characterLevel)
        {
            if (c == null) return originalName;

            try
            {
                // Check if monster exists in DataMonsters
                if (_containsMethod == null || _getLevelMethod == null) return originalName;

                bool containsMonster = (bool)_containsMethod.Invoke(null, new object[] { c.gameObject.name });
                if (!containsMonster) return originalName;

                // Get monster level
                int monsterLevel = (int)_getLevelMethod.Invoke(null, new object[] { c.gameObject.name });

                // Apply star levels if configured
                if (_mobLvlPerStar == true)
                    monsterLevel += characterLevel - 1;

                string mobLevelString = monsterLevel.ToString();

                // Handle level 0 (unknown)
                if (monsterLevel == 0)
                    mobLevelString = "???";

                // Apply color based on level difference
                string colorTag = GetLevelColor(monsterLevel);

                // Apply formatting
                string levelFormat = _mobLVLChars ?? "[@]";
                levelFormat = levelFormat.Replace("@", mobLevelString);

                return $"{originalName} <color={colorTag}>{levelFormat}</color>";
            }
            catch
            {
                return originalName;
            }
        }

        private static string GetLevelColor(int monsterLevel)
        {
            // Get player level (simplified - you might want to get this from EpicMMOSystem)
            int playerLevel = 1; // Default

            try
            {
                // Try to get player level from EpicMMOSystem
                var levelSystemType = AccessTools.TypeByName("EpicMMOSystem.LevelSystem");
                if (levelSystemType != null)
                {
                    var instanceProperty = AccessTools.Property(levelSystemType, "Instance");
                    var getLevelMethod = AccessTools.Method(levelSystemType, "getLevel");

                    if (instanceProperty != null && getLevelMethod != null)
                    {
                        var instance = instanceProperty.GetValue(null);
                        if (instance != null)
                        {
                            playerLevel = (int)getLevelMethod.Invoke(instance, null);
                        }
                    }
                }
            }
            catch
            {
                // If we can't get player level, use default
            }

            int maxLevelExp = playerLevel + _maxLevelExp;
            int minLevelExp = playerLevel - _minLevelExp;

            if (monsterLevel > maxLevelExp)
                return "red";
            else if (monsterLevel < minLevelExp)
                return "#2FFFDC"; // Cyan
            else
                return "white";
        }
    }
}