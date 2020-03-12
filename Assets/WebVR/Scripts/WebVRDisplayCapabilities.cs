using System;
using UnityEngine;

[Obsolete("Use WebXRDisplayCapabilities")]
[System.Serializable]
public class WebVRDisplayCapabilities
{
	public bool canPresent;
	public bool hasPosition;
	public bool hasExternalDisplay;
}
