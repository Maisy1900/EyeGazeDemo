# Eye Gaze Demo (Unity + Oculus)

This Unity scene shows basic eye-gaze visualization using the Oculus `OVREyeGaze` script and a simple ray visualizer.

---

## 1. Requirements

- Unity (2021.3.5f1 recommended)
- Oculus Integration package
- Headset with eye tracking (e.g. Meta Quest Pro)

---

## 2. Open the Scene

1. Open the project in Unity.
2. In the Project window go to:

   `Assets/Scene/EyeGazeDemo.unity`

3. Open this scene.

---

## 3. Scene Hierarchy

In the Hierarchy you should see:

```text
InteractionRigOVR-FullSynthetic
└── OVRCameraRig
    └── TrackingSpace
        ├── LeftEyeAnchor
        ├── CenterEyeAnchor
        ├── RightEyeAnchor
        ├── TrackerAnchor
        ├── LeftHandAnchor
        ├── RightHandAnchor
        ├── LeftEyeModel
        ├── RightEyeModel
        └── ReferenceFrame
```
LeftEyeModel and RightEyeModel each have an OVREyeGaze component.

They may also have the EyeTrackingRays script to draw a line showing where the eye is looking.

4. How Eye Gaze Works
OVREyeGaze reads the eye-tracking data from the headset.

EyeTrackingRays:

Draws a line (ray) forward from the eye.

Changes state when the ray hits an object:

Default color when not hitting anything.

“Hover” color when looking at an object.

Contains some pinch-selection logic, but this demo mainly uses it to show the gaze ray.

For hover to work:

You must have at least one GameObject with a Collider in front of the eyes.

That object should be on a layer included in the layersToInclude mask on EyeTrackingRays.

(Optional) If you use the provided EyeModels script, it will react to hover events.

5. Custom Logic
You can add your own logic inside EyeTrackingRays where objects are detected.
For example, trigger events when the ray hits a specific object or when gaze is held for a certain time.