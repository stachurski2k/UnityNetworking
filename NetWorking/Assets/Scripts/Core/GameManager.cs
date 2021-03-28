using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    [SerializeField] bool addNewPlayersInGame=false;
    [SerializeField] NetIdentity playerPrefab;
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
        Server.CanAcceptClients=addNewPlayersInGame;
        NetworkManager.instance.LoadOnlineScene();
        ServerSend.StartGame();
        foreach(var player in NetPlayer.players.Values){
            NetworkManager.instance.ServerSpawn(playerPrefab.prefabID,player.id,Vector3.zero);
        }
    }
    #endregion
    #region Client
    public void ClientStartGame(){
        if(!NetworkManager.isClient())return;
        NetworkManager.instance.LoadOnlineScene();
    }
    #endregion
}
