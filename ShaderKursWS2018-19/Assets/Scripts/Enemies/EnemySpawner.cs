using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    EnemyMovement[] enemies;    // pool of enemies


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    // Start is called before the first frame update
    void Awake()
    {
        enemies = new EnemyMovement[transform.childCount];
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i] = transform.GetChild(i).GetComponent<EnemyMovement>();
            enemies[i].DisableEnemy();
        }
    }

    // Called by maze when entering the room.
    // Spawns enemy out of the pool.
    public void SpawnEnemy(EnemySpawn spawn, RoomCoordinate room)
    {// check first if pool is ready
        // store the ready object
        int index = CheckPool();

        // continue if pool is ready
        if (index == -1)
        {
            return;
        }

        enemies[index].Spawn(spawn, room);
    }

    // check pool aviability
    // should be always ready, but for safety reasons
    int CheckPool()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].Lifetime <= 0)
            {
                return i;
            }
        }

        return -1;
    }

    // Called when leaving the room.
    // Decrease lifetime of all enemies.
    public void DecreaseEnemyLifetime()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].DecreaseLifetime();
        }
    }
}
