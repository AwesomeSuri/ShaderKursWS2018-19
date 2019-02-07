using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerToGlobalDissolve
{
    IEnumerator JumpIntoRoom(RoomCoordinate room);
    IEnumerator DissolveAll();
    IEnumerator CloseRoom(char dir);
    IEnumerator OpenRoom(char dir);
}

public interface IGameManagerToGlobalDissolve
{
    IEnumerator JumpIntoRoom(RoomCoordinate room);
    IEnumerator DissolveAll();
}

public class GlobalDissolveToBlackController : MonoBehaviour, IPlayerToGlobalDissolve, IGameManagerToGlobalDissolve
{
    public Texture pattern;
    public Vector2 size = new Vector2(8, 8);
    public Vector2 offset = new Vector2(-5,-5);
    public Color glowColorTop = Color.cyan;
    public Color glowColorBottom = Color.cyan;
    public float glowThickness = .2f;
    public float glowIntensity = 3;
    public float glowEquator = 0;
    public float glowAmplitude = 1;

    [Tooltip("(left, right, bottom, top) on xz-plane")]
    public Vector4 visualArea = new Vector4(-5, 5, -5, 5);
    [Tooltip("Speed of the transition effect.")]
    public float speed = 5;
    [Tooltip("Offset of the visible area added to room radius of 5.")]
    public float areaSizeOffset = .5f;

    [Space]
    public Vector2 fog = new Vector2(1, -1);


    RoomCoordinate room;    // stores the current visual area location

    private void OnValidate()
    {
        Shader.SetGlobalTexture("_GlobalDissolveToBlackPattern", pattern);
        Shader.SetGlobalVector("_GlobalDissolveToBlackPatternST", new Vector4(size.x, size.x, offset.x, offset.y));
        Shader.SetGlobalColor("_GlobalDissolveToBlackColorTop", glowColorTop);
        Shader.SetGlobalColor("_GlobalDissolveToBlackColorBottom", glowColorBottom);
        Shader.SetGlobalFloat("_GlobalDissolveToBlackGlowThickness", glowThickness);
        Shader.SetGlobalFloat("_GlobalDissolveToBlackGlowIntensity", glowIntensity);
        Shader.SetGlobalFloat("_GlobalDissolveToBlackEquator", glowEquator);
        Shader.SetGlobalFloat("_GlobalDissolveToBlackAmplitude", glowAmplitude);
        Shader.SetGlobalVector("_GlobalDissolveToBlackVisualArea", visualArea);

        Shader.SetGlobalFloat("_GlobalEquator", fog.x);
        Shader.SetGlobalFloat("_GlobalBottom", fog.y);
    }

    // Called by player when jumping into a room.
    public IEnumerator JumpIntoRoom(RoomCoordinate room)
    {
        // get middle position
        this.room = room;
        float x = room.x * 10;
        float y = room.y * 10;
        Vector4 mid = new Vector4(x, x, y, y);

        // animate until area is complete
        float currentSize = -5;
        while(currentSize < 5 + areaSizeOffset)
        {
            currentSize += Time.deltaTime * speed;

            visualArea = mid + new Vector4(-1, 1, -1, 1) * currentSize;
            Shader.SetGlobalVector("_GlobalDissolveToBlackVisualArea", visualArea);

            yield return null;
        }
        visualArea = mid + new Vector4(-1, 1, -1, 1) * (5 + areaSizeOffset);
        Shader.SetGlobalVector("_GlobalDissolveToBlackVisualArea", visualArea);
    }

    // Called by player when he dies.
    public IEnumerator DissolveAll()
    {
        // get middle position
        float x = (visualArea.x + visualArea.y) / 2;
        float y = (visualArea.z + visualArea.w) / 2;
        Vector4 mid = new Vector4(x, x, y, y);

        // animate until area is zero
        float currentSize = 5 + areaSizeOffset;
        while (currentSize > -5)
        {
            currentSize -= Time.deltaTime * speed;

            visualArea = mid + new Vector4(-1, 1, -1, 1) * currentSize;
            Shader.SetGlobalVector("_GlobalDissolveToBlackVisualArea", visualArea);

            yield return null;
        }
        visualArea = mid + new Vector4(-1, 1, -1, 1) * -5;
        Shader.SetGlobalVector("_GlobalDissolveToBlackVisualArea", visualArea);
    }

    // Called by player when leaving the room.
    public IEnumerator CloseRoom(char dir)
    {
        // get current values
        float l = visualArea.x;
        float r = visualArea.y;
        float b = visualArea.z;
        float t = visualArea.w;

        // animate until one side reaches the other
        float currentPos = 0;
        while(currentPos < 10)
        {
            currentPos += Time.deltaTime * speed;

            // check which side will be animated
            switch (dir)
            {
                case 'r':
                    l += Time.deltaTime * speed;
                    break;
                case 'l':
                    r -= Time.deltaTime * speed;
                    break;
                case 't':
                    b += Time.deltaTime * speed;
                    break;
                case 'b':
                    t -= Time.deltaTime * speed;
                    break;
            }

            visualArea = new Vector4(l, r, b, t);
            Shader.SetGlobalVector("_GlobalDissolveToBlackVisualArea", visualArea);

            yield return null;
        }
        switch (dir)
        {
            case 'r':
                l = r - 2 * areaSizeOffset;
                break;
            case 'l':
                r = l + 2 * areaSizeOffset;
                break;
            case 't':
                b = t - 2 * areaSizeOffset;
                break;
            case 'b':
                t = b + 2 * areaSizeOffset;
                break;
        }
        visualArea = new Vector4(l, r, b, t);
        Shader.SetGlobalVector("_GlobalDissolveToBlackVisualArea", visualArea);
    }

    // Called by player when leaving the room.
    public IEnumerator OpenRoom(char dir)
    {
        // get current values
        float l = visualArea.x;
        float r = visualArea.y;
        float b = visualArea.z;
        float t = visualArea.w;

        // animate until one side reaches the other
        float currentPos = 0;
        while (currentPos < 10)
        {
            currentPos += Time.deltaTime * speed;

            // check which side will be animated
            switch (dir)
            {
                case 'l':
                    l -= Time.deltaTime * speed;
                    break;
                case 'r':
                    r += Time.deltaTime * speed;
                    break;
                case 'b':
                    b -= Time.deltaTime * speed;
                    break;
                case 't':
                    t += Time.deltaTime * speed;
                    break;
            }

            visualArea = new Vector4(l, r, b, t);
            Shader.SetGlobalVector("_GlobalDissolveToBlackVisualArea", visualArea);

            yield return null;
        }
        switch (dir)
        {
            case 'l':
                l = r - 2 * (5 + areaSizeOffset);
                break;
            case 'r':
                r = l + 2 * (5 + areaSizeOffset);
                break;
            case 'b':
                b = t - 2 * (5 + areaSizeOffset);
                break;
            case 't':
                t = b + 2 * (5 + areaSizeOffset);
                break;
        }
        visualArea = new Vector4(l, r, b, t);
        Shader.SetGlobalVector("_GlobalDissolveToBlackVisualArea", visualArea);
    }
}