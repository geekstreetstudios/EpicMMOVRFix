using HarmonyLib;
using UnityEngine;

namespace EpicMMO
{
    public static class EpicMMOVRXPBar
    {
        private static GameObject _xpBar;
        private static Transform _originalParent;

        public static void Initialize(Harmony harmony)
        {
            // Inventory open/close
            harmony.Patch(
                AccessTools.Method(typeof(InventoryGui), "Show"),
                postfix: new HarmonyMethod(typeof(EpicMMOVRXPBar), nameof(OnInventoryShow))
            );

            harmony.Patch(
                AccessTools.Method(typeof(InventoryGui), "Hide"),
                postfix: new HarmonyMethod(typeof(EpicMMOVRXPBar), nameof(OnInventoryHide))
            );

            // 🔥 CRITICAL: Player spawn / respawn (fixes death issue)
            harmony.Patch(
                AccessTools.Method(typeof(Game), "SpawnPlayer"),
                postfix: new HarmonyMethod(typeof(EpicMMOVRXPBar), nameof(OnPlayerSpawned))
            );

            // Initial attempt
            TryFindXPBar();
        }

        // =========================
        // 🔍 FIND XP BAR (NO SPAM)
        // =========================
        private static void TryFindXPBar()
        {
            if (_xpBar != null) return;

            _xpBar = GameObject.Find("ExpPanel(Clone)");

            if (_xpBar != null)
            {
                _originalParent = _xpBar.transform.parent;
                EpicMMOVRUIPatch.LogInfo("XPBar found and cached");
                ApplyCurrentMode();
            }
        }

        // =========================
        // 🎮 EVENTS
        // =========================
        private static void OnInventoryShow()
        {
            TryFindXPBar();
            ApplyCurrentMode();
        }

        private static void OnInventoryHide()
        {
            ApplyCurrentMode();
        }

        private static void OnPlayerSpawned()
        {
            // XP bar gets recreated here → we MUST reacquire it
            _xpBar = null;
            _originalParent = null;

            EpicMMOVRUIPatch.Instance.StartCoroutine(DelayedReacquire());
        }

        private static System.Collections.IEnumerator DelayedReacquire()
        {
            // wait 1 frame for UI to spawn
            yield return null;

            TryFindXPBar();
        }

        // =========================
        // 🧠 CORE LOGIC
        // =========================
        public static void ApplyCurrentMode()
        {
            if (_xpBar == null)
                return;

            bool invOpen =
                InventoryGui.instance != null &&
                InventoryGui.instance.gameObject.activeInHierarchy;

            if (EpicMMOVRUIPatch.ConfigEnableInventoryXPBar.Value && invOpen)
            {
                MoveToInventory();
            }
            else
            {
                RestoreToHud();
            }
        }

        // =========================
        // 📦 INVENTORY MODE
        // =========================
        private static void MoveToInventory()
        {
            if (_xpBar == null || InventoryGui.instance == null)
                return;

            Transform target = InventoryGui.instance.transform.Find("root/Player");
            if (target == null)
                target = InventoryGui.instance.transform;

            _xpBar.transform.SetParent(target, false);
            _xpBar.SetActive(true);

            ApplyInventoryPosition();
        }

        // =========================
        // 🖥️ HUD MODE
        // =========================
        private static void RestoreToHud()
        {
            if (_xpBar == null || _originalParent == null)
                return;

            _xpBar.transform.SetParent(_originalParent, false);
            _xpBar.SetActive(true);

            ApplyHudPosition();
        }

        // =========================
        // 📍 POSITIONS
        // =========================
        public static void ApplyInventoryPosition()
        {
            if (_xpBar == null)
                return;

            RectTransform rect = _xpBar.GetComponent<RectTransform>();
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.anchoredPosition = new Vector2(
                EpicMMOVRUIPatch.ConfigInventoryXPBarPosX.Value,
                EpicMMOVRUIPatch.ConfigInventoryXPBarPosY.Value
            );

            rect.localScale =
                Vector3.one *
                EpicMMOVRUIPatch.ConfigInventoryXPBarScale.Value;
        }

        public static void ApplyHudPosition()
        {
            if (_xpBar == null)
                return;

            RectTransform rect = _xpBar.GetComponent<RectTransform>();
            if (rect == null)
                return;

            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);

            rect.anchoredPosition = new Vector2(
                EpicMMOVRUIPatch.ConfigHudXPBarPosX.Value,
                EpicMMOVRUIPatch.ConfigHudXPBarPosY.Value
            );

            rect.localScale =
                Vector3.one *
                EpicMMOVRUIPatch.ConfigHudXPBarScale.Value;
        }
    }
}