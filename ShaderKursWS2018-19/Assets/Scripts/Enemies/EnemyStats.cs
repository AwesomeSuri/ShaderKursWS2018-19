using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [SerializeField]
    [Tooltip("Start health of a Swordsman.")]
    int startHealthSwordsman;
    [SerializeField]
    [Tooltip("Start health of a Hunter.")]
    int startHealthHunter;
    [SerializeField]
    [Tooltip("Start health of a Bat.")]
    int startHealthBat;

    public int Health { get; private set; }             // current health of this enemy
    public float InvincibleTimer { get; private set; }  // the time left where the enemy is invincible


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    // Called when enemy is spawned.
    public void SetStats(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Swordsman:
                Health = startHealthSwordsman;
                break;
            case EnemyType.Hunter:
                Health = startHealthHunter;
                break;
            case EnemyType.Bat:
                Health = startHealthBat;
                break;
        }
    }

    public void GetHit()
    {
        Health--;
        InvincibleTimer = Time.time + ((Health <= 0) ? .5f : .2f);
    }
}