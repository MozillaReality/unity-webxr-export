# Building the Unity Assets Package

If you are a maintainer of this project, and you modified the demo scene distributed with the package, you will want to update the [`Build/` directory](https://github.com/mozilla/unity-webvr-export/tree/master/Build/) (hosted [online here](https://mozilla.github.io/unity-webvr-export/Build/)).

1. Launch `Edit > Build Settings > Project Settings`. From `Player Settings…` (`Edit > Project Settings > Player`), select the **`WebGL settings`** tab (HTML5 icon), toggle the **`Resolution and Presentation`** view, and select **`WebXR`** for the `WebGL Template`.

    ![WebGL template selector](./images/webxr-template.png)

2. Launch `Edit > Build Settings > Project Settings`. Then, press the **`Build and Run`** button, and **`Save`** to the directory named **`Build`**.

    ![Selecting the Build folder](../img/build-webgl.png)

If you are contributing to the Assets, you can build and export a new version of the [`WebXR-Assets.unitypackage` file](../WebXR-Assets.unitypackage).

Notice that the package does not include all the assets in the repository but **only those under `WebVR`,  `WebXR`, and `WebGLTemplates`**:

1. Open **`Assets > Export Package…`**. A window titled `Exporting package` will appear. Press the **`Export…`** button to proceed.

    ![Exporting package](../img/exporting-asset-package.png)

2. When prompted for the file location, set **`WebXR-Assets`** as the filename of the destination Unity Asset Package, and press the **`Save`** button.

    ![Export package …](../img/export-asset-package.png)
