using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetTransform : NetBehaviour
{
   Vector3 previusPos=new Vector3();
   Quaternion previousRot=new Quaternion();
   public override void OnStart(){
       InitData(new List<(System.Func<object, Packet>, System.Action<Packet>)>(){
           (SendPos,SetPos),
           (SendRot,SetRot),
       });
   }
   protected void FixedUpdate()
   {
       if(IsServer){
           if(previusPos!=transform.position){
               previusPos=transform.position;
               ToClients(SendPos,previusPos,false);
           }
           if(previousRot!=transform.rotation){
               previousRot=transform.rotation;
               ToClients(SendRot,previousRot);
           }
       }
   }
   public virtual Packet SendRot(object val){
       Packet packet=new Packet();
       packet.Write((Quaternion)val);
       return packet;
   }
   public virtual void SetRot(Packet packet){
       Quaternion rot=packet.ReadQuaternion();
       this.transform.rotation=rot;
   }
   public virtual Packet SendPos(object val){
        Vector3 pos=(Vector3) val;
        Packet packet=new Packet();
        packet.Write(pos);
        return packet;
   }
   public virtual void SetPos(Packet val){
       Vector3 pos=val.ReadVector3();
       this.transform.position=pos;
   }
}
