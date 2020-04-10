using UnityEditor;

namespace WebXR.Editor
{
	public static class Builder
	{
		[MenuItem("Build/All")]
		public static void BuildAll()
		{
			BuildPackage();
			BuildDesertSample();
		}

		[MenuItem("Build/Package")]
		public static void BuildPackage()
		{
			AssetDatabase.ExportPackage(new[] { "Assets/WebXR", "Assets/WebGLTemplates/WebXR" }, "WebXR-Assets.unitypackage", ExportPackageOptions.Recurse);
		}

		[MenuItem("Build/Desert Sample")]
		public static void BuildDesertSample()
		{
#if !UNITY_2018_4_OR_NEWER
			// There is no explicit api for setting the template as of 2018.4
			PlayerSettings.SetPropertyString("template", "PROJECT:WebXR", BuildTargetGroup.WebGL);
#else
			PlayerSettings.WebGL.template = "WebXR";
#endif
			BuildPipeline.BuildPlayer(new BuildPlayerOptions
			{
				target = BuildTarget.WebGL,
				locationPathName = "Build",
				scenes = new[] { "Assets/WebXR/Samples/Desert/WebXR.unity" },
			});
		}
	}
}
