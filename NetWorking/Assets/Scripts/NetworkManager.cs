using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
public class NetworkManager : MonoBehaviour
{
    [SerializeField] string ipToConnect="127.0.0.1";
    int i;
    public static NetworkManager instance;
    public static int bufferSize=2048;
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
    public bool working=true;
    public static bool isWorking(){
        //for now
        return instance.working;
    }
    private void Start()
    {
        //Client.ConnectToServer(ipToConnect);
        Server.StartServer();
        Sender.StartTcp();
        Sender.StartUdp(null);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S)){
            Client.tcp.SendData("Helllow"+i);
            i++;
            print(i);
        }
        if(Input.GetKeyDown(KeyCode.W)){
            StopAll();
        }
        if(Input.GetKeyDown(KeyCode.A)){
        }
    }
    private void OnApplicationQuit()
    {
        StopAll();
    }
    public void StopAll(){
        working=false;
        Server.StopServer();
        Sender.Stop();
    }
    #region Client
    public virtual void OnServerConnected(bool result){
        if(result){
            //load online scene or lobby
        }else{

        }
    }
    #endregion
}
