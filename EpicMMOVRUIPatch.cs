using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using EpicMMO;
using HarmonyLib;
using UnityEngine;

[BepInEx.BepInPlugin("epicmmovrfix", "EpicMMO VR UI Fix", "1.0.0")]
[BepInEx.BepInDependency("ValheimVRMod", BepInEx.BepInDependency.DependencyFlags.SoftDependency)]
[BepInEx.BepInDependency("EpicMMOSystem", BepInEx.BepInDependency.DependencyFlags.SoftDependency)]
public class EpicMMOVRUIPatch : BaseUnityPlugin
{
    private static EpicMMOVRUIPatch _instance;
    private Harmony _harmony;
    private bool _epicMMOLoaded = false;

    // Configuration settings
    public static ConfigEntry<bool> ConfigEnableMod { get; private set; }
    public static ConfigEntry<bool> ConfigEnableLogs { get; private set; }

    public static EpicMMOVRUIPatch Instance => _instance;

    private void Awake()
    {
        _instance = this;

        // Initialize configuration
        ConfigEnableMod = Config.Bind("General", "EnableMod", true,
            "Enable the EpicMMO VR UI Fix mod");
        ConfigEnableLogs = Config.Bind("General", "EnableLogs", false,
            "Enable debug logging for the EpicMMO VR UI Fix mod");

        try
        {
            // Create Harmony instance
            _harmony = new Harmony("epicmmovrfix");

            // Start coroutine to wait for EpicMMOSystem to load
            StartCoroutine(WaitForEpicMMOSystem());

            Logger.LogInfo("EpicMMO VR UI Fix loaded successfully!");
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to load EpicMMO VR UI Fix: {e}");
        }
    }

    private IEnumerator WaitForEpicMMOSystem()
    {
        if (!ConfigEnableMod.Value)
        {
            Logger.LogInfo("EpicMMO VR UI Fix is disabled in configuration");
            yield break;
        }

        int maxAttempts = 60; // 60 seconds max
        int attempts = 0;

        while (attempts < maxAttempts && !_epicMMOLoaded)
        {
            yield return new WaitForSeconds(1f);
            attempts++;

            // Check if EpicMMOSystem is now loaded
            if (EpicMMOVRUI.IsEpicMMOSystemLoaded())
            {
                _epicMMOLoaded = true;
                ApplyPatches();

                if (ConfigEnableLogs.Value)
                    Logger.LogInfo("EpicMMOSystem detected, patches applied!");
                yield break;
            }

            if (attempts % 10 == 0 && ConfigEnableLogs.Value) // Log every 10 seconds
            {
                Logger.LogInfo($"Waiting for EpicMMOSystem... ({attempts}s)");
            }
        }

        if (!_epicMMOLoaded && ConfigEnableLogs.Value)
        {
            Logger.LogWarning("EpicMMOSystem not detected after waiting. VR UI fix will not be active.");
        }
    }

    private void ApplyPatches()
    {
        try
        {
            // Apply UI patches for main EpicMMO UI
            EpicMMOVRUI.PatchEpicMMOSystemUI(_harmony);

            // Apply HUD patches for enemy/player level display
            EpicMMOVRHUD.Initialize(_harmony);

            if (ConfigEnableLogs.Value)
                Logger.LogInfo("EpicMMOSystem VR UI and HUD patches applied successfully");
        }
        catch (Exception e)
        {
            Logger.LogError($"Error applying patches: {e}");
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }

    // Coroutine starter for static classes
    public new void StartCoroutine(IEnumerator routine)
    {
        base.StartCoroutine(routine);
    }

    // Logging methods for static classes
    public static void LogInfo(string message)
    {
        if (ConfigEnableLogs.Value)
            _instance?.Logger.LogInfo(message);
    }

    public static void LogWarning(string message)
    {
        if (ConfigEnableLogs.Value)
            _instance?.Logger.LogWarning(message);
    }

    public static void LogError(string message)
    {
        _instance?.Logger.LogError(message);
    }

    public static void LogMessage(string message)
    {
        _instance?.Logger.LogMessage(message);
    }
}