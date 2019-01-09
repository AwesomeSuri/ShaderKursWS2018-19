using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator anim;

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateMovement(float x, float y)
    {
        anim.SetFloat("SpeedX", x);
        anim.SetFloat("SpeedY", y);
    }

    public void SetFlying(bool flying)
    {
        anim.SetBool("Flying", flying);
    }
}
