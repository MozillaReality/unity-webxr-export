using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(FixedJoint))]
[RequireComponent(typeof(WebXRController))]
public class DesertControllerInteraction : MonoBehaviour
{
    private FixedJoint attachJoint = null;
    private Rigidbody currentRigidBody = null;
    private List<Rigidbody> contactRigidBodies = new List<Rigidbody> ();
    private WebXRController controller;
    private Transform t;

    private Animator anim;

    void Awake()
    {
        t = transform;
        attachJoint = GetComponent<FixedJoint> ();
        anim = GetComponent<Animator>();
        controller = GetComponent<WebXRController>();
    }

    void Update()
    {
        float normalizedTime = controller.GetButton("Trigger") ? 1 : controller.GetAxis("Grip");

        if (controller.GetButtonDown("Trigger") || controller.GetButtonDown("Grip"))
            Pickup();

        if (controller.GetButtonUp("Trigger") || controller.GetButtonUp("Grip"))
            Drop();

        // Use the controller button or axis position to manipulate the playback time for hand model.
        anim.Play("Take", -1, normalizedTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Interactable"))
            return;

        contactRigidBodies.Add(other.attachedRigidbody);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Interactable"))
            return;

        contactRigidBodies.Remove(other.attachedRigidbody);
    }

    public void Pickup() {
        currentRigidBody = GetNearestRigidBody ();

        if (!currentRigidBody)
            return;

        currentRigidBody.MovePosition(t.position);
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
            distance = (contactBody.transform.position - t.position).sqrMagnitude;

            if (distance < minDistance) {
                minDistance = distance;
                nearestRigidBody = contactBody;
            }
        }

        return nearestRigidBody;
    }
}
