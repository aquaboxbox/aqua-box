# AquaBox

An **immersive sandbox experience** that pairs a real, water-filled physical box with a **1:1 mapped** virtual counterpart in VR. AquaBox features two primary modes—a **maze** and a **fluid simulation**—both controlled by physically moving and rotating a handheld box. Real water inside that box provides **haptic feedback**, bridging the gap between reality and VR.

---

## Table of Contents

1. [Overview](#overview)  
2. [Requirements](#requirements)  
3. [Installation](#installation)  
4. [Usage](#usage)  
5. [Troubleshooting & Tips](#troubleshooting--tips)

---

## Overview

AquaBox uses a **physical, water-filled box** tracked in real time to control a matching box in VR. This setup lets you:
- **Solve a Maze** by tilting the box to guide a glowing particle through a procedurally generated labyrinth.
- **Play with Virtual Fluid** that responds to your box’s movement, with actual water providing weight and splashing sounds that boost immersion.

Switch modes by holding the real box in a specific trigger zone for a few seconds—**listen for an audio cue** indicating the transition.

---

## Requirements

- **VR Headset** (e.g., HTC Vive, Oculus Rift/Quest (link cable), or another PC-based VR system).  
- **Room Setup**:  
  - Ensure you have a room-scale or seated/standing VR setup configured via your headset’s software (SteamVR Room Setup, Oculus Guardian, etc.).  
  - Enough free space to safely move a water-filled box without risk to equipment or surroundings.
- **Physical Box + Tracker**:  
  - A **watertight** box containing real water.  
  - A compatible VR tracker (e.g., Vive Tracker) firmly attached on top or side of the box.
- **AquaBox Files**:  
  - Either the precompiled `.exe` from this repository or the Unity project (if you plan on running from the Editor).

---

## Installation

1. **Obtain AquaBox**  
   - Download or clone the repository from GitHub.  
   - If a **precompiled build** (`AquaBox.exe`) is included, you can skip Unity setup and launch directly from the executable.

2. **Set Up Your VR Environment**  
   - Run **SteamVR** (or your VR headset’s software) and confirm controllers/trackers are visible.  
   - Complete a **Room Setup** (or “Guardian” boundary) so you have clear playspace.

3. **Mount the Tracker**  
   - Securely attach the VR tracker to your real box.  
   - Start SteamVR (or similar) and ensure the tracker is recognized and visible in 3D space.

4. **Launching AquaBox**  
   - **Precompiled:** Double-click the `AquaBox.exe` file.  
     - A VR window should appear, displaying the virtual environment.  
   - **Via Unity (Optional):**  
     1. Open the Unity project in Unity Hub or the Unity Editor.  
     2. Load the AquaBox scene (e.g., `MainAquaBoxScene`).  
     3. Press **Play** in the Editor while wearing your VR headset.

5. **Fill the Physical Box with Water** (Optional but Recommended)  
   - Add a small amount of water to the real box to experience the full haptic effect.  
   - Ensure the box is well-sealed to avoid spills.

---

## Usage

1. **Check Box Alignment**  
   - In VR, locate the virtual box.  
   - Align your **real** box’s orientation so the in-game box moves exactly as you tilt/rotate the physical one.

2. **Maze Mode**  
   - If you see a maze inside the virtual box, you’re in Maze Mode.  
   - Tilt the box in different directions to roll the glowing particle through the corridors.  
   - **Objective**: Reach the green exit at the maze’s end (random side of the box).

3. **Fluid Simulation**  
   - If you see virtual water or fluid-like particles, you’re in Fluid Mode.  
   - Shake, rotate, or gently swirl the real box to see the water react in VR.  
   - The real water’s motion provides **natural feedback** to deepen immersion.

4. **Switching Modes**  
   - Move your box into the on-screen **blue hollow box** (a special 3D area) and hold it there ~3 seconds.  
   - An audio cue indicates the mode switch is in progress.  
   - Release the box once you hear the sound or see the mode change.

5. **Quit**  
   - If running via `.exe`, remove your headset or press **Escape** to close.  
   - In Unity Editor, press the **Stop** button to end Play mode.

---

## Troubleshooting & Tips

- **Tracker Offset**: If the virtual box’s rotation or position feels “off,” recalibrate your tracker position in SteamVR or re-align the device in your VR software.  
- **Performance**: On lower-end systems, you may need to reduce graphical settings in Unity to maintain a smooth VR framerate (~90 FPS is recommended).  
- **Realism**: Try small amounts of water first. Too much water may be heavy to hold or spill.  
- **Audio Feedback**: The built-in sounds help you know when you’re switching modes. Keep VR headset audio on or use external speakers.

---
