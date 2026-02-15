# GLITCHES: BAD GPU

_When it's time to replace the GPU._

---

## 01. OVERVIEW

**Bad GPU** replicates the visual glitches, pixel errors, and signal distortions typically associated with failing graphics hardware or extreme digital interference.

## 02. REQUISITES

To ensure optimal performance and compatibility, your project must meet the following requirements:

*   **Unity:** 6000.0.23f1 or higher.
*   **Universal RP:** 17.0.3 or higher.

## 03. INSTALLATION GUIDE

### Step 1: Add Renderer Feature
The effect must be registered in your project's URP configuration:
1. Locate your **Universal Renderer Data** asset.
2. Click **Add Renderer Feature** and select **Fronkon Games > Glitches > Bad GPU**.

### Step 2: Configure the Volume
To apply the effect to your scene:
1. Create a **Volume** component (Global or Local).
2. In the Volume component, create or assign a **Volume Profile**.
3. Click **Add Override** and select **Fronkon Games > Glitches > Bad GPU**.
4. Enable the '**Intensity**' parameter (and any others you wish to modify).

---

## 04. SETTINGS REFERENCE

### 4.1 Signal Settings
With '**Intensity**' you can control the overall strength of the effect [0.0 - 1.0]. If it is 0, the effect will not be active.

These parameters define the characteristics of the hardware failure. '**Balance**' allows you to shift the visual weight between the original image and the glitch effect [-1.0, 1.0], while '**Booster**' enhances both the intensity and brightness of the artifacts [0.0 - 10.0]. You can customize the color of the distortion using the '**Tint**' and '**Blend**' mode operations.

The system uses four independent glitch layers (**Glitch 1-4**), each providing granular control over its own **Intensity**, **Speed**, and **Threshold**. This allows for the creation of complex, multi-layered patterns that simulate everything from minor bit-errors to total hardware collapse.

### 4.2 Color Settings
Final image grading tools for aesthetic fine-tuning: **Brightness**, **Contrast**, **Gamma**, **Hue**, and **Saturation**.

---

## 05. PROGRAMMING

The effect can be modified at runtime using the standard Unity Volume API:

```csharp
using FronkonGames.Glitches.BadGPU;
using UnityEngine.Rendering;

// ...

if (volumeProfile.TryGet(out BadGPUVolume volume))
{
  volume.intensity.value = 0.5f;
  volume.balance.value = 0.2f;
  volume.blend.value = ColorBlends.Difference;
}
```

For a more detailed code example, check the code of the included demo.

---

## CONTACT & SUPPORT

*   **Email:** [fronkongames@gmail.com](mailto:fronkongames@gmail.com)
*   **Documentation:** [Online Help](https://fronkongames.github.io/store/glitches/)

**NOTICE:** This asset is licensed for use in your projects but **cannot be hosted in public repositories**.
