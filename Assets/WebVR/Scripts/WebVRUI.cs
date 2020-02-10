using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[Obsolete("Use WebXRUI")]
public class WebVRUI {

	[DllImport("__Internal")]
	public static extern void displayElementId(string id);
}
