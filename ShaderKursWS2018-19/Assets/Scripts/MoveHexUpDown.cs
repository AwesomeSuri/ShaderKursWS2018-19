using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHexUpDown : MonoBehaviour
{
    private float speed;
    private float min;
    private float max;
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        speed = Random.Range(0.5f, 1.0f);
        min = .5f;
        max = 1.0f;
        transform.position = startPosition + Vector3.up * min;
    }

    // Update is called once per frame
    void Update()
    {
        //if(transform.position.y <= startPosition.y + 5.0f && transform.position.y >= startPosition.y - 5.0f)
        //{
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        //}
        if(transform.position.y > startPosition.y + max)
        {
            speed = -speed;
        }
        else if(transform.position.y < startPosition.y + min)
        {
            speed = -speed;
        }
    }
}
