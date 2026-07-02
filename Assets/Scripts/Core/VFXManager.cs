using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    [SerializeField] private GameObject _dustParticles;
    [SerializeField] private GameObject _bloodParticles;
    [SerializeField] private GameObject _alertParticles;

    private static Material _runtimeParticleMaterial;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayAlert(Vector3 position)
    {
        if (_alertParticles != null)
        {
            Instantiate(_alertParticles, position, Quaternion.identity);
            return;
        }

        SpawnParticles(position, Color.yellow, 10, 0.45f, 1.4f, 2.5f);
    }

    public void PlayBlood(Vector3 position)
    {
        if (_bloodParticles != null)
        {
            Instantiate(_bloodParticles, position, Quaternion.identity);
            return;
        }

        SpawnParticles(position, new Color(0.85f, 0.02f, 0.02f), 18, 0.55f, 1.1f, 1.8f);
    }

    public void PlayDust(Vector3 position)
    {
        if (_dustParticles != null)
        {
            Instantiate(_dustParticles, position, Quaternion.identity);
            return;
        }

        SpawnParticles(position + Vector3.up * 0.05f, new Color(0.55f, 0.5f, 0.42f, 0.45f), 6, 0.22f, 0.12f, 0.28f);
    }

    private void SpawnParticles(Vector3 position, Color color, int burstCount, float lifetime, float size, float speed)
    {
        GameObject fx = new GameObject("RuntimeVFX");
        fx.transform.position = position;

        ParticleSystem particles = fx.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = lifetime;
        main.startSize = size;
        main.startSpeed = speed;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.15f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = Mathf.Max(size * 0.45f, 0.03f);

        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = GetRuntimeParticleMaterial(color);

        particles.Play();
        Destroy(fx, lifetime + 0.5f);
    }

    private Material GetRuntimeParticleMaterial(Color color)
    {
        if (_runtimeParticleMaterial == null)
        {
            Shader shader =
                Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("Standard");

            _runtimeParticleMaterial = new Material(shader);
            _runtimeParticleMaterial.name = "RuntimeParticleMaterial";
        }

        if (_runtimeParticleMaterial.HasProperty("_BaseColor"))
            _runtimeParticleMaterial.SetColor("_BaseColor", color);
        if (_runtimeParticleMaterial.HasProperty("_Color"))
            _runtimeParticleMaterial.SetColor("_Color", color);

        return _runtimeParticleMaterial;
    }
}
