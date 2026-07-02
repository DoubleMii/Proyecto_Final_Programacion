using UnityEngine;
using UnityEngine.AI;

public static class RuntimeBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateEssentialManagers()
    {
        GameObject root = new GameObject("RuntimeBootstrapper");
        Object.DontDestroyOnLoad(root);

        if (GameManager.Instance == null)
            root.AddComponent<GameManager>();

        if (PersistenceManager.Instance == null)
        {
            PersistenceManager persistence = root.AddComponent<PersistenceManager>();
            persistence.autoSaveInterval = 0f;
        }

        if (Object.FindAnyObjectByType<BuildStabilityChecker>() == null)
            root.AddComponent<BuildStabilityChecker>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateFallbackMusic()
    {
        EnsureAudioManager();
        EnsurePlayerAnimation();
        EnsureFootIKForHumanoids();

        if (Object.FindAnyObjectByType<AdaptiveMusic>() != null)
            return;

        GameObject root = new GameObject("FallbackAdaptiveMusic");
        Object.DontDestroyOnLoad(root);
        EnsureAdaptiveMusic(root);
    }

    private static void EnsureAdaptiveMusic(GameObject root)
    {
        if (Object.FindAnyObjectByType<AdaptiveMusic>() != null)
            return;

        AudioSource source = root.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = 0.35f;

        root.AddComponent<AdaptiveMusic>().Configure(source, null, null);
    }

    private static void EnsureAudioManager()
    {
        if (Object.FindAnyObjectByType<AudioManager>() != null)
            return;

        GameObject root = new GameObject("FallbackAudioManager");
        Object.DontDestroyOnLoad(root);
        root.AddComponent<AudioManager>();
    }

    private static void EnsurePlayerAnimation()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.skinWidth = 0.08f;
        }

        SnapPlayerToGround(player, controller);
        GroundVisualModel(player);

        if (player.GetComponent<RuntimeCharacterAnimation>() == null)
            player.AddComponent<RuntimeCharacterAnimation>();

        if (player.GetComponent<AudioSource>() == null)
            player.AddComponent<AudioSource>();

        if (player.GetComponent<RuntimeFootstepAudio>() == null)
            player.AddComponent<RuntimeFootstepAudio>();
    }

    private static void EnsureFootIKForHumanoids()
    {
        Animator[] animators = Object.FindObjectsByType<Animator>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Animator animator in animators)
        {
            if (animator == null || animator.avatar == null || !animator.isHuman)
                continue;

            if (animator.runtimeAnimatorController == null)
                continue;

            if (animator.GetComponent<SimpleFootIK>() == null)
                animator.gameObject.AddComponent<SimpleFootIK>();
        }
    }

    private static void SnapPlayerToGround(GameObject player, CharacterController controller)
    {
        if (TryFindGround(player.transform.position, player.transform, out Vector3 groundPosition))
        {
            bool wasEnabled = controller != null && controller.enabled;
            if (controller != null) controller.enabled = false;

            player.transform.position = new Vector3(
                player.transform.position.x,
                groundPosition.y,
                player.transform.position.z
            );

            if (controller != null) controller.enabled = wasEnabled;
        }
    }

    private static void GroundVisualModel(GameObject player)
    {
        Animator visualAnimator = player.GetComponent<PlayerController>()?.Animator ?? player.GetComponentInChildren<Animator>(true);
        if (visualAnimator == null || visualAnimator.transform == player.transform)
            return;

        Renderer[] renderers = visualAnimator.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        if (!TryFindGround(player.transform.position, player.transform, out Vector3 groundPosition))
            groundPosition = player.transform.position;

        float verticalError = groundPosition.y - bounds.min.y;
        if (Mathf.Abs(verticalError) < 0.04f || Mathf.Abs(verticalError) > 3f)
            return;

        Vector3 localPosition = visualAnimator.transform.localPosition;
        localPosition.y += verticalError;
        visualAnimator.transform.localPosition = localPosition;
    }

    private static bool TryFindGround(Vector3 origin, Transform ignoredRoot, out Vector3 groundPosition)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin + Vector3.up * 8f, Vector3.down, 30f, ~0, QueryTriggerInteraction.Ignore);
        float bestDistance = float.MaxValue;
        groundPosition = origin;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(ignoredRoot))
                continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                groundPosition = hit.point;
                found = true;
            }
        }

        if (found)
            return true;

        if (NavMesh.SamplePosition(origin, out NavMeshHit navHit, 6f, NavMesh.AllAreas))
        {
            groundPosition = navHit.position;
            return true;
        }

        return false;
    }
}
