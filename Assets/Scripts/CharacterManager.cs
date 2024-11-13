using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterManager : MonoBehaviour
{
    public Transform spawn1, spawn2;
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            string nickname = PlayerPrefs.GetString("nickname1");
            GameObject player1 = PhotonNetwork.Instantiate("GasMaskSoldier", spawn1.position, spawn1.rotation);
            player1.GetComponent<PhotonView>().RPC("SetNickname",RpcTarget.All,nickname);
            Debug.Log("ActualNickname: " +nickname);
        } else
        {
            string nickname = PlayerPrefs.GetString("nickname2");
            GameObject player2 = PhotonNetwork.Instantiate("SwatSoldier", spawn2.position, spawn2.rotation);
            player2.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.All, nickname);
            Debug.Log("ActualNickname: " + nickname);
        }
    }
}
