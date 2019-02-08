using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinController : MonoBehaviour
{

    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        float seconds = GameManager.timeLeft;
        int m1;
        int m2;
        int s1;
        int s2;

        float minutes = seconds / 60;
        seconds = seconds % 60;

        m1 = Mathf.FloorToInt(minutes / 10);
        m2 = Mathf.FloorToInt(minutes % 10);
        s1 = Mathf.FloorToInt(seconds / 10);
        s2 = Mathf.FloorToInt(seconds % 10);

        string time = "" + m1 + "" + m2 + ":" + s1 + "" + s2;

        text.text = time;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
