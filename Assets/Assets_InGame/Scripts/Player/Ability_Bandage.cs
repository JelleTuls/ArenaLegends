using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using Photon.Pun; 
using CJ; // Assuming Player_Handle_Movement is in the CJ namespace

namespace CJ
{
    public class Ability_Bandage : MonoBehaviour
    {
// ################################################################################################################################

// VARIABLES:

// ################################################################################################################################
        // Reference Scripts
        public Player_Handle_Movement Player_Handle_Movement; // Reference to HandlerMovementOnPlayer  

        // Game Objects
        public GameObject bandageObject; // Reference to bandage VFX parent object

        // Booleans
        private bool doBandage = false; // Set (Default) bandage state
        
        // Floats
        private float bandageActivated; // Variable for holding bandage activation time
        private float bandageHealth = 4.0f; // Health gained per second
        private float bandageCooldown = 15f; // Bandage cooldown
        
        // Audio
        public AudioSource healBandage; // Audio effect
        
        // Images
        [SerializeField] private Image uiFillBandage; // UI Cooldown

// ################################################################################################################################

// VOIDS:

// ################################################################################################################################
         
        //---------------------------------------------------------------------------------------------------------------------
        // START
        //---------------------------------------------------------------------------------------------------------------------
        // Start is called before the first frame update
        void Start()
        {
            bandageObject.SetActive(false);
            uiFillBandage.gameObject.SetActive(false);
            bandageCooldown = 0f;
        }

        //---------------------------------------------------------------------------------------------------------------------
        // UPDATE
        //---------------------------------------------------------------------------------------------------------------------
        void Update()
        {
            // BANDAGE SOUND EFFECT:
            if (doBandage == true && healBandage.isPlaying == false)
            {
                healBandage.volume = Random.Range(0.55f, 0.65f);
                healBandage.pitch = Random.Range(1.2f, 1.5f);
                healBandage.Play();
            }
        }

// ################################################################################################################################

// FUNCTIONS:

// ################################################################################################################################
        
        public void FBandage()
        {
            // Use Player_Handle_Movement to check the isRolling state and other movement states
            if (!Player_Handle_Movement.isRolling && !Player_Handle_Movement.isEvading && !Player_Handle_Movement.isShielded 
                && Time.time - bandageActivated > (bandageCooldown) && Player_Handle_Movement.controller.isGrounded)
            {
                bandageActivated = Time.time; // Set activation time to now
                Player_Handle_Movement.isCasting = true; // Set casting state
                doBandage = true; // Set bandage state
                Player_Handle_Movement.moveSpeed = 0; // Stand still
                bandageCooldown = 15f; // Set cooldown

                // Instantiate bandage effect over the network
                PhotonNetwork.Instantiate(bandageObject.name, new Vector3(transform.position.x, transform.position.y + 0.81f, transform.position.z), Quaternion.identity); 

                StartCoroutine(Bandage()); // Start IEnumerator Bandage (Channel bandage)
                StartCoroutine(Player_Handle_Movement.Channel(5.0f)); // Channel bar for 5 seconds
                StartCoroutine(Player_Handle_Movement.updateCooldown(bandageCooldown, uiFillBandage)); // Start cooldown
                Invoke("F_BandageBreak", 5.0f); // Bandage function breaks after 5 seconds.

                Player_Handle_Movement.anim.SetTrigger("CastBandage"); // Start animation
                Player_Handle_Movement.anim.ResetTrigger("StopBandage"); // Set stop bandage trigger to false
            }
        }

        // Function to break bandage channeling at max time or when button released
        public void F_BandageBreak()
        {
            Player_Handle_Movement.moveSpeed = Player_Handle_Movement.moveSpeedStore; // Restore movement speed to original
            Player_Handle_Movement.channelBar.gameObject.SetActive(false); // Disable channel bar
            Player_Handle_Movement.channelGroup.SetActive(false); // De-activate channel object
            doBandage = false; // Cancel bandage state
            Player_Handle_Movement.isCasting = false; // Stop casting state

            GameObject[] bandages = GameObject.FindGameObjectsWithTag("Bandage"); // Find all bandage objects on network
            foreach (GameObject bandage in bandages)
            {
                if (bandage.GetComponent<PhotonView>().Owner == Player_Handle_Movement.playerGameObject.GetComponent<PhotonView>().Owner)
                { 
                    PhotonNetwork.Destroy(bandage); // Remove my bandage object over the network
                    break;
                }

                StopCoroutine(Player_Handle_Movement.Channel(5.0f)); // Stop channeling coroutine
                StopCoroutine(Bandage()); // Stop bandage coroutine

                Player_Handle_Movement.anim.ResetTrigger("CastBandage"); // Set CastBandage animation trigger to false
                Player_Handle_Movement.anim.SetTrigger("StopBandage"); // Set StopBandage trigger to true
            }
        }

        // Actual bandage function (While loop each second)
        private IEnumerator Bandage()
        {
            while (doBandage == true)
            { 
                // Restore health of player
                Player_Handle_Movement.GetComponent<Player_Handle_Stats>().RestoreHealth(bandageHealth); 
                yield return new WaitForSeconds(1f); // Wait a second for the next health restoration
            }
        }
    }
}
