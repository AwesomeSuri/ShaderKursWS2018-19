using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [SerializeField]
    [Tooltip("Speed of lerping when switching rooms.")]
    float speed = 5;

    Vector3 initPos;                                // the position the camera has at the beninging

    public bool IsMoving { get; private set; }      // true if is still lerping

    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    // Start is called before the first frame update
    void Awake()
    {
        initPos = transform.position;
        IsMoving = false;
    }

    // Starts moving the camera to desired room coordinate
    public void MoveCamera(RoomCoordinate room)
    {
        StartCoroutine(MovingCamera(room));
    }

    IEnumerator MovingCamera(RoomCoordinate room)
    {
        IsMoving = true;

        Vector3 toPos = initPos + 10 * Vector3.right * room.x + 10 * Vector3.forward * room.y;

        while(Vector3.Distance(transform.position, toPos) > .01f)
        {
            transform.position = Vector3.Lerp(transform.position, toPos, speed * Time.deltaTime);

            yield return null;
        }

        //transform.position = toPos;

        IsMoving = false;
    }

    // Set the camera to desired room coordinate
    public void JumpCamera(RoomCoordinate room)
    {
        transform.position = initPos + Vector3.right * room.x + Vector3.forward * room.y;
    }
}
