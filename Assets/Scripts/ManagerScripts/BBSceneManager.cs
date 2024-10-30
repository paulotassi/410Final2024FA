using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BBSceneManager : MonoBehaviour
{
    //Eventual intent is to make the play to go as follows. Player hits initial button choice bringing you to either coop gamestate. when players finish a round they returnt to titlescreen and the button changes from coopmode to continue coop
    //A state will exist that will continue guiding players through their playthrough. an additional button will appear that will say to reset coop run or reset comp run to bring player game states back to first level.
    public void LoadCompetetivePlaythrough()
    {
        SceneManager.LoadScene("CompetetiveSceneRound1");
    }

    public void LoadCooperativePlaythrough()
    {
        SceneManager.LoadScene("BossLevel");
    }

    public void LoadComboPlaythrough()
    {
        SceneManager.LoadScene("MVPScene");
    }
}
