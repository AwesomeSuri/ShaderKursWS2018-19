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
    [SerializeField]
    float offset;

    int current;
    float toRot;
    int maxChildCount;
    bool IsSwitching;

    private void Awake()
    {
        for (int i = 0; i < activatedObjects.childCount; i++)
        {
            maxChildCount = Mathf.Max(maxChildCount, activatedObjects.GetChild(i).childCount);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(Switching());
    }

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
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerWeapon") && !IsSwitching)
        {
            toRot = (toRot + 90) % 360;
            current = (current + 1) % activatedObjects.childCount;

            StartCoroutine(Switching());
        }
    }

    IEnumerator Switching()
    {
        IsSwitching = true;

        for (int i = 0; i < maxChildCount; i++)
        {
            for (int j = 0; j < activatedObjects.childCount; j++)
            {
                if(i < activatedObjects.GetChild(j).childCount)
                {
                    ISwitchToActivatedObject activatedObject = activatedObjects.GetChild(j).GetChild(i).GetComponent<ISwitchToActivatedObject>();

                    if(j == current)
                    {
                        activatedObject.Activate(true);
                    }
                    else
                    {
                        activatedObject.Activate(false);
                    }
                }
            }

            yield return new WaitForSeconds(offset);
        }

        IsSwitching = false;
    }
}
