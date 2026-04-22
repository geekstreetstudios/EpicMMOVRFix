using System;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using EpicMMO;

[BepInPlugin("epicmmovrfix", "EpicMMO VR UI Fix", "1.2.0")]
[BepInDependency("ValheimVRMod", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("EpicMMOSystem", BepInDependency.DependencyFlags.SoftDependency)]
public class EpicMMOVRUIPatch : BaseUnityPlugin
{
    private static EpicMMOVRUIPatch _instance;
    private Harmony _harmony;
    private bool _epicLoaded;

    public static EpicMMOVRUIPatch Instance => _instance;

    public static ConfigEntry<bool> ConfigEnableMod { get; private set; }
    public static ConfigEntry<bool> ConfigEnableLogs { get; private set; }

    public static ConfigEntry<bool> ConfigEnableInventoryXPBar { get; private set; }

    public static ConfigEntry<float> ConfigInventoryXPBarPosX { get; private set; }
    public static ConfigEntry<float> ConfigInventoryXPBarPosY { get; private set; }
    public static ConfigEntry<float> ConfigInventoryXPBarScale { get; private set; }

    public static ConfigEntry<float> ConfigHudXPBarPosX { get; private set; }
    public static ConfigEntry<float> ConfigHudXPBarPosY { get; private set; }
    public static ConfigEntry<float> ConfigHudXPBarScale { get; private set; }

    private void Awake()
    {
        _instance = this;

        ConfigEnableMod = Config.Bind(
            "General",
            "EnableMod",
            true,
            "Enable EpicMMO VR UI Fix"
        );

        ConfigEnableLogs = Config.Bind(
            "General",
            "EnableLogs",
            false,
            "Enable debug logging"
        );

        ConfigEnableInventoryXPBar = Config.Bind(
            "XP Bar",
            "EnableInventoryXPBar",
            true,
            "Move EpicMMO XP bar into inventory while inventory is open"
        );

        ConfigInventoryXPBarPosX = Config.Bind(
            "XP Bar",
            "InventoryXPBarPosX",
            500f,
            "Inventory XP bar X position"
        );

        ConfigInventoryXPBarPosY = Config.Bind(
            "XP Bar",
            "InventoryXPBarPosY",
            750f,
            "Inventory XP bar Y position"
        );

        ConfigInventoryXPBarScale = Config.Bind(
            "XP Bar",
            "InventoryXPBarScale",
            0.90f,
            "Inventory XP bar scale"
        );

        ConfigHudXPBarPosX = Config.Bind(
            "XP Bar",
            "HudXPBarPosX",
            -550f,
            "HUD XP bar X position"
        );

        ConfigHudXPBarPosY = Config.Bind(
            "XP Bar",
            "HudXPBarPosY",
            -950f,
            "HUD XP bar Y position"
        );

        ConfigHudXPBarScale = Config.Bind(
            "XP Bar",
            "HudXPBarScale",
            0.5f,
            "HUD XP bar scale"
        );

        try
        {
            _harmony = new Harmony("epicmmovrfix");

            ConfigEnableInventoryXPBar.SettingChanged += ConfigChanged;
            ConfigInventoryXPBarPosX.SettingChanged += ConfigChanged;
            ConfigInventoryXPBarPosY.SettingChanged += ConfigChanged;
            ConfigInventoryXPBarScale.SettingChanged += ConfigChanged;

            ConfigHudXPBarPosX.SettingChanged += ConfigChanged;
            ConfigHudXPBarPosY.SettingChanged += ConfigChanged;
            ConfigHudXPBarScale.SettingChanged += ConfigChanged;

            StartCoroutine(WaitForEpicMMO());

            Logger.LogInfo("EpicMMO VR UI Fix loaded");
        }
        catch (Exception e)
        {
            Logger.LogError($"Load failed: {e}");
        }
    }

    private void ConfigChanged(object sender, EventArgs e)
    {
        EpicMMOVRXPBar.ApplyCurrentMode();
    }

    private IEnumerator WaitForEpicMMO()
    {
        if (!ConfigEnableMod.Value)
            yield break;

        int tries = 0;

        while (!_epicLoaded && tries < 60)
        {
            yield return new WaitForSeconds(1f);
            tries++;

            if (EpicMMOVRUI.IsEpicMMOSystemLoaded())
            {
                _epicLoaded = true;
                ApplyPatches();
                yield break;
            }
        }

        Logger.LogWarning("EpicMMOSystem not detected");
    }

    private void ApplyPatches()
    {
        try
        {
            EpicMMOVRUI.PatchEpicMMOSystemUI(_harmony);
            EpicMMOVRHUD.Initialize(_harmony);
            EpicMMOVRXPBar.Initialize(_harmony);
            EpicMMOVRCrit.Initialize(_harmony);

            Logger.LogInfo("Patches applied");
        }
        catch (Exception e)
        {
            Logger.LogError($"Patch error: {e}");
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }

    public new void StartCoroutine(IEnumerator routine)
    {
        base.StartCoroutine(routine);
    }

    public static void LogInfo(string msg)
    {
        if (ConfigEnableLogs.Value)
            _instance?.Logger.LogInfo(msg);
    }

    public static void LogWarning(string msg)
    {
        if (ConfigEnableLogs.Value)
            _instance?.Logger.LogWarning(msg);
    }

    public static void LogError(string msg)
    {
        _instance?.Logger.LogError(msg);
    }
}