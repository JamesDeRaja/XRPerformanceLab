using UnityEngine;

public class OverdrawToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform xrCamera;                 // Drag Main Camera transform here
    [SerializeField] private Transform experimentRoot;           // ExperimentB_Overdraw root
    [SerializeField] private Transform stackRoot;                // OverdrawStackRoot
    [SerializeField] private Material overdrawMaterial;          // M_OverdrawTransparent

    [Header("Overdraw Geometry")]
    [Range(4, 400)] public int quadCount = 12;
    [Range(4, 400)] public int quadNukeCount = 200;
    public Vector2 quadSize = new Vector2(4f, 4f);
    public float startDistance = 1.2f;                           // In front of camera
    public float zStep = 0.01f;                                  // Small separation to avoid z-fighting
    [Range(0.05f, 0.5f)] public float alpha = 0.18f;

    [Header("Optional: Make it heavier")]
    public bool addDoubleSided = true;                           // doubles fragments (backfaces)
    public bool attachToCamera = true;                           // keeps stack always in view

    private GameObject[] _quads;

    private void Awake()
    {
        if (experimentRoot != null)
            experimentRoot.gameObject.SetActive(false);
    }

    public void OverdrawOn()
    {
        if (experimentRoot == null || xrCamera == null || stackRoot == null || overdrawMaterial == null)
        {
            Debug.LogError("[OverdrawToggle] Missing references.");
            return;
        }

        experimentRoot.gameObject.SetActive(true);

        // Ensure the stack sits in front of the camera and follows it (deterministic coverage).
        if (attachToCamera)
        {
            stackRoot.SetParent(xrCamera, worldPositionStays: false);
        }
        else
        {
            stackRoot.SetParent(experimentRoot, worldPositionStays: true);
        }

        // Rebuild to guarantee a clean state every time.
        ClearQuads();
        BuildQuads(quadCount);
    }
    public void OverdrawNuke()
    {
        if (experimentRoot == null || xrCamera == null || stackRoot == null || overdrawMaterial == null)
        {
            Debug.LogError("[OverdrawToggle] Missing references.");
            return;
        }

        experimentRoot.gameObject.SetActive(true);

        // Ensure the stack sits in front of the camera and follows it (deterministic coverage).
        if (attachToCamera)
        {
            stackRoot.SetParent(xrCamera, worldPositionStays: false);
        }
        else
        {
            stackRoot.SetParent(experimentRoot, worldPositionStays: true);
        }

        // Rebuild to guarantee a clean state every time.
        ClearQuads();
        BuildQuads(quadNukeCount);
    }

    public void OverdrawOff()
    {
        ClearQuads();

        // Put it back under the experiment root to avoid camera hierarchy surprises.
        if (stackRoot != null && experimentRoot != null)
            stackRoot.SetParent(experimentRoot, worldPositionStays: false);

        if (experimentRoot != null)
            experimentRoot.gameObject.SetActive(false);
    }

    private void BuildQuads(int quadCountOverride)
    {
        _quads = new GameObject[quadCountOverride];

        // Create a unique material instance so we can set alpha without touching the asset.
        Material matInstance = new Material(overdrawMaterial);
        Color c = matInstance.color;
        c.a = alpha;
        matInstance.color = c;

        // Center the stack directly in front of camera.
        // Each quad covers large part of view; stacking creates overdraw.
        for (int i = 0; i < quadCountOverride; i++)
        {
            GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = $"OverdrawQuad_{i:00}";
            q.transform.SetParent(stackRoot, worldPositionStays: false);

            q.transform.localPosition = new Vector3(0f, 0f, startDistance + (i * zStep));
            q.transform.localRotation = Quaternion.identity;
            q.transform.localScale = new Vector3(quadSize.x, quadSize.y, 1f);

            // Remove collider (no CPU physics nonsense)
            var col = q.GetComponent<Collider>();
            if (col) Destroy(col);

            var r = q.GetComponent<MeshRenderer>();
            r.sharedMaterial = matInstance;

            // If you want it heavier: ensure backfaces render too.
            // For URP Unlit, easiest is material setting "Render Face: Both" in the asset.
            // This bool exists as a reminder: the material setting is what matters.
            _quads[i] = q;
        }
    }

    private void ClearQuads()
    {
        if (_quads == null) return;
        for (int i = 0; i < _quads.Length; i++)
        {
            if (_quads[i] != null)
                Destroy(_quads[i]);
        }
        _quads = null;
    }
}