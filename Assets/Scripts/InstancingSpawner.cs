using UnityEngine;

public class InstancingSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Mesh mesh;
    public Material materialNoInstancing;
    public Material materialInstancing;

    [Range(10, 25000)]
    public int count = 1000;

    public Vector3 area = new Vector3(30f, 10f, 30f);
    public Vector3 baseScale = Vector3.one;

    [Header("Optional")]
    public bool addColliders = false; // keep false for submission test
    public bool markStatic = true;     // helps stability (not required)

    private GameObject[] spawned;
    private Material currentMaterial;

    void Awake()
    {
        if (mesh == null)
        {
            // Unity built-in cube mesh fallback
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh = tmp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(tmp);
        }
    }

    public void SpawnNoInstancing()
    {
        Spawn(materialNoInstancing);
    }

    public void SpawnInstancing()
    {
        Spawn(materialInstancing);
    }

    public void ClearAll()
    {
        // Destroy objects referenced in the spawned array
        if (spawned != null)
        {
            for (int i = 0; i < spawned.Length; i++)
            {
                if (spawned[i] != null)
                {
                    if (Application.isPlaying)
                        Destroy(spawned[i]);
                    else
                        DestroyImmediate(spawned[i]);
                }
            }
        }

        // Safety: also destroy any leftover children under this spawner
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }

        spawned = null;
        currentMaterial = null;
    }

    // private void Spawn(Material mat)
    // {
    //     if (mat == null)
    //     {
    //         Debug.LogError("[InstancingSpawner] Material is missing.");
    //         return;
    //     }

    //     // If same mode already active, do nothing
    //     if (currentMaterial == mat && spawned != null) return;

    //     ClearAll();

    //     spawned = new GameObject[count];
    //     currentMaterial = mat;

    //     for (int i = 0; i < count; i++)
    //     {
    //         var go = new GameObject($"I_{i:0000}");
    //         go.transform.SetParent(transform, false);

    //         // Random distribution in a box around the spawner
    //         go.transform.localPosition = new Vector3(
    //             Random.Range(-area.x, area.x),
    //             Random.Range(0f, area.y),
    //             Random.Range(-area.z, area.z)
    //         );

    //         go.transform.localRotation = Random.rotation;
    //         go.transform.localScale = baseScale;

    //         var mf = go.AddComponent<MeshFilter>();
    //         mf.sharedMesh = mesh;

    //         var mr = go.AddComponent<MeshRenderer>();
    //         mr.sharedMaterial = mat; // IMPORTANT: sharedMaterial (no instancing via material copies)

    //         if (addColliders)
    //             go.AddComponent<BoxCollider>();

    //         if (markStatic)
    //             go.isStatic = true;
    //     }
    // }
    private void Spawn(Material mat)
    {
        if (mat == null)
        {
            Debug.LogError("[InstancingSpawner] Material is missing.");
            return;
        }

        // If already spawned, just swap materials (do NOT destroy & respawn)
        if (spawned != null && spawned.Length > 0)
        {
            currentMaterial = mat;

            for (int i = 0; i < spawned.Length; i++)
            {
                if (spawned[i] == null) continue;

                var mr = spawned[i].GetComponent<MeshRenderer>();
                if (mr != null)
                    mr.sharedMaterial = mat; // swap submission mode without reallocating objects
            }
            return;
        }

        // First-time spawn only
        spawned = new GameObject[count];
        currentMaterial = mat;

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"I_{i:0000}");
            go.transform.SetParent(transform, false);

            go.transform.localPosition = new Vector3(
                Random.Range(-area.x, area.x),
                Random.Range(0f, area.y),
                Random.Range(-area.z, area.z)
            );

            go.transform.localRotation = Random.rotation;
            go.transform.localScale = baseScale;

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat; // IMPORTANT: sharedMaterial

            if (addColliders)
                go.AddComponent<BoxCollider>();

            if (markStatic)
                go.isStatic = true;

            spawned[i] = go;
        }
    }
}