using UnityEngine;

/// <summary>
/// Smoothly rotates this GameObject forever by continuously choosing random target orientations
/// and easing toward them over time.
/// 
/// Attach to any object you want to spin/turn in a natural, non-jittery way.
/// </summary>
public class RandomRotation : MonoBehaviour
{
    [Header("Random Target")]
    [Tooltip("Minimum time (seconds) to rotate toward a random target orientation.")]
    [Min(0.01f)]
    public float minSegmentDuration = 1.0f;

    [Tooltip("Maximum time (seconds) to rotate toward a random target orientation.")]
    [Min(0.01f)]
    public float maxSegmentDuration = 3.0f;

    [Tooltip("Max random rotation in degrees applied per segment on each axis.")]
    [Range(0f, 180f)]
    public float maxDeltaDegreesPerAxis = 45f;

    [Header("Motion")]
    [Tooltip("If true, rotation is applied in local space (relative to parent). If false, world space.")]
    public bool useLocalSpace = true;

    [Tooltip("If true, constrains rotation changes to the Y axis only (useful for 'turntable' spinning).")]
    public bool yAxisOnly = false;

    // Current segment state
    private Quaternion _from;
    private Quaternion _to;
    private float _t;
    private float _duration;

    private void Start()
    {
        // Initialize with a first random target.
        _from = GetRotation();
        PickNextTarget();
    }

    private void Update()
    {
        // Advance segment time.
        _t += Time.deltaTime;
        float u = Mathf.Clamp01(_t / _duration);

        // Smoothstep easing: avoids robotic constant-speed feel.
        float eased = u * u * (3f - 2f * u);

        // Interpolate between orientations.
        Quaternion current = Quaternion.Slerp(_from, _to, eased);
        SetRotation(current);

        // When we reach the target, start a new segment.
        if (u >= 1f)
        {
            _from = _to;
            PickNextTarget();
        }
    }

    private void PickNextTarget()
    {
        _t = 0f;
        _duration = Random.Range(minSegmentDuration, maxSegmentDuration);

        Vector3 delta;
        if (yAxisOnly)
        {
            delta = new Vector3(0f, Random.Range(-maxDeltaDegreesPerAxis, maxDeltaDegreesPerAxis), 0f);
        }
        else
        {
            delta = new Vector3(
                Random.Range(-maxDeltaDegreesPerAxis, maxDeltaDegreesPerAxis),
                Random.Range(-maxDeltaDegreesPerAxis, maxDeltaDegreesPerAxis),
                Random.Range(-maxDeltaDegreesPerAxis, maxDeltaDegreesPerAxis)
            );
        }

        // Apply a random delta on top of the current rotation.
        _to = _from * Quaternion.Euler(delta);
    }

    private Quaternion GetRotation()
    {
        return useLocalSpace ? transform.localRotation : transform.rotation;
    }

    private void SetRotation(Quaternion q)
    {
        if (useLocalSpace)
            transform.localRotation = q;
        else
            transform.rotation = q;
    }
}
