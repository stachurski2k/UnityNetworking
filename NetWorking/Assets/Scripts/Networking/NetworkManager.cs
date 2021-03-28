using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System;
public class NetworkManager : MonoBehaviour
{
    [SerializeField] string ipToConnect="127.0.0.1";
    [SerializeField] bool authoritiveClients=false;
    [SerializeField] int MaxPlayers=5;
    public string playerName;
    [SerializeField] NetPlayer localPlayerPrefab;
    [SerializeField] NetPlayer playerPrefab;
    [SerializeField] string onlineScene,offlineScene;
    [SerializeField] NetIdentity[] spawnablePrefabs;
    public static NetworkManager instance;
    public static event Action<NetPlayer> OnPlayerCreated;
    public static event Action<NetPlayer> OnPlayerDestroyed;
    public static event Action OnSuccesStart;
    public static event Action OnEnd;
    public static int bufferSize=2048;
    private void Awake()
    {
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else{
            Destroy(this.gameObject);
        }
        for(int i=0;i<spawnablePrefabs.Length;i++){
            spawnablePrefabs[i].prefabID=i;
        }
    }
    public static bool isServer(){
        return Server.isRunning;
    }
    public static bool isClient(){
        return Client.isConnected;
    }
    public static bool isWorking(){
        return isServer()||isClient()||Client.isConnecting;
    }
    private void OnApplicationQuit()
    {
        StopAll();
    }
    #region Shared
    public void LoadOnlineScene(){
        SceneManager.LoadScene(onlineScene);
    }
    public void LoadOfflineScene(){
        SceneManager.LoadScene(offlineScene);
    }
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
            InvokeOnPlayerDestroyed(p);
            Destroy(p.gameObject);
        }
        NetPlayer.players.Clear();
    }
    #endregion
    #region  Server
    public void StartServer(){
        if(isWorking())return;
        Server.OnStartServer+=HandleOnStartServer;
        Server.StartServer(MaxPlayers);
    }
    void HandleOnStartServer(bool result){
        if(result){
            ServerCreateLocalPlayer();
            InvokeOnSuccessStart();
            Server.OnStopServer+=HandleOnStopServer;
        }
        Server.OnStartServer-=HandleOnStartServer;
    }
    void HandleOnStopServer(){
        Server.OnStopServer-=HandleOnStopServer;

        DestroyAllPlayers();
        InvokeOnEnd();
        LoadOfflineScene();
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
    public void HandleRequestSpawn(int fromClient,int prefabID,Vector3 pos=new Vector3()){
        if(!isServer()||!authoritiveClients)return;
        var identity=Instantiate<NetIdentity>(spawnablePrefabs[prefabID],pos,Quaternion.identity);
        identity.SetOwnerID(fromClient);
    }
    public void HandleRequestDestroyID(int id){
        if(!isServer()||!authoritiveClients)return;
        if(NetIdentity.identities.TryGetValue(id, out NetIdentity identity)){
            identity.DestroyID();
        }
    }
    public void ServerSpawn(int prefabID,int owner,Vector3 pos=new Vector3()){
        var identity=Instantiate<NetIdentity>(spawnablePrefabs[prefabID],pos,Quaternion.identity);
        identity.SetOwnerID(owner);
    }
    public void ServerDestroyID(NetIdentity identity){
        if(!isServer())return;
        ServerSend.DestroyID(identity);
    }
    #endregion
    #region Client
      public void StartClient(string ip){
        if(isWorking())return;
        Client.OnServerConnected+=ClientHandleServerConnected;
        Client.ConnectToServer(ip);
    }
    public virtual void ClientHandleServerConnected(bool result){
        if(result){
            InvokeOnSuccessStart();
            Client.OnServerDisconnected+=ClientHandleServerDisconnected;
        }
        Client.OnServerConnected-=ClientHandleServerConnected;
    }
    public virtual void ClientHandleServerDisconnected(){
        Client.OnServerDisconnected-=ClientHandleServerDisconnected;
        DestroyAllPlayers();
        InvokeOnEnd();
        LoadOfflineScene();
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
    public void ClientRequestSpawn(int prefabID,Vector3 pos=new Vector3()){
        ClientSend.RequestSpawn(prefabID,pos);
    }
    // public void ClientRequestSpawn(NetIdentity identity){
    //     ClientSend.RequestSpawn(identity.prefabID,identity.transform.position);
    // }
    public void ClientRequestDestroyID(NetIdentity identity){
        ClientSend.RequestDestroyID(identity.id);
    }
    public void ClientSpawn(int prefabID,int id,int ownerId,Vector3 pos,int[] behavioursIds){
        var identity=Instantiate<NetIdentity>(spawnablePrefabs[prefabID]);
        identity.SetOwnerID(ownerId);
        identity.Init(id,behavioursIds);
        identity.transform.position=pos;
    }
    public void ClientDestroyID(int id){
        if(NetIdentity.identities.TryGetValue(id, out NetIdentity identity)){
        identity.DestroyID();
        }
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
    void InvokeOnEnd(){
        if(OnEnd!=null){
            OnEnd();
        }
    }
    #endregion
}
