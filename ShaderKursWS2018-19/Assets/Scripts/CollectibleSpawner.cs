using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyToCollectibleSpawner
{
    void Drop(Vector3 position);
    void Drop(Vector3 position, CollectibleType type);
}

public interface IMazeToCollectibleSpawner
{
    void DisableCollectibles();
    void Drop(Vector3 position, CollectibleType type);
}

public class CollectibleSpawner : MonoBehaviour, IEnemyToCollectibleSpawner, IMazeToCollectibleSpawner
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [SerializeField]
    [Tooltip("Stats component of the player.")]
    PlayerStats playerObject;
    [SerializeField]
    [Tooltip("The probability a collectible will be dropped.")]
    [Range(0, 1)]
    float probability = .5f;

    ICollectibleSpawnerToPlayerStats player;
    ICollectibleSpawnerToCollectible[] collectibles;    // array of the collectible pool


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    // Start is called before the first frame update
    void Awake()
    {
        player = playerObject;
        playerObject = null;

        collectibles = new ICollectibleSpawnerToCollectible[transform.childCount];
        for (int i = 0; i < collectibles.Length; i++)
        {
            collectibles[i] = transform.GetChild(i).GetComponent<Collectible>();
            collectibles[i].DisableCollectible();
        }
    }

    // called when leaving the room
    public void DisableCollectibles()
    {
        for (int i = 0; i < collectibles.Length; i++)
        {
            collectibles[i].DisableCollectible();
        }
    }

    // check pool aviability
    // should be always ready, but for safety reasons
    int CheckPool()
    {
        for (int i = 0; i < collectibles.Length; i++)
        {
            if (!collectibles[i].IsActive)
            {
                return i;
            }
        }
        
        return -1;
    }

    // called when enemy iz ded lol
    public void Drop(Vector3 position)
    {
        // check first if pool is ready
        // store the ready object
        int index = CheckPool();

        // continue if pool is ready
        if (index == -1)
        {
            return;
        }

        // check if something will be dropped
        if (Random.Range(0, 1) > probability)
        {
            return;
        }

        // check if player already has a bow
        // drop heart if not
        if (!player.BowCollected)
        {
            collectibles[index].Drop(position, CollectibleType.Heart);
        }

        // check if player needs a heart
        // make heart probability larger
        float heartProbability = (4 - player.Health) * .25f;

        // drop heart if randomizer is lower
        if(Random.Range(0,1) <= heartProbability)
        {
            collectibles[index].Drop(position, CollectibleType.Heart);
        }
        else
        {
            // drop arrow otherwise
            collectibles[index].Drop(position, CollectibleType.Arrow);
        }
    }

    // is called when dropping an artefact
    public void Drop(Vector3 position, CollectibleType type)
    {
        // check first if pool is ready
        // store the ready object
        int index = CheckPool();

        // continue if pool is ready
        if (index == -1)
        {
            return;
        }

        // check for bow
        // if player has already a bow, drop standard
        if(type == CollectibleType.Bow && player.BowCollected)
        {
            Drop(position);
        }
        else
        {
            // drop desired type otherwise
            collectibles[index].Drop(position, type);
        }
    }
}