
# VR Typing Experiment README (Onboarding)
---
Navigate to branch *typingapproachV2*
---
_Vive Trackpad + Eye Gaze · Unity + Python (OSC) · Audio Prompts · CSV Telemetry_

**Main scene:** `Assets/Scenes/NewTypingapproachv2`  
Other scenes (legacy/tests): `EvaluationStudy`, `EyeTypingandVivenewKeyboard`, `MainDatacollection`, `mltest`, `TouchandPhraseTyping`

---

## 0) What you build & how it talks

```
Vive Controllers ──SteamVR──> Python (OpenVR) ──OSC/UDP:5005──> Unity (extOSC)
                                                          |
Unity (gaze/key/centers) ──OSC/UDP:5006──> Python (Prediction Server)
Python (top3/coords)      <─OSC/UDP:5007── Unity (UI/Controller)
```

- Trackpad navigation → **NavigationManagerPressed** (intent‑gated + latched) → **KeyboardLayout** (QWERTY logic)  
- Gaze → **EyeTrackingRays** (per‑eye raycasts)  
- Controller I/O → **ViveTracker** (OSC in)  
- Prediction (optional) → **MLFeatures** + **MLListenerAndSender** ↔ Python `prediction_server.py`  
- Experiment loop → **TypingManagerv2**  
- Audio prompts → **TypingMetricsAudio**  
- Telemetry → **KeyboardDataRecorder** (`layout.csv` + per‑phrase `keylog_*.csv`)  
- UI → **MiniPadView** (5‑slot pad for highlights/predictions)

---

## 1) Requirements

- **Unity** 2021 LTS or newer (TMP/UGUI)  
- **Packages:** TextMeshPro, **extOSC**  
- **Meta OVR** (optional; otherwise use mock pinch in `EyeTrackingRays`)  
- **Python 3.8+** with `python-osc`, `numpy`, `joblib`  
- **SteamVR** running; controllers/base stations **green**

---

## 2) Quick start (single machine)

1. Start **SteamVR**.  
2. `python3 vive_sender.py` (controller → Unity, OSC **5005**).  
3. `python3 prediction_server.py` (Unity features **5006** → top‑3 back on **5007**).  
4. Open **NewTypingapproachv2** and **Play**.

You should navigate with the trackpad, see predictions (if server is on), hear audio, and get CSVs under:  
`Application.persistentDataPath/<UserID>/`

> Tip: firewall must allow UDP **5005/5006/5007**.

---

## 3) Scene wiring (minimal, from scratch)

Create an empty GameObject **`Scriptv2`**, add these components:

| Component                | Role                   | Required links |
|-------------------------|------------------------|----------------|
| **TypingManagerv2**     | Experiment runner      | Keyboard Controller → `KeyboardControllerPressed` • Keyboard Root → parent of keys • Audio Player → `TypingMetricsAudio` • Input Display → TMP |
| **KeyboardControllerPressed** | Input policy + UX | `ViveTracker` • Left/Right `EyeTrackingRays` • `KeyboardLayout` instance • **Space Colliders** (each box on space bar) • MiniPad refs/materials |
| **ViveTracker**         | OSC in (5005)          | none (auto‑binds) |
| **TypingMetricsAudio**  | Plays prompts          | none if using auto‑load (see §6) |

**Add two eye ray objects**  
Create `LeftEyeModel`, `RightEyeModel` → add **LineRenderer** + **EyeTrackingRays**.  
Set `layersToInclude` to your **Keyboard** layer. If no OVR hands, tick **mock pinch**.

**Add keyboard**  
`Keyboard Root`: parent of key objects.  
Each key: has a **Collider**, is on **Keyboard** layer, and is named **`Key_<NAME>`** (e.g., `Key_A`, `Key_Space`, `Key_Delete`).  
Drag `Keyboard Root` into `TypingManagerv2 → Keyboard Root`.

**Add MiniPad UI**  
Place **MiniPadView** (or assign prefab) and plug into  
`KeyboardControllerPressed → V3 MiniPad System` (scene + prefab refs).

That’s enough to press **Play**.

---

## 4) Networking — single source of truth

Pick your **Unity box IP** and reuse it everywhere.

| Path / Component             | Direction | IP:Port             | Notes                       |
|-----------------------------|-----------|---------------------|-----------------------------|
| `vive_sender.py`            | → Unity   | `<UNITY_IP>:5005`   | Controller OSC              |
| Unity **ViveTracker**       | listen    | `5005`              | extOSC                      |
| Unity **MLListenerAndSender** | → Python | `pythonHost:5006`   | Send features/centers       |
| `prediction_server.py`      | listen    | `0.0.0.0:5006`      | From Unity                  |
| `prediction_server.py`      | → Unity   | `<UNITY_IP>:5007`   | `/ml/top3`, `/ml/coords`    |
| Unity **MLListenerAndSender** | listen  | `5007`              | Matches server              |

**Spaces:** If `MLListenerAndSender.useKeyboardLocalSpace` is **true**, Python must score in **keyboard‑local space** (server expects centers).

---

## 5) Python services

### 5.1 Controller sender — `vive_sender.py`

Samples Vive via OpenVR; sends OSC to Unity **5005**.

### 5.2 Prediction server — `prediction_server.py`

**Set constants at file top**
```python
UNITY_IP    = "192.168.0.2"
UNITY_PORT  = 5007
LISTEN_IP   = "0.0.0.0"
LISTEN_PORT = 5006
```

**OSC addresses**
- **Listen** `/ml/features` → args: `gazeKey:str, gx:float, gy:float, gz:float, tNow:float, lastKey:str`  
- **Listen** `/ml/clear_centers` • `/ml/set_center (key:str, x,y,z)`  
- **Send** `/ml/top3` `string[3]` • `/ml/coords` `float[9]` (xyz for each top‑3, optional)

**Models (optional, `ml/`)**
- `gaze_model.pkl` (per‑key Gaussian: `mu`, `cov`)  
- `lm_data.pkl` (bigrams). If missing, server falls back to legacy scoring.

**Run**
```bash
pip install python-osc numpy joblib
python3 prediction_server.py
```

**Expected log**
```
Mode: Optimized | Model: 26 chars | LM: yes
Listening 0.0.0.0:5006 -> sending <UNITY_IP>:5007
Prediction: Gaze(...) -> ['E','R','D'] | 1.4ms
```

---

## 6) Audio assets (auto‑load by `TypingMetricsAudio`)

Place clips in **Resources** (inspector arrays may stay empty):

```
Resources/Audio/
  Letters/  A.wav ... Z.wav  space.wav
  Words/    the.wav quick.wav ...
  Phrases/  phrase1.wav  p1.wav  the_quick_brown_fox.wav
```

- **Letters:** case‑insensitive (A..Z, `space`)  
- **Words:** clip **name equals word** (case‑insensitive)  
- **Phrases:** prefer `phrase{id}` / `p{id}`; else text with **spaces→underscores**

---

## 7) Data logging (`KeyboardDataRecorder`)

**Per‑session** → `AddLayoutRecord(...)` for each key → `WriteLayoutOnce()` ⇒ **`layout.csv`**  
Header: `key,x,y,z,rx,ry,rz,bx,by,bz`

**Per‑phrase** → `BeginPhrase(id, participantId)` → `AddFrame(FrameSample)` at ~90 Hz →  
`SavePhraseCsv()` ⇒ **`keylog_phrase_{PID}_{PHRASE}.csv`**

**Frame CSV header (excerpt)**
```
t,frame,lPressed,rPressed,lX,lY,rX,rY,
pressed,trigger01,preferLeft,tx,ty,
selectedKey,gazeKey,phraseId,wordIdx,targetWord,typedText,
rHit,rX,rY,rZ,lHit,lX,lY,lZ,gazeMidX,gazeMidY,gazeMidZ,
kbX,kbY,kbZ,kbRX,kbRY,kbRZ
```

**Output root:** `Application.persistentDataPath/<PID>/` (configurable in constructor).

---

## 8) Core scripts (what matters)

### `NavigationManagerPressed`
- Thresholds: `DeadRadius=0.30`, `FullAxis=0.70`, `MoveIntentRadius=0.10`  
- No events until **intent threshold**; emits **RowUp/RowDown/Next/Prev** at full deflection;  
  **Reset** when returning to **dead zone**; **vertical** takes precedence; **axis latching** prevents repeats.

### `KeyboardLayout`
- Rows: `QWERTYUIOP` / `ASDFGHJKL` / `ZXCVBNM Delete` / `Space Submit`  
- Resolves `NavigationEvent` (and the *Pressed* adapter) to concrete key names;  
  preserves relative column on vertical moves; **Space** stays put unless explicitly navigated.  
- Helpers: `GetAdjacentKeys`, `GetAdjacentKeys8/GetRing8`, `PickTop3FromRing(anchor, top3)`.

### `EyeTrackingRays`
- Per‑eye `Physics.Raycast`; exposes `intercepting` and cached `hit`.  
- `GetCurrentGazedLetter()` returns **Key_ suffix** (e.g., `"A"`) or `"null"`.  
- Has optional OVR pinch or **mock pinch**.

### `ViveTracker`
- Binds **extOSC** on **5005**.  
- Left/Right paths: `/trigger`, `/trackpad_x`, `/trackpad_y`, `/trackpad_touched (bool)`, `/trackpad_pressed (bool)`, `/grip_button (bool)`, `/menu_button (bool)`.  
- Logs tap start/end and pressed changes.

### `MiniPadView`
- 5 slots (Up/Right/Left/Down/Center); labels + materials; `SetSelectedIndex`, `ApplyActiveMask`, `SetPalette`.

### `TypingMetricsAudio`
- `PlayAudioForTyping(utterance, onStart, onEnd)` chooses **letter / word / phrase**;  
  fallbacks **spell words** or **step through phrase**.  
- `InitializePhrase(id, text, clip)` to pre‑seed; auto‑loads assets if arrays are empty.

### `KeyboardDataRecorder`
- See §7.

> Two higher‑level components are assumed and wired as above:  
> **TypingManagerv2** (experiment runner) and **KeyboardControllerPressed** (policy + UX).

---

## 9) TypingManagerv2 & KeyboardControllerPressed (recommended defaults)

### TypingManagerv2
- **User ID:** `User01` (affects folder)  
- **Start Experiment Key:** `Return`  
- **Phrases Per Break:** `100` • **Per Condition:** `10`  
- **Min Space Interval:** `0.5` • **Debounce:** `0.1` • **Repeat Threshold:** `0.05`  
- **Inter Phrase Delay:** `3s`  
- Drag **Keyboard Root**, **Keyboard Controller**, **Audio Player**

### KeyboardControllerPressed
- **Initial Key:** `B`  
- **Gaze hysteresis:** H `0.7`, V `0.6`, D `0.55`  
- **Press Select Cooldown:** `0.2`  
- **Freeze On Touch (V2 & V3):** enabled  
- **Prediction TTL:** `0.35` • **Fallback label:** `ETA`  
- **Trigger Lock Threshold:** `0.5` • **Choose Leniency:** `0.015` • **Use Fallback Neighbors:** on  
- Assign **ViveTracker**, **Left/Right EyeTrackingRays**, **Space Colliders**, **MiniPad** refs, and **materials**

---

## 10) Phrase data

Phrase text is a **list of strings** (full phrases).  
Source can be the inspector list, a text asset, or runtime feed.  
Audio lookup for phrases follows §6 (by **id** or **underscored text**).

---

## 11) Testing checklist

- **Network:** same subnet, UDP **5005/5006/5007** open  
- **Unity:** extOSC present; **ViveTracker**, **MLFeatures**, **MLListenerAndSender** enabled; console shows bindings and `/ml/features` sends  
- **Server:** logs “Mode: Optimized”; receives centers after `/ml/clear_centers`; prints predictions in ~**1–5 ms**  
- **Loop:** Unity logs `/ml/top3: [X,Y,Z]`; MiniPad updates; CSVs appear in user folder

---

## 12) Tuning & troubleshooting

- **No gaze hits** → Keys on **Keyboard** layer? Ray `layersToInclude` includes it? Names start with **Key_**?  
- **Over‑eager moves** → raise `MoveIntentRadius`/`DeadRadius`, increase **Press Select Cooldown**  
- **Stuck repeats** → ensure you **return to dead zone** (that fires **Reset** and clears latches)  
- **No audio** → check `Resources/Audio/...` names; `TypingManagerv2 → Audio Player` assigned  
- **No CSVs** → confirm `BeginPhrase()` / `SavePhraseCsv()` are called  
- **No OSC** → IPs/ports; verify Unity console shows **OSCReceiver** bound; firewalls  
- **Space behavior** → see commented options in `KeyboardLayout` to make Space behave like bottom‑row center

---

## 14) Handy snippets

```csharp
// Navigation (per frame)
nav.Initialize(startX, startY);
nav.OnNavigationEvent += (evt, dir) => Debug.Log($"{evt}:{dir}");
var emitted = nav.UpdatePosition(x, y); // call each Update
nav.Reset();
```

```csharp
// Ask gaze
string gazed = rightEye.GetComponent<EyeTrackingRays>().GetCurrentGazedLetter(); // "A" or "null"
```

```csharp
// Audio
typingAudio.PlayAudioForTyping("the quick brown fox", () => Pause(), () => Resume());
```

```csharp
// Logging
rec.AddLayoutRecord(new KeyLayoutRecord{ key="A", pos=..., euler=..., size=... });
rec.WriteLayoutOnce();
rec.BeginPhrase(phraseId:1, participantId:"User01");
rec.AddFrame(new FrameSample{ t=Time.fixedTime, frame=Time.frameCount, selectedKey="A" });
var path = rec.SavePhraseCsv();
```

---

## 15) Suggested project layout

```
Assets/
  Scripts/
    Input/      (ViveTracker, EyeTrackingRays)
    Logic/      (NavigationManagerPressed, KeyboardLayout, enums)
    UI/         (MiniPadView)
    Audio/      (TypingMetricsAudio)
    Logging/    (KeyboardDataRecorder, FrameSample, KeyLayoutRecord)
    ML/         (MLFeatures, MLListenerAndSender)
  Resources/
    Audio/      (Letters/ Words/ Phrases/)
  Scenes/
    NewTypingapproachv2.unity
python/
  vive_sender.py
  prediction_server.py
  ml/
    gaze_model.pkl
    lm_data.pkl
```

---

# 🧭 Quest Pro Typing Capture with Eyegaze and Hand Tracking

This Unity project enables VR typing tracking using a **real physical keyboard** and the **Meta Quest Pro**'s hand tracking and eye gaze capabilities. It is designed for **data collection**, prototyping, and natural typing experiments in mixed reality. The main scene is EyeHands4TrackingWKeyboard.

---

## 🧠 Overview

### What this project does

- Uses **hand tracking** and **eyegaze** from Meta Quest Pro
- Displays characters typed on a **real Apple Magic Keyboard**
- Aligns the virtual keyboard using a **manual calibration gesture**
- Renders text live in 3D VR space as you type

---

## ⌨️ How the Keyboard Input Works

- The **Apple Magic Keyboard** is connected to a PC
- A **Python script** reads keypresses (using `pynput`) and sends characters via **UDP**
- Unity receives these characters and displays them using TextMeshPro

### Usage:

- 🧪 **In Editor (same PC):** use `127.0.0.1` (localhost)
- 🚀 **On Quest Pro (deployed APK):**
  - Ensure both PC and Quest are on the **same WiFi**
  - Set the Python script’s target IP to the **Quest’s IP**
  - You can find it with:
    ```bash
    adb shell ip addr show wlan0
    ```

---

## 📡 Python UDP Sender

> 🧩 **Python requirements:** install `pynput` via pip

```bash
pip install pynput
```

Then run this:

```python
from pynput import keyboard
import socket

UDP_IP = "127.0.0.1"  # Change to Quest's IP when deployed
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

def on_press(key):
    try:
        sock.sendto(key.char.encode('utf-8'), (UDP_IP, UDP_PORT))
    except:
        pass

with keyboard.Listener(on_press=on_press) as listener:
    listener.join()
```

---

## 🎯 Calibration Process

Before using the system, the user must perform a **manual calibration** to align the virtual keyboard with the physical one.

### ✅ Orientation Step (Before Calibration)

> Before registering the keyboard, the user must **align themselves properly**:
> - Look toward the **virtual keyboard model**
> - Rest hands on the **real keyboard** in a natural typing pose
> - Ensure the **index fingertip is flat on the key surface**
> - The accuracy of calibration depends on **realistic hand placement**

### 🧩 Calibration Steps

1. Using your **left index finger**, press the following physical keys:
   - `Q`
   - `P`
   - `Z`
2. The system:
   - Records the **fingertip world-space positions**
   - Matches them to the virtual keyboard model
   - Applies a **pure translation** (no rotation or scaling)

> You can repeat this process as needed. If you press the same points again with the same hand posture, the virtual keyboard should **not move again** (delta ≈ 0).

---

## 📐 Hierarchy & Object Layout

- The `pivot` object is the parent of:
  - `keyboard`
  - `hands`
  - `TextMeshPro` text display
  - `TextEntryVR.cs` (UDP receiver)
  - `Calibration.cs`
- The `pivot` updates its position every frame by **copying the position and rotation of the Quest Pro's `CenterEyeAnchor`**, effectively tracking the user's head.

- The calibration and text input systems operate **relative to the pivot**, ensuring the entire setup moves naturally with the user.

> ⚠️ Object transforms may differ between **Editor view** and **Quest standalone build** — fine tuning may be required after deployment.

---

## ⚠️ Notes on Precision

- **Oculus Quest Pro** hand and eye tracking are not perfectly accurate
- Expect small deviations in position and gaze due to:
  - Sensor noise
  - Pose ambiguity
  - Finger occlusion
- Recalibration may be needed periodically during testing

---

## 📂 Project Files

| File/Script              | Purpose                                                |
|--------------------------|--------------------------------------------------------|
| `EyeHands4TrackingWKeyboard` | Main Unity scene                                     |
| `TextEntryVR.cs`         | Handles incoming characters over UDP                   |
| `Calibration.cs`         | Captures calibration touches and applies alignment     |
| `EyetrackingGaze.cs`     | Reads and logs eyegaze input                           |
| Red fingertip spheres    | Used instead of full hand meshes for visual debugging  |

---

## 🖼️ Example Screenshots

<!-- Replace these with your own screenshots -->
### ✅ Calibrated Keyboard Alignment
![Keyboard Calibration Screenshot](images/calibration_example.png)

### 👁️ Live Typing with Eye Gaze
![Eyegaze Tracking Screenshot](images/eyegaze_typing.png)

---

## 🧪 Testing Notes

- ✅ Tested with **Unity 2022.3.5f1**
- 🛰 UDP server listens on **port 5005**
- 🔁 You can recalibrate restarting the VR Application (use Q, P, Z again)
- 🧩 The calibration points (`Qcalibration`, etc.) should **not move** with the keyboard — place them outside its hierarchy if needed

---

## 📝 TODO

- [ ] Add logging of:
  - [ ] Keypresses with timestamps
  - [ ] Hand fingertip positions
  - [ ] Eye gaze raycast hit positions
  - [ ] Keyboard model transform (position)
- [ ] Add instrution panel
- [ ] Connect to the new panel (from the previous experiment)
- [ ] Connect the logic of the previous experiment (63 sentences)

---

## 💡 Tips

- Don’t attach calibration transforms (`Qcalibration`, etc.) as children of the keyboard model
- For best results, **disable full hand rendering** and use **red fingertip spheres** for visual calibration accuracy
- `EyetrackingGaze.cs` can be expanded for experimental gaze tracking or visual analytics

---

## 📎 License


---

## 🙋‍♂️ Contact

For questions or extensions, contact:

- Daniele Giunchi — d.giunchi@bham.ac.com
- Maisy Rapata — m.rapata@bham.ac.uk

📍 VRLab — University of Birmingham, 2025
