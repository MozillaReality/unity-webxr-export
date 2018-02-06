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

	void OnTriggerEnter(Collider other) {
		if (!other.gameObject.CompareTag ("Interactable"))
			return;

		contactRigidBodies.Add (other.gameObject.GetComponent<Rigidbody> ());
	}

	void OnTriggerExit(Collider other) {
		if (!other.gameObject.CompareTag ("Interactable"))
			return;

		contactRigidBodies.Remove(other.gameObject.GetComponent<Rigidbody> ());
	}

	public void Pickup() {
		currentRigidBody = GetNearestRigidBody ();
		if (!currentRigidBody)
			return;
		currentRigidBody.transform.position = transform.position;
		attachJoint.connectedBody = currentRigidBody;
	}

	public void Drop() {
		if (!currentRigidBody)
			return;
		attachJoint.connectedBody = null;
		currentRigidBody = null;
	}

	private Rigidbody GetNearestRigidBody() {
		Rigidbody nearestRigidBody = null;
		float minDistance = float.MaxValue;
		float distance = 0.0f;

		foreach (Rigidbody contactBody in contactRigidBodies) {
			Debug.Log (contactBody.gameObject);
			distance = (contactBody.gameObject.transform.position - transform.position).sqrMagnitude;

			if (distance < minDistance) {
				minDistance = distance;
				nearestRigidBody = contactBody;
			}
		}

		return nearestRigidBody;
	}
}
