using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebVRInput : MonoBehaviour {
	private Interaction interaction = null;
	private WebVRCamera webVRCamera = null;

	void Awake() {
		Rigidbody rigidBody = GetComponent<Rigidbody>();
		rigidBody.isKinematic = true;

		interaction = GetComponent<Interaction>();
		webVRCamera = GetComponentInParent<WebVRCamera> ();
	}

	void Update() {
		if (webVRCamera.GetKeyDown(gameObject, 1)) {
			interaction.Pickup ();
		}

		if (webVRCamera.GetKeyUp(gameObject, 1)) {
			interaction.Drop (webVRCamera);
		}
	}
}
