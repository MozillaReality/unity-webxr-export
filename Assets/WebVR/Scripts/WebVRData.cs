using System;
using UnityEngine;

[Obsolete("Use WebXRData")]
[System.Serializable]
class WebVRData
{
	public WebVRControllerData[] controllers = new WebVRControllerData[0];
	public static WebVRData CreateFromJSON(string jsonString)
	{
		return JsonUtility.FromJson<WebVRData> (jsonString);
	}
}