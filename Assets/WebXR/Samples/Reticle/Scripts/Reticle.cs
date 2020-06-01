using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Reticle : MonoBehaviour
{
    public GameObject reticleCentre;
    public Camera cameraFacing;
    private Vector3 originalScale;
    public GameObject thePlayer;

    // Start is called before the first frame update
    void Start()
    {
        // Setup the variables automatically to make life easier. Ensure you placed the reticle as a child of a camera object.
        originalScale = transform.localScale;
        reticleCentre = this.transform.GetChild(0).gameObject;
        cameraFacing = this.transform.parent.gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
            // Cast a ray from the camera's position to wear it is facing
            RaycastHit hit;
            float distance;
            if (Physics.Raycast(new Ray(cameraFacing.transform.position, cameraFacing.transform.rotation * Vector3.forward), out hit))
            {
                // Get the distance of the thing we hit
                distance = hit.distance;
            }
            else
            {
                // If we don't hit anything, set the distance to the far clip plane
                distance = cameraFacing.farClipPlane * 0.95f;
            }

            // Position the reticle so its distance is based on what we hit. Set the reticle to always face the camera with LookAt, and then keep it rotated properly.
            transform.position = cameraFacing.transform.position + (cameraFacing.transform.rotation * Vector3.forward * distance);
            transform.LookAt(cameraFacing.transform.position);
            transform.Rotate(0, 180, 0);

            // Keep the reticle scaled appropriatly, even when we are close to something
            if (distance < 10)
            {
                distance *= 1 + 5 * Mathf.Exp(-distance);
            }
            transform.localScale = originalScale * distance;

        // UNCOMMENT BELOW FOR RAYCAST DEBUGGING PURPOSES
        // Debug.DrawRay(cameraFacing.transform.position, cameraFacing.transform.rotation * Vector3.forward * 10, Color.green);


        // INTERACTION
        // Input.GetMouseButtonDown(0) checks if the left mouse button was pressed down. Is also used for the Google Cardboard switch.
        if (Input.GetMouseButtonDown(0))
        {
            // Trigger the pulse animation of the centre part of the reticle
            reticleCentre.GetComponent<Animator>().SetTrigger("pulse");

            // Cast a ray from the camera's position to wear it is facing
            if (Physics.Raycast(cameraFacing.transform.position, cameraFacing.transform.rotation * Vector3.forward, out hit, Mathf.Infinity))
            {
                Debug.Log("Clicked on " + hit.transform.name);

                if(hit.transform.name == "teleObj")
                {
                    
                }

                // Simulate a mouse click on whatever the ray hits. This enables us to use the Event Trigger component in the Unity GUI to handle what happens when a particular object is clicked.
                ExecuteEvents.Execute<IPointerClickHandler>(hit.transform.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            }
            else
            {
                Debug.Log("Didn't click on anything!");
            }
        }
    }
}
