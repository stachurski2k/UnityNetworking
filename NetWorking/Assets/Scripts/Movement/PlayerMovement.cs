using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(NetTransform))]
public class PlayerMovement : NetBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float rotSpeed;
    [SerializeField] Transform body;
    NetTransform netTransform;
    Camera cam;
    bool[] inputs=new bool[4];
    Rigidbody2D rBody;
    public override void OnStart()
    {
        InitData(new List<(System.Func<object, Packet>, System.Action<Packet>)>(){
           (SendInput,HandleInput),
           (SendRot,HandleRot),
       });
       DontDestroyOnLoad(gameObject);
       netTransform=GetComponent<NetTransform>();
       cam=GetComponentInChildren<Camera>();
       rBody=GetComponent<Rigidbody2D>();
       if(!hasAuthority){
           cam.gameObject.SetActive(false);
       }
    }
   public Packet SendInput(object val){
        Packet packet=new Packet();
        packet.Write(inputs[0]);
        packet.Write(inputs[1]);
        packet.Write(inputs[2]);
        packet.Write(inputs[3]);
        return packet;
   }
   public void HandleInput(Packet packet){
       inputs[0]=packet.ReadBool();
       inputs[1]=packet.ReadBool();
       inputs[2]=packet.ReadBool();
       inputs[3]=packet.ReadBool();
   }
   public Packet SendRot(object val){
       Packet packet=new Packet();
       packet.Write((Quaternion)val);
       return packet;
   }
   public void HandleRot(Packet packet){
       Quaternion rot=packet.ReadQuaternion();
       body.transform.rotation=rot;
   }
   private void Update()
   {
       if(!hasAuthority)return;
    //    float angle=Vector2.Angle(transform.forward,Input.mousePosition);
        Vector2 direction= cam.ScreenToWorldPoint(Input.mousePosition)-transform.position;
        float angle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90f;
        Quaternion rotation=Quaternion.AngleAxis(angle,Vector3.forward);
        body.transform.rotation=Quaternion.Slerp(body.rotation,rotation,rotSpeed*Time.deltaTime);
   }
   private void FixedUpdate()
   {
        if(hasAuthority){
            inputs[0]=Input.GetKey(KeyCode.W);
            inputs[1]=Input.GetKey(KeyCode.S);
            inputs[2]=Input.GetKey(KeyCode.A);
            inputs[3]=Input.GetKey(KeyCode.D);
        }
        if(IsServer){
            Vector2 inputDir=Vector2.zero;
            if(inputs[0]){
                inputDir.y+=1;
            }
            if(inputs[1]){
                inputDir.y-=1;
            }
            if(inputs[2]){
                inputDir.x-=1;
            }
            if(inputs[3]){
                inputDir.x+=1;
            }
            rBody.MovePosition(rBody.position+(Vector2.up*inputDir.y+Vector2.right*inputDir.x)*moveSpeed*Time.fixedDeltaTime);
            ToClients(SendRot,body.transform.rotation,false,false,parent.ownerId);

        }else if(hasAuthority){
            ToServer(SendInput);
            ToServer(SendRot,body.transform.rotation);
        }
   }
}
