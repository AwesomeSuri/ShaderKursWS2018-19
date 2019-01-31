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

    Animator anim;      // animator that is currently used


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
    }

    public void UpdateMovement(float x, float y)
    {
        //anim.SetFloat("SpeedX", x);
        //anim.SetFloat("SpeedY", y);
    }
}