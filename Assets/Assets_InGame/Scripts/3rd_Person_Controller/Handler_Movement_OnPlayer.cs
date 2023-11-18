
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

//_____________________________________________________________________________________________________________________
//NOTES
//---------------------------------------------------------------------------------------------------------------------
//NOTE: isInteracting yet to apply!!


namespace CJ
{
public class Handler_Movement_OnPlayer : MonoBehaviour
{


//_____________________________________________________________________________________________________________________
//GENERAL VARIABLES:
//---------------------------------------------------------------------------------------------------------------------
#region General Variables:
    #region Photon Requirements Variables:
        PhotonView view;
        PhotonView cameraView;
        PhotonView uiCanvasView;
        PhotonView miniMapCameraView;
    #endregion

    #region Player Requirements Variables:
        // Player objects:
            public CharacterController controller;
            public GameObject playerModel;
            public GameObject playerGameObject;
            private Rigidbody rb;
        // Rigidbody:
            
        // Orientations:
            public Transform horizontalOrientation;
            public GameObject backwardsOrientation; 
            private Vector3 horizontalRotation;
            private Vector3 moveDirection; //Determine direction based on Horizontal / Veritcal Input
            private Vector3 rollDirection;
        // Targetting:
            private GameObject isAttackTarget;
    #endregion

    #region State Checks Variables:
        //MOVEMENT STATES
        //---------------------------------------------------------------------------------------------------------------------
        //private bool isInteracting;
        private bool canWalk; // Default = TRUE | Roll & Evade = FALSE
        private bool isStandingStill; // If moving => TRUE | TRUE --> Evade | FALSE = Roll
        private bool isJumping = false; // Jump => TRUE, Else = FALSE
        private bool isRolling; // Roll => TRUE, Else = FALSE
        private bool isEvading; // Evade => TRUE, Else = FALSE

        // ABILITY STATES:
        //---------------------------------------------------------------------------------------------------------------------
        [SerializeField] public bool isShielded; // Shield => TRUE, ShieldBreak = FALSE, Else = FALSE
        [SerializeField] public bool isCasting; // Channel => TRUE, Else = FALSE
        [SerializeField] public bool isInCombat; // BasicAttack => TRUE, Else = FALSE
        [SerializeField] public bool isSlowed; // Roll&Evade => FALSE, Else = False
        [SerializeField] public bool isTrapped; // Roll&Evade => FALSE, Else = FALSE

        // ATTACK STATES:
        //---------------------------------------------------------------------------------------------------------------------
        private bool isAttacking; // Momental & Duration check!
        public float isInRange; // Checked in Void Update!
    #endregion

    #region Player Movement Variables:
        public float moveSpeed; // Horizontal movement speed
        private float halfMoveSpeed; // Used for slow effects (And Shield/Bandage Ability)
        public float moveSpeedStore; // Used to remember previous state's movement speed
        public float rotateSpeed; // Speed of horizontal character object rotation
        public float gravityScale; // Normal Gravity Multiplyer
        public float jumpGravityScale; // Different Gravity Multiplyer while jumping
        //---------------------------------------------------------------------------------------------------------------------
        // AUDIO:
        //---------------------------------------------------------------------------------------------------------------------
        public AudioSource footstepsBasic; // Subtle grass movement/walking sound
        public AudioSource footstepsHeavy; // Heavy body movement/walking sound
    #endregion

    #region Player UI Variables:
        public Image myPortraitImage; // Player's portrait 
        public Image myPortraitClass; // Player's class icon
        public Sprite myPortrait; // Player's portrait SPRITE object
        public Sprite myClass; // Player's class icon SPRITE object
        public TextMeshProUGUI myName; // Player's portrait name TEXT object
        [SerializeField] private Image channelBar; // Player's channel bar
        public GameObject channelGroup; // Used to activate & deactivate channel bar
        public GameObject joystickMovement; // Used to calculate movement velocity based on (Mobile) joystick input
    #endregion   

    #region De-Parent Variables:
        public GameObject uiCanvas; // Used to Photon isMine uiCanvas
        public GameObject mainCamera; // Used for Photon isMine mainCamera
        public GameObject miniMapCamera; // Used for Photon isMine mainCamera
    #endregion

    #region Animation Variables:
        public Animator anim; // Main character animator (Animation flow/network)
    #endregion   
#endregion

//_____________________________________________________________________________________________________________________
//GENERAL VOIDS:
//---------------------------------------------------------------------------------------------------------------------
#region GENERAL VOIDS

        // void Awake plays before the start of the game scene
        void Awake(){

            rb = GetComponent<Rigidbody>(); // Determine the Rigidbody of the player object

            // De-parenting Cameras & Canvas
            #region De-Parenting from Prefab:
                uiCanvas.transform.SetParent(null);
                mainCamera.transform.SetParent(null);
                miniMapCamera.transform.SetParent(null);
            #endregion

            // Determine Photon Views
            #region Photon View Components:
                view = GetComponent<PhotonView>();
                cameraView = mainCamera.GetComponent<PhotonView>();
                uiCanvasView = uiCanvas.GetComponent<PhotonView>();
                miniMapCameraView = miniMapCamera.GetComponent<PhotonView>();
            #endregion
        }

        // void Start plays at the start of the game scene
        void Start(){
            // Destroy !isMine Cameras, Rigidbodies, and Canvas
            #region If(.....!View.IsMine) Destroy Photon Components:
                if(!uiCanvasView.IsMine){Destroy(uiCanvas);}
                if(!cameraView.IsMine){Destroy(mainCamera);}
                if(!miniMapCameraView.IsMine){Destroy(miniMapCamera);}
                if(!view.IsMine){Destroy(rb);}
            #endregion

            // Set basic components (Such as: CharacterController)
            #region Get Components:
                controller = GetComponent<CharacterController>();
            #endregion
            
            //---------------------------------------------------------------------------------------------------------------------
            // ACTIVE/DEACTIVE STATE OF OBJECTS
            //---------------------------------------------------------------------------------------------------------------------
            #region SetActive (True & False):
            //Objects:
                bandageObject.SetActive(false);
                swapIconA.gameObject.SetActive(false);
                swapIconB.gameObject.SetActive(false);
                
                //UI:
                //Cooldown images:
                uiFillAttack.gameObject.SetActive(false);
                uiFillDodge.gameObject.SetActive(false);
                uiFillShield.gameObject.SetActive(false);
                uiFillSwap.gameObject.SetActive(false);
                uiFillBandage.gameObject.SetActive(false);

                //Channel
                channelGroup.SetActive(false);
                channelBar.gameObject.SetActive(false);

                //Swap:
                groupStanceA.SetActive(true);
                groupStanceB.SetActive(false);
            #endregion

            //---------------------------------------------------------------------------------------------------------------------
            // SET STARTING VALUES
            //---------------------------------------------------------------------------------------------------------------------
            #region Setting Up Initial Values:
                //Store Movement Speed:
                moveSpeedStore = moveSpeed;
                halfMoveSpeed = moveSpeed / 2;

                //Set all starting Cooldowns to 0:
                dodgeCooldown = 0f;
                attackCooldown = 0f;
                shieldCooldown = 0f;
                swapCooldown = 0f;
                bandageCooldown = 0f;
            #endregion

            //---------------------------------------------------------------------------------------------------------------------
            // SET UP STATE CHECKS
            //---------------------------------------------------------------------------------------------------------------------
            #region Setting Up Initial State Checks:
                //Movement Interactions (Stun & Rolling etc.):
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
            #endregion
        
            // UI Settings
            #region Initial UI Settings:
                myPortraitImage.sprite = myPortrait;
                myPortraitClass.sprite = myClass;
                myName.text = playerGameObject.name;
            #endregion
        }

        // Constant update per frame
        void Update(){
            // Remove new !isMine incoming Cameras, Canvas, and Rigidbodies
            #region If(.....!View.IsMine) Destroy Photon Components:
                if(!uiCanvasView.IsMine){PhotonNetwork.Destroy(uiCanvas);}
                if(!cameraView.IsMine){PhotonNetwork.Destroy(mainCamera);}
                if(!miniMapCameraView.IsMine){PhotonNetwork.Destroy(miniMapCamera);}
                if(!view.IsMine){Destroy(rb);}
            #endregion

            //_____________________________________________________________________________________________________________________
            // THIS ONLY PLAYS IF PLAYER_OBJECT.VIEW ISMINE:
            //---------------------------------------------------------------------------------------------------------------------
            if(view.IsMine){
                //Initial Updates:
                #region Initial Settings (Per Frame):
                    isAttackTarget = gameObject.GetComponent<Sense>().isTarget; // Get target from Sense script
                    if(Time.time - attackActivated > attackDuration){ // Check if attack is ready:
                        isAttacking = false;
                    }
                #endregion

                //_____________________________________________________________________________________________________________________
                // TRACKING PER FRAME:
                //---------------------------------------------------------------------------------------------------------------------
                #region Registering Values (Per Frame):
                    float yStore = moveDirection.y; // Prevent flickering up and down by saving the vector3.y value:
                    isInRange = Vector3.Distance(transform.position, isAttackTarget.transform.position); // Range Detection:
                #endregion

                //_____________________________________________________________________________________________________________________
                // STATE CHECKS PER FRAME:
                //---------------------------------------------------------------------------------------------------------------------
                #region Initial Independent State Checks:
                    //isStandingStill (MOVEMENT DIRECTION CHECK)
                    if(moveDirection.x + moveDirection.z > 0.001 || moveDirection.x + moveDirection.z < -0.001){
                        isStandingStill = false;
                    }
                    else{
                        isStandingStill = true;
                    }
                
                    //isJumping (GROUNDED CHECK)
                    if(controller.isGrounded){
                        isJumping = false;
                        isJumpingAnimation = false;
                    }
                #endregion

                //_____________________________________________________________________________________________________________________
                // MOVEMENT:
                //---------------------------------------------------------------------------------------------------------------------
                #region Basic Movement:
                    if(canWalk)
                    {         
                        // Movement Velocity based on Input       
                        moveDirection = (transform.forward * joystickMovement.GetComponent<UltimateJoystick>().GetVerticalAxis()) + 
                            (transform.right * joystickMovement.GetComponent<UltimateJoystick>().GetHorizontalAxis()); 
                        // Actually moving the Player
                        moveDirection = moveDirection.normalized * moveSpeed; 
                    }
                    // After each frame, set Vector3.y value back to its previous stored position
                    moveDirection.y = yStore; 
                #endregion

                //_____________________________________________________________________________________________________________________
                // ROTATION:
                //---------------------------------------------------------------------------------------------------------------------
                #region playerModel Rotation:
                    if(canWalk){
                        // Move player in different directions based on camera look direction:
                        if (!isStandingStill){
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
                #endregion

                //_____________________________________________________________________________________________________________________
                // MOVEMENT COMMAND:
                //---------------------------------------------------------------------------------------------------------------------
                #region Movement Command
                    controller.Move(moveDirection * Time.deltaTime);
                #endregion

                //_____________________________________________________________________________________________________________________
                // ANIMATION VARIABLES
                //---------------------------------------------------------------------------------------------------------------------
                #region Animation Handler
                    anim.SetBool("isJumpingAnimation", isJumpingAnimation);
                    anim.SetBool("isGrounded", controller.isGrounded);
                    anim.SetBool("isJumping", isJumping);
                    anim.SetFloat("Speed", (Mathf.Abs(joystickMovement.GetComponent<UltimateJoystick>().GetVerticalAxis()) + Mathf.Abs(joystickMovement.GetComponent<UltimateJoystick>().GetHorizontalAxis())));
                    anim.SetBool("isRolling", isRolling);
                    anim.SetBool("isEvading", isEvading);
                    anim.SetBool("isAttacking", isAttacking);
                #endregion

                //_____________________________________________________________________________________________________________________
                // LOOPING SOUND EFFECTS:
                //---------------------------------------------------------------------------------------------------------------------
                #region Sound Effects
                    // BANDAGE:
                    if(doBandage == true && healBandage.isPlaying == false){
                        healBandage.volume = Random.Range(0.55f, 0.65f);
                        healBandage.pitch = Random.Range(1.2f, 1.5f);
                        healBandage.Play();
                    }

                    // SHIELD
                    if(isShielded == true && SE_Shield.isPlaying == false){
                        SE_Shield.Play();
                    }

                    // WALKING (FOOTSTEPS & MOVEMENT)
                    if(controller.isGrounded && !isJumping && !isRolling && controller.velocity.magnitude > 2f && footstepsBasic.isPlaying == false){
                        footstepsBasic.volume = Random.Range(0.80f, 1.00f);
                        footstepsBasic.pitch = Random.Range(0.95f, 1.05f);
                        footstepsBasic.Play();
                        footstepsHeavy.volume = Random.Range(0.05f, 0.10f);
                        footstepsHeavy.Play();
                    }
                #endregion
            }
        }

        // Update plays every frame subsequent to the regular void update
        void LateUpdate()
            {
                #region Set Gravity Format:
                    //Determine which gravityScale to apply:
                    if(isJumping){
                        // APPLY JUMP GRAVITY
                        moveDirection.y += Physics.gravity.y * jumpGravityScale * Time.deltaTime;
                    }
                    else{
                        // APPLY NORMAL GRAVITY
                        moveDirection.y += Physics.gravity.y * gravityScale * Time.deltaTime;
                    }
                #endregion
            }

        // Update which plays every fixed frame
        void FixedUpdate()
            {
                if(!view.IsMine){
                    return;
                }
            }
#endregion

//_____________________________________________________________________________________________________________________
//ABILITIES (Functions):
//---------------------------------------------------------------------------------------------------------------------
#region ABILITIES
        //---------------------------------------------------------------------------------------------------------------------
        // JUMP
        //---------------------------------------------------------------------------------------------------------------------
        #region Jump Ability:
            #region Jump Variables:
                //General Jump & Dodge Variables:
                public float jumpForce; // moveDicreion.y
                private bool isJumpingAnimation = false; // for anim Bool | isGrounded = FALSE | Jump = TRUE
            #endregion

            #region Jump Functions:
                // Jump when triggered:
                public void FJumping()
                    {
                        if(!isJumping && !isRolling && !isEvading && !isShielded)
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
                        if(!isJumping && !isRolling && !isEvading && !isShielded){
                        Invoke("FJumping_Cancel",0.15f);
                        Invoke("FJumping_Animation",0.02f);
                        }
                    }
            #endregion
        #endregion

        //---------------------------------------------------------------------------------------------------------------------
        // ROLL AND EVADE
        //---------------------------------------------------------------------------------------------------------------------
        #region ROLL & EVADE
            #region Roll & Evade Variables:
                public float rollSpeed; // Horizontal push speed of a roll
                public float rollTime; // Amount of 0.seconds of applying rollSpeed
                
                public float evadeSpeed; // Horizontal push speed of evade
                public float evadeTime; // Amount of 0.seconds of applying evadeSpeed

                private float dodgeCooldown; // Cooldown time of roll/evade
                private float dodgeRemainingCooldown; // Tracker for remaining time of cooldown
                float dodgeActivated; // Track the activation time of roll/evade

                [SerializeField] private Image uiFillDodge; // Cooldown image

                public AudioSource SE_Roll; // Sound Effect Roll
            #endregion
        
            #region Roll & Evade Functions:
                // After button press --> Roll or Evade based on movement speed.
                // The decision whether to Roll or Evade is made in: IEnumerator Roll()
                public void FRollAndEvade()
                    {
                        if(!isJumping && canWalk && !isRolling){ // Movement checks
                            if(Time.time - dodgeActivated > (dodgeCooldown)){ // Cooldown check
                                isSlowed = false; // Remove slow effects
                                isCasting = false; // Deactivate channeling
                                isTrapped = false; // Remove immmobilizing effects
                                dodgeCooldown = 30f; // Set cooldown timer
                                StartCoroutine(Roll()); // Play IEnumerator Roll()
                                StartCoroutine(WaitForRollSound(0.1f)); // Play sound
                                StartCoroutine(updateCooldown(dodgeCooldown, uiFillDodge)); // Play IEnumerator updateCooldown()
                                dodgeActivated = Time.time; // Set activation time
                            }
                            else{
                                return;
                            }
                        }
                    }

                // IEnumerator function plays the actual roll
                private IEnumerator Roll(){
                    rollDirection = moveDirection; // Set rollDireciton based on the current moveDirection
                    float startTime = Time.time; // Set starting time of roll
                    // Depending if character object is standing still or moving, play evade or roll
                    if(!isStandingStill){
                        while(Time.time < startTime + rollTime){ // As long as rollTime hasn't passed
                            isRolling = true; // Set isRolling state check to TRUE
                            canWalk = false; // Can't walk while rolling
                            controller.Move(rollDirection * rollSpeed * Time.deltaTime); // Actual movement
                            yield return null;
                        } 
                    }
                    else{
                        while(Time.time < startTime + evadeTime){ // While evadeTime hasn't passed
                            isEvading = true; // Set isEvading state check to TRUE
                            canWalk = false; // Can't walk while evading
                            // Move in the direftion of Object: backwardsOrientation: (This object is a child of PlayerModel)
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
            #endregion
        #endregion
        
        //---------------------------------------------------------------------------------------------------------------------
        // BASIC ATTACK
        //---------------------------------------------------------------------------------------------------------------------
        #region BASIC ATTACK:
            #region Basic Attack Variables:
                private float basicAttackTime = 0.5f; // Force player to stand still for 0.seconds
                public float attackRange; // Set range of attack
                private float attackDuration = 0.3f; // Used for a constant check whether player is currently attacking (Attack Speed)
                
                private float attackCooldown; // Cooldown time between attacks
                private float attackRemainingCooldown; // Tracking remaining cooldown time
                float attackActivated; // Set when attack is activated
                
                public GameObject basicAttackSlash1; // Slash VFX effect 1
                public GameObject basicAttackSlash2; // Slash VFX effect 2
                public GameObject basicImpact; // Imact VFX effect 1
                public GameObject basicImpact2; // Impact VFX effect 2

                public AudioSource basicAttackSound1; // Sound effect 1
                public AudioSource basicAttackSound2; // Sound effect 2
                public AudioSource swordGotHit1; // Impact (Clash) sound effect
                public AudioSource HitHeavy; // Heavy Impact sound effect

                [SerializeField] private Image uiFillAttack; // Cooldown image
            #endregion
            // BASIC ATTACK
            #region Basic Attack Functions:
                public void FBasicAttack()
                {
                    if(!isJumping && !isRolling && !isEvading && !isShielded){ // Check movement states
                        if(Time.time - attackActivated > (attackCooldown)){ // Check cooldown
                            if(isInRange < attackRange && isAttackTarget != GameObject.Find("Empty_Target")){ // Check range & if isAttackTarget
                                isCasting = false; // Stop casting
                                isInCombat = true; // Bring plalyer in combat
                                isAttacking = true; // Prevent double attacks
                                anim.SetInteger("BasicAttackIndex", Random.Range(0,2)); // Choose random attack animation
                                attackActivated = Time.time; // Set attack activation time
                                attackCooldown = 1.2f; // Set cooldown -- NOTE!! Will be based on weapon speed!

                                StartCoroutine(BasicAttack_Still()); // Stand still for 0.seconds
                                StartCoroutine(WaitForSlash(0.12f, 0.17f)); // Timing for slash VFX effect
                                StartCoroutine(WaitForImpact(0.2f)); // Timing for impact VFX effect
                                StartCoroutine(updateCooldown(attackCooldown, uiFillAttack)); // Start cooldown timer

                                isAttackTarget.GetComponent<Handler_Stats>().TakeDamage(Random.Range(5, 10)); // Damage attack target
                            }
                        }
                        else{
                            return;
                        }
                    }
                }
                
                // Used to prevent player from walking when attacking
                private IEnumerator BasicAttack_Still(){
                    float attackStartTime = Time.time; // Set start of attack time
                    while(Time.time < attackStartTime + basicAttackTime){ // As long as basicAttackTime hasn't passed
                        moveSpeed = 0; // Stand still
                        yield return null;
                    }
                    moveSpeed = moveSpeedStore; // Restore back to original move speed
                }

                // Basic Attack Slash effects & Sound effects.
                private IEnumerator WaitForSlash(float seconds1, float seconds2)
                {
                    if (anim.GetInteger("BasicAttackIndex") == 0){
                        basicAttackSound1.volume = Random.Range(0.2f, 0.4f);
                        basicAttackSound1.pitch = Random.Range(0.9f, 1.1f);
                        basicAttackSound1.Play();
                        yield return new WaitForSeconds(seconds1);
                        basicAttackSlash1.SetActive(true);
                    }
                    if (anim.GetInteger("BasicAttackIndex") == 1){
                        basicAttackSound2.volume = Random.Range(0.2f, 0.4f);
                        basicAttackSound2.pitch = Random.Range(0.9f, 1.1f);
                        basicAttackSound2.Play();
                        yield return new WaitForSeconds(seconds2);
                        basicAttackSlash2.SetActive(true);
                    }
                    yield return new WaitForSeconds(0.5f);
                    basicAttackSlash1.SetActive(false);
                    basicAttackSlash2.SetActive(false);
                }
                
                // Basic Attack Impact effect & Sound effect.
                private IEnumerator WaitForImpact(float seconds1)
        {
            yield return new WaitForSeconds(seconds1);
            swordGotHit1.Play();
            HitHeavy.Play();
            PhotonNetwork.Instantiate(basicImpact.name, new Vector3(isAttackTarget.transform.position.x, isAttackTarget.transform.position.y + 1.2f, isAttackTarget.transform.position.z), Quaternion.identity);
            PhotonNetwork.Instantiate(basicImpact2.name, new Vector3(isAttackTarget.transform.position.x, isAttackTarget.transform.position.y + 1.2f, isAttackTarget.transform.position.z), Quaternion.identity);
        }
            #endregion
        #endregion

        //---------------------------------------------------------------------------------------------------------------------
        // SHIELD
        //---------------------------------------------------------------------------------------------------------------------
        #region SHIELD:
            #region Shield Variables:
                public GameObject shieldBreakObject; // VFX Break effecct
                public GameObject shieldObject; // Game object Shield

                private float shieldCooldown = 30.0f; // Cooldown
                private float shieldActivated; // Time value when shield got activated
                private float shieldDuration = 5.0f; // Max duration of holding shield

                [SerializeField] private Image uiFillShield; // Cooldown image

                public AudioSource SE_Shield; // Sound effect shield (Loop)
                public AudioSource SE_ShieldBreak; // Sound effect when shield breaks
            #endregion    
        
            // While holding shield button play this function
            public void FShield()
            {
                if(!isEvading && !isRolling && Time.time - shieldActivated > (shieldCooldown)){ // Check movement states & Cooldown
                    isShielded = true; // Shielded state (For decreased damage calculation)
                    isCasting = false; // Stop casting
                    moveSpeed = halfMoveSpeed; // Reduce movement speed
                    shieldActivated = Time.time; // Set activation time
                    shieldCooldown = 30.0f; // Determine cooldown time

                    PhotonNetwork.Instantiate(shieldObject.name, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity); // Spawn shield object on network
                    StartCoroutine(updateCooldown(shieldCooldown, uiFillShield)); // Start cooldown
                    Invoke("F_ShieldBreak", 5.0f); // Break after max 5 sec
                }
            }

            // Function to force a shield break
            public void F_ShieldBreak(){
            if(isShielded){
                isShielded = false; // Stop shielded state
                PhotonNetwork.Instantiate(shieldBreakObject.name, new Vector3(transform.position.x, transform.position.y +1.6f, transform.position.z), Quaternion.identity); // Spawn shield break VFX on Photon network
                SE_ShieldBreak.Play(); // Play VFX effect
                moveSpeed = moveSpeedStore; // Restore movementSpeed to what it was before
                GameObject[] shields = GameObject.FindGameObjectsWithTag("Shield");  // Find the shielf object owned by player
                foreach (GameObject shield in shields){ // Go through all shields in scene
                    if(shield.GetComponent<PhotonView>().Owner == playerGameObject.GetComponent<PhotonView>().Owner){ // If isMine
                        PhotonNetwork.Destroy(shield); // Destroy shield on the network
                        break;
                    }
                }
            }
        }
        #endregion

        //---------------------------------------------------------------------------------------------------------------------
        // BANDAGE
        //---------------------------------------------------------------------------------------------------------------------
        #region BANDAGE
            #region Bandage Variables;
                public GameObject bandageObject; // Reference to bandage VFX parent object
                private bool doBandage = false; // Set (Default) bandage state
                private float bandageActivated; // Variable for holding bandage activation time

                private float bandageHealth = 4.0f; // Health gained per second
                private float bandageCooldown = 15f; // Bandage cooldown
                
                public AudioSource healBandage; // Audio effect
                [SerializeField] private Image uiFillBandage; // UI Cooldown
            #endregion
        
        // Function to perform bandage while holding the bandage button
        public void FBandage()
        {
            if(!isRolling && !isEvading && !isShielded && Time.time - bandageActivated > (bandageCooldown) && controller.isGrounded){ // Movement state checks & Cooldown check
                bandageActivated = Time.time; // Set activation time to .Now
                isCasting = true; // Set casting state
                doBandage = true; // Set bandage state
                moveSpeed = 0; // Stand still
                bandageCooldown = 15f; // Set cooldown

                PhotonNetwork.Instantiate(bandageObject.name, new Vector3(transform.position.x, transform.position.y + 0.81f, transform.position.z), Quaternion.identity); // Instantiate game object over network: 
                
                StartCoroutine(Bandage()); // Start IEnumerator Bandage (Channel bandage)
                StartCoroutine(Channel(5.0f)); // Channel bar for 5 seconds
                StartCoroutine(updateCooldown(bandageCooldown, uiFillBandage)); // Start cooldown
                Invoke("F_BandageBreak", 5.0f); // Bandage function breaks after 5 seconds.

                anim.SetTrigger("CastBandage"); // Start animation
                anim.ResetTrigger("StopBandage"); // Set stop bandage trigger to false
            }
        }

        // Function to break bandage channeling at max time or when button released
        public void F_BandageBreak(){
            moveSpeed = moveSpeedStore; // Restore movement speed to original
            channelBar.gameObject.SetActive(false); // Disable channel bar
            channelGroup.SetActive(false); // De-activate channel object
            doBandage = false; // Cancel bandage state
            isCasting = false; // Stop casting state

            GameObject[] bandages = GameObject.FindGameObjectsWithTag("Bandage"); // Find all bandage objects on network
            foreach (GameObject bandage in bandages){
                if(bandage.GetComponent<PhotonView>().Owner == playerGameObject.GetComponent<PhotonView>().Owner){ // if isMine
                    PhotonNetwork.Destroy(bandage); // Remove my bandage object over the network
                    break;
                }

            StopCoroutine(Channel(5.0f)); // Stop channeling coroutine
            StopCoroutine(Bandage()); // Stop bandage coroutine

            anim.ResetTrigger("CastBandage"); // Set CastBandage animation trigger to false
            anim.SetTrigger("StopBandage"); // Set StopBandage trigger to True
            }
        }

        // Actual bandage function (While loop each second)
        private IEnumerator Bandage(){
            while(doBandage == true){ // As long as in doBandage state
                GetComponent<Handler_Stats>().RestoreHealth(bandageHealth); // Restore health of player.self
                yield return new WaitForSeconds(1f); // Wait a second for the next + health
            }
        }
        #endregion

        //---------------------------------------------------------------------------------------------------------------------
        // SWAP
        //---------------------------------------------------------------------------------------------------------------------
        #region SWAP
            #region Swap Variables:
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
            #endregion
        
            // Function to swap available buttons between Stance A & Stance B
            public void FSwap()
            {
                if(Time.time - swapActivated > (swapCooldown)){ // Cooldown check
                    uiFillSwap.gameObject.SetActive(true); // Set cooldown image active
                    swapCooldown = 60f; // Cooldown time
                    swapActivated = Time.time; // Set activation time to .Now
                    
                    PhotonNetwork.Instantiate(swapVFX.name, new Vector3(transform.position.x, transform.position.y +0.65f, transform.position.z), Quaternion.identity); // Spawn Swap VFX over the network
                    
                    StartCoroutine(updateCooldown(swapCooldown, uiFillSwap));  // Actual Swap function
                    if(isStanceA){ // If in Stance A --> Switch to B
                        StartCoroutine(swapToIconB());
                    }
                    else if(isStanceB){ // If in Stance B --> Switch to A
                        StartCoroutine(swapToIconA());
                    }

                    PowerUp1.Play(); // Play beam audio effect 
                    StartCoroutine(WaitForPop());
                }
            }

            // IEnumerator function to Swap --> A (Visuals)
            private IEnumerator swapToIconA(){
                float dissapearSwap = 7f; // After x Seconds --> Swap effect dissapears
                isStanceA = true; // Activate being in stance A
                isStanceB = false; // De-Activate being in stance B
                groupStanceB.SetActive(false); // D-Activate button group B
                groupStanceA.SetActive(true); // Activate button group A
                swapIconB.SetActive(false); // Deactivate VFX icon B
                swapIconA.SetActive(true); // Activate VFX icon A
                while(dissapearSwap >= 0){ // As long as dissapearSwap time isn't over
                    dissapearSwap -= Time.deltaTime; // Lower countdown timer
                    swapIconA.SetActive(true); // Play VFX Icon A
                    yield return null; 
                }
                swapIconA.SetActive(false); // After countdown also deactivate icon A
            }

            // IEnumerator function to Swap --> B (Visuals)
            private IEnumerator swapToIconB(){
                float dissapearSwap = 7f; // After x Seconds --> Swap effect dissapears
                isStanceA = false; // De-activate being in stance A
                isStanceB = true; // Activate being in stance B
                groupStanceA.SetActive(false); // De-Activate button group A
                groupStanceB.SetActive(true); // Activate button group B
                swapIconA.SetActive(false); // De-Activate VFX icon A
                swapIconB.SetActive(true); // Activate VFX icon B
                while(dissapearSwap >= 0){ // As long as dissapearSwap time isn't over
                    dissapearSwap -= Time.deltaTime; // Lower countdown timer
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
        #endregion
#endregion // Region Abilities


//_____________________________________________________________________________________________________________________
//COOLDOWN & CHANNELING (Functions):
//---------------------------------------------------------------------------------------------------------------------
#region COOLDOWN & CHANNELING
        // (FOR TESTING) function to damage self
        public void FSelfDamage()
        {
            GetComponent<Handler_Stats>().TakeDamage(5);
        }

        // Function to force casting/channeling break
        public void F_CastBreak(){
            isCasting = false; // Set isCasting to false
        }

        //_____________________________________________________________________________________________________________________
        // CHANNELING -- to regulate cooldowns (Arguments: Cooldown time, Cooldown Image)
        //---------------------------------------------------------------------------------------------------------------------
        private IEnumerator updateCooldown(float cooldown, Image cooldownImage){
            float cooldownRemaining = cooldown;  // Set remaining cooldown based on argument
            cooldownImage.gameObject.SetActive(true); // Activate cooldown image (overlay image)

            while(cooldownRemaining >= 0){ // As long as on cooldown
                    cooldownRemaining -= Time.deltaTime; // Keep countdown timer going
                    cooldownImage.fillAmount = cooldownRemaining / cooldown; // Fill image (circular percentage)
                    yield return null;
            }
            cooldownImage.gameObject.SetActive(false); // De-Activate cooldown image
        }

        //_____________________________________________________________________________________________________________________
        // CHANNELING --  to regulate channeling time / visual (Arguments: Channeling duration)
        //---------------------------------------------------------------------------------------------------------------------
        private IEnumerator Channel(float duration){
            float channelRemainingDuration = duration; // Set duration based on argument
            channelGroup.SetActive(true); // Activate channel group (visuals)
            channelBar.gameObject.SetActive(true); // Activate channeling bar (visual)

            while(channelRemainingDuration >= 0){ // As long as duration hasn't passed
                isCasting = true; // Stay in casting state
                channelRemainingDuration -= Time.deltaTime; // Continue countdown
                channelBar.fillAmount = channelRemainingDuration / duration; // Fill channel bar from 1% - 100% (Speed is based on duration)
                yield return null;
            }

            channelBar.gameObject.SetActive(false); // After finishing channeling: De-Activate channel bar
            channelGroup.SetActive(false); // After finishing channeling: De-Activate channel group
        }
#endregion
    }
}
