using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    float timeLeft = 300;
    [SerializeField]
    LevelUIController levelUIObject;
    [SerializeField]
    GameObject playerObject;

    IGameManagerToUI levelUI;
    IGameManagerToPlayerStats playerStats;


    // Start is called before the first frame update
    void Start()
    {
        levelUI = levelUIObject;
        levelUIObject = null;

        playerStats = playerObject.GetComponent<IGameManagerToPlayerStats>();
        playerObject = null;

        playerStats.PlayerActive = true;

        levelUI.UpdateTime(timeLeft);
        StartCoroutine(Intro());
    }

    // Update is called once per frame
    void Update()
    {
        if (timeLeft <= 0)
        {
            SceneManager.LoadScene("Lose");
        }
    }

    // The events that play in the beginning automatically
    IEnumerator Intro()
    {
        yield return null;

        playerStats.PickUp(CollectibleType.Sword);

        StartCoroutine(CountingDown());
    }

    // Counts down time.
    // Initiated after intro.
    IEnumerator CountingDown()
    {
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft > 0)
            {
                levelUI.UpdateTime(timeLeft);
            }

            yield return null;
        }

        timeLeft = 0;
        levelUI.UpdateTime(timeLeft);
    }
}
