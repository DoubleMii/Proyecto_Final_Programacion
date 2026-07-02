using UnityEngine;
using UnityEngine.SceneManagement;

public class RuntimeMaterialRepair : MonoBehaviour
{
    private const string UrpLitShaderName = "Universal Render Pipeline/Lit";
    private const string UrpUnlitShaderName = "Universal Render Pipeline/Unlit";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindAnyObjectByType<RuntimeMaterialRepair>() != null)
            return;

        GameObject root = new GameObject("RuntimeMaterialRepair");
        DontDestroyOnLoad(root);
        RuntimeMaterialRepair repair = root.AddComponent<RuntimeMaterialRepair>();
        repair.RepairSceneMaterials();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RepairSceneMaterials();
    }

    public void RepairSceneMaterials()
    {
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Shader fallbackShader = Shader.Find(UrpLitShaderName);
        if (fallbackShader == null)
            fallbackShader = Shader.Find(UrpUnlitShaderName);

        if (fallbackShader == null)
            return;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            Material[] materials = renderer.materials;
            bool changed = false;

            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null || IsSupported(material))
                    continue;

                RepairMaterial(material, fallbackShader);
                changed = true;
            }

            if (changed)
                renderer.materials = materials;
        }
    }

    private bool IsSupported(Material material)
    {
        if (material.shader == null)
            return false;

        string shaderName = material.shader.name;
        if (shaderName == "Hidden/InternalErrorShader")
            return false;

        return material.shader.isSupported;
    }

    private void RepairMaterial(Material material, Shader fallbackShader)
    {
        Color color = Color.white;
        Texture mainTexture = null;

        if (material.HasProperty("_BaseColor"))
            color = material.GetColor("_BaseColor");
        else if (material.HasProperty("_Color"))
            color = material.GetColor("_Color");

        if (material.HasProperty("_BaseMap"))
            mainTexture = material.GetTexture("_BaseMap");
        else if (material.HasProperty("_MainTex"))
            mainTexture = material.GetTexture("_MainTex");

        material.shader = fallbackShader;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
        if (mainTexture != null && material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", mainTexture);
        if (mainTexture != null && material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", mainTexture);
    }
}
