using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(UnitGridMovement))]
public class CharacterAnimator : MonoBehaviour
{
    private Animator animator;
    private UnitGridMovement movement;

    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<UnitGridMovement>();
    }

    void Update()
    {
        animator.SetFloat("speed", movement.IsMoving ? 1f : 0f);
    }
}
