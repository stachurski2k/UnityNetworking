using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if(instance==null){
            instance=this;
        }
        else{
            Destroy(this);
        }
    }
    #region Server
    public void ServerStartGame(){
        if(!NetworkManager.isServer())return;
        Server.CanAcceptClients=false;
        NetworkManager.instance.LoadOnlineScene();
        ServerSend.StartGame();
    }
    #endregion
    #region Client
    public void ClientStartGame(){
        if(!NetworkManager.isClient())return;
        NetworkManager.instance.LoadOnlineScene();
    }
    #endregion
}
