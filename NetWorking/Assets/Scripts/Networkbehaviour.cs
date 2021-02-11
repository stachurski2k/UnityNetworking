using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Networkbehaviour : MonoBehaviour
{
    

    void RunMethodAtIndex(int index,params object[] data){
        var methods=this.GetType().GetMethods();
        for (int i = 0; i < methods.Length; i++)
        {
            if(i==index){
                methods[i].Invoke(this,data);
            }
        }
    }
}
