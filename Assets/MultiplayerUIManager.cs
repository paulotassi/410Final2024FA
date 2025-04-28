using UnityEngine;
using Unity.Netcode;

public class MultiplayerUIManager : NetworkBehaviour
{
    public GameObject SessionList;
    public GameObject CreateSessionUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void JoinSessionUIHide()
    {
        SessionList.SetActive(false);
        CreateSessionUI.SetActive(false);
    }
       

}
