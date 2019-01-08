using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    RoomController[] rooms;     // array of controller of all rooms inside this maze


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    // Start is called before the first frame update
    void Awake()
    {
        rooms = new RoomController[transform.childCount];
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i] = transform.GetChild(i).GetComponent<RoomController>();
        }
    }

    private void Start()
    {
        // deactivate all rooms
        foreach (RoomController room in rooms)
        {
            room.DeactivateRoom();
        }

        // activate first room
        // TODO: change this when intro is in the making
        rooms[0].ActivateRoom();
    }

    // Called by player.
    // Activate the room.
    public void DeactivateRoom(RoomCoordinate coordinate)
    {
        rooms[coordinate.x + coordinate.y * 10].DeactivateRoom();
    }

    // Called by player.
    // Deactivate the room.
    public void ActivateRoom(RoomCoordinate coordinate)
    {
        rooms[coordinate.x + coordinate.y * 10].ActivateRoom();
    }


    // Get the type of the room at the room coordinate
    public RoomType GetRoomType(RoomCoordinate coordinate)
    {
        return rooms[coordinate.x + coordinate.y * 10].GetRoomType();
    }
}
