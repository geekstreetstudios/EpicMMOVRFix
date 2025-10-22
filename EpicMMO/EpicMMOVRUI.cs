using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EpicMMO
{
    public static class EpicMMOVRUI
    {
        public static bool IsEpicMMOSystemLoaded()
        {
            try
            {
                var myUIType = AccessTools.TypeByName("EpicMMOSystem.MyUI");
                return myUIType != null;
            }
            catch
            {
                return false;
            }
        }

        public static void PatchEpicMMOSystemUI(Harmony harmony)
        {
            try
            {
                var myUIType = AccessTools.TypeByName("EpicMMOSystem.MyUI");
                if (myUIType == null) return;

                var showMethod = AccessTools.Method(myUIType, "Show");
                if (showMethod != null)
                {
                    harmony.Patch(showMethod, postfix: new HarmonyMethod(typeof(EpicMMOVRUI), nameof(OnEpicMMOUIShown)));
                }

                EpicMMOVRUIPatch.Instance.StartCoroutine(TryProcessExistingUI());
            }
            catch (System.Exception e)
            {
                EpicMMOVRUIPatch.LogError($"Error patching: {e}");
            }
        }

        public static void OnEpicMMOUIShown()
        {
            EpicMMOVRUIPatch.Instance.StartCoroutine(ProcessEpicMMOUIForVR());
        }

        private static IEnumerator TryProcessExistingUI()
        {
            yield return new WaitForSeconds(2f);
            ProcessEpicMMOCanvas();
        }

        private static IEnumerator ProcessEpicMMOUIForVR()
        {
            yield return null;
            ProcessEpicMMOCanvas();
        }

        private static void ProcessEpicMMOCanvas()
        {
            try
            {
                Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
                Canvas epicCanvas = null;

                foreach (Canvas canvas in allCanvases)
                {
                    if (canvas != null && canvas.name == "Canvas" &&
                        canvas.transform.parent != null && canvas.transform.parent.name.Contains("LevelHud"))
                    {
                        epicCanvas = canvas;
                        break;
                    }
                }

                if (epicCanvas == null) return;

                var vrguiInstance = GetVHVRVRGUI();
                if (vrguiInstance == null) return;

                ProcessCanvasForVR(epicCanvas, vrguiInstance);
            }
            catch (System.Exception e)
            {
                EpicMMOVRUIPatch.LogError($"Error: {e}");
            }
        }

        private static void ProcessCanvasForVR(Canvas canvas, object vrguiInstance)
        {
            try
            {
                // 1. Correct layer
                canvas.gameObject.layer = 5; // UI layer

                // 2. WorldSpace with VR camera
                canvas.renderMode = RenderMode.WorldSpace;
                var guiCamera = GetVHGuiCamera(vrguiInstance);
                if (guiCamera != null) canvas.worldCamera = guiCamera;

                // 3. Enable interaction
                var canvasGroup = canvas.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;

                // 4. Let VHVR handle everything else
                AddCanvasToVHVR(canvas, vrguiInstance);

                EpicMMOVRUIPatch.LogInfo("EpicMMO canvas processed for VR");
            }
            catch (System.Exception e)
            {
                EpicMMOVRUIPatch.LogError($"Error processing canvas: {e}");
            }
        }

        private static object GetVHVRVRGUI()
        {
            try
            {
                var vrguiType = AccessTools.TypeByName("ValheimVRMod.VRCore.UI.VRGUI");
                if (vrguiType == null) return null;
                var vrguiInstances = Resources.FindObjectsOfTypeAll(vrguiType);
                return vrguiInstances.Length > 0 ? vrguiInstances[0] : null;
            }
            catch (System.Exception e)
            {
                EpicMMOVRUIPatch.LogError($"Error getting VRGUI: {e}");
                return null;
            }
        }

        private static Camera GetVHGuiCamera(object vrguiInstance)
        {
            try
            {
                var vrguiType = AccessTools.TypeByName("ValheimVRMod.VRCore.UI.VRGUI");
                var guiCameraField = AccessTools.Field(vrguiType, "_guiCamera");
                return guiCameraField?.GetValue(vrguiInstance) as Camera;
            }
            catch (System.Exception e)
            {
                EpicMMOVRUIPatch.LogError($"Error getting camera: {e}");
                return null;
            }
        }

        private static void AddCanvasToVHVR(Canvas canvas, object vrguiInstance)
        {
            try
            {
                var vrguiType = AccessTools.TypeByName("ValheimVRMod.VRCore.UI.VRGUI");
                var guiCanvasesField = AccessTools.Field(vrguiType, "_guiCanvases");
                var guiCanvases = guiCanvasesField?.GetValue(vrguiInstance) as System.Collections.IList;
                if (guiCanvases != null && !guiCanvases.Contains(canvas))
                {
                    guiCanvases.Add(canvas);
                }
            }
            catch (System.Exception e)
            {
                EpicMMOVRUIPatch.LogWarning($"Error adding to VHVR: {e}");
            }
        }
    }
}