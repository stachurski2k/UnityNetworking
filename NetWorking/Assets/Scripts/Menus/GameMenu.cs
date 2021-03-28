using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    [SerializeField] GameObject menuObj;
    [SerializeField] GameObject[] panels;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            menuObj.SetActive(!menuObj.activeSelf);
        }
    }
    public void SetPanelActive(GameObject i){
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
        i.SetActive(true);
    }
}
