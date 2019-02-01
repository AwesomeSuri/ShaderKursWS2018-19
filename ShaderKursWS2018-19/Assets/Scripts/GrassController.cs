using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GrassController : MonoBehaviour
{
    public Material[] GrassMat;
    public Vector3 offset;

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < GrassMat.Length; i++)
        {
            GrassMat[i].SetVector("_PlayerPos", transform.position + offset);
        }
        
    }

    //maybe change this so it will switch when players enters next room -> in player movement script then!
    public void SetOffset(RoomCoordinate room)
    {
        {
            Vector3 localPos = new Vector3(room.x * 5, 0, room.y * 5);
                offset = -localPos;
        }
    }
}
