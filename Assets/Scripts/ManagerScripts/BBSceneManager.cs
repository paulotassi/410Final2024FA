using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BBSceneManager : MonoBehaviour
{
    [SerializeField] private TitleEventSystemManager eventSystemManager;

    [SerializeField] public GameObject SinglePlayer;
    [SerializeField] public GameObject TwoPlayer;

    [SerializeField] public GameObject LevelSelectButton;
    [SerializeField] public GameObject LevelSelect;
    [SerializeField] public GameObject Versus;
    [SerializeField] public GameObject Credits;
    [SerializeField] public GameObject Upgrades;       // "Upgrades" menu button
    [SerializeField] private UpgradeShop upgradeShop;
    [SerializeField] public GameObject Exit;
    [SerializeField] public GameObject BackButton;
    [SerializeField] public GameObject Tutorial;
    [SerializeField] public GameObject TutorialPanel;
    [SerializeField] public GameObject TutorialPanelArcade;


    //Eventual intent is to make the play to go as follows. Player hits initial button choice bringing you to either coop gamestate. when players finish a round they returnt to titlescreen and the button changes from coopmode to continue coop
    //A state will exist that will continue guiding players through their playthrough. an additional button will appear that will say to reset coop run or reset comp run to bring player game states back to first level.
    private void Start()
    {
        LevelSelect.SetActive(false);
        Versus.SetActive(false);
        Credits.SetActive(false);
        if (Upgrades != null) Upgrades.SetActive(false);
        if (upgradeShop != null) upgradeShop.Close();
        Exit.SetActive(true);
        BackButton.SetActive(false);
        TutorialPanel.SetActive(false);
        TutorialPanelArcade.SetActive(false);

        GameSettings.proceduralMode = false;
        RefreshBossLock();
    }
    // Level select buttons: 1 = Small procedural level, 2 = Large procedural level, 3 = Boss (locked until enough is banked)
    private const string ProceduralSceneName = "ProceduralLevel";
    [SerializeField] private int smallMapSize = 3;
    [SerializeField] private int largeMapSize = 8;
    [SerializeField] public UnityEngine.UI.Button bossLevelButton;

    private void RefreshBossLock()
    {
        // Find the level buttons under the (inactive) LevelSelect panel and label them
        foreach (UnityEngine.UI.Button b in FindObjectsByType<UnityEngine.UI.Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            TMPro.TMP_Text text = b.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (text == null) continue;
            if (b.name == "Level 1") text.text = "Small Level";
            else if (b.name == "Level 2") text.text = "Large Level";
            else if (b.name == "Level 3" && bossLevelButton == null) bossLevelButton = b;
        }

        if (bossLevelButton == null) return;

        bool unlocked = SaveManager.BossUnlocked;
        bossLevelButton.interactable = unlocked;

        TMPro.TMP_Text label = bossLevelButton.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (label != null)
        {
            label.text = unlocked
                ? "Boss Level"
                : "Boss Locked " + SaveManager.Data.totalBanked + "/" + SaveManager.BossUnlockTotal;
        }
    }

    private void LoadProcedural(int size)
    {
        GameSettings.proceduralMode = true;
        GameSettings.proceduralMapSize = size;
        SceneManager.LoadScene(ProceduralSceneName);
    }

    public void LoadCoopPlaythrough1()
    {
        LoadProcedural(smallMapSize);
    }

    public void LoadCoopPlaythrough2()
    {
        LoadProcedural(largeMapSize);
    }

    public void LoadCooperativeBossPlaythrough()
    {
        if (!SaveManager.BossUnlocked) return;

        GameSettings.proceduralMode = false;
        SceneManager.LoadScene("BossLevel");
    }

    public void LoadVersusPlaythrough()
    {
        GameSettings.competetiveMode = true;
        GameSettings.proceduralMode = false;
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
        GameSettings.arcadeMode = true;

        LevelSelect.SetActive(true);
        Versus.SetActive(false);
        Credits.SetActive(true);
        if (Upgrades != null) Upgrades.SetActive(true);
        Exit.SetActive(true);
        BackButton.SetActive(true);
        SinglePlayer.SetActive(false);
        TwoPlayer.SetActive(false);

        eventSystemManager.NewButton = LevelSelectButton;
        eventSystemManager.UpdateFirstSelected();

    }

    public void StartTwoPlayerMode()
    {
        GameSettings.singlePlayerMode = false;
        GameSettings.competetiveMode = false;
        GameSettings.arcadeMode = true;

        LevelSelect.SetActive(true);
        Versus.SetActive(true);
        Credits.SetActive(true);
        if (Upgrades != null) Upgrades.SetActive(true);
        Exit.SetActive(true);
        BackButton.SetActive(true);
        SinglePlayer.SetActive(false);
        TwoPlayer.SetActive(false);

        eventSystemManager.NewButton = LevelSelectButton;
        eventSystemManager.UpdateFirstSelected();
    }

    public void HowToPlay() 
    {
        bool isActive = TutorialPanel.activeSelf;
        TutorialPanel.SetActive(!isActive);
    }

    public void HowToPlayArcade()
    {
        bool isActive = TutorialPanelArcade.activeSelf;
        TutorialPanelArcade.SetActive(!isActive);
    }

    public void Back()
    {
        GameSettings.singlePlayerMode = false;
        GameSettings.competetiveMode = false;
        GameSettings.arcadeMode = true;

        LevelSelect.SetActive(false);
        Versus.SetActive(false);
        Credits.SetActive(false);
        if (Upgrades != null) Upgrades.SetActive(false);
        if (upgradeShop != null) upgradeShop.Close();
        BackButton.SetActive(false);
        SinglePlayer.SetActive(true);
        TwoPlayer.SetActive(true);

        eventSystemManager.NewButton = SinglePlayer; 
        eventSystemManager.UpdateFirstSelected();
    }

}
