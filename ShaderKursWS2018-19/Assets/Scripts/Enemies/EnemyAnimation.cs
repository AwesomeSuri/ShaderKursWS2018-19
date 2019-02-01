using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [SerializeField]
    [Tooltip("Animator of the swordsman mesh.")]
    Animator animSwordsman;
    [SerializeField]
    [Tooltip("Animator of the swordsman mesh.")]
    Animator animHunter;
    [SerializeField]
    [Tooltip("Animator of the swordsman mesh.")]
    Animator animBat;

    [Space]
    [SerializeField]
    [Tooltip("Collider of the weapon.")]
    Collider weaponCollider;
    [SerializeField]
    [Tooltip("Wait before next attack.")]
    float wait = 5;

    Animator anim;                                  // animator that is currently used
    float attackTimer;                              // timer for the wait

    public bool IsAttacking { get; private set; }   // true if enemy is currently in attack animation


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    public void SetAnimation(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Swordsman:
                anim = animSwordsman;
                break;
            case EnemyType.Hunter:
                anim = animHunter;
                break;
            case EnemyType.Bat:
                anim = animBat;
                break;
        }

        if(weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

    public void UpdateMovement(float x, float y)
    {
        //anim.SetFloat("SpeedX", x);
        //anim.SetFloat("SpeedY", y);
    }

    private void Update()
    {
        if(IsAttacking)
        attackTimer -= Time.deltaTime;
    }

    public bool StartAttack()
    {
        if(anim.parameterCount > 2)
        {
            IsAttacking = true;

            if (attackTimer < 0)
            {
                anim.SetTrigger("Attack");
                attackTimer = wait;
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public void EndAttack()
    {
        IsAttacking = false;
    }
}