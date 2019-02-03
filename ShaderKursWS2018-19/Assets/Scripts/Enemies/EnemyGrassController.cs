using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGrassController : MonoBehaviour
{
    //this class should be added to an object with grassGenerator

    public Material mat;
    public Mesh mesh;
    private MaterialPropertyBlock block;
    public Vector4 offset;

    private const int SIZE = 10;

    //should not be more than 10
    public GameObject[] enemies = new GameObject[SIZE];
    private Vector4[] enemiesPos = new Vector4[SIZE];

    private void OnValidate()
    {
        if(enemies.Length > 10)
        {
            Debug.LogWarning("enemies should not be longer than 10");
            enemies = new GameObject[SIZE];
        }
        if(enemies.Length != enemiesPos.Length)
        {
            enemiesPos = new Vector4[enemies.Length];
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        block = new MaterialPropertyBlock();
        mesh = this.GetComponent<Mesh>();
        mat = this.GetComponent<Renderer>().material;
        mat.SetInt("_NumberOfEnemies", enemies.Length);
    }

    // Update is called once per frame
    void Update()
    {
        

        for(int i = 0; i < enemies.Length; i++)
        {
            enemiesPos[i] = enemies[i].transform.position;
            //Debug.Log("EnemyPos: " + enemiesPos[i]);
            block.SetVectorArray("_EnemyPositions", enemiesPos);
            block.SetVector("_EnemyPosition", enemiesPos[i] + offset);
            this.GetComponent<Renderer>().SetPropertyBlock(block);
        }
    }
}
