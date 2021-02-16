using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayer : MonoBehaviour
{
    public static Dictionary<int,NetPlayer> players=new Dictionary<int, NetPlayer>();
    public static int nextID=1;
    public int id;
    public string playerName;
    private void Awake()
    {
        id=nextID;
    }
    public void Init(int id,string playerName){
        this.id=id;
        this.playerName=playerName;
        players.Add(id,this);
    }
}
