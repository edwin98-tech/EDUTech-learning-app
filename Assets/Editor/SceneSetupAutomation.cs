#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Edutech.Core;
using Edutech.UI;
using Edutech.AI;
using TMPro;
using UnityEngine.UI;

namespace Edutech.Editor
{
    public class SceneSetupAutomation : MonoBehaviour
    {
        [MenuItem("Edutech/1. Setup Complete Scene", false, 1)]
        private static void ConstructScene()
        {
            // 1. Setup Camera Rig (Simple Standin for AR/VR Rig)
            GameObject rig = new GameObject("PlayerRig");
            Camera cam = rig.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f); // Deep Space
            rig.tag = "MainCamera";

            // 2. Setup Managers
            GameObject managers = new GameObject("--- MANAGERS ---");
            
            GameObject bootstrap = new GameObject("ProjectBootstrap");
            bootstrap.transform.SetParent(managers.transform);
            bootstrap.AddComponent<ProjectBootstrap>();
            
            GameObject dataMgr = new GameObject("DataPersistenceManager");
            dataMgr.transform.SetParent(managers.transform);
            dataMgr.AddComponent<DataPersistenceManager>();

            GameObject aiConnector = new GameObject("AIServiceConnector");
            aiConnector.transform.SetParent(managers.transform);
            aiConnector.AddComponent<AIServiceConnector>();

            GameObject objDetection = new GameObject("ObjectDetectionManager");
            objDetection.transform.SetParent(managers.transform);
            objDetection.AddComponent<ObjectDetectionManager>();
            
            GameObject ttsSys = new GameObject("TextToSpeechSystem");
            ttsSys.transform.SetParent(managers.transform);
            ttsSys.AddComponent<TextToSpeechSystem>();

            // 3. Setup UI Canvas
            GameObject canvasObj = new GameObject("UICanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            UIManager uiManager = canvasObj.AddComponent<UIManager>();
            
            GameObject bgPanel = new GameObject("BackgroundPanel");
            bgPanel.transform.SetParent(canvasObj.transform, false);
            Image bgImg = bgPanel.AddComponent<Image>();
            
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.text = "Initializing AI...";
            txt.alignment = TextAlignmentOptions.Center;
            
            uiManager.backgroundPanel = bgImg;
            uiManager.statusText = txt;

            Undo.RegisterCreatedObjectUndo(rig, "Setup Scene");
            Debug.Log("Scene scaffolding completed using automation!");
        }
    }
}
#endif
