using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class RuntimeEnvironmentBlockers : MonoBehaviour
{
    [SerializeField] private float minBlockerHeight = 0.7f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindAnyObjectByType<RuntimeEnvironmentBlockers>() != null)
            return;

        GameObject root = new GameObject("RuntimeEnvironmentBlockers");
        DontDestroyOnLoad(root);
        root.AddComponent<RuntimeEnvironmentBlockers>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        BuildBlockers();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BuildBlockers();
    }

    private void BuildBlockers()
    {
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Renderer rend in renderers)
        {
            if (!IsSceneBlocker(rend))
                continue;

            GameObject target = rend.gameObject;

            if (target.GetComponent<Collider>() == null)
                AddCollider(target, rend);

            NavMeshObstacle obstacle = target.GetComponent<NavMeshObstacle>();
            if (obstacle == null)
                obstacle = target.AddComponent<NavMeshObstacle>();

            obstacle.carving = true;
            obstacle.carveOnlyStationary = true;
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = target.transform.InverseTransformPoint(rend.bounds.center);
            obstacle.size = LocalSizeFromWorldBounds(target.transform, rend.bounds.size);
        }
    }

    private bool IsSceneBlocker(Renderer rend)
    {
        if (rend == null || rend is SkinnedMeshRenderer)
            return false;

        GameObject go = rend.gameObject;
        if (go.CompareTag("Player") || go.CompareTag("Enemy"))
            return false;

        if (go.GetComponentInParent<PlayerController>() != null ||
            go.GetComponentInParent<NPCController>() != null ||
            go.GetComponentInParent<NavMeshAgent>() != null)
            return false;

        if (go.GetComponentInParent<Canvas>() != null)
            return false;

        string n = go.name.ToLowerInvariant();
        if (n.Contains("ground") || n.Contains("piso") || n.Contains("floor") ||
            n.Contains("ramp") || n.Contains("stairs") || n.Contains("salto") ||
            n.Contains("wayzone") || n.Contains("deathzone"))
            return false;

        Bounds bounds = rend.bounds;
        if (bounds.size.y < minBlockerHeight)
            return false;

        return bounds.size.x > 0.2f && bounds.size.z > 0.2f;
    }

    private void AddCollider(GameObject target, Renderer rend)
    {
        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            BoxCollider box = target.AddComponent<BoxCollider>();
            box.center = meshFilter.sharedMesh.bounds.center;
            box.size = meshFilter.sharedMesh.bounds.size;
            box.isTrigger = true;
            return;
        }

        BoxCollider fallback = target.AddComponent<BoxCollider>();
        fallback.center = target.transform.InverseTransformPoint(rend.bounds.center);
        fallback.size = LocalSizeFromWorldBounds(target.transform, rend.bounds.size);
        fallback.isTrigger = true;
    }

    private Vector3 LocalSizeFromWorldBounds(Transform target, Vector3 worldSize)
    {
        Vector3 scale = target.lossyScale;
        return new Vector3(
            SafeDivide(worldSize.x, scale.x),
            SafeDivide(worldSize.y, scale.y),
            SafeDivide(worldSize.z, scale.z)
        );
    }

    private float SafeDivide(float value, float divisor)
    {
        return Mathf.Abs(divisor) > 0.0001f ? value / Mathf.Abs(divisor) : value;
    }
}
