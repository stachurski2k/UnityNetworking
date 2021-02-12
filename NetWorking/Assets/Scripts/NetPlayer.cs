using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayer : MonoBehaviour
{
    public static Dictionary<int,NetPlayer> players=new Dictionary<int, NetPlayer>();
    public static int nextID=1;
    public int id;
    private void Awake()
    {
        id=nextID;
    }
    public void Init(int id){
        this.id=id;
        players.Add(id,this);
    }
}
