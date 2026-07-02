using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SimpleFootIK : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float rayStartHeight = 0.45f;
    [SerializeField] private float rayDistance = 1.2f;
    [SerializeField] private float footHeightOffset = 0.04f;
    [SerializeField] private float ikWeight = 0.85f;

    private Animator _animator;
    private Transform _ignoredRoot;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _ignoredRoot = transform.root;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator == null || !_animator.isHuman)
            return;

        ApplyFootIK(AvatarIKGoal.LeftFoot);
        ApplyFootIK(AvatarIKGoal.RightFoot);
    }

    private void ApplyFootIK(AvatarIKGoal foot)
    {
        Vector3 footPosition = _animator.GetIKPosition(foot);
        Vector3 rayOrigin = footPosition + Vector3.up * rayStartHeight;

        if (!TryFindGround(rayOrigin, out RaycastHit hit))
        {
            _animator.SetIKPositionWeight(foot, 0f);
            _animator.SetIKRotationWeight(foot, 0f);
            return;
        }

        Vector3 targetPosition = hit.point + Vector3.up * footHeightOffset;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

        _animator.SetIKPositionWeight(foot, ikWeight);
        _animator.SetIKRotationWeight(foot, ikWeight);
        _animator.SetIKPosition(foot, targetPosition);
        _animator.SetIKRotation(foot, targetRotation);
    }

    private bool TryFindGround(Vector3 rayOrigin, out RaycastHit bestHit)
    {
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, rayDistance, groundMask, QueryTriggerInteraction.Ignore);
        bestHit = default;
        float bestDistance = float.MaxValue;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(_ignoredRoot))
                continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                bestHit = hit;
                found = true;
            }
        }

        return found;
    }
}
