using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuController : MonoBehaviour
{
    public GameObject loadingText;
    public Button playButton;
    public Button quitButton;

    public void QuitButton()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif

#if UNITY_STANDALONE
        Application.Quit();
#endif
    }

    public void PlayButton()
    {
        loadingText.SetActive(true);
        playButton.interactable = false;
        quitButton.interactable = false;

        StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel()
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync("Level");

        while (!ao.isDone)
        {
            yield return null;
        }
    }
}
