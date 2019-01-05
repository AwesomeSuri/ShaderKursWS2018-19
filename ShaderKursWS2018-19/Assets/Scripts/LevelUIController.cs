﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [SerializeField] Transform leftPanel;
    [SerializeField] Transform rightPanel;

    [Space]
    [SerializeField] Button menuButton;
    [SerializeField] GameObject menu;
    [SerializeField] Button quitButton;

    [Space]
    [SerializeField] Button mapButton;
    [SerializeField] GameObject map;
    [SerializeField] Transform roomsParent;
    [SerializeField] GameObject noteButtons;
    [SerializeField] GameObject noteBack;

    [Space]
    [SerializeField] Button sword;
    [SerializeField] Button bow;
    [SerializeField] Button wings;
    [SerializeField] Button barrier;

    [Space]
    [SerializeField] Text countdown;
    [SerializeField] Transform hearts;
    [SerializeField] Text arrows;


    bool menuOpen;
    bool mapOpen;
    GameObject roomSelected;

    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    // Start is called before the first frame update
    void Awake()
    {
        menuOpen = false;
        mapOpen = false;
    }


    // Stats /-------------------------------------------------------------------------------------//
    // when player shoots an arrow or picks them up
    // update amount of arrows
    public void UpdateArrows(int amount)
    {
        arrows.transform.parent.gameObject.SetActive(true);
        arrows.text = "x" + amount;
    }

    // when player gets hit or picks up heart
    // update amount of hearts
    public void UpdateHearts(int amount)
    {
        if (amount >= 1)
        {
            hearts.GetChild(0).GetComponent<Image>().color = Color.white;

            if (amount >= 2)
            {
                hearts.GetChild(1).GetComponent<Image>().color = Color.white;

                if (amount >= 3)
                {
                    hearts.GetChild(2).GetComponent<Image>().color = Color.white;
                }
                hearts.GetChild(2).GetComponent<Image>().color = Color.black;
            }
            hearts.GetChild(1).GetComponent<Image>().color = Color.black;
        }
        hearts.GetChild(0).GetComponent<Image>().color = Color.black;
    }

    // Menu /--------------------------------------------------------------------------------------//
    // when menu button is clicked
    // toggles the menu on and off, stop time if on
    // deactivates map
    public void MenuButton()
    {
        if (menuOpen)
        {
            menu.SetActive(false);
            Time.timeScale = 1;

            menuOpen = false;
        }
        else
        {
            map.SetActive(false);
            menu.SetActive(true);

            Time.timeScale = 0;

            mapOpen = false;
            menuOpen = true;
        }
    }

    // when quit button is clicked
    // goes back to main menu
    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }


    // Cutscenes /---------------------------------------------------------------------------------//
    // when player switch rooms or dies
    public void SetItemInteractable(bool interactable)
    {
        sword.interactable = interactable;
        bow.interactable = interactable;
        wings.interactable = interactable;
        barrier.interactable = interactable;
    }

    // when cutscene
    // deactivate ui panels
    public void SwitchToCutscene(bool cutscene)
    {
        leftPanel.gameObject.SetActive(!cutscene);
        rightPanel.gameObject.SetActive(!cutscene);
    }


    // Map /---------------------------------------------------------------------------------------//
    // when map button is clicked
    // toggles the map on and off, stop time if on
    // deactivates menu
    public void MapButton()
    {
        if (mapOpen)
        {
            map.SetActive(false);
            Time.timeScale = 1;

            mapOpen = false;
        }
        else
        {
            map.SetActive(true);
            menu.SetActive(false);

            Time.timeScale = 0;

            mapOpen = true;
            menuOpen = false;
        }
    }

    // when a room in map is clicked
    // note buttons appears
    // invisible return button appears
    public void RoomButton()
    {
        noteBack.SetActive(true);

        // set noteButtons to the position of room
        noteButtons.SetActive(true);
        roomSelected = EventSystem.current.currentSelectedGameObject;
        noteButtons.transform.position = roomSelected.transform.position;

        return;
    }

    // when a note button is clicked
    // toggle the note on that room
    // disable the note buttons
    public void NoteButton(int type)
    {
        // get room to toggle its notes
        GameObject note = roomSelected.transform.GetChild(type).gameObject;
        note.SetActive(!note.activeInHierarchy);

        NoteBack();
    }

    // when clicked outside note buttons
    // disables note buttons
    public void NoteBack()
    {
        noteBack.SetActive(false);
        noteButtons.SetActive(false);

        roomSelected = null;
    }

    // when player enters a room
    // lightens the room in map
    public void EnterRoom(float x, float y)
    {
        // resolve startpoint offset
        x += 5;
        y += 5;

        // room size independence
        x /= 10;
        y /= 10;

        // round coordinates
        int xCoord = Mathf.FloorToInt(x);
        int yCoord = Mathf.FloorToInt(y);

        // parse 2D coordinates to 1D
        int roomIndex = 10 * yCoord + xCoord;

        // lighten the room
        roomsParent.GetChild(roomIndex).GetComponent<Image>().color = Color.white;
    }


    // Items /-------------------------------------------------------------------------------------//
    // when sword obtained
    // activate sword button
    public void ActivateSword()
    {
        sword.gameObject.SetActive(true);
    }

    // when bow obtained
    // activate bow button
    public void ActivateBow()
    {
        bow.gameObject.SetActive(true);
    }

    // when wings obtained
    // activate wings button
    public void ActivateWings()
    {
        wings.gameObject.SetActive(true);
    }

    // when barrier obtained
    // activate barrier button
    public void ActivateBarrier()
    {
        barrier.gameObject.SetActive(true);
    }

    // when sword button is clicked
    // equips sword
    public void EquipSword()
    {
        // TODO
    }

    // when bow button is clicked
    // equips bow
    public void EquipBow()
    {
        // TODO
    }

    // when wings button is clicked
    // equips wings
    public void EquipWings()
    {
        // TODO
    }

    // when barrier button is clicked
    // equips barrier
    public void EquipBarrier()
    {
        // TODO
    }
}
