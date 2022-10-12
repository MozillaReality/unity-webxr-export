# Unity WebXR Exporter

Assets for creating [WebXR](https://developer.mozilla.com/topics/mixed-reality/)-enabled [Unity3D](https://unity.com/) projects.

**[Check out the demo now!](https://mixedreality.mozilla.org/unity-webxr-export/)**<br>
[Read our blog post (March 2020 - WebXR Update).](https://blog.mozvr.com/unity-webxr-exporter-update/)
[Read our blog post (February 2018).](https://hacks.mozilla.org/2018/02/create-vr-on-the-web-using-unity3d/)

[![Preview of Unity WebXR-exported project in the browser](https://raw.githubusercontent.com/mozillareality/unity-webxr-export/master/img/webxr-small.gif)](https://mozillareality.github.io/unity-webxr-export/)

**[Free to download](https://assetstore.unity.com/packages/templates/systems/webxr-exporter-109152)** and available now on the [Unity Asset Store](https://assetstore.unity.com/packages/templates/systems/webxr-exporter-109152).

<a href="https://assetstore.unity.com/packages/templates/systems/webxr-exporter-109152" title="Download the WebXR Assets package for free on the Unity Asset Store">
</a>

<hr>

## Getting started

* [Setting up a Unity project for WebXR](./docs/project-setup.md)
* [Controllers and input system](./docs/controllers.md)
* [In-editor VR playback for rapid testing](./docs/xr-testing.md)
* [Publishing](./docs/publishing.md)
* [Adding Google Analytics to your game](./docs/customization/adding-ga.md)
* [Troubleshooting and FAQ](./docs/troubleshooting-faq.md)

## Need help?

* [Join the WebVR Slack](https://webvr-slack.herokuapp.com/) (join the [#unity channel](https://webvr.slack.com/messages/unity))

## Compatibility

### Unity editor version

* `2018.4` and newer.

### Browser Compatibility

WebXR has limited support at time of writing. See compatibility at [caniuse](https://caniuse.com/#feat=webxr) or on [MDN](https://developer.mozilla.org/en-US/docs/Web/API/WebXR_Device_API)

A special build of Firefox, [Firefox Reality](https://mixedreality.mozilla.org/firefox-reality/), has built-in support for WebXR.

The polyfill will fall back to the old WebVR API if it is available.

| Platform | Browser | Compatible headsets | Notes |
| --- | --- | --- | --- |
| Desktop | Firefox | HTC VIVE, Oculus Rift, Windows Mixed Reality headsets (using Steam VR) | [Setup instructions](https://webvr.rocks/firefox) |
| Desktop | Microsoft Edge | Windows Mixed Reality headsets | [Setup instructions](https://webvr.rocks/microsoft_edge) |
| Desktop | Chrome Canary | HTC VIVE, Oculus Rift, Windows Mixed Reality headsets | Browser flags required. [Setup instructions](https://webvr.rocks/chrome) |
| Mobile | Meta Quest Browser | Meta Quest |
| Mobile | Firefox Reality | Meta Quest |

### Polyfilled WebXR

If the user does not have supported headset, browser or device, the content will still work through the use of the [WebXR Polyfill](https://github.com/immersive-web/webxr-polyfill).

### Mobile support

This asset works by utilizing Unity's WebGL platform support and therefore shares the same limitations. Because of this, mobile support is limited and may not work. See [Unity's WebGL browser compatibility](https://docs.unity3d.com/Manual/webgl-browsercompatibility.html).

### Version History and Notes

* [See all releases](https://github.com/mozilla/unity-webvr-export/releases)


## Contributing

Contributions from the developer community are very important to us. You're encouraged to [open an issue](https://github.com/mozilla/unity-webvr-export/issues/new), report a problem, contribute with code, open a feature request, share your work or ask a question.

Take a look at the contributor guides too.

* [Building the Unity Assets Package](./docs/build.md)


## We want to hear from you!

We’d love to hear about what you come up with using the _WebXR Assets_. **Share your work with us** and use the [#unitywebvr](https://twitter.com/search?f=tweets&q=%23unitywebvr) Twitter hashtag.

The Unity WebXR Assets is an open-source project (licensed under Apache 2) [available on GitHub](https://github.com/mozilla/unity-webvr-export).

* [View known issues](https://github.com/mozilla/unity-webvr-export/issues)
* [File an issue or feature request](https://github.com/mozilla/unity-webvr-export/issues/new)
* [Contribute code or documentation to the project](https://github.com/mozilla/unity-webvr-export#contributing)

Reach out to us with any questions you may have or help you may need, and participate in the discussions on the [WebVR Slack](https://webvr-slack.herokuapp.com/) in the [#unity channel](https://webvr.slack.com/messages/unity).

* [Join the WebVR Slack](https://webvr-slack.herokuapp.com/) (join the [#unity channel](https://webvr.slack.com/messages/unity))


## Developer Privacy Notice for Data Collection

_Last updated: March 2018_

To help improve the [WebVR API](https://immersive-web.github.io/webvr/spec/1.1/) and the [Unity WebVR Assets](https://assetstore.unity.com/packages/templates/systems/webvr-assets-109152) package, each time a web page built using the [WebVR Assets](https://assetstore.unity.com/packages/templates/systems/webvr-assets-109152) is loaded, Mozilla automatically receives general-usage statistics and uncaught JavaScript errors encountered by end-users, using [Google Analytics](https://analytics.google.com/analytics/web/) and [Sentry](https://sentry.io), respectively. [The *complete list of collected data*](https://github.com/mozilla/unity-webvr-export/blob/master/TELEMETRY.md#list-of-collected-data) includes metrics for counting the number of unique web-page sessions; time for web pages to load and time open; JavaScript error exceptions occurred on the page; the number of times a VR device is mounted and worn; number of times VR mode is enabled and time spent; and a random identifier.

You as a developer can turn off this data collection by [modifying the configuration snippet that comes with the VR template](https://github.com/mozilla/unity-webvr-export/blob/master/docs/customization/disabling-telemetry.md). It is your obligation to inform your end-users of this data collection and to inform them that it can be turned off by [enabling “Do-Not-Track”](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/DNT) in their browsers.


## Credits

This project was heavily influenced by early explorations in using Unity to build for WebVR by [@gtk2k](https://github.com/gtk2k), [Chris Miller (@chrmi)](https://github.com/chrmi) and [Anthony Palma](https://twitter.com/anthonyrpalma).

Also, thanks to [Arturo Paracuellos (@arturitu)](https://github.com/arturitu) for creating the [3D-hand models](https://github.com/aframevr/assets/tree/gh-pages/controllers/hands) used for controllers in these examples.

## License

Copyright 2017 - 2020 Mozilla Corporation

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
