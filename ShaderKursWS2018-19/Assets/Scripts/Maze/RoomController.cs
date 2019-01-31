using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RoomCoordinate
{
    public int x;
    public int y;
}

public enum RoomType { Standard, Pit, Lava, Wumpus}

public interface IMazeToRoom
{
    RoomType GetRoomType();
    void ActivateRoom();
    void DeactivateRoom();

    EnemySpawn[] EnemySpawns { get; }

    CollectibleType GetCollectibleType();
}

public class RoomController : MonoBehaviour, IMazeToRoom
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [SerializeField]
    [Tooltip("Describes the type of the room.")]
    RoomType roomType = RoomType.Standard;
    [SerializeField]
    [Tooltip("Parent object of all static objects.")]
    GameObject staticObjects;
    [SerializeField]
    [Tooltip("Parent object of all dynamic objects.")]
    GameObject dynamicObjects;
    [SerializeField]
    [Tooltip("Parent object of all enemy spawn objects.")]
    GameObject enemySpawns;

    [Space]
    [SerializeField]
    [Tooltip("Drop an artefact in the middle. (For debug use)")]
    CollectibleType collectibleType = CollectibleType.Heart;

    public EnemySpawn[] EnemySpawns { get; private set; }   // stores all enemy spawns in this room

    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    void Awake()
    {
        EnemySpawns = new EnemySpawn[enemySpawns.transform.childCount];
        for (int i = 0; i < EnemySpawns.Length; i++)
        {
            EnemySpawns[i] = enemySpawns.transform.GetChild(i).GetComponent<EnemySpawn>();
        }
    }

    // Is called when entering the room. 
    // Returns type of this room.
    public RoomType GetRoomType()
    {
        return roomType;
    }

    // Is called when entering the room. 
    // Resets all changes.
    public void ActivateRoom()
    {
        // activate room
        staticObjects.SetActive(true);
        dynamicObjects.SetActive(true);

        // reset all changes
        // TODO

    }

    // Is called when leaving the room.
    // Deactivates all objects inside.
    public void DeactivateRoom()
    {
        // deactivate room
        staticObjects.SetActive(false);
        dynamicObjects.SetActive(false);

        // deactivate all objects
        // TODO
    }

    // When entering the room.
    // Return dropping artefact.
    // For debug use, later the boss drops the artefact.
    public CollectibleType GetCollectibleType()
    {
        return collectibleType;
    }
}
