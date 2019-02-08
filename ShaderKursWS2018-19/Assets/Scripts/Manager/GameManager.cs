using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    float countDown = 300;
    [SerializeField]
    LevelUIController levelUIObject;
    [SerializeField]
    GameObject playerObject;


    IGameManagerToUI levelUI;
    IGameManagerToPlayerStats playerStats;
    IGameManagerToGlobalDissolve dissolve;

    float timeTo;
    public static float timeLeft;


    // Start is called before the first frame update
    void Start()
    {
        levelUI = levelUIObject;
        levelUIObject = null;

        playerStats = playerObject.GetComponent<IGameManagerToPlayerStats>();
        playerObject = null;

        dissolve = GetComponent<IGameManagerToGlobalDissolve>();

        timeTo = Time.time + countDown;
        timeLeft = timeTo - Time.time;

        levelUI.UpdateTime(countDown);
        StartCoroutine(Intro());
    }

    // Update is called once per frame
    void Update()
    {
        if (timeLeft <= 0)
        {
            SceneManager.LoadScene("Lose");
        }
        else
        {
            timeLeft = timeTo - Time.time;
        }
    }

    // The events that play in the beginning automatically
    IEnumerator Intro()
    {
        yield return null;

        RoomCoordinate room;
        room.x = 0;
        room.y = 0;
        yield return dissolve.JumpIntoRoom(room);

        playerStats.PlayerActive = true;

        playerStats.PickUp(CollectibleType.Sword);

        StartCoroutine(CountingDown());
    }

    // Counts down time.
    // Initiated after intro.
    IEnumerator CountingDown()
    {
        while (Time.time < timeTo)
        {
            timeLeft = timeTo - Time.time;
            if (timeLeft > 0)
            {
                levelUI.UpdateTime(timeLeft);
            }

            yield return null;
        }
        
        levelUI.UpdateTime(0);
    }
}
