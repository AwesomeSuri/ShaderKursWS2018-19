using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GrassVertexGenerator : MonoBehaviour
{
    private Mesh grassMesh;
    public MeshFilter meshFilter;

    public int seed;
    public Vector2 size;

    public float offsetY;
    [Range(1, 5000)]
    public int grassNumber;

    public float startHeight = 1000;

    public LayerMask thisLayerMask;

    private Vector3 lastPosition;
    

    // Update is called once per frame
    void Update()
    {
        if (lastPosition != this.transform.position)
        {

            Random.InitState(seed);
            //Generates positions, Colors and normals for geometry shader
            List<Vector3> positions = new List<Vector3>(grassNumber);
            int[] indices = new int[grassNumber];
            List<Color> colors = new List<Color>(grassNumber);
            List<Vector3> normals = new List<Vector3>(grassNumber);
            for (int i = 0; i < grassNumber; i++)
            {
                Vector3 origin = transform.position;
                origin.y = startHeight;
                origin.x += (size.x * Random.Range(-0.5f, 0.5f));
                origin.z += (size.y * Random.Range(-0.5f, 0.5f));
                Ray ray = new Ray(origin, Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, startHeight, thisLayerMask))
                {
                    origin = hit.point;
                    colors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1));
                    origin.y += offsetY;
                    positions.Add(origin);
                    normals.Add(hit.normal);
                    indices[i] = i;

                    //Debug.DrawRay(origin, Vector3.up);
                    //Debug.DrawLine(origin, hit.normal);
                }
            }
            grassMesh = new Mesh();
            grassMesh.SetVertices(positions);
            grassMesh.SetIndices(indices, MeshTopology.Points, 0);
            grassMesh.SetColors(colors);
            grassMesh.SetNormals(normals);


            meshFilter.mesh = grassMesh;

            lastPosition = this.transform.position;
        }
    }
}