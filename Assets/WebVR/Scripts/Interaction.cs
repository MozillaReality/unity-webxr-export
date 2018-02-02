using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour {
	private FixedJoint attachJoint = null;
	private Rigidbody currentRigidBody = null;
	private List<Rigidbody> contactRigidBodies = new List<Rigidbody> ();

	void Awake () {
		attachJoint = GetComponent<FixedJoint> ();
	}

	void OnTriggerEnter(Collider collider) {
		if (!collider.gameObject.CompareTag ("Interactable"))
			return;

		contactRigidBodies.Add (collider.gameObject.GetComponent<Rigidbody> ());
	}

	void OnTriggerExit(Collider collider) {
		if (!collider.gameObject.CompareTag ("Interactable"))
			return;

		contactRigidBodies.Remove(collider.gameObject.GetComponent<Rigidbody> ());
	}

	public void Pickup() {
		currentRigidBody = GetNearestRigidBody ();
		if (!currentRigidBody)
			return;
		Debug.Log ("picking up");
		currentRigidBody.transform.position = transform.position;
		attachJoint.connectedBody = currentRigidBody;
	}

	public void Drop() {
		if (!currentRigidBody)
			return;
		Debug.Log ("dropping");
		attachJoint.connectedBody = null;
		currentRigidBody = null;
	}

	private Rigidbody GetNearestRigidBody() {
		Rigidbody nearestRigidBody = null;
		float minDistance = float.MaxValue;
		float distance = 0.0f;

		foreach (Rigidbody contactBody in contactRigidBodies) {
			distance = (contactBody.gameObject.transform.position - transform.position).sqrMagnitude;

			if (distance < minDistance) {
				minDistance = distance;
				nearestRigidBody = contactBody;
			}
		}

		return nearestRigidBody;
	}
}
