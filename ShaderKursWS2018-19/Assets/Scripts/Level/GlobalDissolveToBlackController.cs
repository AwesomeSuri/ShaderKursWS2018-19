using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDissolveToBlackController : MonoBehaviour
{
    public Texture pattern;
    public Vector2 size = new Vector2(8, 8);
    public Vector2 offset = new Vector2(-5,-5);
    public Color glowColor = Color.cyan;
    public float glowThickness = .2f;
    public float glowIntensity = 3;

    [Tooltip("(left, right, bottom, top) on xz-plane")]
    public Vector4 visualArea = new Vector4(-5, 5, -5, 5);

    private void OnValidate()
    {
        Shader.SetGlobalTexture("_GlobalDissolveToBlackPattern", pattern);
        Shader.SetGlobalVector("_GlobalDissolveToBlackPatternST", new Vector4(size.x, size.x, offset.x, offset.y));
        Shader.SetGlobalColor("_GlobalDissolveToBlackColor", glowColor);
        Shader.SetGlobalFloat("_GlobalDissolveToBlackGlowThickness", glowThickness);
        Shader.SetGlobalFloat("_GlobalDissolveToBlackGlowIntensity", glowIntensity);
        Shader.SetGlobalVector("_GlobalDissolveToBlackVisualArea", visualArea);
    }
}
