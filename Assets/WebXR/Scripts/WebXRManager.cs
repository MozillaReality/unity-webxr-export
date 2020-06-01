using UnityEngine;
using UnityEngine.XR;
using System.Runtime.InteropServices;

namespace WebXR
{
    public enum WebXRState
    {
        ENABLED,
        NORMAL
    }

    public class WebXRManager : MonoBehaviour
    {
        [Tooltip("Name of the key used to alternate between XR and normal mode. Leave blank to disable.")]
        public string toggleXRKeyName;

        [Tooltip("Preserve the manager across scenes changes.")]
        public bool dontDestroyOnLoad = true;

        [Header("Tracking")] [Tooltip("Default height of camera if no room-scale transform is present.")]
        public float DefaultHeight = 1.2f;

        [Tooltip("Represents the size of physical space available for XR.")]
        public TrackingSpaceType TrackingSpace = TrackingSpaceType.RoomScale;

        private static string GlobalName = "WebXRCameraSet";
        private static WebXRManager instance;
        [HideInInspector] public WebXRState xrState = WebXRState.NORMAL;

        public delegate void XRCapabilitiesUpdate(WebXRDisplayCapabilities capabilities);

        public event XRCapabilitiesUpdate OnXRCapabilitiesUpdate;

        public delegate void XRChange(WebXRState state);

        public event XRChange OnXRChange;

        public delegate void HeadsetUpdate(
            Matrix4x4 leftProjectionMatrix,
            Matrix4x4 leftViewMatrix,
            Matrix4x4 rightProjectionMatrix,
            Matrix4x4 rightViewMatrix,
            Matrix4x4 sitStandMatrix);

        public event HeadsetUpdate OnHeadsetUpdate;

        public delegate void ControllerUpdate(string id,
            int index,
            string hand,
            bool hasOrientation,
            bool hasPosition,
            Quaternion orientation,
            Vector3 position,
            Vector3 linearAcceleration,
            Vector3 linearVelocity,
            WebXRControllerButton[] buttons,
            float[] axes);

        public event ControllerUpdate OnControllerUpdate;

        // link WebGL plugin for interacting with browser scripts.
        [DllImport("__Internal")]
        private static extern void ConfigureToggleXRKeyName(string keyName);

        [DllImport("__Internal")]
        private static extern void XRInitSharedArray(float[] array, int length);

        [DllImport("__Internal")]
        private static extern void ListenWebXRData();

        // Shared array which we will load headset data in from webxr.jslib
        // Array stores  5 matrices, each 16 values, stored linearly.
        float[] sharedArray = new float[5 * 16];

        private WebXRDisplayCapabilities _capabilities;

        public static WebXRManager Instance
        {
            get
            {
                if (!instance)
                {
                    var managerInScene = FindObjectOfType<WebXRManager>();
                    var name = GlobalName;

                    if (managerInScene != null)
                    {
                        instance = managerInScene;
                        instance.name = name;
                    }
                    else
                    {
                        GameObject go = new GameObject(name);
                        go.AddComponent<WebXRManager>();
                    }
                }

                return instance;
            }
        }

        private void Awake()
        {
            Debug.Log("Active Graphics Tier: " + Graphics.activeTier);
            instance = this;

            if (!GlobalName.Equals(name))
            {
                Debug.LogError("The webxr.js script requires the WebXRManager gameobject to be named "
                               + GlobalName + " for proper functioning");
            }

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(instance);
            }
        }

        private void SetTrackingSpaceType()
        {
            if (XRDevice.isPresent)
            {
                XRDevice.SetTrackingSpaceType(TrackingSpace);
                Debug.Log("Tracking Space: " + XRDevice.GetTrackingSpaceType());
            }
        }

        // Handles WebXR data from browser
        public void OnWebXRData(string jsonString)
        {
            WebXRData webXRData = WebXRData.CreateFromJSON(jsonString);

            // Update controllers
            foreach (WebXRControllerData controllerData in webXRData.controllers)
            {
                if (OnControllerUpdate != null)
                    OnControllerUpdate(controllerData.id,
                        controllerData.index,
                        controllerData.hand,
                        controllerData.hasOrientation,
                        controllerData.hasPosition,
                        new Quaternion(controllerData.orientation[0], controllerData.orientation[1],
                            controllerData.orientation[2], controllerData.orientation[3]),
                        new Vector3(controllerData.position[0], controllerData.position[1],
                            controllerData.position[2]),
                        new Vector3(controllerData.linearAcceleration[0], controllerData.linearAcceleration[1],
                            controllerData.linearAcceleration[2]),
                        new Vector3(controllerData.linearVelocity[0], controllerData.linearVelocity[1],
                            controllerData.linearVelocity[2]),
                        controllerData.buttons,
                        controllerData.axes);
            }
        }

        // Handles WebXR capabilities from browser
        public void OnXRCapabilities(string json)
        {
            WebXRDisplayCapabilities capabilities = JsonUtility.FromJson<WebXRDisplayCapabilities>(json);
            OnXRCapabilities(capabilities);
        }

        public void OnXRCapabilities(WebXRDisplayCapabilities capabilities)
        {
#if UNITY_EDITOR
            // Nothing to do
#elif UNITY_WEBGL
            _capabilities = capabilities;
            if (!capabilities.supportsImmersiveVR)
                WebXRUI.displayXRElementId("novr");
#endif

            if (OnXRCapabilitiesUpdate != null)
                OnXRCapabilitiesUpdate(capabilities);
        }

        public void toggleXrState()
        {
#if UNITY_EDITOR
            // No editor specific functionality
#elif UNITY_WEBGL
            if (this.xrState == WebXRState.ENABLED)
                setXrState(WebXRState.NORMAL);
            else
                setXrState(WebXRState.ENABLED);
#endif
        }

        public void setXrState(WebXRState state)
        {
            xrState = state;

            if (OnXRChange != null)
                OnXRChange(state);
        }

        // received start VR from WebXR browser
        public void OnStartXR()
        {
            Instance.setXrState(WebXRState.ENABLED);
        }

        // receive end VR from WebVR browser
        public void OnEndXR()
        {
            Instance.setXrState(WebXRState.NORMAL);
        }

        float[] GetFromSharedArray(int index)
        {
            float[] newArray = new float[16];
            for (int i = 0; i < newArray.Length; i++)
            {
                newArray[i] = sharedArray[index * 16 + i];
            }

            return newArray;
        }

        void Start()
        {
#if UNITY_EDITOR
            // No editor specific functionality
#elif UNITY_WEBGL
            ConfigureToggleXRKeyName(toggleXRKeyName);
            XRInitSharedArray(sharedArray, sharedArray.Length);
            ListenWebXRData();
#endif

            SetTrackingSpaceType();
        }

        void Update()
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            if (string.IsNullOrEmpty(toggleXRKeyName))
                return;
            if (Input.GetKeyUp(toggleXRKeyName))
                toggleXrState();
#endif
        }

        void LateUpdate()
        {
            if (OnHeadsetUpdate == null || xrState != WebXRState.ENABLED) return;

            Matrix4x4 leftProjectionMatrix = WebXRMatrixUtil.NumbersToMatrix(GetFromSharedArray(0));
            Matrix4x4 rightProjectionMatrix = WebXRMatrixUtil.NumbersToMatrix(GetFromSharedArray(1));
            Matrix4x4 leftViewMatrix = WebXRMatrixUtil.NumbersToMatrix(GetFromSharedArray(2));
            Matrix4x4 rightViewMatrix = WebXRMatrixUtil.NumbersToMatrix(GetFromSharedArray(3));
            Matrix4x4 sitStandMatrix = WebXRMatrixUtil.NumbersToMatrix(GetFromSharedArray(4));
            // Matrix4x4 sitStandMatrix = Matrix4x4.Translate(new Vector3(0, DefaultHeight, 0));

            OnHeadsetUpdate(
                leftProjectionMatrix,
                rightProjectionMatrix,
                leftViewMatrix,
                rightViewMatrix,
                sitStandMatrix);
        }
    }
}
