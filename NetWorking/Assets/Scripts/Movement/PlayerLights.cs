using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLights : NetBehaviour
{
    [SerializeField] GameObject baseLight;
    [SerializeField] GameObject torch;
    public override void OnStart()
    {
        InitData(new List<(System.Func<object, Packet>, System.Action<Packet>)>(){
            (ClientTogleLight,ServerHandleTogleLight),
            (ServerTogleLight,ClientHandleTogleLight),
        });
        Debug.Log(parent.ownerId+" "+Client.id);
        if(!hasAuthority){
            baseLight.SetActive(false);
        }
    }
    void Update()
    {
        if(!hasAuthority)return;
        if(Input.GetMouseButtonDown(1)){
            if(IsServer){
                ToClients(ServerTogleLight,safe:true);
            }else{
                ToServer(ClientTogleLight,safe: true);
            }
        }
    }
    public Packet ClientTogleLight(object val){
        return null;
    }
    public void ServerHandleTogleLight(Packet packet){
        ToClients(ServerTogleLight,safe:true);
    }
    public Packet ServerTogleLight(object val){
        Packet packet=new Packet();
        torch.SetActive(!torch.activeSelf);
        packet.Write(torch.activeSelf);
        return packet;
    }
     public void ClientHandleTogleLight(Packet packet){
         torch.SetActive(packet.ReadBool());
    }
}
