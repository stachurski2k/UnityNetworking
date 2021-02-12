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
    int i;
    public static NetworkManager instance;
    public static event Action<NetPlayer> OnPlayerCreated;
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
        working=false;
    }
    public bool working=false;
    public static bool isWorking(){
        //for now
        return instance.working;
    }
    private void Start()
    {
        //Client.ConnectToServer(ipToConnect);
        //Server.StartServer();
       // Sender.StartTcp();
        //Sender.StartUdp(null);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.W)){
            StopAll();
        }
    }
    private void OnApplicationQuit()
    {
        StopAll();
    }
    #region  Server
    public void StartServer(){
        if(working)return;
        working=true;
        ServerCreateLocalPlayer();
        Server.StartServer();
        Sender.StartTcp();
    }
    public void ServerCreatePlayer(int id){
        NetPlayer player=Instantiate<NetPlayer>(playerPrefab);
        player.Init(id);
        InvokeOnPlayerCreated(player);
    }
    public void ServerCreateLocalPlayer(){
        NetPlayer player=Instantiate<NetPlayer>(localPlayerPrefab);
        player.Init(0);
        InvokeOnPlayerCreated(player);
    }
    #endregion
    #region Client
      public void StartClient(string ip){
        if(working)return;
        working=true;
        Client.ConnectToServer(ip);
        Sender.StartTcp();
    }
    public virtual void OnServerConnected(bool result){
        if(result){
            //load online scene or lobby
        }else{

        }
    }
    public void ClientCreatePlayer(int id){
        NetPlayer player=playerPrefab;
        if(id==Client.id){
            player=localPlayerPrefab;
        }
        NetPlayer _p=Instantiate<NetPlayer>(player);
        _p.Init(id);
        InvokeOnPlayerCreated(_p);
    }
    #endregion
     public void StopAll(){
        working=false;
        Server.StopServer();
        Sender.Stop();
    }
    #region Invokers
    void InvokeOnPlayerCreated(NetPlayer player){
        if(OnPlayerCreated!=null)
            OnPlayerCreated(player);
    }
    #endregion
}
