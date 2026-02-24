using UnityEngine;
using UnityEngine.Rendering;

public class MSAAController : MonoBehaviour
{
    [Header("URP Pipeline Assets")]
    [SerializeField] private RenderPipelineAsset urpMsaa0;
    [SerializeField] private RenderPipelineAsset urpMsaa2;
    [SerializeField] private RenderPipelineAsset urpMsaa4;
    [SerializeField] private RenderPipelineAsset urpMsaa8;

    [Header("Optional")]
    [SerializeField] private bool restoreOnDisable = true;
    [SerializeField] private bool logToConsole = true;

    private RenderPipelineAsset _previous;

    void OnEnable()
    {
        // Save whatever pipeline is currently active so we can restore it.
        _previous = QualitySettings.renderPipeline;
    }

    void OnDisable()
    {
        if (restoreOnDisable && _previous != null)
            QualitySettings.renderPipeline = _previous;
    }

    public void SetMSAA0() => Apply(urpMsaa0, "MSAA 0x");
    public void SetMSAA2() => Apply(urpMsaa2, "MSAA 2x");
    public void SetMSAA4() => Apply(urpMsaa4, "MSAA 4x");
    public void SetMSAA8() => Apply(urpMsaa8, "MSAA 8x");

    private void Apply(RenderPipelineAsset asset, string label)
    {
        if (asset == null)
        {
            Debug.LogError($"[MSAAController] Missing pipeline asset for {label}");
            return;
        }

        QualitySettings.renderPipeline = asset;

        if (logToConsole)
            Debug.Log($"[MSAAController] Applied {label}");
    }
}