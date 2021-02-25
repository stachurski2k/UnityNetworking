using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthoritiveBtn : MonoBehaviour
{
    Button button;
    private void Awake()
    {
        button=GetComponent<Button>();
        NetworkManager.OnSuccesStart+=Enable;
        button.interactable=false;
    }
    private void OnDestroy()
    {
        NetworkManager.OnSuccesStart-=Enable;
    }
    private void Enable()
    {
        Debug.Log("lol");
        if(NetworkManager.isServer()){
            button.interactable=true;
        }else{
            button.interactable=false;
        }
    }
}
