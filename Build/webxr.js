(function () {
  'use strict';

  function XRData() {
    this.leftProjectionMatrix = mat4.create();
    this.rightProjectionMatrix = mat4.create();
    this.leftViewMatrix = mat4.create();
    this.rightViewMatrix = mat4.create();
    this.sitStandMatrix = mat4.create();
    this.gamepads = [];
  }

  function XRManager() {
    this.enterXRButton = document.getElementById('enterxr');
    this.gameContainer = document.getElementById('game');
    this.perfStatus = document.getElementById('performance');
    // Unity GameObject name which we will SendMessage to
    this.unityObjectName = 'WebXRCameraSet';

    this.session = null;
    this.refSpace = null;
    this.isVRSupported = false;
    this.isARSupported = false;
    this.isInlineSupported = false;
    this.xrData = new XRData();
    this.canvas = null;
    this.ctx = null;
    this.gameInstance = null;
    this.polyfill = null;
    this.toggleVRKeyName = '';
    this.wasPresenting = false;
    this.init();
  }

  XRManager.prototype.init = function () {
    if (window.WebXRPolyfill) {
      this.polyfill = new WebXRPolyfill();
    }

    this.attachEventListeners();

    navigator.xr.isSessionSupported('inline').then((supported) => {
      // Spec states this mode should always be supported
      this.isInlineSupported = supported;
    });

    navigator.xr.isSessionSupported('immersive-vr').then((supported) => {
      this.isVRSupported = supported;
      this.enterXRButton.dataset.enabled = supported;
    });

    navigator.xr.isSessionSupported('immersive-ar').then((supported) => {
      this.isARSupported = supported;
    });
  }

  XRManager.prototype.attachEventListeners = function () {
    var onResize = this.resize.bind(this);
    var onToggleVR = this.toggleVR.bind(this);
    var onKeyUp = this.keyUp.bind(this);
    var onUnityLoaded = this.unityLoaded.bind(this);
    var onUnityMessage = this.unityMessage.bind(this);

    window.addEventListener('resize', onResize, true);
    window.addEventListener('keyup', onKeyUp, false);

    // dispatched by index.html
    document.addEventListener('UnityLoaded', onUnityLoaded, false);
    document.addEventListener('Unity', onUnityMessage, false);

    this.enterXRButton.addEventListener('click', onToggleVR, false);
  }

  XRManager.prototype.resize = function () {
    if (!this.canvas) return;

    this.canvas.width = window.innerWidth;
    this.canvas.height = window.innerHeight;
    this.gameContainer.style.transform = '';
  }

  XRManager.prototype.requestPresent = function () {
    if (!this.isVRSupported) return;

    navigator.xr.requestSession('immersive-vr', {requiredFeatures: ['local-floor']}).then((session) => {
      this.session = session;

      session.addEventListener('end', this.handleEndSession.bind(this));

      this.ctx.makeXRCompatible();

      session.updateRenderState({ baseLayer: new XRWebGLLayer(session, this.ctx) });

      session.requestReferenceSpace('local-floor').then((refSpace) => {
        this.refSpace = refSpace;
      });
      console.log('Entered VR');
    }).catch(function (err) {
      console.error('Unable to enter VR mode: ', err);
    });
  }

  XRManager.prototype.handleEndSession = function() {
    console.log('Exited VR');
    this.gameInstance.SendMessage(this.unityObjectName, 'OnEndXR');
    this.wasPresenting = false;
    this.session = null;
  }

  XRManager.prototype.endSession = function () {
    if (!this.session ) {
      console.warn('No XR session to end');
      return;
    }

    return this.session.end().then(function () {
      this.handleEndSession();
    }).catch(function (err) {
      console.error('Unable to exit XR mode:', err);
    });
  }

  XRManager.prototype.toggleVR = function () {
    if (this.session && this.gameInstance) {
      this.endSession();
    } else {
      this.requestPresent();
    }
  }

  XRManager.prototype.keyUp = function (evt) {
    if (this.toggleVRKeyName && this.toggleVRKeyName === evt.key) {
      this.toggleVR();
    }

    // performance hud
    if (evt.key == 'p') {
      this.perfStatus.dataset.enabled = this.perfStatus.dataset.enabled === 'true' ? false : true;
    }
  }

  XRManager.prototype.setGameInstance = function (gameInstance) {
    if (!this.gameInstance) {
      this.gameInstance = gameInstance;
      this.canvas = this.gameInstance.Module.canvas;
      this.ctx = this.gameInstance.Module.ctx;
    }
  }

  XRManager.prototype.unityProgressStart = new Promise(function (resolve) {
    // dispatched by index.html
    document.addEventListener('UnityProgressStart', function (evt) {
      resolve(window.gameInstance);
    }, false);
  });

  XRManager.prototype.unityLoaded = async function () {
    document.body.dataset.unityLoaded = 'true';

    this.setGameInstance(await this.unityProgressStart);
    this.resize();

    // Received by WebXRManager.cs
    this.gameInstance.SendMessage(
      this.unityObjectName, 'OnXRCapabilities',
      // Structure should match WebXRDisplayCapabilities.cs
      JSON.stringify({
        supportsInline: this.isInlineSupported,
        supportsImmersiveVR: this.isVRSupported,
        supportsImmersiveAR: this.isARSupported
      })
    );
  }

  XRManager.prototype.getGamepadAxes = function(gamepad) {
    var axes = [];
    for (var i = 0; i < gamepad.axes.length; i++) {
      axes.push(gamepad.axes[i]);
    }
    return axes;
  }

  XRManager.prototype.getGamepadButtons = function(gamepad) {
    var buttons = [];
    for (var i = 0; i < gamepad.buttons.length; i++) {
      buttons.push({
        pressed: gamepad.buttons[i].pressed,
        touched: gamepad.buttons[i].touched,
        value: gamepad.buttons[i].value
      });
    }
    return buttons;
  }

  XRManager.prototype.getGamepads = function(frame) {
    var gamepads = [];
    for (let source of frame.session.inputSources) {
      if (source.gripSpace && source.gamepad) {
        let sourcePose = frame.getPose(source.gripSpace, this.refSpace);
        
        var position = sourcePose.transform.position;
        var orientation = sourcePose.transform.orientation;

        // Structure of this corresponds with WebXRControllerData.cs
        gamepads.push({
          id: source.gamepad.id,
          index: source.gamepad.index,
          hand: source.handedness,
          buttons: this.getGamepadButtons(source.gamepad),
          axes: this.getGamepadAxes(source.gamepad),
          hasOrientation: true,
          hasPosition: true,
          orientation: this.GLQuaternionToUnity([orientation.x, orientation.y, orientation.z, orientation.w]),
          position: this.GLVec3ToUnity([position.x, position.y, position.z]),
          linearAcceleration: [0, 0, 0],
          linearVelocity: [0, 0, 0]
        });
      }
     }
    return gamepads;
  }

  XRManager.prototype.updateFramerate = function () {
    if (this.perfStatus.dataset.enabled === 'false') {
      return;
    }

    var now = performance.now();

    if (this.frameTimes == undefined) {
      this.frameTimes = [];
      this.fps;
    }

    while (this.frameTimes.length > 0 && this.frameTimes[0] <= now - 1000) {
      this.frameTimes.shift();
    }

    this.frameTimes.push(now);
    this.fps = this.frameTimes.length;
    this.perfStatus.innerHTML = this.fps;
  }

  // Convert WebGL to Unity compatible Vector3
  XRManager.prototype.GLVec3ToUnity = function(v) {
    v[2] *= -1;
    return v;
  }

  // Convert WebGL to Unity compatible Quaternion
  XRManager.prototype.GLQuaternionToUnity = function(q) {
    q[0] *= -1;
    q[1] *= -1;
    return q;
  }

  // Convert WebGL to Unity Projection Matrix4
  XRManager.prototype.GLProjectionToUnity = function(m) {
    var out = mat4.create();
    mat4.copy(out, m)
    mat4.transpose(out, out);
    return out;
  }

  // Convert WebGL to Unity View Matrix4
  XRManager.prototype.GLViewToUnity = function(m) {
    var out = mat4.create();
    mat4.copy(out, m);
    mat4.transpose(out, out);
    out[2] *= -1;
    out[6] *= -1;
    out[10] *= -1;
    out[14] *= -1;
    return out;
  }

  XRManager.prototype.animate = function (frame) {
    let pose = frame.getViewerPose(this.refSpace);
    if (!pose) {
      return;
    }

    if (this.session && !this.wasPresenting) {
      this.gameInstance.SendMessage(this.unityObjectName, 'OnStartXR');
      this.wasPresenting = true;
      this.resize();
    }

    var xrData = this.xrData;

    for (let view of pose.views) {
      if (view.eye === 'left') {
        xrData.leftProjectionMatrix = this.GLProjectionToUnity(view.projectionMatrix);
        xrData.leftViewMatrix = this.GLViewToUnity(view.transform.inverse.matrix);
      } else if (view.eye === 'right') {
        xrData.rightProjectionMatrix = this.GLProjectionToUnity(view.projectionMatrix);
        xrData.rightViewMatrix = this.GLViewToUnity(view.transform.inverse.matrix);
      } else {
        xrData.sitStandMatrix = this.GLViewToUnity(view.transform.inverse.matrix);
      }
    }

    // Gamepads
    xrData.gamepads = this.getGamepads(frame);

    // Dispatch event with headset data to be handled in webvr.jslib
    document.dispatchEvent(new CustomEvent('VRData', { detail: {
      leftProjectionMatrix: xrData.leftProjectionMatrix,
      rightProjectionMatrix: xrData.rightProjectionMatrix,
      leftViewMatrix: xrData.leftViewMatrix,
      rightViewMatrix: xrData.rightViewMatrix,
      sitStandMatrix: xrData.sitStandMatrix
    }}));

    gameInstance.SendMessage('WebXRCameraSet', 'OnWebXRData', JSON.stringify({
      controllers: xrData.gamepads
    }));

    this.xrDisplay.submitFrame();
  }

  XRManager.prototype.unityMessage = function (msg) {
      var animate = this.animate.bind(this);

      if (typeof msg.detail === 'string') {
        // Wait for Unity to render the frame; then submit the frame to the VR display.
        if (msg.detail === 'PostRender') {
          if (this.session) {
            this.session.requestAnimationFrame((t, f) => animate(f));
          }
          this.updateFramerate();
        }

        // Assign VR toggle key from Unity on WebXRManager component.
        if (msg.detail.indexOf('ConfigureToggleVRKeyName') === 0) {
          this.toggleVRKeyName = msg.detail.split(':')[1];
        }
      }

      // Handle UI dialogue
      if (msg.detail.type === 'displayElementId') {
        var el = document.getElementById(msg.detail.id);
        this.displayElement(el);
      }
  }

  // Show instruction dialogue for non-VR enabled browsers.
  XRManager.prototype.displayElement = function (el) {
    if (el.dataset.enabled) {
      return;
    }
    var confirmButton = el.querySelector('button');
    el.dataset.enabled = true;

    function onConfirm () {
      el.dataset.enabled = false;
      confirmButton.removeEventListener('click', onConfirm);
    }
    confirmButton.addEventListener('click', onConfirm);
  }

  function initWebXRManager () {
    var xrManager = window.xrManager = new XRManager();
    return xrManager;
  }

  function init() {
    // Detect existing xr
    if ("xr" in navigator) {
      initWebXRManager();
    } else {
      // If not detected, add polyfill

      var script = document.createElement('script');
      script.src = 'vendor/webxr-polyfill.min.js';
      document.getElementsByTagName('head')[0].appendChild(script);

      script.addEventListener('load', function () {
        initWebXRManager();
      });

      script.addEventListener('error', function (err) {
        console.warn('Could not load the WebXR Polyfill script:', err);
      });
    }

  }

  init();
})();
