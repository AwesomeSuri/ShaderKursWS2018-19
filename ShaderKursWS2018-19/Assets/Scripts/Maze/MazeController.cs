using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerToMaze
{
    void DeactivateRoom(RoomCoordinate coordinate);
    void ActivateRoom(RoomCoordinate coordinate);
    RoomType GetRoomType(RoomCoordinate coordinate);
}

public class MazeController : MonoBehaviour, IPlayerToMaze
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [SerializeField]
    [Tooltip("Component of the collectible parent.")]
    CollectibleSpawner collectibleSpawnerObject;
    [SerializeField]
    [Tooltip("Component of the enemy parent.")]
    EnemySpawner enemySpawner;

    IMazeToRoom[] rooms;                            // array of controller of all rooms inside this maze
    IMazeToCollectibleSpawner collectibleSpawner;   // component that controls all collectibles


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    // Start is called before the first frame update
    void Awake()
    {
        rooms = new IMazeToRoom[transform.childCount];
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i] = transform.GetChild(i).GetComponent<RoomController>();
        }

        collectibleSpawner = collectibleSpawnerObject;
        collectibleSpawnerObject = null;
    }

    private void Start()
    {
        // deactivate all rooms
        foreach (RoomController room in rooms)
        {
            room.DeactivateRoom();
        }

        // activate first room
        // TODO: add intro
        rooms[0].ActivateRoom();
    }

    // Called by player.
    // Deactivate the room.
    // Deactivate all collectibles.
    // Deactivate enemies.
    public void DeactivateRoom(RoomCoordinate coordinate)
    {
        rooms[coordinate.x + coordinate.y * 10].DeactivateRoom();
        collectibleSpawner.DecreaseCollectibleLifetime();

        // TODO: deactivate enemies inside this room
    }

    // Called by player.
    // Activate the room.
    // Activate enemies.
    public void ActivateRoom(RoomCoordinate coordinate)
    {
        // activate room
        IMazeToRoom room = rooms[coordinate.x + coordinate.y * 10];
        room.ActivateRoom();

        // activate enemies
        EnemySpawn[] enemies = room.EnemySpawns;
        for (int i = 0; i < enemies.Length; i++)
        {
            print(enemies[i].type);
            enemySpawner.SpawnEnemy(enemies[i], coordinate);
        }

        // Debugging:
        // Spawn artefact
        CollectibleType artefact = room.GetCollectibleType();
        if((int)artefact > 2)
        {
            collectibleSpawner.Drop(new Vector3(10 * coordinate.x, 0, 10 * coordinate.y), artefact);
        }
    }


    // Get the type of the room at the room coordinate
    public RoomType GetRoomType(RoomCoordinate coordinate)
    {
        return rooms[coordinate.x + coordinate.y * 10].GetRoomType();
    }
}
