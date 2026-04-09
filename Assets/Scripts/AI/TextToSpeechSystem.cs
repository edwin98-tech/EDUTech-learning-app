using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace Edutech.AI
{
    public class TextToSpeechSystem : MonoBehaviour
    {
        public static TextToSpeechSystem Instance { get; private set; }
        private AudioSource audioSource;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Auto-heal missing components
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            
            // Unity requires an AudioListener (virtual ear) to hear any AudioSources. 
            // If the default camera was deleted, we automatically inject one!
            if (FindObjectOfType<AudioListener>() == null)
            {
                if (Camera.main != null) Camera.main.gameObject.AddComponent<AudioListener>();
                else gameObject.AddComponent<AudioListener>();
                Debug.Log("[TTS] Auto-injected missing AudioListener into the scene.");
            }
        }

        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            Debug.Log($"[TTS] Synthesizing speech: {text}");
            // Removed redundant status update to prioritize network bandwidth
            
            StartCoroutine(DownloadTheAudio(text));
        }

        private IEnumerator DownloadTheAudio(string text)
        {
            // Google Translate's unofficial free TTS endpoint creates highly responsive MP3s.
            // Note: Limited to 200 chars per API request, so we clamp it safely for prototyping.
            string safeText = text.Length > 200 ? text.Substring(0, 197) + "..." : text;
            
            // Generate the strictly formatted URL with a 1.1x speed boost for a snappier Sci-Fi feel!
            string url = "https://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=32&client=tw-ob&q=" 
                         + UnityWebRequest.EscapeURL(safeText) + "&tl=en-us&ttsspeed=1.1";

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"[TTS Error]: {www.error}");
                }
                else
                {
                    // Extract, map, and play!
                    AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                    if (myClip == null || myClip.length == 0)
                    {
                        Debug.LogError("[TTS Error]: Unity failed to decode the MP3 stream. Google might be returning an invalid format.");
                        yield break;
                    }
                    audioSource.spatialBlend = 0f; // Force 2D sound
                    audioSource.priority = 0; // Highest Priority in the Unity Mixer!
                    audioSource.volume = 1f;
                    audioSource.clip = myClip;
                    audioSource.Play();
                    Debug.Log($"[TTS Success] Playing generated audio stream! Length: {myClip.length} seconds.");
                }
            }
        }
    }
}
