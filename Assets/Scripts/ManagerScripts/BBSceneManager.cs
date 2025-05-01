using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BBSceneManager : MonoBehaviour
{
    [SerializeField] public GameObject SinglePlayer;
    [SerializeField] public GameObject TwoPlayer;
    [SerializeField] public GameObject Level1Object;
    [SerializeField] public GameObject versusObject;
    [SerializeField] public GameObject LevelSelect;
    [SerializeField] public GameObject LevelSelectButton;
    [SerializeField] public GameObject Versus;
    [SerializeField] public GameObject Credits;
    [SerializeField] public GameObject Exit;
    [SerializeField] public GameObject BackButton;
    [SerializeField] public CreditManager CM;


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

    private void Awake()
    {
        StartCoroutine(SelectDefault(SinglePlayer));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            Debug.Log("Selected object: " + (selected ? selected.name : "null"));
        }
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
        GameSettings.competetiveMode = true;
        SceneManager.LoadScene("ArenaScene");
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    public void StartCredit()
    {
        CM.StartCredits();
    }
    public void StartSinglePlayerMode()
    {
        GameSettings.singlePlayerMode = true;
        GameSettings.competetiveMode = false;
        

        LevelSelect.SetActive(true);
        StartCoroutine(SelectDefault(LevelSelectButton));
        
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
        GameSettings.competetiveMode = false;
        

        LevelSelect.SetActive(true);
        StartCoroutine(SelectDefault(LevelSelectButton));

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
        

        StartCoroutine(SelectDefault(SinglePlayer));

        LevelSelect.SetActive(false);
        Versus.SetActive(false);
        Credits.SetActive(false);
        Exit.SetActive(false);
        BackButton.SetActive(false);
        SinglePlayer.SetActive(true);
        TwoPlayer.SetActive(true);
    }

    public void coopPanelSelector()
    {
        StartCoroutine(SelectDefault(Level1Object));
    }

    public void versusPanelSelector()
    {
        StartCoroutine(SelectDefault(versusObject));
    }
    private System.Collections.IEnumerator SelectDefault(GameObject target)
    {
        Debug.Log("running the button");
        yield return null; // Wait 1 frame
        EventSystem.current.SetSelectedGameObject(null); // Clear selection
        EventSystem.current.SetSelectedGameObject(target); // Select the new one
    }

}
