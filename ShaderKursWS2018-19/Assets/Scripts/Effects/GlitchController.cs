using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GlitchController : MonoBehaviour
{
    public Material mat;

    public Vector2 pauseRange;
    public Vector2 effectRange;

    public float maxEffect;

    public float timeSpeed;

    Vector4 effect;
    float inversionTimer;
    float horizontalTimer;
    float verticalTimer;
    float voidTimer;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState((int)Time.time);
        inversionTimer = Time.time + Random.Range(pauseRange.x, pauseRange.y);
        horizontalTimer = Time.time + Random.Range(pauseRange.x, pauseRange.y);
        verticalTimer = Time.time + Random.Range(pauseRange.x, pauseRange.y);
        voidTimer = Time.time + Random.Range(pauseRange.x, pauseRange.y);
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > inversionTimer)
        {
            if(effect.x <= 0)
            {
                Random.InitState((int)Time.time);
                inversionTimer = Time.time + Random.Range(effectRange.x, effectRange.y);
                effect.x = 1;

                Random.InitState((int)Time.time);
                float x = Random.Range(-10, 10);
                float y = Random.Range(-10, 10);
                float z = Random.Range(-100, 100);
                float w = Random.Range(-100, 100);

                //mat.SetVector("_Inversion", new Vector4(x,y,z,w));
            }
            else
            {
                Random.InitState((int)Time.time);
                inversionTimer = Time.time + Random.Range(pauseRange.x, pauseRange.y);
                effect.x = 0;
            }
        }

        if (Time.time > horizontalTimer)
        {
            if (effect.y <= 0)
            {
                Random.InitState((int)Time.time);
                horizontalTimer = Time.time + Random.Range(effectRange.x, effectRange.y);
                effect.y = Random.Range(0, maxEffect);

                Random.InitState((int)Time.time);
                float x = Random.Range(-10, 10);
                float y = Random.Range(-10, 10);
                float z = Random.Range(-100, 100);
                float w = Random.Range(-100, 100);

                //mat.SetVector("_Horizontal", new Vector4(x, y, z, w));
            }
            else
            {
                Random.InitState((int)Time.time);
                horizontalTimer = Time.time + Random.Range(pauseRange.x, pauseRange.y);
                effect.y = 0;
            }
        }

        if (Time.time > verticalTimer)
        {
            if (effect.z <= 0)
            {
                Random.InitState((int)Time.time);
                verticalTimer = Time.time + Random.Range(effectRange.x, effectRange.y);
                effect.z = Random.Range(0, maxEffect);

                Random.InitState((int)Time.time);
                float x = Random.Range(-10, 10);
                float y = Random.Range(-10, 10);
                float z = Random.Range(-100, 100);
                float w = Random.Range(-100, 100);

                //mat.SetVector("_Vertical", new Vector4(x, y, z, w));
            }
            else
            {
                Random.InitState((int)Time.time);
                verticalTimer = Time.time + Random.Range(pauseRange.x, pauseRange.y);
                effect.z = 0;
            }
        }

        if (Time.time > voidTimer)
        {
            if (effect.w <= 0)
            {
                voidTimer = Time.time + Random.Range(effectRange.x, effectRange.y);
                effect.w = 1;

                Random.InitState((int)Time.time);
                float x = Random.Range(-10, 10);
                float y = Random.Range(-10, 10);
                float z = Random.Range(-100, 100);
                float w = Random.Range(-100, 100);

                //mat.SetVector("_Void", new Vector4(x, y, z, w));
            }
            else
            {
                voidTimer = Time.time + Random.Range(pauseRange.x, pauseRange.y);
                effect.w = 0;
            }
        }

        mat.SetVector("_Effect", effect);
    }
}
