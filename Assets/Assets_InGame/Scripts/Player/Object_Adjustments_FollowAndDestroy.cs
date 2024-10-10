using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace CJ
{
    public class Object_Adjustments_FollowAndDestroy : MonoBehaviour
    {
        public float lifetime = 1.0f;
        public Transform follow; // This is the target we are following

        // Start is called before the first frame update
        void Start()
        {
            // Find all players in the scene
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            // Iterate through the players to find the one with the matching PhotonView owner
            foreach (GameObject player in players)
            {
                PhotonView playerPhotonView = player.GetComponent<PhotonView>();

                // Check if PhotonView exists and if the owner matches
                if (playerPhotonView != null)
                {
                    if (playerPhotonView.Owner == this.GetComponent<PhotonView>().Owner)
                    {
                        this.follow = player.transform; // Assign the matching player to follow
                        break;
                    }
                }
                else
                {
                    Debug.LogWarning($"GameObject {player.name} with 'Player' tag has no PhotonView component.");
                }
            }

            // Check if follow was successfully assigned
            if (follow == null)
            {
                Debug.LogWarning("Follow target not found. Destroying object.");
                Destroy(gameObject); // If no player was found, destroy the object immediately
            }
            else
            {
                Destroy(gameObject, lifetime); // Destroy after lifetime if follow target is valid
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (follow != null)
            {
                // Only update position if follow is assigned
                transform.position = new Vector3(follow.position.x, follow.position.y + 1.0f, follow.position.z);
            }
            else
            {
                Debug.LogWarning("Follow target is null. Skipping position update.");
            }
        }
    }
}
