using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    public float velocity;
    private Animator anim;
    private Rigidbody rig;
    private Transform shootPoint;
    private bool canShoot;
    private LineRenderer line;
    public int maxHealth;
    private int health;
    private Vector3 spawnPos;
    private Quaternion spawnRot;
    private int playerHitCounter;
    public GunType gunType;
    private int kills;
    private GameObject scoreboard, crosshair, killsP1Score, killsP2Score, nicknameP1Score, nicknameP2Score;
    public GameObject hitmarker, nicknameBox, otherPlayer;
    private Camera camFirstPerson, camThirdPerson;
    public enum GunType {
        RIFLE,
        SNIPER
    }
    private void Start()
    {
        camFirstPerson = transform.GetChild(2).GetComponent<Camera>();
        camThirdPerson = transform.GetChild(3).GetComponent<Camera>();
        scoreboard = transform.GetChild(6).gameObject;
        crosshair = transform.GetChild(7).gameObject;
        playerHitCounter = 0;
        health = maxHealth;
        spawnPos = transform.position;
        spawnRot = transform.rotation;
        anim = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        shootPoint = transform.GetChild(4);
        canShoot = true;
        scoreboard.SetActive(false);
        line = shootPoint.GetComponent<LineRenderer>();
        killsP1Score = scoreboard.transform.GetChild(2).gameObject;
        killsP2Score = scoreboard.transform.GetChild(4).gameObject;
        nicknameP1Score = scoreboard.transform.GetChild(1).gameObject;
        nicknameP2Score = scoreboard.transform.GetChild(3).gameObject;

        if (!GetComponent<PhotonView>().IsMine)
        {
            Destroy(transform.GetChild(2).GetComponent<Camera>());
            Destroy(transform.GetChild(3).GetComponent<Camera>());
        }

        SetNicknamesScoreboard();
    }

    private void Update()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            Vector3 speed = new Vector3(UnityEngine.Input.GetAxis("Horizontal"), 0, UnityEngine.Input.GetAxis("Vertical"));
            speed = speed.normalized;

            rig.velocity = transform.forward * speed.z * velocity
                + transform.right * speed.x * velocity
                + transform.up * rig.velocity.y;
            transform.GetComponent<PhotonView>().RPC("RotatePlayerX", RpcTarget.All, UnityEngine.Input.GetAxis("Mouse X"));
            RotateCameraY(UnityEngine.Input.GetAxis("Mouse Y"));

            anim.SetFloat("Velocity", rig.velocity.magnitude);

            if (canShoot && UnityEngine.Input.GetButton("Fire1"))
            {
                StartCoroutine("Fire");
            }

            if(UnityEngine.Input.GetKeyDown(KeyCode.F)){
                SwitchCamera();
            }

            if(UnityEngine.Input.GetKey(KeyCode.Tab)){
                ShowScoreboard();
            } else {
                HideScoreboard();
            }
        }
    }
    [PunRPC]
    private void RotatePlayerX(float input)
    {
        transform.Rotate(transform.up * input);
        RotateNickname(input);
    }
    private void RotateCameraY(float input)
    {
        camFirstPerson.transform.Rotate(new Vector3(-1,0,0) * input);
    }
    IEnumerator Fire()
    {
        canShoot = false;
        anim.SetTrigger("Fire");
        RaycastHit hit = new RaycastHit();
        line.enabled = true;
        line.SetPosition(0, shootPoint.position);
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, 50))
        {
            line.SetPosition(1, hit.point);
            if (hit.transform.gameObject.CompareTag("Player"))
            {
                int damage = GetDamage(gunType);
                hit.transform.GetComponent<PhotonView>().RPC("TakeDamage",RpcTarget.All,damage);
                playerHitCounter++;
                hitmarker.SetActive(true);
                Invoke("HideHitmarker",0.3f);
                if (playerHitCounter==maxHealth/GetDamage(gunType))
                {
                    kills++;
                    if(PhotonNetwork.IsMasterClient){
                        hit.transform.GetComponent<PhotonView>().RPC("UpdateScoreboardKillsP1",RpcTarget.All, kills);
                        UpdateScoreboardKillsP1(kills);
                    } else {
                        hit.transform.GetComponent<PhotonView>().RPC("UpdateScoreboardKillsP2",RpcTarget.All, kills);
                        UpdateScoreboardKillsP2(kills);
                    }
                    playerHitCounter = 0;
                    RespawnPlayer();
                }
            }
        }
        else
        {
            line.SetPosition(1, shootPoint.forward * 50);
        }
        yield return new WaitForSeconds(0.1f);
        line.enabled = false;
        anim.ResetTrigger("Fire");
        yield return new WaitForSeconds(GetFirerate(gunType));
        canShoot = true;
    }
    public void HideHitmarker()
    {
        hitmarker.SetActive(false);
    }
    [PunRPC]
    public void TakeDamage(int damage)
    {
        health-=damage;
        if (health <= 0)
        {
            StopAllCoroutines();
            line.enabled = false;
            canShoot = false;
            rig.isKinematic = true;
            anim.SetTrigger("Dead");
            GetComponent<Collider>().enabled = false;
            this.enabled = false;
            RespawnPlayer();
        }
    }
    private void RespawnPlayer()
    {
        anim.ResetTrigger("Fire");
        anim.ResetTrigger("Dead");
        transform.position = spawnPos;
        transform.rotation = spawnRot;
        line.enabled = true;
        canShoot = true;
        rig.isKinematic = false;
        GetComponent<Collider>().enabled = true;
        this.enabled = true;
        health = maxHealth;
    }
    private float GetFirerate(GunType gunType){
        switch(gunType){
            case GunType.RIFLE:
                return 0f;
            case GunType.SNIPER:
                return 0.5f;
            default:
                return 0f;
        }
    }
    private int GetDamage(GunType gunType){
        switch(gunType){
            case GunType.RIFLE:
                return 1;
            case GunType.SNIPER:
                return maxHealth;
            default:
                return 1;
        }
    }
    private void SwitchCamera(){
        camFirstPerson.enabled = !camFirstPerson.enabled;
        camThirdPerson.enabled = !camThirdPerson.enabled;
    }
    private void ShowScoreboard(){
        scoreboard.SetActive(true);
        crosshair.SetActive(false);
    }
    private void HideScoreboard(){
        scoreboard.SetActive(false);
        crosshair.SetActive(true);
    }
    [PunRPC]
    public void SetNickname(string nickname)
    {
        nicknameBox.GetComponent<TMP_Text>().text = nickname;
    }
    private void RotateNickname(float input)
    {
        transform.Rotate(transform.up * input);
    }
    [PunRPC]
    public void UpdateScoreboardKillsP1(int kills)
    {
        killsP1Score.GetComponent<TMP_Text>().text = kills.ToString();
    }
    [PunRPC]
    public void UpdateScoreboardKillsP2(int kills)
    {
        killsP2Score.GetComponent<TMP_Text>().text = kills.ToString();
    }
    private void SetNicknamesScoreboard()
    {
        nicknameP1Score.GetComponent<TMP_Text>().text = PlayerPrefs.GetString("nickname1");
        nicknameP2Score.GetComponent<TMP_Text>().text = PlayerPrefs.GetString("nickname2");
    }
}
