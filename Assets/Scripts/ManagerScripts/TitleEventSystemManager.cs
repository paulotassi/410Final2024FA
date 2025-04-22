using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;


public class TitleEventSystemManager : MonoBehaviour
{

    public GameObject NewButton;
    public GameObject BackButton;
   

    public void UpdateFirstSelected()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem != null && NewButton != null)
        {
            eventSystem.SetSelectedGameObject(NewButton);
            eventSystem.firstSelectedGameObject = NewButton;
        }
    }

    public void UpdateFirstSelectedAfterBack()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem != null && BackButton != null)
        {
            eventSystem.SetSelectedGameObject(BackButton);
            eventSystem.firstSelectedGameObject = BackButton;
        }
    }

}
    
