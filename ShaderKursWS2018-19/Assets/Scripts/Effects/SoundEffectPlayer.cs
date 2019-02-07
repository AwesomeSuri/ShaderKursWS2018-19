using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectPlayer : MonoBehaviour
{
    Dictionary<string, int> audioDict = new Dictionary<string, int>();
    AudioSource[] audios;

    // Start is called before the first frame update
    void Awake()
    {
        audios = new AudioSource[transform.childCount];
        for (int i = 0; i < audios.Length; i++)
        {
            audios[i] = transform.GetChild(i).GetComponent<AudioSource>();
            audioDict.Add(audios[i].gameObject.name, i);
        }
    }

    public void PlayAudio(string name)
    {
        audios[audioDict[name]].Play();
    }

    public void StopAudio(string name)
    {
        audios[audioDict[name]].Stop();
    }
}
