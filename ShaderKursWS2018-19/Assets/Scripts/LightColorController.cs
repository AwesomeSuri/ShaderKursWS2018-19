using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightColorController : MonoBehaviour
{
    public Color firstColor = Color.red;
    public Color secondColor = Color.yellow;
    public float glowPulseLength;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Light myLight = this.gameObject.GetComponent<Light>();
        if (myLight)
        {
            myLight.color = Color.Lerp(firstColor, secondColor, Mathf.Sin(Time.time * glowPulseLength));
        }
    }

    private void OnValidate()
    {
        Light myLight = this.gameObject.GetComponent<Light>();
        if (myLight)
        {
            myLight.color = firstColor;

        }
        
    }
}
