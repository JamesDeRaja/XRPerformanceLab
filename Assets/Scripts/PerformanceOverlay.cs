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

    Queue<float> frameTimes = new Queue<float>();
    float frameTimeSum = 0f;

    void OnEnable()
    {
        cpuMainThreadRecorder = ProfilerRecorder.StartNew(
            ProfilerCategory.Internal,
            "Main Thread",
            sampleCount);

        renderThreadRecorder = ProfilerRecorder.StartNew(
            ProfilerCategory.Internal,
            "Render Thread",
            sampleCount);

        gpuRecorder = ProfilerRecorder.StartNew(
            ProfilerCategory.Render,
            "GPU Frame Time",
            sampleCount);
    }

    void OnDisable()
    {
        cpuMainThreadRecorder.Dispose();
        renderThreadRecorder.Dispose();
        gpuRecorder.Dispose();
    }

    void Update()
    {
        float frameTime = Time.unscaledDeltaTime * 1000f;

        frameTimes.Enqueue(frameTime);
        frameTimeSum += frameTime;

        if (frameTimes.Count > sampleCount)
            frameTimeSum -= frameTimes.Dequeue();

        float avgFrameTime = frameTimeSum / frameTimes.Count;
        float fps = 1000f / avgFrameTime;

        float cpuMain = GetRecorderValue(cpuMainThreadRecorder);
        float renderThread = GetRecorderValue(renderThreadRecorder);
        float gpu = GetRecorderValue(gpuRecorder);

        performanceText.text =
            $"FPS: {fps:F1}\n" +
            $"Frame: {avgFrameTime:F2} ms\n" +
            $"CPU Main: {cpuMain:F2} ms\n" +
            $"Render Thread: {renderThread:F2} ms\n" +
            $"GPU: {(gpu > 0 ? gpu.ToString("F2") : "N/A")} ms";
    }

    float GetRecorderValue(ProfilerRecorder recorder)
    {
        if (!recorder.Valid || recorder.Count == 0)
            return 0f;

        return recorder.LastValue / 1000000f; // nanoseconds → ms
    }
}