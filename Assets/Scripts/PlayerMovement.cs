using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private GameObject scoreboard, crosshair;
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

        if (!GetComponent<PhotonView>().IsMine)
        {
            Destroy(transform.GetChild(2).gameObject);
        }
    }

    private void Update()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            Vector3 speed = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            speed = speed.normalized;

            rig.velocity = transform.forward * speed.z * velocity
                + transform.right * speed.x * velocity
                + transform.up * rig.velocity.y;
            transform.GetComponent<PhotonView>().RPC("RotatePlayerX", RpcTarget.All, Input.GetAxis("Mouse X"));
            transform.GetComponent<PhotonView>().RPC("RotatePlayerY", RpcTarget.All, Input.GetAxis("Mouse Y"));

            anim.SetFloat("Velocity", rig.velocity.magnitude);

            if (canShoot && Input.GetButton("Fire1"))
            {
                StartCoroutine("Fire");
            }

            if(Input.GetKeyDown(KeyCode.F)){
                SwitchCamera();
            }

            if(Input.GetKey(KeyCode.Tab)){
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
    }
    [PunRPC]
    private void RotatePlayerY(float input)
    {
        transform.Rotate(transform.right * input);
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
                if (playerHitCounter==maxHealth/GetDamage(gunType))
                {
                    kills++;
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
}
