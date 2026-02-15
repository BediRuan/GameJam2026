# GLITCHES: INTERFERENCES

_Something is interfering with the signal..._

---

## 01. OVERVIEW

**Interferences** replicates the visual artifacts of signal degradation, electronic interference, and horizontal scanline noise. Ideal for "lo-fi" aesthetics, malfunctioning monitors, or digital transmission errors.

## 02. REQUISITES

To ensure optimal performance and compatibility, your project must meet the following requirements:

*   **Unity:** 6000.0.23f1 or higher.
*   **Universal RP:** 17.0.3 or higher.

## 03. INSTALLATION GUIDE

### Step 1: Add Renderer Feature
The effect must be registered in your project's URP configuration:
1. Locate your **Universal Renderer Data** asset.
2. Click **Add Renderer Feature** and select **Fronkon Games > Glitches > Interferences**.

### Step 2: Configure the Volume
To apply the effect to your scene:
1. Create a **Volume** component (Global or Local).
2. In the Volume component, create or assign a **Volume Profile**.
3. Click **Add Override** and select **Fronkon Games > Glitches > Interferences**.
4. Enable the '**Intensity**' parameter (and any others you wish to modify).

---

## 04. SETTINGS REFERENCE

### 4.1 Interferences Settings
With '**Intensity**' you can control the overall strength of the effect [0.0 - 1.0]. If it is 0, the effect will not be active.

These parameters control the core signal degradation logic. Use '**Blend**' to define the mathematical operation used to mix interference layers, and adjust '**Offset**' to create chromatic aberration by displacing the color channels in the affected zones.

The horizontal image "tearing" is controlled through several distortion parameters: '**Distortion**' sets the maximum displacement, while '**Distortion Speed**' and '**Distortion Density**' define the evolution and vertical scale of the patterns. You can further refine these with '**Distortion Amplitude**' and '**Distortion Frequency**' for more complex wave-like behaviors. Finally, the horizontal scanline overlay can be customized using '**Scanlines**' visibility, '**Scanlines Density**' for spacing, and '**Scanlines Opacity**' for the darkening strength.

### 4.2 Color Settings
Final image grading tools for aesthetic fine-tuning: **Brightness**, **Contrast**, **Gamma**, **Hue**, and **Saturation**.

---

## 05. PROGRAMMING

The effect can be modified at runtime using the standard Unity Volume API:

```csharp
using FronkonGames.Glitches.Interferences;
using UnityEngine.Rendering;

// ...

if (volumeProfile.TryGet(out InterferencesVolume volume))
{
  volume.intensity.value = 0.5f;
  volume.distortion.value = 1.0f;
  volume.blend.value = ColorBlends.Difference;
}
```

For a more detailed code example, check the code of the included demo.

---

## CONTACT & SUPPORT

*   **Email:** [fronkongames@gmail.com](mailto:fronkongames@gmail.com)
*   **Documentation:** [Online Help](https://fronkongames.github.io/store/glitches/)

**NOTICE:** This asset is licensed for use in your projects but **cannot be hosted in public repositories**.
