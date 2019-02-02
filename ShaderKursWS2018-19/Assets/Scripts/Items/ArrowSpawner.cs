using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    Arrow[] arrows;                             // pool of arrows


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    private void Awake()
    {
        arrows = new Arrow[transform.childCount];
    }

    private void Start()
    {
        for (int i = 0; i < arrows.Length; i++)
        {
            arrows[i] = transform.GetChild(i).GetComponent<Arrow>();
            arrows[i].DeactivateArrow();
        }
    }

    // check pool aviability
    // should be always ready, but for safety reasons
    int CheckPool()
    {
        for (int i = 0; i < arrows.Length; i++)
        {
            if (arrows[i].Ready())
            {
                return i;
            }
        }

        return -1;
    }

    public void Shoot(Transform origin, float strength)
    {
        // check first if pool is ready
        // store the ready object
        int index = CheckPool();

        // continue if pool is ready
        if (index == -1)
        {
            return;
        }

        // spawn and shoot the arrow
        arrows[index].Shoot(origin, strength);
    }
}
