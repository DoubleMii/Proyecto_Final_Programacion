using UnityEngine;
using UnityEngine.InputSystem;
public class TestSfx : MonoBehaviour
{
    [SerializeField] private AudioClip testClip;

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            AudioManager.instance.PlaySfx(testClip);
        }
    }
}