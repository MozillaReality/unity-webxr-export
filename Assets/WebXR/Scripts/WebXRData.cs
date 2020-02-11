using UnityEngine;

namespace WebXR
{
	[System.Serializable]
	class WebXRData
	{
		public WebXRControllerData[] controllers = new WebXRControllerData[0];

		public static WebXRData CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<WebXRData>(jsonString);
		}
	}
}