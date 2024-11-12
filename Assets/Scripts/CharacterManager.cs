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
            PhotonNetwork.Instantiate("GasMaskSoldier", spawn1.position, spawn1.rotation);
            string nickname = PlayerPrefs.GetString("nickname1");
            Debug.Log("ActualNickname: " +nickname);
        } else
        {
            PhotonNetwork.Instantiate("SwatSoldier", spawn2.position, spawn2.rotation);
            string nickname = PlayerPrefs.GetString("nickname2");
            Debug.Log("ActualNickname: " + nickname);
        }
    }
}
