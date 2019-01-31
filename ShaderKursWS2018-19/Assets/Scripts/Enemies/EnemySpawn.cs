using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public EnemyType type;

    public Vector3 Position { get; private set; }

    private void Awake()
    {
        Position = transform.position;
    }
}
