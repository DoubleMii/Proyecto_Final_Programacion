using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class NPCBuildIntegrator : MonoBehaviour
{
    private const int RequiredNpcCount = 3;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallForLoadedScene()
    {
        EnsureIntegrator();
    }

    private static void EnsureIntegrator()
    {
        if (FindAnyObjectByType<NPCBuildIntegrator>() != null)
            return;

        GameObject root = new GameObject("NPCBuildIntegrator");
        DontDestroyOnLoad(root);
        root.AddComponent<NPCBuildIntegrator>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        IntegrateCurrentScene();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IntegrateCurrentScene();
    }

    private void IntegrateCurrentScene()
    {
        NPCController[] npcs = FindObjectsByType<NPCController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < npcs.Length; i++)
        {
            if (npcs[i] == null) continue;

            npcs[i].gameObject.SetActive(true);
            npcs[i].ConfigureRole((NPCController.EnemyType)(i % RequiredNpcCount));
            EnsureGroundedAgent(npcs[i].gameObject);
            EnsureAnimation(npcs[i].gameObject);
        }

        for (int i = npcs.Length; i < RequiredNpcCount; i++)
        {
            CreateFallbackNpc((NPCController.EnemyType)(i % RequiredNpcCount), i);
        }
    }

    private void CreateFallbackNpc(NPCController.EnemyType type, int index)
    {
        Vector3 spawn = FindSpawnPosition(index);

        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = "Fallback_" + type;
        npc.transform.position = spawn;
        npc.transform.localScale = new Vector3(1f, 1.15f, 1f);

        NavMeshAgent agent = npc.AddComponent<NavMeshAgent>();
        agent.radius = 0.45f;
        agent.height = 2f;
        agent.baseOffset = 0f;
        agent.stoppingDistance = 1.25f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        NPCController controller = npc.AddComponent<NPCController>();
        controller.ConfigureRole(type);
        EnsureAnimation(npc);
    }

    private void EnsureGroundedAgent(GameObject target)
    {
        NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
        if (agent == null)
            return;

        agent.baseOffset = 0f;

        if (NavMesh.SamplePosition(target.transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            agent.Warp(hit.position);
    }

    private void EnsureAnimation(GameObject target)
    {
        if (target.GetComponent<RuntimeCharacterAnimation>() == null)
            target.AddComponent<RuntimeCharacterAnimation>();

        if (target.GetComponent<AudioSource>() == null)
            target.AddComponent<AudioSource>();

        if (target.GetComponent<RuntimeFootstepAudio>() == null)
            target.AddComponent<RuntimeFootstepAudio>();
    }

    private Vector3 FindSpawnPosition(int index)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 center = player != null ? player.transform.position : Vector3.zero;

        for (int attempt = 0; attempt < 12; attempt++)
        {
            float angle = (index * 120f) + attempt * 35f;
            float distance = 12f + attempt * 2f;
            Vector3 candidate = center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * distance;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 8f, NavMesh.AllAreas))
                return hit.position;
        }

        if (NavMesh.SamplePosition(center, out NavMeshHit fallbackHit, 30f, NavMesh.AllAreas))
            return fallbackHit.position;

        return center + new Vector3(index * 2f, 0f, 8f);
    }
}
