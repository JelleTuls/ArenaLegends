using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using Photon.Pun; 
using CJ; // Assuming Handler_Movement_OnPlayer is in the CJ namespace

namespace CJ
{
    public class Ability_Shield : MonoBehaviour
    {
// ################################################################################################################################

// VARIABLES:

// ################################################################################################################################
        // Reference Scripts
        public Player_Handle_Movement Player_Handle_Movement; // Reference to HandlerMovementOnPlayer  

        // Game Objects
        public GameObject shieldBreakObject; // VFX Break effect
        public GameObject shieldObject; // Game object Shield

        // Booleans

        // Floats
        private float shieldCooldown = 30.0f; // Cooldown
        private float shieldActivated; // Time value when shield got activated
        private float shieldDuration = 5.0f; // Max duration of holding shield
        
        // Audio
        public AudioSource SE_Shield; // Sound effect shield (Loop)
        public AudioSource SE_ShieldBreak; // Sound effect when shield breaks
        
        // Images
        [SerializeField] private Image uiFillShield; // Cooldown image

// ################################################################################################################################

// VOIDS:

// ################################################################################################################################
         
        //---------------------------------------------------------------------------------------------------------------------
        // START
        //---------------------------------------------------------------------------------------------------------------------
        // Start is called before the first frame update
        void Start()
        {
            uiFillShield.gameObject.SetActive(false);
            shieldCooldown = 0f;
            Player_Handle_Movement.isShielded = false;
        }

        //---------------------------------------------------------------------------------------------------------------------
        // UPDATE
        //---------------------------------------------------------------------------------------------------------------------
        void Update()
        {
            // Play shield sound loop:
            if (Player_Handle_Movement.isShielded == true && SE_Shield.isPlaying == false)
            {
                SE_Shield.Play();
            }
        }

// ################################################################################################################################

// FUNCTIONS:

// ################################################################################################################################
        // While holding shield button play this function
        public void FShield()
        {
            if (!Player_Handle_Movement.isEvading && !Player_Handle_Movement.isRolling && Time.time - shieldActivated > (shieldCooldown))
            { // Check movement states & Cooldown
                Player_Handle_Movement.isShielded = true; // Shielded state (For decreased damage calculation)
                Player_Handle_Movement.isCasting = false; // Stop casting
                Player_Handle_Movement.moveSpeed = Player_Handle_Movement.halfMoveSpeed; // Reduce movement speed
                shieldActivated = Time.time; // Set activation time
                shieldCooldown = 30.0f; // Determine cooldown time

                PhotonNetwork.Instantiate(shieldObject.name, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity); // Spawn shield object on network
                StartCoroutine(Player_Handle_Movement.updateCooldown(shieldCooldown, uiFillShield)); // Start cooldown
                Invoke("F_ShieldBreak", 5.0f); // Break after max 5 sec
            }
        }

        // Function to force a shield break
        public void F_ShieldBreak()
        {
            if (Player_Handle_Movement.isShielded)
            {
                Player_Handle_Movement.isShielded = false; // Stop shielded state
                PhotonNetwork.Instantiate(shieldBreakObject.name, new Vector3(transform.position.x, transform.position.y + 1.6f, transform.position.z), Quaternion.identity); // Spawn shield break VFX on Photon network
                SE_ShieldBreak.Play(); // Play VFX effect
                Player_Handle_Movement.moveSpeed = Player_Handle_Movement.moveSpeedStore; // Restore movementSpeed to what it was before
                GameObject[] shields = GameObject.FindGameObjectsWithTag("Shield");  // Find the shield object owned by player
                foreach (GameObject shield in shields)
                { // Go through all shields in scene
                    if (shield.GetComponent<PhotonView>().Owner == Player_Handle_Movement.playerGameObject.GetComponent<PhotonView>().Owner)
                    { // If isMine
                        PhotonNetwork.Destroy(shield); // Destroy shield on the network
                        break;
                    }
                }
            }
        } 
    }
}