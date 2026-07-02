using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class ZombieRuntimeSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindAnyObjectByType<ZombieRuntimeSetup>() != null)
            return;

        GameObject root = new GameObject("ZombieRuntimeSetup");
        DontDestroyOnLoad(root);
        root.AddComponent<ZombieRuntimeSetup>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        SetupZombies();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupZombies();
    }

    private void SetupZombies()
    {
        Animator[] animators = FindObjectsByType<Animator>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Animator animator in animators)
        {
            if (animator == null || !IsZombie(animator.gameObject))
                continue;

            GameObject zombie = animator.gameObject;
            zombie.SetActive(true);
            animator.enabled = true;

            EnsureEnemyTag(zombie);
            EnsureCapsuleCollider(zombie);
            EnsureNavMeshAgent(zombie);
            EnsureNpcController(zombie);
            EnsureRuntimeAnimation(zombie);
        }
    }

    private bool IsZombie(GameObject target)
    {
        Transform current = target.transform;
        while (current != null)
        {
            if (current.name.ToLowerInvariant().Contains("zombie"))
                return true;

            current = current.parent;
        }

        return false;
    }

    private void EnsureEnemyTag(GameObject zombie)
    {
        try
        {
            zombie.tag = "Enemy";
        }
        catch
        {
            // Si el tag no existe en el proyecto, la IA sigue funcionando igualmente.
        }
    }

    private void EnsureCapsuleCollider(GameObject zombie)
    {
        if (zombie.GetComponent<Collider>() != null)
            return;

        CapsuleCollider collider = zombie.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0f, 1f, 0f);
        collider.height = 2f;
        collider.radius = 0.45f;
    }

    private void EnsureNavMeshAgent(GameObject zombie)
    {
        NavMeshAgent agent = zombie.GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = zombie.AddComponent<NavMeshAgent>();

        agent.radius = 0.45f;
        agent.height = 2f;
        agent.baseOffset = 0f;
        agent.speed = 2.2f;
        agent.angularSpeed = 300f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 1.25f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        if (NavMesh.SamplePosition(zombie.transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            agent.Warp(hit.position);
    }

    private void EnsureNpcController(GameObject zombie)
    {
        NPCController controller = zombie.GetComponent<NPCController>();
        if (controller == null)
            controller = zombie.AddComponent<NPCController>();

        controller.ConfigureRole(NPCController.EnemyType.Hunter);
    }

    private void EnsureRuntimeAnimation(GameObject zombie)
    {
        if (zombie.GetComponent<RuntimeCharacterAnimation>() == null)
            zombie.AddComponent<RuntimeCharacterAnimation>();

        if (zombie.GetComponent<AudioSource>() == null)
            zombie.AddComponent<AudioSource>();

        if (zombie.GetComponent<RuntimeFootstepAudio>() == null)
            zombie.AddComponent<RuntimeFootstepAudio>();
    }
}
