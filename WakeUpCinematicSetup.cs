#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Project.EditorScripts
{
    public class WakeUpCinematicSetup : EditorWindow
    {
        [MenuItem("Tools/Setup WakeUp Cinematic")]
        public static void ShowWindow()
        {
            SetupCinematic();
        }

        private static void SetupCinematic()
        {
            // 1. Create or Find Manager
            GameObject managerObj = GameObject.Find("WakeUpCinematicManager");
            if (managerObj == null)
            {
                managerObj = new GameObject("WakeUpCinematicManager");
            }

            // 2. Clear old scripts off it and add main script
            WakeUpCinematic wuLogic = managerObj.GetComponent<WakeUpCinematic>();
            if (wuLogic == null)
            {
                wuLogic = managerObj.AddComponent<WakeUpCinematic>();
            }

            // 3. Setup UI Canvas for Face to Black
            GameObject canvasObj = GameObject.Find("WakeUpFadeCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("WakeUpFadeCanvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999; // Render on top of everything
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // Add Image child
                GameObject imgObj = new GameObject("FadeImage");
                imgObj.transform.SetParent(canvasObj.transform, false);
                Image img = imgObj.AddComponent<Image>();
                img.color = Color.black;
                
                RectTransform rt = img.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                
                wuLogic.fadeOverlay = img;
            }
            else
            {
                Image existingImg = canvasObj.GetComponentInChildren<Image>();
                if (existingImg != null)
                {
                    wuLogic.fadeOverlay = existingImg;
                }
            }

            // 4. Find Player Car & CockpitCamPoint
            GameObject playerCar = GameObject.Find("PlayerCar");
            if (playerCar != null)
            {
                wuLogic.playerCar = playerCar.transform;
                wuLogic.carController = playerCar.GetComponent<CarController3D>();
                
                Transform cockpitPoint = playerCar.transform.Find("CockpitCamPoint");
                if (cockpitPoint == null)
                {
                    // Fallback search, sometimes things are nested
                    cockpitPoint = playerCar.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>()?.transform.parent;
                }
                
                // Ensure there is a CockpitCamPoint
                if (cockpitPoint == null)
                {
                    Debug.LogWarning("[SetupWakeUpCinematic] Could not find CockpitCamPoint on PlayerCar. Please run 'Setup Cockpit Camera' first.");
                }
                else
                {
                    wuLogic.cockpitCamPoint = cockpitPoint;
                }
            }
            else
            {
                Debug.LogWarning("[SetupWakeUpCinematic] Could not find GameObject named 'PlayerCar'.");
            }

            Selection.activeGameObject = managerObj;
            EditorGUIUtility.PingObject(managerObj);

            Debug.Log("<color=green>SUCCESS:</color> Wake-Up Cinematic Manager created and configured. Press Play to test the scene.");
        }
    }
}
#endif
