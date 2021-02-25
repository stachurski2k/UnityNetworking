using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetIdentity : MonoBehaviour
{
    public static Dictionary<int,NetIdentity> identities=new Dictionary<int, NetIdentity>();
    public static int nextId=1;
    [HideInInspector]
    public int prefabID;
    public int id=-1;
    [HideInInspector]
    public NetBehaviour[] behaviours;
    bool setupDone=false,tryingToDestroy=false;
    public int ownerId{get; private set;}=-1;

    private void Start()
    {
        if(NetworkManager.isServer()){
            Init(nextId);
            nextId++;
            behaviours=GetComponents<NetBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                behaviours[i].Init(this);
            }
            ServerSend.Spawn(this);
        }else if(NetworkManager.isClient()){
            if(setupDone)return;
            ClientSend.RequestSpawn(this.prefabID , transform.position);
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        identities.Remove(id);
        if(NetworkManager.isServer()&&!tryingToDestroy){
            NetworkManager.instance.ServerDestroyID(this);
        }
        else if(NetworkManager.isClient()&&!tryingToDestroy){
            NetworkManager.instance.ClientRequestDestroyID(this);
        }
        foreach (var b in behaviours)
        {
            b.Remove();
        }
    }
    public void Init(int id){
        this.id=id;
        setupDone=true;
        identities.Add(id,this);
    }
    public void SetOwnerID(int ownerId){
        this.ownerId=ownerId;
    }
    public void Init(int id,int[] behavioursIds){
        this.id=id;
        setupDone=true;
        identities.Add(id,this);
        behaviours=GetComponents<NetBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            behaviours[i].Init(behavioursIds[i],this);
        }
    }
    public void DestroyID(){
        identities.Remove(id);
        tryingToDestroy=true;
        NetworkManager.instance.ServerDestroyID(this);
        Destroy(gameObject);
    }
}
