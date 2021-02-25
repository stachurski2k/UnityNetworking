using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetTransform : NetBehaviour
{
   Vector3 previusPos=new Vector3();
   private void Awake()
   {
       InitData(new List<(System.Func<object, Packet>, System.Action<Packet>)>(){
           (SendPos,SetPos),
       });
   }
   private void Update()
   {
       if(IsServer){
           if(previusPos!=transform.position){
               previusPos=transform.position;
               ToClients(SendPos,previusPos,false);
           }
       }
   }
   public Packet SendPos(object val){
        Vector3 pos=(Vector3) val;
        Packet packet=new Packet();
        packet.Write(pos);
        return packet;
   }
   public void SetPos(Packet val){
       Vector3 pos=val.ReadVector3();
       this.transform.position=pos;
   }
}
