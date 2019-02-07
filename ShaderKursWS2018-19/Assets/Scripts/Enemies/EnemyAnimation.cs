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
    Collider sword;
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

    [Space]
    [SerializeField]
    SoundEffectPlayer sfx;

    Animator anim;                                  // animator that is currently used

    public bool IsAttacking { get; private set; }   // true if enemy is currently in attack animation

    Vector3 swordIdlePos;                               // bug workaround
    Vector3 swordIdleRot;


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

        if (sword != null)
        {
            sword.enabled = false;
        }

        anim.Play("Idle");
        IsAttacking = false;
    }

    public void UpdateMovement(float x, float y)
    {
        anim.SetFloat("SpeedX", x);
        anim.SetFloat("SpeedY", y);
    }

    public IEnumerator Swinging()
    {
        IsAttacking = true;

        anim.Play("Swing", 1);
        sfx.PlayAudio("WhooshMetal");

        float weight = 0;
        while (anim.GetLayerWeight(1) < 1)
        {
            weight += Time.deltaTime * 20;

            sword.transform.localScale = Vector3.one * (1 + weight * .5f);
            anim.SetLayerWeight(1, weight);

            yield return null;
        }

        sword.enabled = true;

        yield return new WaitForSeconds(.2f);

        sword.enabled = false;

        while (anim.GetLayerWeight(1) > 0)
        {
            weight -= Time.deltaTime * 10;

            sword.transform.localScale = Vector3.one * (1 + weight * .5f);
            anim.SetLayerWeight(1, weight);

            yield return null;
        }

        yield return new WaitForSeconds(wait);

        sword.transform.localPosition = swordIdlePos;
        sword.transform.localEulerAngles = swordIdleRot;

        IsAttacking = false;
    }

    public IEnumerator AimingAndShooting()
    {
        IsAttacking = true;

        anim.SetBool("Aiming", true);
        bow.BendBow(true);
        arrow.SetActive(true);
        sfx.PlayAudio("BowReady");

        yield return new WaitForSeconds(wait);

        spawner.Shoot(origin, 1);

        anim.SetBool("Aiming", false);
        bow.BendBow(false);
        arrow.SetActive(false);
        sfx.PlayAudio("BowFire");

        yield return new WaitForSeconds(1);

        IsAttacking = false;
    }

    public void Die()
    {
        StopAllCoroutines();

        anim.Play("Dying");
    }
}