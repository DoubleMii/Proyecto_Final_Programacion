using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    // Variables de movimiento y físicas
    // Variables de Input

    private CharacterController characterController;
    private Animator animator;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleMovement();
        HandleAnimations();
    }

    private void HandleMovement()
    {
        // Lógica de movimiento usando CharacterController
    }

    private void HandleAnimations()
    {
        // Actualizar Blend Trees en el Animator (Mecanim)
    }

    private void OnAnimatorIK(int layerIndex)
    {
        // Lógica de Inverse Kinematics (IK) para manos y pies
    }
}
