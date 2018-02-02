using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebVRInput : MonoBehaviour {
	private Interaction interaction = null;
	private int Joysticks = 0;

	void Awake() {
		interaction = GetComponent<Interaction>();
	}

	void Update() {
		if (Input.GetKey("joystick button 1")) {
			interaction.Pickup ();
		} 

		if (Input.GetKeyUp("joystick button 1")) {
			interaction.Drop ();
		}
	}
}
