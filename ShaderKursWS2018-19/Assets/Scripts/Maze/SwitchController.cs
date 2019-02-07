using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [SerializeField]
    float speed;
    [SerializeField]
    Transform switchObject;
    [SerializeField]
    Transform activatedObjects;

    int current;
    float toRot;

    private void Update()
    {
        if(Mathf.Abs(switchObject.localEulerAngles.y - toRot) > 1f)
        {
            float y = Mathf.Lerp(switchObject.localEulerAngles.y, toRot, Time.deltaTime * speed);
            switchObject.localEulerAngles = new Vector3(switchObject.localEulerAngles.x, y, switchObject.localEulerAngles.z);
        }
        else if(switchObject.localEulerAngles.y != toRot)
        {
            switchObject.localEulerAngles = new Vector3(switchObject.localEulerAngles.x, toRot, switchObject.localEulerAngles.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //print(LayerMask.other.gameObject.layer);
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerWeapon"))
        {
            toRot = (toRot + 90) % 360;
            current = (current + 1) % activatedObjects.childCount;

            // switch off all objects
            for (int i = 0; i < activatedObjects.childCount; i++)
            {
                activatedObjects.GetChild(i).gameObject.SetActive(false);
            }

            // switch on current object
            activatedObjects.GetChild(current).gameObject.SetActive(true);
        }
    }
}
