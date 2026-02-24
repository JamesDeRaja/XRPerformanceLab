
# Experiment B – Overdraw (Fragment Bound Test)

## Objective
Demonstrate GPU fragment overload caused by stacked transparent surfaces in XR.

---

## Baseline (Overdraw OFF)

Frame: 8.54 ms  
CPU Main: 8.53 ms  
GPU: 6.88 ms  
FPS: ~117  

System near 120Hz budget. GPU has headroom.

---

## Stress (Overdraw ON – 200 Transparent Quads)

Frame: 16.51 ms  
CPU Main: 16.50 ms  
GPU: 14.26 ms  
FPS: ~60  

GPU time increased by +7.38 ms (~107%).

---

## Root Cause

- Transparent surfaces bypass efficient early-Z rejection.
- Each pixel shaded multiple times.
- XR stereo doubles fragment workload.
- GPU became fragment-bound.
- CPU main time increased due to XR frame pacing synchronization.

---

## Bottleneck Classification

GPU-bound (fragment/fill-rate limited) with runtime sync influence.

---

## Mitigation Strategies

- Reduce transparency stacking.
- Prefer alpha test/cutout where possible.
- Avoid full-screen transparent overlays.
- Reduce render scale.
- Use foveated rendering in XR.
