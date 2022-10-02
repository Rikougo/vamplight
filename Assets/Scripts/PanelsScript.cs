using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelsScript : MonoBehaviour
{
    [Header("Panel Parameters")]
    public CanvasGroup Blackfade;


    void Start()
    {
        Blackfade.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartScene(int idscene)
    {
        Blackfade.gameObject.gameObject.SetActive(true);
    AsyncOperation asyncScene = SceneManager.LoadSceneAsync((int)idscene, LoadSceneMode.Single);
    asyncScene.allowSceneActivation = false;
        Blackfade.LeanAlpha(1, 1.5f).setOnComplete(() =>
        {
            asyncScene.allowSceneActivation = true;
            Debug.Log("Scene Loaded");
        });
    }



}
