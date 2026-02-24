# XR Performance Stress Lab – Overdraw Experiment

This repository contains controlled rendering stress experiments built in Unity (URP + OpenXR) to analyze real-time GPU and CPU bottlenecks under XR constraints.

This specific experiment isolates **transparent overdraw** and measures its impact on render-thread cost and fragment workload scaling.

---

# 🎯 Objective

To demonstrate:

- How transparent stacking affects fragment workload
- Why overdraw scales render-thread cost linearly
- How to validate bottlenecks using:
  - Unity Profiler (CPU + Render Thread)
  - Frame Debugger (pass-level inspection)
  - Controlled baseline vs stress comparison

This is not an FPS measurement exercise.
This is a pipeline-level validation exercise.

---

# 🧪 Experiment Setup

## Engine
- Unity (URP)
- OpenXR enabled
- Mock HMD for XR simulation
- Forward Rendering

## Scene Configuration

### Baseline Scene
- Opaque geometry only
- No transparent stacking
- Identical camera + resolution as stress test
- MSAA and render scale held constant

### Overdraw Stress Scene
- 201 transparent quads
- Identical mesh (4 vertices, 6 indices)
- Same material
- Alpha blending enabled
- `ZWrite Off`
- Stacked in the same screen region

This ensures fragment overlap and disables early depth rejection.

---

# 📊 Results

## 🔹 CPU (Main Thread)

| Condition | PlayerLoop |
|-----------|------------|
| Baseline  | ~0.5 ms    |
| Overdraw  | ~0.5 ms    |

CPU cost remained stable.

No script or update bottleneck introduced.

---

## 🔹 Render Thread

| Condition | RenderLoop |
|-----------|------------|
| Baseline  | 6.37 ms    |
| Overdraw  | 13.64 ms   |

**Δ ≈ +7.27 ms**

Transparent stacking nearly doubled render-thread cost.

---

# 🔍 Frame Debugger Validation

Under `DrawTransparentObjects`:

- 201 sequential `Draw Mesh` calls
- Same quad mesh
- Same material
- Blend: `SrcAlpha / OneMinusSrcAlpha`
- `ZWrite Off`

Because depth writes are disabled:

- Early-Z rejection cannot occur
- Every fragment executes
- Cost scales linearly with layer count

This confirms fragment-bound behavior.

---

# 🧠 Technical Interpretation

Transparent objects:

- Are rendered after opaque pass
- Are sorted back-to-front
- Cannot rely on depth buffer for early rejection
- Execute full fragment shader per layer

When stacked in the same pixel region:

Fragment workload = Layers × Pixel Coverage

Under XR (dual eye rendering), this cost is effectively doubled again.

---

# 🏁 Conclusion

This experiment validates:

- Overdraw is a fragment-bound GPU bottleneck
- Transparent stacking directly impacts render-thread cost
- CPU remains unaffected
- Frame Debugger confirms pass-level multiplication

The bottleneck shift is measurable, reproducible, and pipeline-consistent.

---

# 🚀 Why This Matters (XR Context)

In XR:

- Render resolution is high
- Stereo rendering doubles workload
- Fragment cost scales aggressively
- Frame budget (72–120 Hz) is tight

Transparent overdraw becomes significantly more expensive under headset constraints.

Understanding pass-level behavior is critical for:

- UI systems
- Particle systems
- Post-processing overlays
- Shader-heavy VFX

---

# 🔬 Methodology Integrity

To maintain experiment validity:

- Same resolution
- Same MSAA
- Same render scale
- Same camera
- Same lighting
- Only transparent stacking modified

No unrelated variables introduced.

---

# 📁 Evidence Files

- [Overdraw_Profiler_Comparison](https://james.alphaden.club/lab/Overdraw_CPU_RenderThread_Comparison.png)
- [Overdraw_FrameDebugger_TransparentPass](https://james.alphaden.club/lab/Overdraw_FrameDebugger_TransparentPass.png)
- [Overdraw_Experiment_Summary](https://james.alphaden.club/lab/Overdraw_Experiment_Summary.png)

Each capture corresponds to the metrics reported above.

---

# 📌 Future Extensions

- Repeat test with MSAA x4 vs x1
- Repeat with Cutout (ZWrite On) for comparison
- Measure scaling at 2× resolution
- Repeat under real headset hardware
- Compare against instancing stress

---

# 👤 Author

James De Raja  
Senior Real-Time Performance Engineer  
Unity Rendering | Frame Pacing | XR Optimization  
[Portfolio](https://james.alphaden.club) | [LinkedIn](https://www.linkedin.com/in/james-de-raja/)

---

# License

MIT (for experimental code only)
