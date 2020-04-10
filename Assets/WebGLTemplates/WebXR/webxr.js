(function () {
  'use strict';

  function XRData() {
    this.leftProjectionMatrix = mat4.create();
    this.rightProjectionMatrix = mat4.create();
    this.leftViewMatrix = mat4.create();
    this.rightViewMatrix = mat4.create();
    this.sitStandMatrix = mat4.create();
    this.gamepads = [];
    this.xrData = null;
  }

  function XRManager() {
    this.enterXRButton = document.getElementById('enterxr');
    this.gameContainer = document.getElementById('game');
    this.perfStatus = document.getElementById('performance');

    // Unity GameObject name which we will SendMessage to
    this.unityObjectName = 'WebXRCameraSet';

    this.vrSession = null;
    this.isInVRSession = false;
    this.inlineSession = null;
    this.xrData = new XRData();
    this.canvas = null;
    this.ctx = null;
    this.gameInstance = null;
    this.polyfill = null;
    this.toggleVRKeyName = '';
    this.notifiedStartToUnity = false;
    this.isVRSupported = false;
    this.refSpace = null;
    this.vrImmersiveRefSpace = null;
    this.xrInlineRefSpace = null;
    this.rAFCB = null;
    this.originalWidth = null;
    this.originalHeight = null;
    this.init();
  }

  XRManager.prototype.init = async function () {
    if (window.WebXRPolyfill) {
      this.polyfill = new WebXRPolyfill();
    }

    this.attachEventListeners();

    navigator.xr.isSessionSupported('immersive-vr').then((supported) => {
      this.isVRSupported = supported;
      this.enterXRButton.dataset.enabled = supported;
    });
  }

  XRManager.prototype.resize = function () {
    if (!this.canvas) return;

    this.canvas.width = window.innerWidth;
    this.canvas.height = window.innerHeight;
    this.gameContainer.style.transform = '';
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

  XRManager.prototype.requestPresent = function () {
    if (!this.isVRSupported) return;
    navigator.xr.requestSession('immersive-vr', {
      requiredFeatures: ['local-floor']
    }).then(async (session) => {
      session.isImmersive = true;
      this.isInVRSession = true;
      this.vrSession = session;
      this.onSessionStarted(session);
    });
  }

  XRManager.prototype.exitSession = function () {
    if (!this.vrSession && !this.isInVRSession) {
      console.warn('No VR display to exit VR mode');
      return;
    }

    this.vrSession.end();
  }

  XRManager.prototype.onEndSession = function (session) {
    if (session && session.end) {
      session.end();
    }
    
    this.gameInstance.SendMessage(this.unityObjectName, 'OnEndXR');
    this.isInVRSession = false;
    this.notifiedStartToUnity = false;
    this.canvas.width = this.originalWidth;
    this.canvas.height = this.originalHeight;
  }

  XRManager.prototype.toggleVR = function () {
    if (this.isVRSupported && this.isInVRSession && this.gameInstance) {
      this.exitSession();
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
      this.resize();
      
      this.ctx = this.gameInstance.Module.ctx;
      var thisXRMananger = this;
      this.gameInstance.Module.InternalBrowser.requestAnimationFrame = function (func) {
        if (!thisXRMananger.rAFCB) {
          thisXRMananger.rAFCB = func;
        }
        if (thisXRMananger.vrSession && thisXRMananger.isInVRSession) {
          return thisXRMananger.vrSession.requestAnimationFrame((time, xrFrame) =>
          {
            thisXRMananger.animate(time, xrFrame);
            if (func) {
              func(time);
            }
          });
        } else if (thisXRMananger.inlineSession) {
          return thisXRMananger.inlineSession.requestAnimationFrame((time, xrFrame) =>
          {
            if (func) {
              func(time);
            }
          });
        } else {
          window.requestAnimationFrame(func);
        }
      };
    }
  }

  XRManager.prototype.unityProgressStart = new Promise(function (resolve) {
    // dispatched by index.html
    document.addEventListener('UnityProgressStart', function (evt) {
      resolve(window.gameInstance);
    }, false);
  });

  XRManager.prototype.unityLoaded = function () {
    document.body.dataset.unityLoaded = 'true';

    // Send browser capabilities to Unity.
    var canPresent = this.isVRSupported;
    var hasPosition = true;
    var hasExternalDisplay = false;

    this.setGameInstance(gameInstance);
    
    this.enterXRButton.disabled = !this.isVRSupported;

    this.gameInstance.SendMessage(
      this.unityObjectName, 'OnXRCapabilities',
      JSON.stringify({
        canPresent: canPresent,
        hasPosition: hasPosition,
        hasExternalDisplay: hasExternalDisplay,
        supportsImmersiveVR: this.isVRSupported,
      })
    );
    
    navigator.xr.requestSession('inline').then((session) => {
      this.inlineSession = session;
      this.onSessionStarted(session);
    });
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

  XRManager.prototype.getGamepads = function(frame, refSpace) {
    var gamepads = [];
    for (let source of frame.session.inputSources) {
      if (source.gripSpace && source.gamepad) {
        let sourcePose = frame.getPose(source.gripSpace, refSpace);

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

  XRManager.prototype.onSessionStarted = function (session) {
    var onSessionEnded = this.onEndSession.bind(this);
    session.addEventListener('end', onSessionEnded);

    var glLayer = new XRWebGLLayer( session, this.ctx);

    session.updateRenderState({ baseLayer: glLayer });

    let refSpaceType = 'viewer';
    if (session.isImmersive){
      refSpaceType = 'local-floor';
      this.originalWidth = this.canvas.width;
      this.originalHeight = this.canvas.height;
      this.canvas.width = glLayer.framebufferWidth;
      this.canvas.height = glLayer.framebufferHeight;
    }

    session.requestReferenceSpace(refSpaceType).then((refSpace) => {
      if (session.isImmersive) {
        this.vrImmersiveRefSpace = refSpace;
        // Inform the session that we're ready to begin drawing.
        this.gameInstance.Module.InternalBrowser.requestAnimationFrame(this.rAFCB);
      } else {
        this.xrInlineRefSpace = refSpace;
      }
    });
  }

  XRManager.prototype.animate = function (t, frame) {
    let session = frame.session;

    if (!session && !session.isImmersive)
    {
      return;
    }
    
    let refSpace = this.vrImmersiveRefSpace;

    let pose = frame.getViewerPose(refSpace);
    if (!pose) {
      return;
    }

    let glLayer = session.renderState.baseLayer;
    this.canvas.width = glLayer.framebufferWidth;
    this.canvas.height = glLayer.framebufferHeight;

    this.ctx.bindFramebuffer(this.ctx.FRAMEBUFFER, glLayer.framebuffer);
    this.ctx.clear(this.ctx.COLOR_BUFFER_BIT | this.ctx.DEPTH_BUFFER_BIT);
    
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
    xrData.gamepads = this.getGamepads(frame, refSpace);

    // Dispatch event with headset data to be handled in webxr.jslib
    document.dispatchEvent(new CustomEvent('VRData', { detail: {
      leftProjectionMatrix: xrData.leftProjectionMatrix,
      rightProjectionMatrix: xrData.rightProjectionMatrix,
      leftViewMatrix: xrData.leftViewMatrix,
      rightViewMatrix: xrData.rightViewMatrix,
      sitStandMatrix: xrData.sitStandMatrix
    }}));

    if (!this.notifiedStartToUnity)
    {
      this.gameInstance.SendMessage(this.unityObjectName, 'OnStartXR');
      this.notifiedStartToUnity = true;
    }

    this.gameInstance.SendMessage(this.unityObjectName, 'OnWebXRData', JSON.stringify({
      controllers: xrData.gamepads
    }));

    this.updateFramerate();
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

  XRManager.prototype.unityMessage = function (msg) {

      if (typeof msg.detail === 'string') {
        // Wait for Unity to render the frame; then submit the frame to the VR display.
        if (msg.detail === 'PostRender') {
          // TODO: remove calls for PostRender
        }

        // Assign VR toggle key from Unity on WebXRManager component.
        if (msg.detail.indexOf('ConfigureToggleVRKeyName') === 0) {
          this.toggleVRKeyName = msg.detail.split(':')[1];
        }
      }

      // Handle UI dialogue
      if (msg.detail.type === 'displayElementId') {
        var el = document.getElementById(msg.detail.id);
        if (el) {
          this.displayElement(el);
        }
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
