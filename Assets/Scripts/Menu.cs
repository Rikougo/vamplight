using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviour
{
    [Header("Menu Componnents")]
    public CanvasGroup blackFade;
    public GameObject buttons;
    private bool canSwitchScenes = false;

    
    public CanvasGroup credits;
    public CanvasGroup creditsBlackFade;
    public GameObject teamNames;
    public GameObject thanks;
    public GameObject backButton;

    private void Awake()
    {
        blackFade.gameObject.SetActive(true);
        creditsBlackFade.alpha = 0;
        credits.gameObject.SetActive(false);
        canSwitchScenes = false;
    }

    void Start()
    {
        BlackFadeIn();
        Invoke("ButtonsIn", 3.5f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Fades from black to display the title screen menu.
    /// </summary>
    /// <param name="context"></param>
    public void BlackFadeIn()
    {
        Invoke("BlackFadeOff", 3);
        blackFade.alpha = 1;
        blackFade.LeanAlpha(0, 3f);
    }
    private void BlackFadeOff()
    {
        blackFade.gameObject.SetActive(false);
    }


    /// <summary>
    /// Displays the "play", "credits", and "quit" buttons on screen.
    /// </summary>
    /// <param name="context"></param>
    public void ButtonsIn()
    {
        buttons.LeanMoveLocal(new Vector3(0, -280f, 0), 1.25f).setEase(LeanTweenType.easeOutCubic); ;
    }

    /// <summary>
    /// Loads the level while fading to black.
    /// </summary>
    /// <param name="context"></param>
    public void StartLevel(int idScene)
    {
        AsyncOperation asyncScene = SceneManager.LoadSceneAsync((int)idScene, LoadSceneMode.Single);
        asyncScene.allowSceneActivation = false;
        blackFade.gameObject.gameObject.SetActive(true);
        blackFade.LeanAlpha(1f, 1f).setOnComplete(() =>
        {
            asyncScene.allowSceneActivation = true;
            Debug.Log("Scene Loaded");
        });
    }

    /// <summary>
    /// Displays the credits.
    /// </summary>
    /// <param name="context"></param>
    public void CreditsIn()
    {
        credits.gameObject.SetActive(true );
        creditsBlackFade.LeanAlpha(0.8f, 0.5f);
        teamNames.LeanMoveLocal(new Vector3(-340, 0f, 0), 1.25f).setEase(LeanTweenType.easeOutCubic); ;
        thanks.LeanMoveLocal(new Vector3(470, -270f, 0), 1.25f).setEase(LeanTweenType.easeOutCubic); ;
        backButton.LeanMoveLocal(new Vector3(810, 450, 0), 1.25f).setEase(LeanTweenType.easeOutCubic); ;
    }

    /// <summary>
    /// Disables the display of the credits.
    /// </summary>
    /// <param name="context"></param>
    public void CreditsOut()
    {
        Invoke("CreditsOff", 0.5f);
        creditsBlackFade.LeanAlpha(0f, 0.5f);
        teamNames.LeanMoveLocal(new Vector3(-1360, 0f, 0), 1.25f).setEase(LeanTweenType.easeOutCubic); ;
        thanks.LeanMoveLocal(new Vector3(1880, -1080f, 0), 1.25f).setEase(LeanTweenType.easeOutCubic); ;
        backButton.LeanMoveLocal(new Vector3(1880, 1080f, 0), 1.25f).setEase(LeanTweenType.easeOutCubic); ;
    }
    private void CreditsOff()
    {
        credits.gameObject.SetActive(false );
    }

    public void Quit()
    {
        Debug.Log("Quitting");
        Quit();
    }
}
