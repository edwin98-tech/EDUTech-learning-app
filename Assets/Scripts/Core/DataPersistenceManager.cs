using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Edutech.Core
{
    public class DataPersistenceManager : MonoBehaviour
    {
        public static DataPersistenceManager Instance { get; private set; }
        
        private string workspacePath;
        public string GeminiApiKey { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null); // Unparent to satisfy DontDestroyOnLoad constraints
                DontDestroyOnLoad(gameObject);
                InitializePaths();
                LoadEnvironmentVariables();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePaths()
        {
            // Use persistentDataPath, which is guaranteed read/write on both PC and Android natively.
            // This prevents an UnauthorizedAccessException crash on mobile!
            workspacePath = Application.persistentDataPath;
        }

        private void LoadEnvironmentVariables()
        {
            // Look for .env file at the project root (default for git repos)
            string envPath = Path.Combine(Application.dataPath, "../.env");
            
            if (File.Exists(envPath))
            {
                string[] lines = File.ReadAllLines(envPath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    
                    string[] parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        
                        if (key == "GEMINI_API_KEY")
                        {
                            GeminiApiKey = value;
                            Debug.Log("[DataPersistenceManager] API Key loaded successfully from .env.");
                            return;
                        }
                    }
                }
            }

            // Fallback warning
            GeminiApiKey = "";
            Debug.LogWarning("[DataPersistenceManager] .env file not found or GEMINI_API_KEY missing. Please create a .env file at the project root.");
        }

        public void SaveLearningLog(string objectName, string explanation)
        {
            string logPath = Path.Combine(workspacePath, "learning_log.json");
            string entry = $"{{\"timestamp\": \"{System.DateTime.UtcNow}\", \"object\": \"{objectName}\", \"explanation\": \"{explanation}\"}}\n";
            File.AppendAllText(logPath, entry);
        }
    }
}
