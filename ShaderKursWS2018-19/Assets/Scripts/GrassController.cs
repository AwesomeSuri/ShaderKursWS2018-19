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

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collided with " + collision.gameObject.name);
        if (collision.gameObject.layer.Equals(LayerMask.NameToLayer("Ground")))
        {
            GrassVertexGenerator localPos = collision.gameObject.GetComponentInChildren<GrassVertexGenerator>();
            if (localPos)
            {
                offset = -localPos.transform.position;
            }
            
            
        }
    }
}
