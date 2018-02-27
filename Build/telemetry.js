/* global localStorage, location, Raven */
(function () {
// This checks for "production"-looking origins (e.g., `https://example.com`).
if (window.isSecureContext === false ||
    (location.hostname === 'localhost' ||
     location.hostname === '127.0.0.1' ||
     location.hostname === '0.0.0.0' ||
     location.hostname.indexOf('ngrok.io') > -1 ||
     location.hostname.indexOf('localtunnel.me') > -1)) {
  return;
}

injectScript('https://cdn.ravenjs.com/3.22.3/console/raven.min.js', function (err) {
  if (err) {
    console.warn('Could not load Raven.js script:', err);
    return;
  }
  if (!('Raven' in window)) {
    console.warn('Could not find `window.Raven` global');
    return;
  }
  ravenLoaded();
});

/**
 * Raven.js Offline Storage plugin
 * (Borrowed from incoming PR: https://github.com/getsentry/raven-js/pull/1165/files)
 *
 * Stores errors failed to send and try to send them when network comes back or on initialization.
 */

var offlineStorageKey = 'raven-js-offline-queue';

function offlineStoragePlugin(Raven, options) {
  options = options || {};

  function processOfflineQueue() {
    // Let's stop here if there's no connection
    if (!navigator.onLine) {
      return;
    }

    try {
      // Get the queue
      var queue = JSON.parse(localStorage.getItem(offlineStorageKey)) || [];

      // Store an empty queue. If processing these one fails they get back to the queue
      localStorage.removeItem(offlineStorageKey);

      queue.forEach(function processOfflinePayload(data) {
        // Avoid duplication verification for offline stored
        // as they may try multiple times to be processed
        Raven._lastData = null;

        // Try to process it again
        Raven._sendProcessedPayload(data);
      });
    } catch (error) {
      Raven._logDebug('error', 'Raven transport failed to store offline: ', error);
    }
  }

  // Process queue on start
  processOfflineQueue();

  // Add event listener on onravenFailure and store error on localstorage
  document.addEventListener('ravenFailure', function(event) {
    if (!event.data) {
      return;
    }

    try {
      var queue = JSON.parse(localStorage.getItem(offlineStorageKey)) || [];
      queue.push(event.data);
      localStorage.setItem(offlineStorageKey, JSON.stringify(queue));
    } catch (error) {
      Raven._logDebug('error', 'Raven failed to store payload offline: ', error);
    }
  });

  // Add event listener on online or custom event to trigger offline queue sending
  window.addEventListener(options.onlineEventName || 'online', processOfflineQueue);
}

function ravenLoaded () {
  console.log('Raven.js script loaded');
  Raven.config('https://e359be9fb9324addb0dc97b664cf5ee6@sentry.io/294878')
       .install();
  offlineStoragePlugin(Raven);
}

function injectScript (src, callback) {
  var script = document.createElement('script');
  script.src = src;
  script.crossorigin = 'anonymous';
  script.addEventListener('load', function () {
    if (callback) {
      callback(null, true);
    }
  });
  script.addEventListener('error', function (err) {
    if (callback) {
      callback(err);
    }
  });
  document.head.appendChild(script);
  return script;
}
})();
