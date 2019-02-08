using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField]
    Text time;

    // Start is called before the first frame update
    void Start()
    {
        // if won, get time left
        if(time != null)
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

            this.time.text = time;
        }
    }

    // when quit button is clicked
    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
