using UnityEngine;
using TMPro;
using Unity.Profiling;
using System.Collections.Generic;

public class PerformanceOverlay : MonoBehaviour
{
    public TMP_Text performanceText;
    public int sampleCount = 120;

    ProfilerRecorder cpuMainThreadRecorder;
    ProfilerRecorder renderThreadRecorder;
    ProfilerRecorder gpuRecorder;

    // Frame-time smoothing
    readonly Queue<float> frameTimes = new Queue<float>();
    float frameTimeSum = 0f;

    // Reused sample lists (no per-frame allocations once capacity is set)
    readonly List<ProfilerRecorderSample> cpuSamples = new List<ProfilerRecorderSample>(256);
    readonly List<ProfilerRecorderSample> renderSamples = new List<ProfilerRecorderSample>(256);
    readonly List<ProfilerRecorderSample> gpuSamples = new List<ProfilerRecorderSample>(256);

    void OnEnable()
    {
        cpuMainThreadRecorder = ProfilerRecorder.StartNew(
            ProfilerCategory.Internal,
            "Main Thread",
            sampleCount);

        // Render thread name is not consistent across platforms/Unity versions.
        // We'll start with "Render Thread" and if it never produces samples, overlay will show N/A.
        // If you want to be aggressive, swap to StartFirstValidRecorder() below.
        renderThreadRecorder = ProfilerRecorder.StartNew(
            ProfilerCategory.Internal,
            "Render Thread",
            sampleCount);

        gpuRecorder = ProfilerRecorder.StartNew(
            ProfilerCategory.Render,
            "GPU Frame Time",
            sampleCount);

        EnsureCapacity(cpuSamples, sampleCount);
        EnsureCapacity(renderSamples, sampleCount);
        EnsureCapacity(gpuSamples, sampleCount);
    }

    void OnDisable()
    {
        if (cpuMainThreadRecorder.Valid) cpuMainThreadRecorder.Dispose();
        if (renderThreadRecorder.Valid) renderThreadRecorder.Dispose();
        if (gpuRecorder.Valid) gpuRecorder.Dispose();
    }

    void Update()
    {
        // Smooth frame time (ms)
        float frameTime = Time.unscaledDeltaTime * 1000f;

        frameTimes.Enqueue(frameTime);
        frameTimeSum += frameTime;

        if (frameTimes.Count > sampleCount)
            frameTimeSum -= frameTimes.Dequeue();

        float avgFrameTime = frameTimeSum / frameTimes.Count;
        float fps = 1000f / avgFrameTime;

        // Smooth CPU/GPU via recorder window average
        float cpuMainAvg = GetRecorderAverageMs(cpuMainThreadRecorder, cpuSamples);
        float renderAvg = GetRecorderAverageMs(renderThreadRecorder, renderSamples);
        float gpuAvg = GetRecorderAverageMs(gpuRecorder, gpuSamples);

        performanceText.text =
            $"FPS: {fps:F1}\n" +
            $"Frame: {avgFrameTime:F2} ms\n" +
            $"CPU Main (avg): {FormatMs(cpuMainAvg)}\n" +
            $"Render Thread (avg): {FormatMs(renderAvg)}\n" +
            $"GPU (avg): {FormatMs(gpuAvg)}";
    }

    static float GetRecorderAverageMs(ProfilerRecorder recorder, List<ProfilerRecorderSample> samples)
    {
        if (!recorder.Valid) return -1f;

        int count = recorder.Count;
        if (count <= 0) return -1f;

        samples.Clear();
        if (samples.Capacity < count) samples.Capacity = count;

        recorder.CopyTo(samples);
        if (samples.Count == 0) return -1f;

        double sum = 0.0;
        for (int i = 0; i < samples.Count; i++)
            sum += samples[i].Value;

        // nanoseconds -> milliseconds
        return (float)(sum / samples.Count / 1_000_000.0);
    }

    static string FormatMs(float ms)
    {
        // Any negative value means "not available / no samples"
        return ms >= 0f ? $"{ms:F2} ms" : "N/A";
    }

    static void EnsureCapacity<T>(List<T> list, int capacity)
    {
        if (list.Capacity < capacity)
            list.Capacity = capacity;
    }

    // OPTIONAL: if you want to try multiple names for render thread, use this instead of StartNew().
    // Note: Valid can still be true even if Count stays 0; that's why we still rely on "N/A" display.
    /*
    static ProfilerRecorder StartFirstValidRecorder(ProfilerCategory cat, int capacity, params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            var r = ProfilerRecorder.StartNew(cat, names[i], capacity);
            if (r.Valid) return r;
        }
        return default;
    }
    */
}