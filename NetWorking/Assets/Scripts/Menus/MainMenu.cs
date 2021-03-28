using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MainMenu : MonoBehaviour
{
    public GameObject mainPage;
    public GameObject lobbyPage;
    public GameObject clientDataPage;
    public GameObject serverDataPage;
    [SerializeField] TMP_InputField serverNameInputField;
    [SerializeField] TMP_InputField clientNameInputField;
    [SerializeField] TMP_InputField ipInputField;
    [SerializeField] Button returnBtn;
    private void Awake()
    {
        NetworkManager.OnSuccesStart+=SetLobby;
        NetworkManager.OnEnd+=SetMain;
    }
    void Start()
    {
        SetOnTop(mainPage);
    }
    private void OnDestroy()
    {
        NetworkManager.OnSuccesStart-=SetLobby;
        NetworkManager.OnEnd-=SetMain;

    }
    public void SetOnTop(GameObject obj){
        obj.SetActive(true);
        if(obj!=lobbyPage){
            lobbyPage.SetActive(false);
        }
        if(obj!=clientDataPage){
            clientDataPage.SetActive(false);
        }
        if(obj!=serverDataPage){
            serverDataPage.SetActive(false);
        }
        returnBtn.gameObject.SetActive(false);
        if(obj!=mainPage){
            mainPage.SetActive(false);
            returnBtn.gameObject.SetActive(true);
        }
    }
    void SetMain(){
        SetOnTop(mainPage);
    }
    void SetLobby(){
        SetOnTop(lobbyPage);
    }
    #region  BTns
    public void StartServerBtn(){
        SetOnTop(serverDataPage);
    }
    public void StartServer(){
        if(NetworkManager.instance.SetName(serverNameInputField.text)){
            NetworkManager.instance.StartServer();
        }
    }
    public void StartClientBtn(){
        SetOnTop(clientDataPage);
    }
    public void ConnectClient(){
        if(NetworkManager.instance.SetName(clientNameInputField.text)){
            NetworkManager.instance.StartClient(ipInputField.text);
        }
    }
    public void ReturnBtn(){
        NetworkManager.instance.StopAll();
        SetOnTop(mainPage);
    }
    public void StartGameBtn(){
        GameManager.instance.ServerStartGame();
    }
    #endregion

}
