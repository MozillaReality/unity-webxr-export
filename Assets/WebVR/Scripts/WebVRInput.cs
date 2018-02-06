using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebVRInput : MonoBehaviour {
	private Interaction interaction = null;
	private WebVRCamera webVRCamera = null;

	void Awake() {
		interaction = GetComponent<Interaction>();
		webVRCamera = GetComponentInParent<WebVRCamera> ();
	}

	void Update() {
		if (webVRCamera.GetKey(gameObject, 1)) {
			Debug.Log(gameObject.name + " button 1 getkey !!!!");
		}

		if (webVRCamera.GetKeyDown(gameObject, 1)) {
			Debug.Log(gameObject.name + " button 1 getkeydown !!!!");
			interaction.Pickup ();
		}

		if (webVRCamera.GetKeyUp(gameObject, 1)) {
			Debug.Log(gameObject.name + " button 1 getkeup !!!!");
			interaction.Drop ();
		}

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
