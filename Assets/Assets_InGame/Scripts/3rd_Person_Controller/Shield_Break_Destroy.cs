using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace CJ
{
    
public class Shield_Break_Destroy : MonoBehaviour
{
    public float lifetime = 1.0f;
    public Transform follow;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players){
            if(player.GetComponent<PhotonView>().Owner == this.GetComponent<PhotonView>().Owner){
                this.follow = player.transform;
                break;
            }
        }
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(follow.position.x, follow.position.y + 1.0f, follow.position.z);
    }
    }
}

