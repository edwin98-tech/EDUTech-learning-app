using UnityEngine;

namespace Edutech.Core
{
    public class ProjectBootstrap : MonoBehaviour
    {
        public static ProjectBootstrap Instance { get; private set; }

        public enum TargetPlatform { MobileAR, QuestVR, EditorSim }
        public TargetPlatform currentPlatform;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null); // Unparent to satisfy DontDestroyOnLoad constraints
                DontDestroyOnLoad(gameObject);
                InitializePlatform();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePlatform()
        {
            #if UNITY_EDITOR
                currentPlatform = TargetPlatform.EditorSim;
                Application.targetFrameRate = 60;
            #elif UNITY_ANDROID || UNITY_IOS
                if (UnityEngine.XR.XRSettings.isDeviceActive && UnityEngine.XR.XRSettings.loadedDeviceName.Contains("Oculus"))
                {
                    currentPlatform = TargetPlatform.QuestVR;
                    Application.targetFrameRate = 90; // Meta Quest target
                }
                else
                {
                    currentPlatform = TargetPlatform.MobileAR;
                    Application.targetFrameRate = 60; // Mobile AR target
                    Screen.orientation = ScreenOrientation.LandscapeLeft; // Force Horizontal Landscape Mode!
                }
            #else
                currentPlatform = TargetPlatform.EditorSim;
                Application.targetFrameRate = 60;
            #endif
            
            Debug.Log($"[ProjectBootstrap] Initialized for platform: {currentPlatform} at {Application.targetFrameRate} FPS.");
        }
    }
}
