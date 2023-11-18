using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

//_____________________________________________________________________________________________________________________
//NOTES
//---------------------------------------------------------------------------------------------------------------------
//NOTE: Combine with Bandage_Follow.cs


namespace CJ
{ 
public class Shield_Follow : MonoBehaviour
{

//_____________________________________________________________________________________________________________________
// GENERAL VARIABLES:
//---------------------------------------------------------------------------------------------------------------------
#region GENERAL VARIABLES
    public Transform follow; // Target to follow (copy transform.position)
    public Transform cam; // Camera to direct billboard towards
#endregion GENERAL VARIABLES


//_____________________________________________________________________________________________________________________
// GENERAL VOIDS:
//---------------------------------------------------------------------------------------------------------------------
#region GENERAL VOIDS    
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); // Get list of players in scene
        foreach (GameObject player in players){ // Loop trhough list
            if(player.GetComponent<PhotonView>().Owner == this.GetComponent<PhotonView>().Owner){ // Check if view of camera equals photon.myView
                this.follow = player.transform; // Set follow variable to self player
                break;
            }
        }
        GameObject[] cameras = GameObject.FindGameObjectsWithTag("Camera"); // Get list of cameras in scene
        foreach (GameObject camera in cameras){ // Loop through list
            if(camera.GetComponent<PhotonView>().Owner == this.GetComponent<PhotonView>().Owner){ // Check if view of camera equals photon.myView
                this.cam = camera.transform; // Set cam variable to self camera
                break;
            }
        }
    }

    void Update()
    {
        transform.position = new Vector3(follow.position.x, 
        follow.position.y + 0.05f, follow.position.z); // Set transform.position equal to target to follow
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward); // direct self transform.LookAt towards self player's camera
    }
#endregion GENERAL VOIDS
    }
}

