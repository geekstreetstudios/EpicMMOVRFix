using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EpicMMO
{
    public static class EpicMMOVRCrit
    {
        private static Type _vrDamageTextType;
        private static MethodInfo _poolMethod;
        private static MethodInfo _createTextMethod;

        public static void Initialize(Harmony harmony)
        {
            try
            {
                var critType = AccessTools.TypeByName("EpicMMOSystem.CritDmgVFX");
                if (critType == null)
                {
                    EpicMMOVRUIPatch.LogWarning("Crit patch: CritDmgVFX not found");
                    return;
                }

                var method = AccessTools.Method(critType, "CriticalVFX");
                if (method == null)
                {
                    EpicMMOVRUIPatch.LogWarning("Crit patch: CriticalVFX method not found");
                    return;
                }

                // ValheimVR text system
                _vrDamageTextType = AccessTools.TypeByName("ValheimVRMod.Scripts.VRDamageTexts");

                if (_vrDamageTextType != null)
                {
                    _poolMethod = AccessTools.Method(_vrDamageTextType, "Pool");
                    _createTextMethod = AccessTools.Method(
                        _vrDamageTextType,
                        "CreateText",
                        new Type[]
                        {
                            typeof(string),
                            typeof(Vector3),
                            typeof(Color),
                            typeof(bool),
                            typeof(float)
                        });
                }

                harmony.Patch(
                    method,
                    postfix: new HarmonyMethod(typeof(EpicMMOVRCrit), nameof(OnCriticalVFXPostfix))
                );

                EpicMMOVRUIPatch.LogInfo("EpicMMO Crit VR patch loaded");
            }
            catch (Exception e)
            {
                EpicMMOVRUIPatch.LogError($"Crit patch init failed: {e}");
            }
        }

        private static void OnCriticalVFXPostfix(Vector3 position, float damage)
        {
            try
            {
                if (_vrDamageTextType == null ||
                    _poolMethod == null ||
                    _createTextMethod == null)
                    return;

                object pooled = _poolMethod.Invoke(null, null);
                if (pooled == null)
                    return;

                string text = "Crit " + Mathf.RoundToInt(damage).ToString();

                _createTextMethod.Invoke(
                    pooled,
                    new object[]
                    {
                        text,
                        position,
                        Color.cyan,
                        false,
                        1.5f
                    });

                // 🔥 ADD OUTLINE HERE
                TryAddOutline(pooled);

                if (EpicMMOVRUIPatch.ConfigEnableLogs.Value)
                    EpicMMOVRUIPatch.LogInfo($"VR Crit spawned: {text}");
            }
            catch (Exception e)
            {
                EpicMMOVRUIPatch.LogWarning($"Crit spawn failed: {e.Message}");
            }
        }

        // =========================
        // 🖤 OUTLINE ADDER
        // =========================
        private static void TryAddOutline(object pooled)
        {
            try
            {
                var mb = pooled as MonoBehaviour;
                if (mb == null || mb.gameObject == null)
                    return;

                var text = mb.GetComponent<Text>();
                if (text == null)
                    return;

                var outline = text.GetComponent<Outline>();
                if (outline == null)
                    outline = text.gameObject.AddComponent<Outline>();

                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(2f, -2f);
            }
            catch (Exception e)
            {
                EpicMMOVRUIPatch.LogWarning($"Outline failed: {e.Message}");
            }
        }
    }
}