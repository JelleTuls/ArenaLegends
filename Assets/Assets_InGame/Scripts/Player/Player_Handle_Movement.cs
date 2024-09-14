using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using CJ;

namespace CJ
{
    public class Player_Handle_Movement : MonoBehaviour
    {

// ################################################################################################################################

// VARIABLES:

// ################################################################################################################################
        //---------------------------------------------------------------------------------------------------------------------
        // PHOTON: Variables Required by Photon
        //---------------------------------------------------------------------------------------------------------------------
        PhotonView view;
        PhotonView cameraView;
        PhotonView uiCanvasView;
        PhotonView miniMapCameraView;

        //---------------------------------------------------------------------------------------------------------------------
        // PLAYER: Player Required Variables
        //---------------------------------------------------------------------------------------------------------------------
        // Player Objects
        [Header("Player Objects")]
        public GameObject playerModel;
        public GameObject playerGameObject;
        public CharacterController controller;
        private Rigidbody rb;

        // Orientation
        [Header("Orientation")]
        public Transform horizontalOrientation;
        public GameObject backwardsOrientation;
        private Vector3 horizontalRotation;
        private Vector3 moveDirection; //Determine direction based on Horizontal / Veritcal Input
        private Vector3 rollDirection;

        // Targeting:
        public GameObject isAttackTarget;

        // Attacking: 
        public float attackActivated; // Set when attack is activated
        public float attackDuration = 0.3f; // Used for a constant check whether player is currently attacking (Attack Speed)

        //---------------------------------------------------------------------------------------------------------------------
        // STATE CHECKS: State Checks Required Variables
        //---------------------------------------------------------------------------------------------------------------------
        // Movement States
        private bool canWalk; // Default = TRUE | Roll & Evade = FALSE
        private bool isStandingStill; // If moving => TRUE | TRUE --> Evade | FALSE = Roll
        public bool isJumping = false; // Jump => TRUE, Else = FALSE
        public bool isRolling; // Roll => TRUE, Else = FALSE
        public bool isEvading; // Evade => TRUE, Else = FALSE

        [Header("Ability States")]
        [SerializeField] public bool isShielded; // Shield => TRUE, ShieldBreak = FALSE, Else = FALSE
        [SerializeField] public bool isCasting; // Channel => TRUE, Else = FALSE
        [SerializeField] public bool isInCombat; // BasicAttack => TRUE, Else = FALSE
        [SerializeField] public bool isSlowed; // Roll&Evade => FALSE, Else = False
        [SerializeField] public bool isTrapped; // Roll&Evade => FALSE, Else = FALSE

        [Header("Attack States")]
        public float isInRange; // Checked in Void Update!
        public bool isAttacking; // Momental & Duration check!

        //---------------------------------------------------------------------------------------------------------------------
        // MOVEMENT: Movement Required Variables
        //---------------------------------------------------------------------------------------------------------------------
        [Header("Movement")]
        public float moveSpeed; // Horizontal movement speed
        public float moveSpeedStore; // Used to remember previous state's movement speed
        public float rotateSpeed; // Speed of horizontal character object rotation
        public float gravityScale; // Normal Gravity Multiplyer
        public float jumpGravityScale; // Different Gravity Multiplyer while jumping
        public float halfMoveSpeed; // Used for slow effects (And Shield/Bandage Ability)

        [Header("Movement Audio")]
        public AudioSource footstepsBasic; // Subtle grass movement/walking sound
        public AudioSource footstepsHeavy; // Heavy body movement/walking sound

        //---------------------------------------------------------------------------------------------------------------------
        // USER INTERFACE: UI Required Variables
        //---------------------------------------------------------------------------------------------------------------------
        [Header("User Interface")]
        public Image myPortraitImage; // Player's portrait 
        public Image myPortraitClass; // Player's class icon
        public Sprite myPortrait; // Player's portrait SPRITE object
        public Sprite myClass; // Player's class icon SPRITE object
        public TextMeshProUGUI myName; // Player's portrait name TEXT object
        public GameObject channelGroup; // Used to activate & deactivate channel bar
        public GameObject joystickMovement; // Used to calculate movement velocity based on (Mobile) joystick input
        [SerializeField] public Image channelBar; // Player's channel bar

        //---------------------------------------------------------------------------------------------------------------------
        // BASICS: Set Up All Basic Components
        //---------------------------------------------------------------------------------------------------------------------
        [Header("Basics")]
        public GameObject uiCanvas; // Used to Photon isMine uiCanvas
        public GameObject mainCamera; // Used for Photon isMine mainCamera
        public GameObject miniMapCamera; // Used for Photon isMine mainCamera

        public Animator anim; // Main character animator (Animation flow/network)

// ################################################################################################################################

// VOID AWAKE:

// ################################################################################################################################
        void Awake()
        {
        //---------------------------------------------------------------------------------------------------------------------
        // BASICS: Set Up All Basic Components
        //---------------------------------------------------------------------------------------------------------------------
            rb = GetComponent<Rigidbody>(); // Determine the Rigidbody of the player object

        //---------------------------------------------------------------------------------------------------------------------
        // DE-PARENTING: De-parenting Cameras & Canvas
        //---------------------------------------------------------------------------------------------------------------------
            uiCanvas.transform.SetParent(null);
            mainCamera.transform.SetParent(null);
            miniMapCamera.transform.SetParent(null);

        //---------------------------------------------------------------------------------------------------------------------
        // PHOTON: Get player's own photon view objects
        //---------------------------------------------------------------------------------------------------------------------
            view = GetComponent<PhotonView>();
            cameraView = mainCamera.GetComponent<PhotonView>();
            uiCanvasView = uiCanvas.GetComponent<PhotonView>();
            miniMapCameraView = miniMapCamera.GetComponent<PhotonView>();
        }

// ################################################################################################################################

// VOID START:

// ################################################################################################################################
        void Start()
        {
        //---------------------------------------------------------------------------------------------------------------------
        // PHOTON: Destroy All Duplicated Photon Objects Whic are not Mine
        //---------------------------------------------------------------------------------------------------------------------
            if (!uiCanvasView.IsMine) { Destroy(uiCanvas); }
            if (!cameraView.IsMine) { Destroy(mainCamera); }
            if (!miniMapCameraView.IsMine) { Destroy(miniMapCamera); }
            if (!view.IsMine) { Destroy(rb); }

        //---------------------------------------------------------------------------------------------------------------------
        // // BASICS: Set Up All Basic Components
        //---------------------------------------------------------------------------------------------------------------------
            controller = GetComponent<CharacterController>();

        //---------------------------------------------------------------------------------------------------------------------
        // GAME OBJECTS: Activate / Deactivate Game Objects at Start
        //---------------------------------------------------------------------------------------------------------------------
        //Objects:
            swapIconA.gameObject.SetActive(false);
            swapIconB.gameObject.SetActive(false);

        //UI:
            //Cooldown images:
            uiFillDodge.gameObject.SetActive(false);
            uiFillSwap.gameObject.SetActive(false);

            //Channel
            channelGroup.SetActive(false);
            channelBar.gameObject.SetActive(false);

            //Swap:
            groupStanceA.SetActive(true);
            groupStanceB.SetActive(false);

        //---------------------------------------------------------------------------------------------------------------------
        // VARIABLES MOVEMENT SPEED:
        //---------------------------------------------------------------------------------------------------------------------
            //Store Movement Speed:
            moveSpeedStore = moveSpeed;
            halfMoveSpeed = moveSpeed / 2;

        //---------------------------------------------------------------------------------------------------------------------
        // VARIABLES COOLDOWNS: Set All Cooldowns to 0 at Start
        //---------------------------------------------------------------------------------------------------------------------
            dodgeCooldown = 0f;
            swapCooldown = 0f;

        //---------------------------------------------------------------------------------------------------------------------
        // VARIABLES STATE CHECKS: Set All State Check Variables at Start
        //---------------------------------------------------------------------------------------------------------------------
            canWalk = true;

            //Dodging:
            isRolling = false;
            isEvading = false;
            isShielded = false;
            isCasting = false;
            isInCombat = false;
            isSlowed = false;
            isTrapped = false;

            //Stances:
            isStanceA = true;
            isStanceB = false;

        //---------------------------------------------------------------------------------------------------------------------
        // UI SETTINGS: Set All State Check Variables at Start
        //---------------------------------------------------------------------------------------------------------------------
            myPortraitImage.sprite = myPortrait;
            myPortraitClass.sprite = myClass;
            myName.text = playerGameObject.name;
        }

// ################################################################################################################################

// VOID UPDATE:

// ################################################################################################################################
        void Update()
        {
            //_____________________________________________________________________________________________________________________
            // PHOTON: Remove new !isMine incoming Cameras, Canvas, and Rigidbodies
            //---------------------------------------------------------------------------------------------------------------------
            if (!uiCanvasView.IsMine) { PhotonNetwork.Destroy(uiCanvas); }
            if (!cameraView.IsMine) { PhotonNetwork.Destroy(mainCamera); }
            if (!miniMapCameraView.IsMine) { PhotonNetwork.Destroy(miniMapCamera); }
            if (!view.IsMine) { Destroy(rb); }

            //_____________________________________________________________________________________________________________________
            // THIS ONLY PLAYS IF PLAYER_OBJECT.VIEW ISMINE:
            //---------------------------------------------------------------------------------------------------------------------
            if (view.IsMine)
            {
                //Check target and its distance:
                isAttackTarget = gameObject.GetComponent<Player_Handle_Target>().isTarget; // Get target from Sense script
                if (Time.time - attackActivated > attackDuration)
                { // Check if attack is ready:
                    isAttacking = false;
                }

                isInRange = Vector3.Distance(transform.position, isAttackTarget.transform.position); // Range Detection:


                //_____________________________________________________________________________________________________________________
                // Store values:
                //---------------------------------------------------------------------------------------------------------------------
                float yStore = moveDirection.y; // Prevent flickering up and down by saving the vector3.y value:

                //_____________________________________________________________________________________________________________________
                // STATE CHECKS PER FRAME:
                //---------------------------------------------------------------------------------------------------------------------
                //isStandingStill (MOVEMENT DIRECTION CHECK)
                if (moveDirection.x + moveDirection.z > 0.001 || moveDirection.x + moveDirection.z < -0.001)
                {
                    isStandingStill = false;
                }
                else
                {
                    isStandingStill = true;
                }

                //isJumping (GROUNDED CHECK)
                if (controller.isGrounded)
                {
                    isJumping = false;
                    isJumpingAnimation = false;
                }

                //_____________________________________________________________________________________________________________________
                // MOVEMENT:
                //---------------------------------------------------------------------------------------------------------------------
                if (canWalk)
                {
                    // Movement Velocity based on Input       
                    moveDirection = (transform.forward * joystickMovement.GetComponent<UltimateJoystick>().GetVerticalAxis()) +
                        (transform.right * joystickMovement.GetComponent<UltimateJoystick>().GetHorizontalAxis());
                    // Actually moving the Player
                    moveDirection = moveDirection.normalized * moveSpeed;
                }
                // After each frame, set Vector3.y value back to its previous stored position
                moveDirection.y = yStore;

                //_____________________________________________________________________________________________________________________
                // ROTATION:
                //---------------------------------------------------------------------------------------------------------------------
                if (canWalk)
                {
                    // Move player in different directions based on camera look direction:
                    if (!isStandingStill)
                    {
                        // Rotation of player = horizontalOrientation object ==> Which is guided by Mouse Y:
                        // When standing still, camera can rotate around the Player!
                        transform.rotation = Quaternion.Euler(0f, horizontalOrientation.rotation.eulerAngles.y, 0f);

                        //Calculate direciton to rotate towards:
                        // 1) Store direction in a Variable
                        // 2) Calculate the direction to rotate towards (toRotation):
                        horizontalRotation = new Vector3(moveDirection.x, 0f, moveDirection.z);
                        Quaternion toRotation = Quaternion.LookRotation(horizontalRotation, Vector3.up);
                        // Rotate from original direction => new calculated direction (toRotation):
                        playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, toRotation, rotateSpeed * Time.deltaTime);
                    }
                }

                //_____________________________________________________________________________________________________________________
                // MOVEMENT COMMAND:
                //---------------------------------------------------------------------------------------------------------------------
                controller.Move(moveDirection * Time.deltaTime);

                //_____________________________________________________________________________________________________________________
                // ANIMATION VARIABLES
                //---------------------------------------------------------------------------------------------------------------------
                anim.SetBool("isJumpingAnimation", isJumpingAnimation);
                anim.SetBool("isGrounded", controller.isGrounded);
                anim.SetBool("isJumping", isJumping);
                anim.SetFloat("Speed", (Mathf.Abs(joystickMovement.GetComponent<UltimateJoystick>().GetVerticalAxis()) + Mathf.Abs(joystickMovement.GetComponent<UltimateJoystick>().GetHorizontalAxis())));
                anim.SetBool("isRolling", isRolling);
                anim.SetBool("isEvading", isEvading);
                anim.SetBool("isAttacking", isAttacking);

                //_____________________________________________________________________________________________________________________
                // LOOPING SOUND EFFECTS:
                //---------------------------------------------------------------------------------------------------------------------
                // WALKING (FOOTSTEPS & MOVEMENT)
                if (controller.isGrounded && !isJumping && !isRolling && controller.velocity.magnitude > 2f && footstepsBasic.isPlaying == false)
                {
                    footstepsBasic.volume = Random.Range(0.80f, 1.00f);
                    footstepsBasic.pitch = Random.Range(0.95f, 1.05f);
                    footstepsBasic.Play();
                    footstepsHeavy.volume = Random.Range(0.05f, 0.10f);
                    footstepsHeavy.Play();
                }
            }
        }

        // Update plays every frame subsequent to the regular void update
        void LateUpdate()
        {
            //Determine which gravityScale to apply:
            if (isJumping)
            {
                // APPLY JUMP GRAVITY
                moveDirection.y += Physics.gravity.y * jumpGravityScale * Time.deltaTime;
            }
            else
            {
                // APPLY NORMAL GRAVITY
                moveDirection.y += Physics.gravity.y * gravityScale * Time.deltaTime;
            }
        }

        // Update which plays every fixed frame
        void FixedUpdate()
        {
            if (!view.IsMine)
            {
                return;
            }
        }


// ################################################################################################################################

// FUNCTIONS BASIC ABILITIES:

// ################################################################################################################################
        //---------------------------------------------------------------------------------------------------------------------
        // JUMP
        //---------------------------------------------------------------------------------------------------------------------

        public float jumpForce; // moveDicreion.y
        private bool isJumpingAnimation = false; // for anim Bool | isGrounded = FALSE | Jump = TRUE

        //_______________________________________________

        public void FJumping() // The jump function:
        {
            if (!isJumping && !isRolling && !isEvading && !isShielded)
            {
                moveDirection.y = jumpForce;
            }
        }

        // This function is support mobile jump button:
        public void FJumping_Cancel()
        {
            isJumping = true;
            moveDirection.y = jumpForce;
        }

        // Triggers jump animation:
        public void FJumping_Animation()
        {
            isJumpingAnimation = true;
        }

        // This function prevents infinite jumping:
        public void FJumping_Two()
        {
            isJumpingAnimation = true;
            if (!isJumping && !isRolling && !isEvading && !isShielded)
            {
                Invoke("FJumping_Cancel", 0.15f);
                Invoke("FJumping_Animation", 0.02f);
            }
        }

        //---------------------------------------------------------------------------------------------------------------------
        // ROLL AND EVADE
        //---------------------------------------------------------------------------------------------------------------------
        public float rollSpeed; // Horizontal push speed of a roll
        public float rollTime; // Amount of 0.seconds of applying rollSpeed

        public float evadeSpeed; // Horizontal push speed of evade
        public float evadeTime; // Amount of 0.seconds of applying evadeSpeed

        private float dodgeCooldown; // Cooldown time of roll/evade
        private float dodgeRemainingCooldown; // Tracker for remaining time of cooldown
        float dodgeActivated; // Track the activation time of roll/evade

        [SerializeField] private Image uiFillDodge; // Cooldown image

        public AudioSource SE_Roll; // Sound Effect Roll

        //_______________________________________________
        // After button press --> Roll or Evade based on movement speed.
        // The decision whether to Roll or Evade is made in: IEnumerator Roll()
        public void FRollAndEvade()
        {
            if (!isJumping && canWalk && !isRolling)
            { // Movement checks
                if (Time.time - dodgeActivated > (dodgeCooldown))
                { // Cooldown check
                    isSlowed = false; // Remove slow effects
                    isCasting = false; // Deactivate channeling
                    isTrapped = false; // Remove immobilizing effects
                    dodgeCooldown = 30f; // Set cooldown timer
                    StartCoroutine(Roll()); // Play IEnumerator Roll()
                    StartCoroutine(WaitForRollSound(0.1f)); // Play sound
                    StartCoroutine(updateCooldown(dodgeCooldown, uiFillDodge)); // Play IEnumerator updateCooldown()
                    dodgeActivated = Time.time; // Set activation time
                }
                else
                {
                    return;
                }
            }
        }

        // IEnumerator function plays the actual roll
        private IEnumerator Roll()
        {
            rollDirection = moveDirection; // Set rollDirection based on the current moveDirection
            float startTime = Time.time; // Set starting time of roll
                                         // Depending if character object is standing still or moving, play evade or roll
            if (!isStandingStill)
            {
                while (Time.time < startTime + rollTime)
                { // As long as rollTime hasn't passed
                    isRolling = true; // Set isRolling state check to TRUE
                    canWalk = false; // Can't walk while rolling
                    controller.Move(rollDirection * rollSpeed * Time.deltaTime); // Actual movement
                    yield return null;
                }
            }
            else
            {
                while (Time.time < startTime + evadeTime)
                { // While evadeTime hasn't passed
                    isEvading = true; // Set isEvading state check to TRUE
                    canWalk = false; // Can't walk while evading
                                     // Move in the direction of Object: backwardsOrientation: (This object is a child of PlayerModel)
                    transform.position = Vector3.MoveTowards(transform.position, backwardsOrientation.transform.position, evadeSpeed * Time.deltaTime);
                    yield return null;
                }
            }
            // Reset state checks
            canWalk = true;
            isRolling = false;
            isEvading = false;
        }

        // Function to wait X amount of 0.seconds before playing the sound
        private IEnumerator WaitForRollSound(float seconds1)
        {
            yield return new WaitForSeconds(seconds1); // Wait for x amount of 0.seconds
            SE_Roll.Play(); // Play sound
        }

        //---------------------------------------------------------------------------------------------------------------------
        // SWAP
        //---------------------------------------------------------------------------------------------------------------------
        private bool isStanceA; // State bool to track which stance is activated (A)
        private bool isStanceB; // State bool to track which stance is activated (B)
        private float swapCooldown = 90f; // Cooldown between swaps
        private float swapActivated; // Variable to hold activation time

        public GameObject groupStanceA; // Group of buttons while in stance A
        public GameObject groupStanceB; // Group of buttons while in stance B

        public GameObject swapVFX; // Swap VFX particle effect
        public GameObject swapIconA; // Swap VFX stance icon (A)
        public GameObject swapIconB; // Swap VFX Stance icon (B)

        [SerializeField] private Image uiFillSwap; // Cooldown image
        public AudioSource PowerUp1; // Beam VFX audio effect
        public AudioSource PowerUpPop; // Icon (metal) pop audio effect

        // Function to swap available buttons between Stance A & Stance B
        public void FSwap()
        {
            if (Time.time - swapActivated > (swapCooldown))
            { // Cooldown check
                uiFillSwap.gameObject.SetActive(true); // Set cooldown image active
                swapCooldown = 60f; // Cooldown time
                swapActivated = Time.time; // Set activation time to .Now

                PhotonNetwork.Instantiate(swapVFX.name, new Vector3(transform.position.x, transform.position.y + 0.65f, transform.position.z), Quaternion.identity); // Spawn Swap VFX over the network

                StartCoroutine(updateCooldown(swapCooldown, uiFillSwap));  // Actual Swap function
                if (isStanceA)
                { // If in Stance A --> Switch to B
                    StartCoroutine(swapToIconB());
                }
                else if (isStanceB)
                { // If in Stance B --> Switch to A
                    StartCoroutine(swapToIconA());
                }

                PowerUp1.Play(); // Play beam audio effect 
                StartCoroutine(WaitForPop());
            }
        }

        // IEnumerator function to Swap --> A (Visuals)
        private IEnumerator swapToIconA()
        {
            float disappearSwap = 7f; // After x Seconds --> Swap effect disappears
            isStanceA = true; // Activate being in stance A
            isStanceB = false; // De-Activate being in stance B
            groupStanceB.SetActive(false); // D-Activate button group B
            groupStanceA.SetActive(true); // Activate button group A
            swapIconB.SetActive(false); // Deactivate VFX icon B
            swapIconA.SetActive(true); // Activate VFX icon A
            while (disappearSwap >= 0)
            { // As long as disappearSwap time isn't over
                disappearSwap -= Time.deltaTime; // Lower countdown timer
                swapIconA.SetActive(true); // Play VFX Icon A
                yield return null;
            }
            swapIconA.SetActive(false); // After countdown also deactivate icon A
        }

        // IEnumerator function to Swap --> B (Visuals)
        private IEnumerator swapToIconB()
        {
            float disappearSwap = 7f; // After x Seconds --> Swap effect disappears
            isStanceA = false; // De-activate being in stance A
            isStanceB = true; // Activate being in stance B
            groupStanceA.SetActive(false); // De-Activate button group A
            groupStanceB.SetActive(true); // Activate button group B
            swapIconA.SetActive(false); // De-Activate VFX icon A
            swapIconB.SetActive(true); // Activate VFX icon B
            while (disappearSwap >= 0)
            { // As long as disappearSwap time isn't over
                disappearSwap -= Time.deltaTime; // Lower countdown timer
                swapIconB.SetActive(true); // Play VFX Icon B
                yield return null;
            }
            swapIconB.SetActive(false); // After countdown also deactivate icon A
        }

        // IEnumerator function regulating timing of icon pop audio effect
        private IEnumerator WaitForPop()
        {
            yield return new WaitForSeconds(2.3f); // Wait for x.seconds
            PowerUpPop.Play(); // Player power up pop audio effect
        }

        //_____________________________________________________________________________________________________________________
        //COOLDOWN & CHANNELING (Functions):
        //---------------------------------------------------------------------------------------------------------------------
        // (FOR TESTING) function to damage self
        public void FSelfDamage()
        {
            GetComponent<Player_Handle_Stats>().TakeDamage(5);
        }

        // Function to force casting/channeling break
        public void F_CastBreak()
        {
            isCasting = false; // Set isCasting to false
        }

        //_____________________________________________________________________________________________________________________
        // CHANNELING -- to regulate cooldowns (Arguments: Cooldown time, Cooldown Image)
        //---------------------------------------------------------------------------------------------------------------------
        public IEnumerator updateCooldown(float cooldown, Image cooldownImage)
        {
            float cooldownRemaining = cooldown;  // Set remaining cooldown based on argument
            cooldownImage.gameObject.SetActive(true); // Activate cooldown image (overlay image)

            while (cooldownRemaining >= 0)
            { // As long as on cooldown
                cooldownRemaining -= Time.deltaTime; // Keep countdown timer going
                cooldownImage.fillAmount = cooldownRemaining / cooldown; // Fill image (circular percentage)
                yield return null;
            }
            cooldownImage.gameObject.SetActive(false); // De-Activate cooldown image
        }

        //_____________________________________________________________________________________________________________________
        // CHANNELING --  to regulate channeling time / visual (Arguments: Channeling duration)
        //---------------------------------------------------------------------------------------------------------------------
        public IEnumerator Channel(float duration)
        {
            float channelRemainingDuration = duration; // Set duration based on argument
            channelGroup.SetActive(true); // Activate channel group (visuals)
            channelBar.gameObject.SetActive(true); // Activate channeling bar (visual)

            while (channelRemainingDuration >= 0)
            { // As long as duration hasn't passed
                isCasting = true; // Stay in casting state
                channelRemainingDuration -= Time.deltaTime; // Continue countdown
                channelBar.fillAmount = channelRemainingDuration / duration; // Fill channel bar from 1% - 100% (Speed is based on duration)
                yield return null;
            }

            channelBar.gameObject.SetActive(false); // After finishing channeling: De-Activate channel bar
            channelGroup.SetActive(false); // After finishing channeling: De-Activate channel group
        }
    }
}