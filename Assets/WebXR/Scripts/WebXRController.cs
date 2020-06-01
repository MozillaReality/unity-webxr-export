using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;

namespace WebXR
{
    public enum WebXRControllerHand
    {
        NONE,
        LEFT,
        RIGHT
    };

    [Serializable]
    public class WebXRControllerButton
    {
        public bool pressed;
        public bool prevPressedState;
        public bool touched;
        public float value;

        public WebXRControllerButton(bool isPressed, float buttonValue)
        {
            pressed = isPressed;
            prevPressedState = false;
            value = buttonValue;
        }
    }

    public class WebXRController : MonoBehaviour
    {
        [Tooltip("Controller hand to use.")] public WebXRControllerHand hand = WebXRControllerHand.NONE;

        [Tooltip("Controller input settings.")]
        public WebXRControllerInputMap inputMap;

        [Tooltip("Simulate 3dof controller")] public bool simulate3dof = false;
        [Tooltip("Vector from head to elbow")] public Vector3 eyesToElbow = new Vector3(0.1f, -0.4f, 0.15f);
        [Tooltip("Vector from elbow to hand")] public Vector3 elbowHand = new Vector3(0, 0, 0.25f);
        private Matrix4x4 sitStand;
        private float[] axes;

        private XRNode handNode;
        private Quaternion headRotation;
        private Vector3 headPosition;
        private Transform _t;

        private Dictionary<string, WebXRControllerButton>
            buttonStates = new Dictionary<string, WebXRControllerButton>();

        // Updates button states from Web gamepad API.
        private void UpdateButtons(WebXRControllerButton[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                WebXRControllerButton button = buttons[i];
                foreach (var input in inputMap.inputs)
                {
                    if (input.gamepadId == i)
                        SetButtonState(input.actionName, button.pressed, button.value);
                }
            }
        }

        public float GetAxis(string action)
        {
            for (var i = 0; i < inputMap.inputs.Count; i++)
            {
                WebXRControllerInput input = inputMap.inputs[i];
                if (action == input.actionName)
                {
                    if (XRDevice.isPresent && !input.unityInputIsButton)
                    {
                        return Input.GetAxis(input.unityInputName);
                    }

                    if (!input.gamepadIsButton) return axes[i];

                    if (!buttonStates.ContainsKey(action))
                        return 0;
                    return buttonStates[action].value;

                }
            }

            return 0;
        }

        public bool GetButton(string action)
        {
            if (XRDevice.isPresent)
            {
                foreach (WebXRControllerInput input in inputMap.inputs)
                {
                    if (action == input.actionName && input.unityInputIsButton)
                        return Input.GetButton(input.unityInputName);
                }
            }

            return buttonStates.ContainsKey(action) && buttonStates[action].pressed;
        }

        private bool GetPastButtonState(string action)
        {
            return buttonStates.ContainsKey(action) && buttonStates[action].prevPressedState;
        }

        private void SetButtonState(string action, bool isPressed, float value)
        {
            if (buttonStates.ContainsKey(action))
            {
                buttonStates[action].pressed = isPressed;
                buttonStates[action].value = value;
            }
            else
                buttonStates.Add(action, new WebXRControllerButton(isPressed, value));
        }

        private void SetPastButtonState(string action, bool isPressed)
        {
            if (!buttonStates.ContainsKey(action))
                return;
            buttonStates[action].prevPressedState = isPressed;
        }

        public bool GetButtonDown(string action)
        {
            // Use Unity Input Manager when XR is enabled and WebXR is not being used (eg: standalone or from within editor).
            if (XRDevice.isPresent)
            {
                foreach (WebXRControllerInput input in inputMap.inputs)
                {
                    if (action == input.actionName && input.unityInputIsButton)
                        return Input.GetButtonDown(input.unityInputName);
                }
            }

            if (GetButton(action) && !GetPastButtonState(action))
            {
                SetPastButtonState(action, true);
                return true;
            }

            return false;
        }

        public bool GetButtonUp(string action)
        {
            // Use Unity Input Manager when XR is enabled and WebXR is not being used (eg: standalone or from within editor).
            if (XRDevice.isPresent)
            {
                foreach (WebXRControllerInput input in inputMap.inputs)
                {
                    if (action == input.actionName && input.unityInputIsButton)
                        return Input.GetButtonUp(input.unityInputName);
                }
            }

            if (!GetButton(action) && GetPastButtonState(action))
            {
                SetPastButtonState(action, false);
                return true;
            }

            return false;
        }

        private void onHeadsetUpdate(Matrix4x4 leftProjectionMatrix,
            Matrix4x4 rightProjectionMatrix,
            Matrix4x4 leftViewMatrix,
            Matrix4x4 rightViewMatrix,
            Matrix4x4 sitStandMatrix)
        {
            Matrix4x4 trs = WebXRMatrixUtil.TransformViewMatrixToTRS(leftViewMatrix);
            this.headRotation = WebXRMatrixUtil.GetRotationFromMatrix(trs);
            this.headPosition = WebXRMatrixUtil.GetTranslationFromMatrix(trs);
            this.sitStand = sitStandMatrix;
        }

        private void onControllerUpdate(string id,
            int index,
            string handValue,
            bool hasOrientation,
            bool hasPosition,
            Quaternion orientation,
            Vector3 position,
            Vector3 linearAcceleration,
            Vector3 linearVelocity,
            WebXRControllerButton[] buttonValues,
            float[] axesValues)
        {
            if (handFromString(handValue) != hand) return;
            
            SetVisible(true);

            Quaternion sitStandRotation = WebXRMatrixUtil.GetRotationFromMatrix(sitStand);
            Quaternion rotation = sitStandRotation * orientation;

            if (!hasPosition || simulate3dof)
            {
                position = applyArmModel(
                    sitStand.MultiplyPoint(headPosition),
                    rotation,
                    headRotation);
            }
            else
            {
                position = sitStand.MultiplyPoint(position);
            }

            _t.SetPositionAndRotation(position, rotation);

            UpdateButtons(buttonValues);
            axes = axesValues;
        }

        private WebXRControllerHand handFromString(string handValue)
        {
            if (String.IsNullOrEmpty(handValue)) return WebXRControllerHand.NONE;

            try
            {
                return (WebXRControllerHand) Enum.Parse(typeof(WebXRControllerHand), handValue.ToUpper(), true);
            }
            catch
            {
                Debug.LogError("Unrecognized controller Hand '" + handValue + "'!");
            }

            return WebXRControllerHand.NONE;
        }

        private void SetVisible(bool visible)
        {
            Renderer[] rendererComponents = GetComponentsInChildren<Renderer>();
            foreach (Renderer component in rendererComponents)
            {
                component.enabled = visible;
            }
        }

        // Arm model adapted from: https://github.com/aframevr/aframe/blob/master/src/components/tracked-controls.js
        private Vector3 applyArmModel(Vector3 controllerPosition, Quaternion controllerRotation,
            Quaternion headRotation)
        {
            // Set offset for degenerate "arm model" to elbow.
            Vector3 deltaControllerPosition = new Vector3(
                this.eyesToElbow.x * (this.hand == WebXRControllerHand.LEFT ? -1 :
                    this.hand == WebXRControllerHand.RIGHT ? 1 : 0),
                this.eyesToElbow.y,
                this.eyesToElbow.z);

            // Apply camera Y rotation (not X or Z, so you can look down at your hand).
            Quaternion headYRotation = Quaternion.Euler(0, headRotation.eulerAngles.y, 0);
            deltaControllerPosition = (headYRotation * deltaControllerPosition);
            controllerPosition += deltaControllerPosition;

            // Set offset for forearm sticking out from elbow.
            deltaControllerPosition.Set(this.elbowHand.x, this.elbowHand.y, this.elbowHand.z);
            deltaControllerPosition =
                Quaternion.Euler(controllerRotation.eulerAngles.x, controllerRotation.eulerAngles.y, 0) *
                deltaControllerPosition;
            controllerPosition += deltaControllerPosition;
            return controllerPosition;
        }

        void Update()
        {
            // Use Unity XR Input when enabled. When using WebXR, updates are performed onControllerUpdate.
            if (!XRDevice.isPresent) return;
            
            SetVisible(true);

            if (this.hand == WebXRControllerHand.LEFT)
                handNode = XRNode.LeftHand;

            if (this.hand == WebXRControllerHand.RIGHT)
                handNode = XRNode.RightHand;

            if (this.simulate3dof)
            {
                _t.SetPositionAndRotation(applyArmModel(
                    InputTracking.GetLocalPosition(XRNode.Head), // we use head position as origin
                    InputTracking.GetLocalRotation(handNode),
                    InputTracking.GetLocalRotation(XRNode.Head)
                ), InputTracking.GetLocalRotation(handNode));
            }
            else
            {
                _t.SetPositionAndRotation(InputTracking.GetLocalPosition(handNode), InputTracking.GetLocalRotation(handNode));
            }

            foreach (WebXRControllerInput input in inputMap.inputs)
            {
                if (!input.unityInputIsButton)
                {
                    if (Input.GetAxis(input.unityInputName) != 0)
                        SetButtonState(input.actionName, true, Input.GetAxis(input.unityInputName));
                    if (Input.GetAxis(input.unityInputName) < 1)
                        SetButtonState(input.actionName, false, Input.GetAxis(input.unityInputName));
                }
            }
        }

        private void Awake()
        {
            _t = transform;
        }

        void OnEnable()
        {
            if (inputMap == null)
            {
                Debug.LogError("A Input Map must be assigned to WebXRController!");
                return;
            }

            WebXRManager.Instance.OnControllerUpdate += onControllerUpdate;
            WebXRManager.Instance.OnHeadsetUpdate += onHeadsetUpdate;
            SetVisible(false);
        }

        void OnDisable()
        {
            WebXRManager.Instance.OnControllerUpdate -= onControllerUpdate;
            WebXRManager.Instance.OnHeadsetUpdate -= onHeadsetUpdate;
            SetVisible(false);
        }
    }
}
