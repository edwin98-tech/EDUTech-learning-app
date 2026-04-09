using UnityEngine;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Edutech.Core;

namespace Edutech.AI
{
    public class AIServiceConnector : MonoBehaviour
    {
        public static AIServiceConnector Instance { get; private set; }
        
        // System routing debug analysis confirmed that Google completely deprecated the 1.5 pipeline for this key!
        // We are deploying exclusively onto the core gemini-2.5-flash engine using the heavily optimized 512px proxy.
        private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
        
        // Removed static definition to prevent Unity Editor socket exhaustion during fast play/stop cycles
        private HttpClient httpClient;
        private System.Threading.SynchronizationContext mainThreadContext;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            httpClient = new HttpClient();
            
            // Capture the Unity Main Thread context to safely update UI from async tasks
            mainThreadContext = System.Threading.SynchronizationContext.Current;
        }

        private void OnDestroy()
        {
            httpClient?.Dispose();
        }

        public void RequestExplanation(string base64Image, string difficultyLevel, System.Action<string> onSuccess, System.Action<string> onError)
        {
            _ = SendRequestAsync(base64Image, difficultyLevel, onSuccess, onError);
        }

        private async Task SendRequestAsync(string base64Image, string difficultyLevel, System.Action<string> onSuccess, System.Action<string> onError)
        {
            if (DataPersistenceManager.Instance == null)
            {
                DispatchToMainThread(() => onError?.Invoke("System Error: No DataPersistenceManager."));
                return;
            }

            string apiKey = DataPersistenceManager.Instance.GeminiApiKey;
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
            {
                DispatchToMainThread(() => onError?.Invoke("API Key is missing. Update the .env file."));
                return;
            }

            string url = $"{apiEndpoint}?key={apiKey}";
            
            // Highly technical system prompt natively enforcing Spatial Geometry detection via JSON schema!
            // CRITICAL: Used single quotes around 'ymin,xmin,ymax,xmax' to ensure we do not break the escaping in the outgoing JSON packet
            string prompt = $"You are an advanced sci-fi AI visor. Output strictly in JSON format. Identify the primary physical object in the image into 'object_name'. Generate 3 technical sci-fi data points about the object into an array 'stats'. Find 2 very distinct physical sub-features on the object, and provide their spatial bounding boxes using the format 'ymin,xmin,ymax,xmax' (scaled 0-1000) inside an array 'features' with a 'label' string. Finally, provide exactly 2 short sentences into 'spoken_explanation' following this tone: {difficultyLevel}";
            
            // Constructing a native Multimodal JSON packet containing both the text instructions and the Base64 frame, locked explicitly into JSON Mime Type output!
            string jsonBody = $"{{\"contents\":[{{\"parts\":[{{\"text\":\"{prompt}\"}},{{\"inline_data\":{{\"mime_type\":\"image/jpeg\",\"data\":\"{base64Image}\"}}}}]}}], \"generationConfig\": {{\"responseMimeType\": \"application/json\"}}}}";

            try
            {
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, content);
                string responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Parse native Google Error
                    DispatchToMainThread(() => onError?.Invoke($"(API Error {response.StatusCode}) {responseText}"));
                }
                else
                {
                    string extractedText = ParseGeminiResponse(responseText);
                    DispatchToMainThread(() => 
                    {
                        onSuccess?.Invoke(extractedText);
                        DataPersistenceManager.Instance.SaveLearningLog("AR Camera Frame", extractedText);
                    });
                }
            }
            catch (System.Exception ex)
            {
                DispatchToMainThread(() => onError?.Invoke($"HTTP Exception: {ex.Message}"));
            }
        }

        private void DispatchToMainThread(System.Action action)
        {
            if (mainThreadContext != null)
            {
                mainThreadContext.Post(_ => action?.Invoke(), null);
            }
            else
            {
                action?.Invoke(); // Fallback
            }
        }

        // Native Unity JSON Object Mappers for Google's API Wrapper
        [System.Serializable] private class GeminiAPIResponse { public Candidate[] candidates; }
        [System.Serializable] private class Candidate { public Content content; }
        [System.Serializable] private class Content { public Part[] parts; }
        [System.Serializable] private class Part { public string text; }

        private string ParseGeminiResponse(string jsonResponse)
        {
            try
            {
                var res = JsonUtility.FromJson<GeminiAPIResponse>(jsonResponse);
                if (res != null && res.candidates != null && res.candidates.Length > 0 && res.candidates[0].content != null)
                {
                    return res.candidates[0].content.parts[0].text;
                }
            }
            catch (System.Exception e) {
                Debug.LogError($"[Gemini Parser] JSON wrapper failure: {e.Message}");
            }
            return "";
        }
    }
}
