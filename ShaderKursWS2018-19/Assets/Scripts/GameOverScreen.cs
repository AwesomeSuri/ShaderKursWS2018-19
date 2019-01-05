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
            // TODO
        }
    }

    // when quit button is clicked
    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
