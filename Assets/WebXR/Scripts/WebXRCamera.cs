using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace WebXR
{
    public class WebXRCamera : MonoBehaviour
    {
        [SerializeField] private Camera cameraMain, cameraL, cameraR;
        private bool xrActive;
        private WaitForEndOfFrame wait = new WaitForEndOfFrame();
        private Coroutine postRenderCoroutine;

        [DllImport("__Internal")]
        private static extern void XRPostRender();

        private IEnumerator endOfFrame()
        {
            // Wait until end of frame to report back to WebXR browser to submit frame.
            while (enabled)
            {
                yield return wait;
                XRPostRender();
            }
        }

        void OnEnable()
        {
            WebXRManager.Instance.OnXRChange += onVRChange;
            WebXRManager.Instance.OnHeadsetUpdate += onHeadsetUpdate;

            cameraMain.transform.localPosition = new Vector3(0, WebXRManager.Instance.DefaultHeight, 0);

#if UNITY_EDITOR
			// No editor specific funtionality
#elif UNITY_WEBGL
			postRenderCoroutine = StartCoroutine(endOfFrame());
#endif
        }

        private void OnDisable()
        {
			if (postRenderCoroutine != null)
			{
				StopCoroutine(postRenderCoroutine);
			}
		}

        private void onVRChange(WebXRState state)
        {
            xrActive = state == WebXRState.ENABLED;

            if (xrActive)
            {
                cameraMain.enabled = false;
                cameraL.enabled = true;
                cameraR.enabled = true;
            }
            else
            {
                cameraMain.enabled = true;
                cameraL.enabled = false;
                cameraR.enabled = false;
            }
        }

        private void onHeadsetUpdate(
            Matrix4x4 leftProjectionMatrix,
            Matrix4x4 rightProjectionMatrix,
            Matrix4x4 leftViewMatrix,
            Matrix4x4 rightViewMatrix,
            Matrix4x4 sitStandMatrix)
        {
            if (xrActive)
            {
                WebXRMatrixUtil.SetTransformFromViewMatrix(cameraL.transform, leftViewMatrix * sitStandMatrix.inverse);
                cameraL.projectionMatrix = leftProjectionMatrix;
                WebXRMatrixUtil.SetTransformFromViewMatrix(cameraR.transform, rightViewMatrix * sitStandMatrix.inverse);
                cameraR.projectionMatrix = rightProjectionMatrix;
            }
        }
    }
}