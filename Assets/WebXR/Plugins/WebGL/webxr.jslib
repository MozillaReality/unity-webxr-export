/* functions called from unity */
mergeInto(LibraryManager.library, {
  // Declared in WebXRCamera.cs
  XRPostRender: function () {
    document.dispatchEvent(new CustomEvent('Unity', {detail: 'PostRender'}));
  },

  // Declared in WebXRManager.cs
  ConfigureToggleXRKeyName: function (keyName) {
    document.dispatchEvent(new CustomEvent('Unity', {detail: 'ConfigureToggleVRKeyName:' + Pointer_stringify(keyName)}));
  },

  // Declared in WebXRUI.cs
  displayXRElementId: function (id) {
    document.dispatchEvent(new CustomEvent('Unity', {detail: {type: 'displayElementId', id: Pointer_stringify(id)}}));
  },

  // Declared in WebXRManager.cs
  XRInitSharedArray: function(byteOffset, length) {
    SharedArray = new Float32Array(buffer, byteOffset, length);
  },

  // Declared in WebXRManager.cs
  ListenWebXRData: function() {
    // Listen for headset updates from webxr.js and load data into shared Array which we pick up in Unity.
    document.addEventListener('VRData', function(evt) {
      var data = evt.detail;

      Object.keys(data).forEach(function (key, i) {
        var dataLength = data[key].length;
        for (var x = 0; x < dataLength; x++) {
          SharedArray[i * dataLength + x] = data[key][x];
        }
      });
    });
  }
});
