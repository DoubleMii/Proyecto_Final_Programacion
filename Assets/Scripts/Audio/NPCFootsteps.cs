using UnityEngine;
using UnityEngine.AI;

public class NPCFootsteps : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private Transform footPoint;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private AudioClip[] woodSteps;
    [SerializeField] private AudioClip[] metalSteps;
    [SerializeField] private AudioClip[] waterSteps;
    [SerializeField] private AudioClip[] stairSteps;
    [SerializeField] private AudioClip[] defaultSteps;

    private NavMeshAgent agent;

    private SurfaceType currentSurface;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void PlayFootstep()
    {
        if (agent.velocity.magnitude < 0.1f)
            return;

        DetectSurface();

        AudioClip clip = GetClip();

        if (clip != null)
        {
            AudioManager.instance.PlaySfxAtSource(audioSource, clip);
        }
    }

    void DetectSurface()
    {
        RaycastHit hit;

        if (Physics.Raycast(
            footPoint.position,
            Vector3.down,
            out hit,
            1.5f,
            groundMask))
        {
            SurfaceIdentifier surface =
                hit.collider.GetComponent<SurfaceIdentifier>();

            if (surface != null)
            {
                currentSurface = surface.surfaceType;
                return;
            }
        }

        currentSurface = SurfaceType.Default;
    }

    AudioClip GetClip()
    {
        AudioClip[] clips = defaultSteps;

        switch (currentSurface)
        {
            case SurfaceType.Wood:
                clips = woodSteps;
                break;

            case SurfaceType.Metal:
                clips = metalSteps;
                break;
        }

        if (clips.Length == 0)
            return null;

        return clips[Random.Range(0, clips.Length)];
    }
}