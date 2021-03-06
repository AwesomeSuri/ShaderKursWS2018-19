﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Equipment { None, Sword, Bow, Barrier, Wings }

public interface IGameManagerToPlayerStats
{
    bool PlayerActive { get; set; }
    void PickUp(CollectibleType type);
}

public interface ICollectibleSpawnerToPlayerStats
{
    int Health { get; }
    int ArrowAmount { get; }
    bool BowCollected { get; }
}

public interface IUIToPlayerStats
{
    void Equip(Equipment equipment);
}

public interface IEnemyToPlayer
{
    bool PlayerActive { get; }
}

public class PlayerStats : MonoBehaviour,
    ICollectibleSpawnerToPlayerStats, IUIToPlayerStats, IGameManagerToPlayerStats, IEnemyToPlayer
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [SerializeField]
    [Tooltip("Controller of the UI.")]
    LevelUIController levelUIObject;
    [SerializeField]
    [Tooltip("Mesh of the sword.")]
    GameObject sword;
    [SerializeField]
    [Tooltip("Mesh of the bow.")]
    GameObject bow;
    [SerializeField]
    [Tooltip("Mesh of the barrier.")]
    GameObject barrierEars;
    [SerializeField]
    [Tooltip("Mesh of the wings.")]
    GameObject wings;
    [SerializeField]
    Material barrier;

    [Space]
    [SerializeField]
    SoundEffectPlayer sfx;

    IPlayerStatsToUI levelUI;                               // interface between this script and the ui

    public int Health { get; private set; }                 // stores the amount of hearts
    public int ArrowAmount { get; private set; }            // stores the amount of arrows
    public bool BowCollected { get; private set; }          // true if bow is already collected
    public bool WumpusSlayer { get; private set; }          // true if wumpusSlayer is already collected
    public Equipment CurrentEquipment { get; private set; } // says what item is currently used
    public bool PlayerActive { get; set; }                  // true if player can controll this object
    public bool RoomTranfering { get; private set; }        // true if player moves to another room
    public float InvincibleTimer { get; private set; }      // time after getting hit

    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    // Start is called before the first frame update
    void Awake()
    {
        levelUI = levelUIObject;
        levelUIObject = null;

        Health = 3;
        ArrowAmount = 0;
        BowCollected = false;
        WumpusSlayer = false;
        PlayerActive = false;
    }

    void Start()
    {
        sword.SetActive(false);
        bow.SetActive(false);
        barrierEars.SetActive(false);
        wings.SetActive(false);

        barrier.SetInt("_BarrierActive", 0);
    }

    // Called by player movement transfering room. 
    public void SetRoomTransfering(bool transfering)
    {
        RoomTranfering = transfering;
        levelUI.SetItemInteractable(!transfering);
    }

    // Add stat of the collectible.
    public void PickUp(CollectibleType type)
    {
        switch (type)
        {
            case CollectibleType.Heart:
                if (Health < 3)
                {
                    Health++;
                    levelUI.UpdateHearts(Health);
                }

                sfx.PlayAudio("Blop");

                break;

            case CollectibleType.Arrow:
                if (ArrowAmount < 15)
                {
                    ArrowAmount += 3;
                    if (ArrowAmount > 15)
                    {
                        ArrowAmount = 15;
                    }

                    levelUI.UpdateArrows(ArrowAmount);
                }

                sfx.PlayAudio("Wood");

                break;

            case CollectibleType.Bow:
                BowCollected = true;
                ArrowAmount = 15;
                levelUI.ActivateBow();

                sfx.PlayAudio("Artefact");

                break;

            case CollectibleType.Barrier:
                levelUI.ActivateBarrier();

                sfx.PlayAudio("Artefact");

                break;

            case CollectibleType.Wings:
                levelUI.ActivateWings();

                sfx.PlayAudio("Artefact");

                break;

            case CollectibleType.WumpusSlayer:
                WumpusSlayer = true;
                print("WumpusSlayer");

                sfx.PlayAudio("Artefact");

                // TODO: change appeareance
                break;

            case CollectibleType.Sword:
                levelUI.ActivateSword();

                sfx.PlayAudio("Artefact");

                break;
        }
    }


    // UI /----------------------------------------------------------------------------------------//
    // When the button is pressed.
    public void Equip(Equipment equipment)
    {
        CurrentEquipment = equipment;
        barrier.SetInt("_BarrierActive", 0);

        switch (CurrentEquipment)
        {
            case Equipment.Sword:
                sword.SetActive(true);
                bow.SetActive(false);
                barrierEars.SetActive(false);
                wings.SetActive(false);

                sfx.PlayAudio("WhooshMetal");
                break;
            case Equipment.Bow:
                sword.SetActive(false);
                bow.SetActive(true);
                barrierEars.SetActive(false);
                wings.SetActive(false);

                sfx.PlayAudio("Wood");
                break;
            case Equipment.Barrier:
                sword.SetActive(false);
                bow.SetActive(false);
                barrierEars.SetActive(true);
                wings.SetActive(false);
                barrier.SetInt("_BarrierActive", 1);
                break;
            case Equipment.Wings:
                sword.SetActive(false);
                bow.SetActive(false);
                barrierEars.SetActive(false);
                wings.SetActive(true);
                break;
        }
    }


    // Hit /---------------------------------------------------------------------------------------//
    // Player is invincible after getting hit.
    public void GetHit()
    {
        Health--;
        levelUI.UpdateHearts(Health);

        InvincibleTimer = Time.time + .2f;
    }

    public void ResetStats()
    {
        Health = 3;
        levelUI.UpdateHearts(Health);
    }

    public void Shoot()
    {
        ArrowAmount--;
        levelUI.UpdateArrows(ArrowAmount);
    }
}