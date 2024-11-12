using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using WebSocketSharp;

public class Connection : MonoBehaviourPunCallbacks
{
    public Button button;
    private bool isLoading = false;
    public GameObject inputField;
    private string nickname;
    private void Start()
    {
        nickname = "";
        inputField = inputField.transform.GetChild(0).GetChild(2).gameObject;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Update()
    {
        SaveNickname();
        if (!isLoading && PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount>1)
        {
            isLoading = true;
            PhotonNetwork.LoadLevel(1);
        }
    }
    override
    public void OnConnectedToMaster()
    {
        button.interactable = true;
    }
    public void OnClickButton()
    {
        nickname = inputField.GetComponent<TMP_Text>().text;
        Debug.Log((int)nickname.ToCharArray()[0]);

        if (nickname.IsNullOrEmpty() || nickname.ToCharArray()[0]==8203)
        {
            return;
        }

        PhotonNetwork.JoinOrCreateRoom("sala1",new RoomOptions(),TypedLobby.Default);
        button.interactable = false;
    }
    private void SaveNickname()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerPrefs.SetString("nickname1", nickname);
        } else
        {
            PlayerPrefs.SetString("nickname2", nickname);
        }
    }
}
