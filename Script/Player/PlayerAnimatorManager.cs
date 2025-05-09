using UnityEngine;

public class PlayerAnimatorManager : MonoBehaviour
{
    public Animator animator;

    int move;
    public bool isUseWeapon;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        move = Animator.StringToHash("Move");
        animator.SetBool("isUseWeapon", isUseWeapon);
        animator.Play("Use_Weapon");
    }

    public void PlayTargetAnimation(string targetAnimation, bool isInteracting)
    {
        animator.SetBool("isInteracting", isInteracting);
        animator.CrossFade(targetAnimation, 0.2f);
    }

    public void PlayLobbyInteractionAnimation(string animName, bool isExit)
    {
        animator.Play(animName);
        animator.SetBool("isExitInteract", isExit);
    }

    public void ExitInteractionAnimation()
    {
        animator.SetBool("isExitInteract", true);
    }

    public void UpdateAnimatorValues(float moveValue)
    {
        float snapperMove;

        if (moveValue > 0.5)
        {
            snapperMove = 1;
        }
        else
        {
            snapperMove = 0;
        }

        animator.SetFloat(move, snapperMove, 0.1f, Time.deltaTime);
    }

    public void StopMoveAnimation()
    {
        animator.SetFloat(move, 0);
    }

    public void SetToggleWeapon()
    {
        isUseWeapon = animator.GetBool("isUseWeapon");
        isUseWeapon = !isUseWeapon;

        animator.SetBool("isUseWeapon", isUseWeapon);

        if (isUseWeapon)
        {
            animator.Play("Use_Weapon");
        }
        else
        {
            animator.Play("Disarm_Weapon");
        }
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
    }

    public void Dance(bool isDancing)
    {
        animator.SetBool("isDancing", isDancing);

        if (isDancing)
            animator.Play("Dance");

    }

    public void CleanUp(bool isCleanUp)
    {
        animator.SetBool("isCleanUp", isCleanUp);

        if (isCleanUp)
            animator.Play("CleanUpStart");
    }

    public void Stun()
    {
        animator.Play("StartStun");
    }

    public void Stunning(bool isStunning)
    {
        animator.SetBool("isStunning", isStunning);

        if (isStunning)
            animator.Play("Stunning");
        else
        {
            animator.Play("StunFinish");
        }
    }

    public void Jump()
    {
        animator.SetBool("isJumping", true);
        animator.Play("Jump");
    }

    public void Rolling()
    {
        PlayTargetAnimation("Rolling", false);
    }

    public void SetBoolRolling(bool isRolling)
    {
        animator.SetBool("isRolling", isRolling);
    }

    public void SetBoolHolding(bool isHolding)
    {
        animator.SetBool("isHolding", isHolding);
    }
}
