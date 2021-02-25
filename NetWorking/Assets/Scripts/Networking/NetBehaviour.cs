using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
[RequireComponent(typeof(NetIdentity))]
public class NetBehaviour : MonoBehaviour
{
    public static Dictionary<int , NetBehaviour> behaviours=new Dictionary<int, NetBehaviour>();
    public Dictionary<string, SenderData> senders=new Dictionary<string, SenderData>();
    public Dictionary<int, Action<Packet>> handlers=new Dictionary<int, Action<Packet>>();
    public static int nextId=1;
   // [SerializeField ]Color color;
    public int id;
    NetIdentity parent;
    protected bool IsClient{
        get{return NetworkManager.isClient();}
    }
    protected bool IsServer{
        get{return NetworkManager.isServer();}
    }
    protected bool hasAuthority{
        get{
            return parent.id<0||parent.ownerId==Client.id;}
    }
    private void Awake()
    {   
       InitData(
           new List<(Func<object, Packet>,Action<Packet>)>(){
               (SendTestMethod,HandleTestMethod),
           }
       );
    }
    protected virtual void InitData(List<(Func<object, Packet>,Action<Packet>)> funcs){
        for(int i=0;i<funcs.Count;i++){
            senders.Add(funcs[i].Item1.Method.Name,new SenderData(i,funcs[i].Item1));
            handlers.Add(i,funcs[i].Item2);
        }
    }
    public virtual Packet SendTestMethod(object obj){
        Color color=(Color)obj;
        Packet packet=new Packet();
        packet.Write(color.r);
        packet.Write(color.g);
        packet.Write(color.b);
        return packet;
    }
    public virtual void HandleTestMethod(Packet obj){
        if(TryGetComponent<Renderer>(out Renderer ren)){
            float r=obj.ReadFloat();
            float g=obj.ReadFloat();
            float b=obj.ReadFloat();

            ren.material.color=new Color(r,g,b);
        }
    }
   
    #region NetworkHandlers
    public void ToServer(Func<object,Packet> fun,object val,bool execute=true,bool safe=false){
        Packet data=fun(val);
        ClientSend.SendExecuteFunc(id,senders[fun.Method.Name].id,safe,data.ToArray());
        if(execute){
            handlers[senders[fun.Method.Name].id](data);
        }
    }
    public void FromServer(Packet packet){
        int funId=packet.ReadInt();
        handlers[funId](packet);
    }
    public void ToClients(Func<object,Packet> fun,object val,bool execute=true,bool safe=false){
        Packet data=fun(val);
        ServerSend.SendExecuteFunc(id,senders[fun.Method.Name].id,safe,data.ToArray());
        if(execute){
            handlers[senders[fun.Method.Name].id](data);
        }
    }
    public void ToClients(int funId,Packet packet,bool safe=false){
        ServerSend.SendExecuteFunc(id,funId,safe,packet.ToArray());
    }
    public void FromClient(int fromClient,Packet packet){
        int funId=packet.ReadInt();
        using(Packet packet1=new Packet(packet.ReadBytes())){
            handlers[funId](packet1);
            ToClients(funId,packet1);
        }
    }
    #endregion
    public void Init(NetIdentity identity){
        this.id=nextId;
        nextId++;
        this.parent=identity;
        behaviours.Add(id,this);
    }
    public void Init(int id,NetIdentity identity){
        this.id=id;
        this.parent=identity;
        behaviours.Add(id,this);
    }
    public void Remove(){
        behaviours.Remove(id);
    }
}
public class SenderData{
    public int id;
    public Func<object,Packet> func;
    public SenderData(int id,Func<object,Packet> fun){
        this.func=fun;
        this.id=id;
    }
}

