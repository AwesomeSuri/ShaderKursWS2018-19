using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    TDS_PlayerMovement player;

    // Start is called before the first frame update
    void Start()
    {
        player.PlayerActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
