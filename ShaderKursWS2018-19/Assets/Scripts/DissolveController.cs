using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveController : MonoBehaviour
{
    public Material matDissolve;
    public Vector4 dissolve;

    private void OnValidate()
    {
        matDissolve.SetColor("_Dissolve", new Color(dissolve.x, dissolve.y, dissolve.z, dissolve.w));
    }
}
