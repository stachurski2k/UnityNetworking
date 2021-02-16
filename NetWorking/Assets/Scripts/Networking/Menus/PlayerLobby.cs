using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLobby : MonoBehaviour
{
    [SerializeField] GameObject KickBtn;
    int playeId;
    private void Start()
    {
        if(playeId==0||NetworkManager.isClient())
        KickBtn.gameObject.SetActive(false);
    }
    public void Init(int id){
        playeId=id;
    }
    public void Kick(){
        NetworkManager.instance.ServerDisconnectClient(playeId);
    }
}
