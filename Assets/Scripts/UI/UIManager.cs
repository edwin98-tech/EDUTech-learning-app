using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Edutech.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        public CanvasGroup mainPanel;
        public CanvasGroup infoPanel;

        [Header("UI Elements")]
        public Image backgroundPanel;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI infoText;
        private Image[] diffBgs = new Image[3];
        private GameObject privacyPanel;
        private GameObject controlDock;
        private GameObject reticleObj;
        
        private List<GameObject> activeHolograms = new List<GameObject>(); // Tracks spawned UI

        [Header("Colors (Design System)")]
        private readonly Color colDeepSpace = new Color(0.043f, 0.055f, 0.078f);    // #0B0E14
        private readonly Color colElectricCyan = new Color(0f, 0.949f, 1f);       // #00F2FF
        private readonly Color colCloudGrey = new Color(0.878f, 0.878f, 0.878f);  // #E0E0E0
        private readonly Color colAlertRed = new Color(1f, 0.294f, 0.294f);       // #FF4B4B

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            // EMERGENCY SAFEGUARDS: If Unity's AR Foundation deleted the UI Raycasters, rebuild them dynamically!
            if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void Start()
        {
            ApplyTheme();
            CreatePrivacyGate();
            CreateReticle();
            CreatePersonalityButtons();
            
            // Hide the runtime AR HUD until activation
            if (controlDock != null) controlDock.SetActive(false);
            if (reticleObj != null) reticleObj.SetActive(false);
        }

        private void CreatePrivacyGate()
        {
            // Dynamically intercept the physical camera before it drains battery!
            // We must disable the script Component, NOT the GameObject, to prevent ARCore native boot crashes!
            MonoBehaviour arSessionComponent = null;
            GameObject rootSession = GameObject.Find("AR Session") ?? GameObject.Find("XR Origin");
            if (rootSession != null)
            {
                arSessionComponent = rootSession.GetComponent("ARSession") as MonoBehaviour;
                if (arSessionComponent != null) arSessionComponent.enabled = false; // Freeze Camera polling safely
            }

            privacyPanel = new GameObject("PrivacyScreen");
            privacyPanel.transform.SetParent(transform, false);
            
            Image bg = privacyPanel.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.08f, 1f); // 100% Opaque Deep Space Privacy Wall
            
            RectTransform bgRt = privacyPanel.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            
            // Form Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(privacyPanel.transform, false);
            TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.text = "EDUTECH";
            titleTxt.fontSize = 60;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = colCloudGrey;
            titleTxt.enableWordWrapping = false; // Force single line
            
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.sizeDelta = new Vector2(800, 100); // Wide enough for single line display
            titleRt.anchorMin = new Vector2(0.5f, 0.65f);
            titleRt.anchorMax = new Vector2(0.5f, 0.65f);
            titleRt.anchoredPosition = Vector2.zero;
            
            // Call To Action (Camera Unlock)
            GameObject btnObj = new GameObject("StartButton");
            btnObj.transform.SetParent(privacyPanel.transform, false);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0, 0.5f, 0.8f, 1f); // Vibrant Cyan backing
            
            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() => {
                if (arSessionComponent != null) arSessionComponent.enabled = true; // Wake up the Camera Matrix!
                
                AI.ObjectDetectionManager.Instance.UnlockVision();
                Destroy(privacyPanel);
                
                // Show AR HUD
                if (controlDock != null) controlDock.SetActive(true);
                if (reticleObj != null) reticleObj.SetActive(true);
                if (statusText != null) statusText.text = "AR Online. Tap to Scan.";
            });
            
            GameObject btnTxtObj = new GameObject("BtnText");
            btnTxtObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI btnTxt = btnTxtObj.AddComponent<TextMeshProUGUI>();
            btnTxt.text = "INITIALIZE LENS";
            btnTxt.fontSize = 32;
            btnTxt.alignment = TextAlignmentOptions.Center;
            btnTxt.color = Color.white;
            btnTxt.raycastTarget = false; // CRITICAL: Stop text from absorbing physical touch events!
            
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0.4f);
            btnRt.anchorMax = new Vector2(0.5f, 0.4f);
            btnRt.sizeDelta = new Vector2(400, 100);
            
            RectTransform btnTxtRt = btnTxtObj.GetComponent<RectTransform>();
            btnTxtRt.anchorMin = Vector2.zero;
            btnTxtRt.anchorMax = Vector2.one;
            btnTxtRt.offsetMin = Vector2.zero;
            btnTxtRt.offsetMax = Vector2.zero;
        }

        private void CreateReticle()
        {
            reticleObj = new GameObject("Reticle");
            reticleObj.transform.SetParent(transform, false);
            TextMeshProUGUI txt = reticleObj.AddComponent<TextMeshProUGUI>();
            txt.text = "+";
            txt.fontSize = 72;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = new Color(1, 1, 1, 0.5f); // Glassmorphic white lock-on
            
            RectTransform rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        private void CreatePersonalityButtons()
        {
            controlDock = new GameObject("ControlDock");
            controlDock.transform.SetParent(transform, false);
            
            Image dockBg = controlDock.AddComponent<Image>();
            dockBg.color = new Color(0, 0, 0, 0f); // 100% Transparent, floating aggressively over the AR feed!
            
            RectTransform dockRt = controlDock.GetComponent<RectTransform>();
            dockRt.anchorMin = new Vector2(0, 0);
            dockRt.anchorMax = new Vector2(1, 0);
            dockRt.pivot = new Vector2(0.5f, 0);
            dockRt.sizeDelta = new Vector2(0, 200); // Massive 200px tall dock anchored absolutely at the bottom
            dockRt.anchoredPosition = Vector2.zero;

            float[] xOffsets = { -300f, 0f, 300f };
            string[] names = { "Like I'm 5", "College Level", "Story Mode" };
            
            for (int i = 0; i < 3; i++)
            {
                int index = i; // Closure capture safety
                GameObject btnObj = new GameObject($"Btn_{names[index]}");
                btnObj.transform.SetParent(controlDock.transform, false);
                
                Image img = btnObj.AddComponent<Image>();
                img.color = (index == 0) ? new Color(0, 0.4f, 0.6f, 0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.6f);
                diffBgs[index] = img;
                
                Button btn = btnObj.AddComponent<Button>();
                btn.onClick.AddListener(() => 
                {
                    // Clear all highlights
                    foreach(var bg in diffBgs) if(bg != null) bg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
                    // Lock-in current button
                    diffBgs[index].color = new Color(0, 0.4f, 0.6f, 0.8f);

                    if (index == 0) AI.ObjectDetectionManager.Instance.SetDifficulty(AI.ObjectDetectionManager.DifficultyTier.Beginner);
                    if (index == 1) AI.ObjectDetectionManager.Instance.SetDifficulty(AI.ObjectDetectionManager.DifficultyTier.Advanced);
                    if (index == 2) AI.ObjectDetectionManager.Instance.SetDifficulty(AI.ObjectDetectionManager.DifficultyTier.Creative);
                    ShowStatus($"AI Mode Locked: {names[index]}");
                });
                
                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(btnObj.transform, false);
                TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
                txt.text = names[index];
                txt.color = colCloudGrey;
                txt.fontSize = 28; // Massive typography
                txt.alignment = TextAlignmentOptions.Center;
                txt.raycastTarget = false; // Safegaurd AI Personality dock text too!
                
                RectTransform rt = btnObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(250, 90); // Bulky interactable hitboxes
                rt.anchoredPosition = new Vector2(xOffsets[index], 0);
                
                RectTransform txtRt = txt.GetComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.offsetMin = Vector2.zero;
                txtRt.offsetMax = Vector2.zero;
            }
        }

        private void ApplyTheme()
        {
            if (backgroundPanel != null) backgroundPanel.color = colDeepSpace;
            if (statusText != null) statusText.color = colCloudGrey;
            if (infoText != null) infoText.color = colCloudGrey;
        }

        public void ShowError(string msg)
        {
            if (statusText != null)
            {
                statusText.color = colAlertRed;
                statusText.text = msg;
            }
        }

        public void ShowStatus(string msg)
        {
            if (statusText != null)
            {
                statusText.color = colElectricCyan;
                statusText.text = msg;
            }
        }

        public void SpawnHologram(AI.SciFiVisionPayload payload, Vector3 worldPos)
        {
            // Clear any old sci-fi overlays
            foreach(var h in activeHolograms) if (h != null) Destroy(h);
            activeHolograms.Clear();

            // 1. Draw Bounding Boxes (Temporarily on screen for scanning effect)
            if (payload.features != null)
            {
                foreach (var feature in payload.features)
                {
                    if (string.IsNullOrEmpty(feature.coords)) continue;
                    
                    string[] c = feature.coords.Split(',');
                    if (c.Length != 4) continue;
                    
                    float yMinGemini = float.Parse(c[0]) / 1000f;
                    float xMinGemini = float.Parse(c[1]) / 1000f;
                    float yMaxGemini = float.Parse(c[2]) / 1000f;
                    float xMaxGemini = float.Parse(c[3]) / 1000f;

                    float xMinUnity = xMinGemini;
                    float xMaxUnity = xMaxGemini;
                    float yMinUnity = 1.0f - yMaxGemini; 
                    float yMaxUnity = 1.0f - yMinGemini;

                    GameObject boxObj = new GameObject($"Box_{feature.label}");
                    boxObj.transform.SetParent(transform, false);
                    Image boxImg = boxObj.AddComponent<Image>();
                    boxImg.color = new Color(0f, 0.8f, 1f, 0.2f);
                    boxImg.raycastTarget = false; // INPUT FIX: Make Ghost UI
                    
                    UnityEngine.UI.Outline outline = boxObj.AddComponent<UnityEngine.UI.Outline>();
                    outline.effectColor = colElectricCyan;
                    outline.effectDistance = new Vector2(4, -4);
                    
                    RectTransform rt = boxObj.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(xMinUnity, yMinUnity);
                    rt.anchorMax = new Vector2(xMaxUnity, yMaxUnity);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    
                    GameObject labelObj = new GameObject("BoxLabel");
                    labelObj.transform.SetParent(boxObj.transform, false);
                    Image lblBg = labelObj.AddComponent<Image>();
                    lblBg.color = new Color(0, 0, 0, 0.8f);
                    lblBg.raycastTarget = false; // INPUT FIX
                    
                    GameObject txtObj = new GameObject("LabelText");
                    txtObj.transform.SetParent(labelObj.transform, false);
                    TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
                    txt.text = feature.label.ToUpper();
                    txt.fontSize = 20;
                    txt.color = colElectricCyan;
                    txt.alignment = TextAlignmentOptions.Center;
                    txt.raycastTarget = false;
                    
                    RectTransform lblRt = labelObj.GetComponent<RectTransform>();
                    lblRt.anchorMin = new Vector2(0.5f, 1f);
                    lblRt.anchorMax = new Vector2(0.5f, 1f);
                    lblRt.sizeDelta = new Vector2(250, 40);
                    lblRt.anchoredPosition = new Vector2(0, 30);

                    activeHolograms.Add(boxObj);
                }
            }

            // 2. Spawn Persistent World Space Stat Card
            GameObject worldCanvasObj = new GameObject("WorldHologram");
            Canvas cWorld = worldCanvasObj.AddComponent<Canvas>();
            cWorld.renderMode = RenderMode.WorldSpace;
            CanvasGroup group = worldCanvasObj.AddComponent<CanvasGroup>();
            group.alpha = 0; // Start invisible for the smooth sync fade!
            
            // Move slightly up and offset to the RIGHT so it doesn't block the object
            worldCanvasObj.transform.position = worldPos + Vector3.up * 0.1f + Camera.main.transform.right * 0.5f; 
            worldCanvasObj.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f); // 40% smaller
            
            // Add Billboard script logic
            worldCanvasObj.AddComponent<HUDCardLookAt>();

            GameObject cardObj = new GameObject("SciFiStatCard");
            cardObj.transform.SetParent(worldCanvasObj.transform, false);
            
            Image bgImg = cardObj.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.08f, 0.12f, 0.85f);
            bgImg.raycastTarget = false; // INPUT FIX
            
            UnityEngine.UI.Outline cardOutline = cardObj.AddComponent<UnityEngine.UI.Outline>();
            cardOutline.effectColor = new Color(0f, 0.4f, 0.6f, 0.8f);
            cardOutline.effectDistance = new Vector2(4, -4);
            
            RectTransform cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(550, 500); // Taller to 'cover' the bottom of the text!
            
            string formattedText = $"<size=40><b>OBJECT: <color=#00e5ff>{payload.object_name.ToUpper()}</color></b></size>\n\n"; // Smaller header
            if (payload.stats != null)
            {
                foreach(string s in payload.stats) formattedText += $"<color=#00e5ff>▪</color> {s}\n";
            }
            
            GameObject statDataObj = new GameObject("StatData");
            statDataObj.transform.SetParent(cardObj.transform, false);
            TextMeshProUGUI statTxt = statDataObj.AddComponent<TextMeshProUGUI>();
            statTxt.text = formattedText;
            statTxt.fontSize = 24; // Scaled down for readability
            statTxt.color = colCloudGrey;
            statTxt.alignment = TextAlignmentOptions.TopLeft;
            statTxt.enableWordWrapping = true;
            statTxt.raycastTarget = false;
            
            RectTransform statRt = statDataObj.GetComponent<RectTransform>();
            statRt.anchorMin = Vector2.zero;
            statRt.anchorMax = Vector2.one;
            statRt.offsetMin = new Vector2(30, 30);
            statRt.offsetMax = new Vector2(-30, -30);
            
            activeHolograms.Add(worldCanvasObj);
            
            // HUD Bloom Animation: Syncs the visual appearance with the incoming audio stream
            StartCoroutine(FadeHologramIn(group));
        }

        private IEnumerator FadeHologramIn(CanvasGroup group)
        {
            float elapsed = 0;
            while (elapsed < 0.6f)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Clamp01(elapsed / 0.6f);
                yield return null;
            }
        }

        // Internal helper to make HUD face the camera
        private class HUDCardLookAt : MonoBehaviour {
            void Update() {
                if (Camera.main != null) {
                    transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
                }
            }
        }

        // 0.3s ease-in-out transitions for UI panels
        public void TogglePanel(CanvasGroup panel, bool show)
        {
            if (panel == null) return;
            StopAllCoroutines();
            StartCoroutine(FadePanel(panel, show ? 1f : 0f, 0.3f));
        }

        private IEnumerator FadePanel(CanvasGroup panel, float targetAlpha, float duration)
        {
            float startAlpha = panel.alpha;
            float time = 0;

            if (targetAlpha > 0) panel.gameObject.SetActive(true);

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                t = t * t * (3f - 2f * t); // Ease In Out
                panel.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            panel.alpha = targetAlpha;
            if (targetAlpha == 0) panel.gameObject.SetActive(false);
            
            panel.interactable = targetAlpha > 0;
            panel.blocksRaycasts = targetAlpha > 0;
        }
    }
}
