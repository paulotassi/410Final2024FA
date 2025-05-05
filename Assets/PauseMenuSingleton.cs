using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuSingleton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject resumeButtonObject;

    public void Awake()
    {
        StartCoroutine(SelectDefault(resumeButtonObject));
    }
    private System.Collections.IEnumerator SelectDefault(GameObject target)
    {
        Debug.Log("running the button");
        yield return null; // Wait 1 frame
        //EventSystem.current.SetSelectedGameObject(null); // Clear selection
        EventSystem.current.SetSelectedGameObject(target); // Select the new one
    }

}
