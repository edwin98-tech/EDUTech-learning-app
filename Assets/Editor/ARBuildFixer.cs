#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Edutech.Editor
{
    public class ARBuildFixer
    {
        [MenuItem("Edutech/2. Fix Android AR Build Settings", false, 2)]
        public static void FixAndroidARSettings()
        {
            // 1. ARCore strictly forbids Vulkan. We must force OpenGLES3.
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES3 });

            // 2. Set Minimum API Level to Android 8.0 (API Level 26) which is highly recommended for stable modern ARCore
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;

            // 3. Google Play highly demands IL2CPP and ARM64 architecture for performance
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            Debug.Log("[AR Setup] Successfully wiped Vulkan! Forced OpenGLES3, IL2CPP, ARM64, and Android API 26 for pristine ARCore compatibility.");
        }
    }
}
#endif
