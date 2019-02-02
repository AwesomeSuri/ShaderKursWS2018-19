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
    [Tooltip("Bow mesh.")]
    BowController bow;
    [SerializeField]
    [Tooltip("Collider of the weapon.")]
    Collider weaponCollider;
    [SerializeField]
    [Tooltip("Wait before next attack.")]
    float wait = 5;

    [Space]
    [SerializeField]
    [Tooltip("Arrow at the hand.")]
    GameObject arrow;
    [SerializeField]
    [Tooltip("Pool that spawns and shoots the arrows.")]
    ArrowSpawner spawner;
    [SerializeField]
    [Tooltip("Position where the arrow shoots from.")]
    Transform origin;

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
        anim.SetFloat("SpeedX", x);
        anim.SetFloat("SpeedY", y);
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

    public IEnumerator AimingAndShooting()
    {
        IsAttacking = true;

        anim.SetBool("Aiming", true);
        bow.BendBow(true);
        arrow.SetActive(true);

        yield return new WaitForSeconds(wait);

        spawner.Shoot(origin, 1);

        anim.SetBool("Aiming", false);
        bow.BendBow(false);
        arrow.SetActive(false);

        yield return new WaitForSeconds(1);

        IsAttacking = false;
    }
}