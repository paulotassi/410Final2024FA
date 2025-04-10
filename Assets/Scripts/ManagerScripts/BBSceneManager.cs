using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BBSceneManager : MonoBehaviour
{
    [SerializeField] public GameObject SinglePlayer;
    [SerializeField] public GameObject TwoPlayer;

    [SerializeField] public GameObject LevelSelect;
    [SerializeField] public GameObject Versus;
    [SerializeField] public GameObject Credits;
    [SerializeField] public GameObject Exit;
    [SerializeField] public GameObject BackButton;


    //Eventual intent is to make the play to go as follows. Player hits initial button choice bringing you to either coop gamestate. when players finish a round they returnt to titlescreen and the button changes from coopmode to continue coop
    //A state will exist that will continue guiding players through their playthrough. an additional button will appear that will say to reset coop run or reset comp run to bring player game states back to first level.
    private void Start()
    {
        LevelSelect.SetActive(false);
        Versus.SetActive(false);
        Credits.SetActive(false);
        Exit.SetActive(false);
        BackButton.SetActive(false);
    }
    public void LoadCoopPlaythrough1()
    {
        SceneManager.LoadScene("CompetetiveLevel1");
    }

    public void LoadCoopPlaythrough2()
    {
        SceneManager.LoadScene("CompetitiveLevel2");
    }

    public void LoadCooperativeBossPlaythrough()
    {
        SceneManager.LoadScene("BossLevel");
    }

    public void LoadVersusPlaythrough()
    {
        SceneManager.LoadScene("ArenaScene");
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
    public void StartSinglePlayerMode()
    {
        GameSettings.singlePlayerMode = true;
        GameSettings.competetiveMode = false;
        GameSettings.arcadeMode = false;

        LevelSelect.SetActive(true);
        Versus.SetActive(false);
        Credits.SetActive(true);
        Exit.SetActive(true);
        BackButton.SetActive(true);
        SinglePlayer.SetActive(false);
        TwoPlayer.SetActive(false);

    }

    public void StartTwoPlayerMode()
    {
        GameSettings.singlePlayerMode = false;
        GameSettings.competetiveMode = true;
        GameSettings.arcadeMode = false;

        LevelSelect.SetActive(true);
        Versus.SetActive(true);
        Credits.SetActive(true);
        Exit.SetActive(true);
        BackButton.SetActive(true);
        SinglePlayer.SetActive(false);
        TwoPlayer.SetActive(false);
    }

    public void Back()
    {
        GameSettings.singlePlayerMode = false;
        GameSettings.competetiveMode = false;
        GameSettings.arcadeMode = false;

        LevelSelect.SetActive(false);
        Versus.SetActive(false);
        Credits.SetActive(false);
        Exit.SetActive(false);
        BackButton.SetActive(false);
        SinglePlayer.SetActive(true);
        TwoPlayer.SetActive(true);
    }

}
