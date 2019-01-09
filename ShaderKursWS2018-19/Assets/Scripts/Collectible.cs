using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectibleType { Heart, Arrow, Sword, Bow, Barrier, Wings, WumpusSlayer }

public interface ICollectibleSpawnerToCollectible
{
    bool IsActive { get; }
    void Drop(Vector3 position, CollectibleType type);
    void DisableCollectible();
}

public interface IPlayerToCollectible
{
    CollectibleType PickUp();
}

public class Collectible : MonoBehaviour, ICollectibleSpawnerToCollectible, IPlayerToCollectible
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [SerializeField]
    [Tooltip("GameObject of the heart mesh.")]
    GameObject heart;
    [SerializeField]
    [Tooltip("GameObject of the arrow mesh.")]
    GameObject arrow;
    [SerializeField]
    [Tooltip("GameObject of the bow mesh.")]
    GameObject bow;
    [SerializeField]
    [Tooltip("GameObject of the barrier mesh.")]
    GameObject barrier;
    [SerializeField]
    [Tooltip("GameObject of the wings mesh.")]
    GameObject wings;
    [SerializeField]
    [Tooltip("GameObject of the wumpus slayer mesh.")]
    GameObject wumpusSlayer;

    CollectibleType type;                       // stores the type of collectible

    public bool IsActive { get; private set; }  // true if lootable, false when ready to be dropped


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    // Called when dropped
    public void Drop(Vector3 position, CollectibleType type)
    {
        // set drop zone
        transform.position = position;

        // activate the right mesh
        this.type = type;
        switch (type)
        {
            case CollectibleType.Heart:
                heart.SetActive(true);
                break;
            case CollectibleType.Arrow:
                arrow.SetActive(true);
                break;
            case CollectibleType.Bow:
                bow.SetActive(true);
                break;
            case CollectibleType.Barrier:
                barrier.SetActive(true);
                break;
            case CollectibleType.Wings:
                wings.SetActive(true);
                break;
            case CollectibleType.WumpusSlayer:
                wumpusSlayer.SetActive(true);
                break;
        }

        // set as lootable
        IsActive = true;
    }

    // called when player touches the collectible
    public CollectibleType PickUp()
    {
        // play the right pick up sound
        // TODO

        // play the right particle effect
        // TODO

        // disable mesh
        DisableCollectible();

        // tell player what has been picked up
        return type;
    }

    // called when leaving the room or picking up
    public void DisableCollectible()
    {
        // disable all meshes
        heart.SetActive(false);
        arrow.SetActive(false);
        bow.SetActive(false);
        barrier.SetActive(false);
        wings.SetActive(false);
        wumpusSlayer.SetActive(false);

        // set ready to be dropped
        IsActive = false;
    }
}