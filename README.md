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

---

## 4. How Eye Gaze Works
```text
OVREyeGaze
```
Reads real-time eye-tracking data from the headset.

Automatically updates the GameObject’s position and rotation based on eye movement.
```text
EyeTrackingRays
```
Draws a ray in front of the eye using a LineRenderer.

Ray color states:

🟡 Default: Not hitting any object

🔴 Hover: Looking at an object

Includes optional pinch logic, though this demo focuses on gaze visualization only.

---

## 5. Hover Interaction

To enable hover detection:

Add at least one GameObject with a Collider in front of the eye models.

Ensure the object’s layer is included in the layersToInclude mask in the EyeTrackingRays component.

(Optional) Use the provided EyeModels script to make objects react to hover events.

6. Custom Logic
You can add your own logic inside EyeTrackingRays where objects are detected.
For example, trigger events when the ray hits a specific object or when gaze is held for a certain time.
