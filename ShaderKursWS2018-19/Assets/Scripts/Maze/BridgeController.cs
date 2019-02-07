using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISwitchToActivatedObject
{
    void Activate(bool activate);
}

public class BridgeController : MonoBehaviour, ISwitchToActivatedObject
{
    [SerializeField]
    Texture pattern;
    [SerializeField]
    float dissolve = 0;
    [SerializeField]
    float edge = .1f;
    [SerializeField]
    Color glow = Color.white;
    [SerializeField]
    float intensity = 5;
    [SerializeField]
    float speed = 1;

    Material mat;
    bool vanish;
    Collider col;

    private void OnValidate()
    {
        if (mat == null)
        {
            mat = GetComponent<Renderer>().sharedMaterial;
        }

        if (pattern != null)
        {
            mat.SetTexture("_Pattern", pattern);
        }

        mat.SetFloat("_Dissolve", dissolve);
        mat.SetFloat("_DissolveEdge", edge);
        mat.SetColor("_DissolveGlow", glow);
        mat.SetFloat("_DissolveIntensity", intensity);
    }

    private void Awake()
    {
        mat = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        if (mat == null)
        {
            mat = GetComponent<Renderer>().material;
        }

        if (vanish && dissolve < 1)
        {
            dissolve += Time.deltaTime * speed;
            mat.SetFloat("_Dissolve", dissolve);
        }
        else if (!vanish && dissolve > 0)
        {
            dissolve -= Time.deltaTime * speed;
            mat.SetFloat("_Dissolve", dissolve);
        }
    }

    void Dissolve(bool vanish)
    {
        this.vanish = vanish;
        dissolve = vanish ? 0 : 1;

        if (col == null)
        {
            col = GetComponent<Collider>();
        }

        col.enabled = !vanish;
    }

    public void Activate(bool activate)
    {
        Dissolve(!activate);
    }
}
