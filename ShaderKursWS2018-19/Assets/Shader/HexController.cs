using UnityEngine;

[ExecuteInEditMode]
public class HexController : MonoBehaviour
{
    public Material HexMat;

	// Update is called once per frame
	void Update ()
    {
        HexMat.SetVector("_PlayerPos", transform.position);
	}
}
