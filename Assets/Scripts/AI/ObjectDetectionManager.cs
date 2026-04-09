using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

namespace Edutech.AI {
    [System.Serializable]
    public class SpatialFeature
    {
        public string label;
        public string coords; // "ymin,xmin,ymax,xmax" 0-1000 format
    }

    [System.Serializable]
    public class SciFiVisionPayload
    {
        public string object_name;
        public string[] stats;
        public SpatialFeature[] features;
        public string spoken_explanation;
    }

    public class ObjectDetectionManager : MonoBehaviour
    {
        public static ObjectDetectionManager Instance { get; private set; }

        public enum DifficultyTier { Beginner, Intermediate, Advanced, Creative }
        public DifficultyTier currentDifficulty = DifficultyTier.Beginner;

        [Header("TFLite Model settings")]
        public string defaultModelUrl = "https://tfhub.dev/tensorflow/lite-model/mobilenet_v2_1.0_224/1/default/1";
        private bool isModelLoaded = false;
        private bool isProcessing = false; // Anti-spam click blocker
        private bool isVisionActive = false; // Prevents camera scraping before privacy screen is solved
        
        private ARRaycastManager arRaycastManager;
        private Vector3 lastHitPoint = Vector3.zero;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            LoadTFLiteModelFallback();
            
            // Auto-locate the AR Raycast Manager on the AR Session Origin
            arRaycastManager = FindObjectOfType<ARRaycastManager>();
            if (arRaycastManager == null)
            {
                GameObject origin = GameObject.Find("AR Session Origin") ?? GameObject.Find("XR Origin");
                if (origin != null) arRaycastManager = origin.AddComponent<ARRaycastManager>();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                DetectObjectInView();
                return;
            }

            // Mobile Touch Support
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                // PREVENT UI CLICK BLEED-THROUGH! If we are tapping the HUD Dock, do NOT trigger the camera!
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
                
                CalculateSpatialHitPoint(Input.GetTouch(0).position);
                DetectObjectInView();
            }
            // Editor PC Mouse Support
            else if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                
                CalculateSpatialHitPoint(Input.mousePosition);
                DetectObjectInView();
            }
        }

        private void CalculateSpatialHitPoint(Vector2 screenPos)
        {
            lastHitPoint = Camera.main.transform.position + Camera.main.transform.forward * 1.5f; // Fallback: 1.5m away

            if (arRaycastManager != null)
            {
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (arRaycastManager.Raycast(screenPos, hits, TrackableType.All))
                {
                    lastHitPoint = hits[0].pose.position;
                    Debug.Log($"[Spatial Vision] Surface Detected at: {lastHitPoint}");
                }
            }
        }

        private void LoadTFLiteModelFallback()
        {
            // Usually requires Unity Barracuda or TFLite Unity Plugin.
            // We use a generic interface so any pre-trained model can be swapped in.
            Debug.Log("[ObjectDetection] Initializing TFLite local detection hook.");
            isModelLoaded = true;
        }

        public void UnlockVision()
        {
            isVisionActive = true;
        }

        public void DetectObjectInView()
        {
            if (!isModelLoaded || isProcessing || !isVisionActive) return;
            isProcessing = true;

            UI.UIManager.Instance.ShowStatus("Scanning AR Camera...");
            StartCoroutine(CaptureAndAnalyzeFrame());
        }

        private IEnumerator CaptureAndAnalyzeFrame()
        {
            yield return new WaitForEndOfFrame();

            // Capture raw 1080p+ physical screen pixels
            Texture2D rawScreen = ScreenCapture.CaptureScreenshotAsTexture();
            
            // Generate a hyper-compressed 512x512 proxy to bypass Google Token Exhuastion Quota!
            Texture2D compressedProxy = ResizeTexture(rawScreen, 512, 512);
            
            // Encode the 512x512 proxy to standard JPEG
            byte[] jpegData = compressedProxy.EncodeToJPG(50);
            string base64Image = System.Convert.ToBase64String(jpegData);
            
            // Nuke memory leaks immediately
            Destroy(rawScreen);
            Destroy(compressedProxy);
            
            UI.UIManager.Instance.ShowStatus("Uploading to AI Vision Engine...");
            
            string stylingPrompt = "";
            switch (currentDifficulty)
            {
                case DifficultyTier.Beginner: stylingPrompt = "Explain this simply and playfully, as if speaking to a 5-year-old child."; break;
                case DifficultyTier.Advanced: stylingPrompt = "Provide a highly technical, college-level explanation using advanced scientific and engineering terminology."; break;
                case DifficultyTier.Creative: stylingPrompt = "Write a fun, highly creative, and imaginative 2-sentence story about this object."; break;
            }

            AIServiceConnector.Instance.RequestExplanation(
                base64Image, 
                stylingPrompt,
                OnExplanationSuccess,
                OnExplanationError
            );
        }

        private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(source, rt); // Hardware accelerated blit
            
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            
            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        private void OnExplanationSuccess(string explanationText)
        {
            isProcessing = false;
            UI.UIManager.Instance.ShowStatus("Analysis Complete");
            
            try
            {
                SciFiVisionPayload payload = JsonUtility.FromJson<SciFiVisionPayload>(explanationText);
                if (payload != null)
                {
                    // 1. Start Audio download immediately (Async Network Lag starts now)
                    TextToSpeechSystem.Instance.Speak(payload.spoken_explanation);
                    
                    // 2. Build and Fade-in the Hologram (Visuals appear while audio loads)
                    UI.UIManager.Instance.SpawnHologram(payload, lastHitPoint);
                }
                else
                {
                    UI.UIManager.Instance.ShowError("AI Vision Payload Malformed");
                }
            }
            catch (System.Exception e)
            {
                UI.UIManager.Instance.ShowError($"JSON Matrix Error: {e.Message}");
            }
        }

        private void OnExplanationError(string errorMsg)
        {
            isProcessing = false;
            UI.UIManager.Instance.ShowError($"AI Error: {errorMsg}");
        }
        
        public void SetDifficulty(DifficultyTier tier)
        {
            currentDifficulty = tier;
        }
    }
}
