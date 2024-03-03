using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEditor.SearchService;

public class GameManager : MonoBehaviour
{
    public AudioSource Ambiance;
    public Image FadePanel;
    public float duration;
    public GameObject StartScreen;
    public PlayerInput Inputs;
    public GameObject StartScreenCamera, PlayerCamera;
    bool startgame = false;
    public GameObject CutScene;
    public GameObject Gameplay, Wolves;
    public Transform Offset, PlayerTransform;
    private void Awake()
    {
        Fade(0);
    }

    public void Fade(int val)
    {
        FadePanel.DOFade(val, duration);
    }
    private void Update()
    {
        if(!startgame)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }
        }
    }

    public void ResumeGame()
    {
        PlayerTransform.position = Offset.position;
        PlayerTransform.rotation = Offset.rotation;
        CutScene.SetActive(false);
        Gameplay.SetActive(true);

    }

    public void StartCutScene()
    {
        CutScene.SetActive(true);
        Gameplay.SetActive(false);
    }

    public void StartGame()
    {
        startgame = true;
        Inputs.enabled = true;
        StartScreenCamera.SetActive(false);
        PlayerCamera.SetActive(true);
        StartScreen.SetActive(false);
        Wolves.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }


}
