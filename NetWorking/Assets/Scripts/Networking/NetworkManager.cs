using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
public class NetworkManager : MonoBehaviour
{
    [SerializeField] string ipToConnect="127.0.0.1";
    [SerializeField] NetPlayer localPlayerPrefab;
    [SerializeField] NetPlayer playerPrefab;
    public string playerName;
    public static NetworkManager instance;
    public static event Action<NetPlayer> OnPlayerCreated;
    public static event Action<NetPlayer> OnPlayerDestroyed;
    public static event Action OnSuccesStart;
    public static int bufferSize=2048;
    private void Awake()
    {
        //DontDestroyOnLoad(this.gameObject);
        if(instance==null){
            instance=this;
        }
        else{
            Destroy(this);
        }
    }
    public static bool isServer(){
        return Server.isRunning;
    }
    public static bool isClient(){
        return Client.isConnected;
    }
    public static bool isWorking(){
        return isServer()||isClient();
    }
    private void OnApplicationQuit()
    {
        StopAll();
    }
    #region Shared
    public bool SetName(string newName){
        if(string.IsNullOrEmpty(newName)||string.IsNullOrWhiteSpace(newName)){return false;}
        if(newName.Length<=3||newName.Length>=11){return false;}
        playerName=newName;
        return true;
    }
    public void StopAll(){
        if(isServer()){
            Server.StopServer();
        }else if(isClient()){
            Client.Disconnect();
        }
    }
    void DestroyAllPlayers(){
        foreach (var p in NetPlayer.players.Values)
        {
            Destroy(p.gameObject);
            InvokeOnPlayerDestroyed(p);
        }
        NetPlayer.players.Clear();
    }
    #endregion
    #region  Server
    public void StartServer(){
        if(isWorking())return;
        Server.OnStartServer+=HandleOnStartServer;
        Server.OnStopServer+=HandleOnStopServer;
        ServerClient.ServerOnClientConnected+=ServerHandleClientConnected;
        ServerClient.ServerOnClientDisconnected+=ServerHandleClientDisconnected;
        Server.StartServer();
    }
    void HandleOnStartServer(bool result){
        if(result){
            ServerCreateLocalPlayer();
            InvokeOnSuccessStart();
            Sender.StartTcp();
        }else{
            ServerClient.ServerOnClientConnected-=ServerHandleClientConnected;
            ServerClient.ServerOnClientDisconnected-=ServerHandleClientDisconnected;
        }
        Server.OnStartServer-=HandleOnStartServer;
    }
    void HandleOnStopServer(){
        Server.OnStopServer+=HandleOnStopServer;
        ServerClient.ServerOnClientConnected-=ServerHandleClientConnected;
        ServerClient.ServerOnClientDisconnected-=ServerHandleClientDisconnected;
        Sender.Stop();
        DestroyAllPlayers();
    }
    void ServerHandleClientConnected(ServerClient client, string name){
        ServerCreatePlayer(client.id,name);
    }
    void ServerHandleClientDisconnected(ServerClient client){
        ServerDestroyPlayer(client);
    }
    public void ServerCreatePlayer(int id,string pName){
        NetPlayer player=Instantiate<NetPlayer>(playerPrefab);
        player.Init(id,pName);
        InvokeOnPlayerCreated(player);
    }
    public void ServerCreateLocalPlayer(){
        NetPlayer player=Instantiate<NetPlayer>(localPlayerPrefab);
        player.Init(0,playerName);
        InvokeOnPlayerCreated(player);
    }
    public void ServerDestroyPlayer(ServerClient client){
        if(NetPlayer.players.TryGetValue(client.id,out NetPlayer player)){
            NetPlayer.players.Remove(client.id);
            InvokeOnPlayerDestroyed(player);
            GameObject.Destroy(player.gameObject);
        }
    }
    public void ServerDisconnectClient(int clientId){
        Server.clients[clientId].Disconnect();
    }
    #endregion
    #region Client
      public void StartClient(string ip){
        if(isWorking())return;
        Client.OnServerConnected+=ClientHandleServerConnected;
        Client.OnServerDisconnected+=ClientHandleServerDisconnected;
        Client.ConnectToServer(ip);
    }
    public virtual void ClientHandleServerConnected(bool result){
        if(result){
            //load online scene or lobby
            InvokeOnSuccessStart();
            Sender.StartTcp();
        }else{
          
        }
        Client.OnServerConnected-=ClientHandleServerConnected;
    }
    public virtual void ClientHandleServerDisconnected(){
        Client.OnServerDisconnected-=ClientHandleServerDisconnected;
        Sender.Stop();
        DestroyAllPlayers();
    }
    public void ClientCreatePlayer(int id,string pName){
        NetPlayer player=playerPrefab;
        if(id==Client.id){
            player=localPlayerPrefab;
        }
        NetPlayer _p=Instantiate<NetPlayer>(player);
        _p.Init(id,pName);
        InvokeOnPlayerCreated(_p);
    }
    public void ClientDespawnPlayer(int id){
        ThreadManager.ExecuteOnMainThread(()=>{
            if(id==Client.id){
                Client.Disconnect();
            }else{
                if(NetPlayer.players.TryGetValue(id,out NetPlayer player)){
                    NetPlayer.players.Remove(id);
                    InvokeOnPlayerDestroyed(player);
                    Destroy(player.gameObject);
                }
            }
        });
    }
    #endregion
 
    #region Invokers
    void InvokeOnPlayerCreated(NetPlayer player){
        if(OnPlayerCreated!=null)
            OnPlayerCreated(player);
    }
    void InvokeOnPlayerDestroyed(NetPlayer player){
        if(OnPlayerDestroyed!=null)
            OnPlayerDestroyed(player);
    }
    void InvokeOnSuccessStart(){
        if(OnSuccesStart!=null){
            OnSuccesStart();
        }
    }
    #endregion
}
