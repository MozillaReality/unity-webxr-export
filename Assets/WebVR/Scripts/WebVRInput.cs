using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebVRInput : MonoBehaviour {
//	private Interaction interaction = null;
	private WebVRCamera webVRCamera = null;

	void Awake() {
//		interaction = GetComponent<Interaction>();
		webVRCamera = GetComponentInParent<WebVRCamera> ();
	}

	void Update() {

//		public bool GetPress(ulong buttonMask) { Update(); return (state.ulButtonPressed & buttonMask) != 0; }
//		public bool GetPressDown(ulong buttonMask) { Update(); return (state.ulButtonPressed & buttonMask) != 0 && (prevState.ulButtonPressed & buttonMask) == 0; }
//		public bool GetPressUp(ulong buttonMask) { Update(); return (state.ulButtonPressed & buttonMask) == 0 && (prevState.ulButtonPressed & buttonMask) != 0; }


//		string[] names = Input.GetJoystickNames();

		// log joystick buttons as they are pressed.
//		for(int i = 0; i < names.Length; i++) {
//			for (int j = 0; j < 10; j++) {
//				string joy = "joystick " + (i + 1) + " button " + j;
//				if (Input.GetKeyDown (joy)) {
//					Debug.Log(names[i] + " = " + joy);
//				}
//			}
//		}

//		if (webVRCamera.GetKeyDown(gameObject, 1)) {
//			Debug.Log(gameObject.name + " button 1 down!!!!");
//		}
//
//		if (webVRCamera.GetKeyUp(gameObject, 1)) {
//			Debug.Log(gameObject.name + " button 1 up!!!!");
//		}

		if (webVRCamera.GetKey(gameObject, 1)) {
			Debug.Log(gameObject.name + " button 1!!!!");
		}

//		if (webVRCamera.GetDown(gameObject, 1)) {
//			Debug.Log(gameObject.name + " button 1 down !!!!");
//		}

//		if (controllerId != null) {
//			if (Input.GetKeyDown ("joystick " + controllerId + " button 1")) {
//				Debug.Log ("joy " + controllerId + " down");
//				//interaction.Pickup ();
//			} 
//
//			if (Input.GetKeyUp ("joystick " + controllerId + " button 1")) {
//				Debug.Log ("joy " + controllerId + " up");
//				//interaction.Drop ();
//			}
//		}
	}
}
