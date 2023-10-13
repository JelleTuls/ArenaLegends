
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

//NOTE: isInteracting yet to apply!!

namespace CJ
{
    public class Handler_Movement_OnPlayer : MonoBehaviour
    {
        //Variables:
        #region Photon Requirements:
        //Photon Views:
        PhotonView view;
        PhotonView cameraView;
        PhotonView uiCanvasView;
        PhotonView miniMapCameraView;
        #endregion

        #region Player Requirements:
        public CharacterController controller;
        public GameObject playerModel;
        public GameObject playerGameObject;
        Rigidbody rb;

        public Transform horizontalOrientation;
        public GameObject backwardsOrientation; 
        private Vector3 horizontalRotation;
        private Vector3 moveDirection; //Determine direction based on Horizontal / Veritcal Input
        private Vector3 rollDirection;

        private GameObject isAttackTarget;
        #endregion

        #region State Checks:
        //private bool isInteracting;
        private bool canWalk; //Always true, EXCEPT: isRolling, isEvading!
        private bool isStandingStill; //Currently for regulation purpuse only!
        private bool isJumping = false;
        private bool isJumpingAnimation = false;

        private bool isRolling;
        private bool isEvading;

        [SerializeField] public bool isShielded;
        [SerializeField] public bool isCasting;
        [SerializeField] public bool isInCombat;
        [SerializeField] public bool isSlowed;
        [SerializeField] public bool isTrapped;

        private bool isAttacking; //Momental & Duration check!

        public float isInRange;
        #endregion

        #region Player Movement:
        public float moveSpeed;
        private float halfMoveSpeed;
        public float moveSpeedStore;
        public float rotateSpeed;
        public float gravityScale; //Gravity Multiplyer
        public AudioSource footstepsBasic;
        public AudioSource footstepsHeavy;
        #endregion

        #region Player Jumping, Rolling, Dodge:
        //General Jump & Dodge Variables:
        public float jumpForce;
        public float jumpGravityScale;

        public float rollSpeed;
        public float rollTime;
        public float evadeSpeed;
        public float evadeTime;
        public AudioSource SE_Roll;
        
        //Dodge Cooldown Variables:
        private float dodgeCooldown;
        private float dodgeRemainingCooldown;
        float dodgeActivated;
        [SerializeField] private Image uiFillDodge;
        #endregion

        #region Basic Attack:
        //Range Detection:
        private float basicAttackTime = 0.5f;
        public float attackRange;
        private float attackDuration = 0.3f;
        public GameObject basicAttackSlash1; 
        public GameObject basicAttackSlash2;
        public GameObject basicImpact;
        public GameObject basicImpact2;
        public AudioSource basicAttackSound1;
        public AudioSource basicAttackSound2;
        public AudioSource swordGotHit1;
        public AudioSource HitHeavy;

        //Dodge Cooldown Variables:
        private float attackCooldown;
        private float attackRemainingCooldown;
        float attackActivated;
        [SerializeField] private Image uiFillAttack;
        #endregion

        #region Shield:
        public GameObject shieldBreakObject;
        public GameObject shieldObject;

        private float shieldCooldown = 30.0f;
        private float shieldActivated;
        private float shieldDuration = 5.0f;
        [SerializeField] private Image uiFillShield;
        public AudioSource SE_Shield;
        public AudioSource SE_ShieldBreak;
        #endregion    

        #region Bandage;
        public GameObject bandageObject;
        private float bandageHealth = 4.0f;
        [SerializeField] private Image uiFillBandage;
        private float bandageCooldown = 15f;
        private float bandageActivated;
        private bool doBandage = false;
        public AudioSource healBandage;
        #endregion

        #region Swap:
        private bool isStanceA;
        private bool isStanceB;
        public GameObject groupStanceA;
        public GameObject groupStanceB;
        public GameObject swapVFX;
        private float swapCooldown = 90f;
        private float swapActivated;
        [SerializeField] private Image uiFillSwap;
        public GameObject swapIconA;
        public GameObject swapIconB;
        public AudioSource PowerUp1;

        public AudioSource PowerUpPop;
        #endregion

        #region Player UI:
        public Image myPortraitImage;
        public Image myPortraitClass;
        public Sprite myPortrait;
        public Sprite myClass;
        public TextMeshProUGUI myName;
        [SerializeField] private Image channelBar;
        public GameObject channelGroup;

        public GameObject joystickMovement;
        private float verticalMovement;
        #endregion   

        #region De-Parent Start:
        public GameObject uiCanvas;
        public GameObject mainCamera;
        public GameObject miniMapCamera;
        #endregion

        #region Animation Handlers:
        public Animator anim;
        #endregion   

        void Awake(){
            //Happens before Void Start!
            rb = GetComponent<Rigidbody>();

            #region De-Parenting from Prefab:
            uiCanvas.transform.SetParent(null);
            mainCamera.transform.SetParent(null);
            miniMapCamera.transform.SetParent(null);
            #endregion

            #region Photon View Components:
            view = GetComponent<PhotonView>();
            cameraView = mainCamera.GetComponent<PhotonView>();
            uiCanvasView = uiCanvas.GetComponent<PhotonView>();
            miniMapCameraView = miniMapCamera.GetComponent<PhotonView>();
            #endregion
        }

        void Start(){
            #region If(.....!View.IsMine) Destroy Photon Components:
            if(!uiCanvasView.IsMine){Destroy(uiCanvas);}
            if(!cameraView.IsMine){Destroy(mainCamera);}
            if(!miniMapCameraView.IsMine){Destroy(miniMapCamera);}
            if(!view.IsMine){Destroy(rb);}
            #endregion

            #region Get Components:
            controller = GetComponent<CharacterController>();
            #endregion
            
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
        
            #region Initial UI Settings:
            myPortraitImage.sprite = myPortrait;
            myPortraitClass.sprite = myClass;
            myName.text = playerGameObject.name;
            #endregion
        }

        void Update(){
            #region If(.....!View.IsMine) Destroy Photon Components:
            if(!uiCanvasView.IsMine){PhotonNetwork.Destroy(uiCanvas);}
            if(!cameraView.IsMine){PhotonNetwork.Destroy(mainCamera);}
            if(!miniMapCameraView.IsMine){PhotonNetwork.Destroy(miniMapCamera);}
            if(!view.IsMine){Destroy(rb);}
            #endregion

            //Only play if Player_Object.View IsMine:
            if(view.IsMine){
                //Initial Updates:
                #region Initial Settings (Per Frame):
                //Check target:
                isAttackTarget = gameObject.GetComponent<Sense>().isTarget;
                //Check if attack is ready:
                if(Time.time - attackActivated > attackDuration){
                    isAttacking = false;
                }
                #endregion

                //Basic Movement:
                #region Registering Values (Per Frame):
                //Prevent flickering up and down by saving the vector3.y value:
                float yStore = moveDirection.y;
                //Range Detection:
                isInRange = Vector3.Distance(transform.position, isAttackTarget.transform.position);
                #endregion

                #region Initial Independent State Checks:
                //isStandingStill:
                if(moveDirection.x + moveDirection.z > 0.001 || moveDirection.x + moveDirection.z < -0.001){
                    isStandingStill = false;
                }
                else{
                    isStandingStill = true;
                }
            
                //isJumping:
                if(controller.isGrounded){
                    isJumping = false;
                    isJumpingAnimation = false;
                }
                #endregion

                #region Basic Movement:
                if(canWalk)
                {
                    //Movement Velocity based on Input:                 
                    moveDirection = (transform.forward * joystickMovement.GetComponent<UltimateJoystick>().GetVerticalAxis()) + (transform.right * joystickMovement.GetComponent<UltimateJoystick>().GetHorizontalAxis());
                    //Actually moving the Player:
                    moveDirection = moveDirection.normalized * moveSpeed;
                }
                //After each frame, set Vector3.y value back to its previous stored position:
                moveDirection.y = yStore;
                #endregion

                #region playerModel Rotation:
                if(canWalk){
                    //Move player in different directions based on camera look direction:
                    if (!isStandingStill){
                        //Rotation of player = horizontalOrientation object ==> Which is guided by Mouse Y:
                            //When standing still, camera can rotate around the Player!
                        transform.rotation = Quaternion.Euler(0f, horizontalOrientation.rotation.eulerAngles.y, 0f);

                        //Calculate direciton to rotate towards:
                            //1) Store direction in a Variable
                            //2) Calculate the direction to rotate towards (toRotation):
                        horizontalRotation = new Vector3(moveDirection.x, 0f, moveDirection.z);
                        Quaternion toRotation = Quaternion.LookRotation(horizontalRotation, Vector3.up);
                        //Rotate from original direction => new calculated direction (toRotation):
                        playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, toRotation, rotateSpeed * Time.deltaTime);
                    }
                }
                #endregion

                //Final command to move:
                controller.Move(moveDirection * Time.deltaTime);

                //Animation Checks:
                #region Animation Handler
                anim.SetBool("isJumpingAnimation", isJumpingAnimation);
                anim.SetBool("isGrounded", controller.isGrounded);
                anim.SetBool("isJumping", isJumping);
                anim.SetFloat("Speed", (Mathf.Abs(joystickMovement.GetComponent<UltimateJoystick>().GetVerticalAxis()) + Mathf.Abs(joystickMovement.GetComponent<UltimateJoystick>().GetHorizontalAxis())));
                anim.SetBool("isRolling", isRolling);
                anim.SetBool("isEvading", isEvading);
                anim.SetBool("isAttacking", isAttacking);
                #endregion

                // Play Sound Effects:
                if(doBandage == true && healBandage.isPlaying == false){
                    healBandage.volume = Random.Range(0.55f, 0.65f);
                    healBandage.pitch = Random.Range(1.2f, 1.5f);
                    healBandage.Play();
                }

                // Play Sound Effects:
                if(isShielded == true && SE_Shield.isPlaying == false){
                    SE_Shield.Play();
                }

                if(controller.isGrounded && !isJumping && !isRolling && controller.velocity.magnitude > 2f && footstepsBasic.isPlaying == false){
                    footstepsBasic.volume = Random.Range(0.80f, 1.00f);
                    footstepsBasic.pitch = Random.Range(0.95f, 1.05f);
                    footstepsBasic.Play();
                    footstepsHeavy.volume = Random.Range(0.05f, 0.10f);
                    footstepsHeavy.Play();
                }
            }
        }


        //_______________________________________
        //ABILITIES (Functions):
        //---------------------------------------

        // JUMP
        #region Jump Ability:
        public void FJumping()
        {
            //Jump when triggered:
            if(!isJumping && !isRolling && !isEvading && !isShielded)
            {
                moveDirection.y = jumpForce;
            }
        }
        public void FJumping_Cancel()
        {
            isJumping = true;
            moveDirection.y = jumpForce;
        }

        public void FJumping_Animation()
        {
            isJumpingAnimation = true;
        }

        public void FJumping_Two()
        {   
            isJumpingAnimation = true;
            if(!isJumping && !isRolling && !isEvading && !isShielded){
            Invoke("FJumping_Cancel",0.15f);
            Invoke("FJumping_Animation",0.02f);
            }
        }
        #endregion

        // ROLL AND EVADE
        #region ROLL & EVADE
        public void FRollAndEvade()
        {
            if(!isJumping && canWalk && !isRolling){
                if(Time.time - dodgeActivated > (dodgeCooldown)){
                    //Remove Immobilzations
                    isSlowed = false;
                    isCasting = false;
                    isTrapped = false;
                    //Set Cooldown Timer:
                    dodgeCooldown = 30f;
                    //Call actual Dodging:
                    StartCoroutine(Roll());
                    StartCoroutine(WaitForRollSound(0.1f));
                    //Call Dodge Cooldown Timer:
                    StartCoroutine(updateCooldown(dodgeCooldown, uiFillDodge));
                    dodgeActivated = Time.time;
                }
                else{
                    return;
                }
                
            }
        }

        private IEnumerator Roll(){
            #region IEnumerator Roll:
            rollDirection = moveDirection;
            float startTime = Time.time;
            if(!isStandingStill){
                while(Time.time < startTime + rollTime){
                    isRolling = true;
                    canWalk = false;
                    controller.Move(rollDirection * rollSpeed * Time.deltaTime);
                    yield return null;
                }
                
            }
            else{
                while(Time.time < startTime + evadeTime){
                    isEvading = true;
                    canWalk = false;
                    // Move in the direftion of Object: backwardsOrientation: (This object is a child of PlayerModel)
                    transform.position = Vector3.MoveTowards(transform.position, backwardsOrientation.transform.position, evadeSpeed * Time.deltaTime);
                    yield return null;
                }
            }
            canWalk = true;
            isRolling = false;
            isEvading = false;
            #endregion
        }

        private IEnumerator WaitForRollSound(float seconds1)
        {
            yield return new WaitForSeconds(seconds1);
            SE_Roll.Play();
        }
        #endregion

        // BASIC ATTACK
        #region BASIC ATTACK:
        public void FBasicAttack()
        {
            isInCombat = true;
            //Attack:
            if(!isJumping && !isRolling && !isEvading && !isShielded){
                if(Time.time - attackActivated > (attackCooldown)){
                    if(isInRange < attackRange && isAttackTarget != GameObject.Find("Empty_Target")){
                        
                        isCasting = false;
                        isInCombat = true;
                        isAttacking = true;
                        anim.SetInteger("BasicAttackIndex", Random.Range(0,2));
                        StartCoroutine(BasicAttack_Still());
                        StartCoroutine(WaitForSlash(0.12f, 0.17f));
                        StartCoroutine(WaitForImpact(0.2f));
                        //NOTE!! Will be based on weapon speed!
                        attackCooldown = 1.2f;
                        //Call Attack Cooldown Timer:
                        StartCoroutine(updateCooldown(attackCooldown, uiFillAttack));
                        attackActivated = Time.time;
                        //Doing damage to isAttackTarget:
                        isAttackTarget.GetComponent<Handler_Stats>().TakeDamage(Random.Range(5, 10));
                    }
                }
                else{
                    return;
                }
            }
        }


        // Used to prevent player from walking when attacking
        private IEnumerator BasicAttack_Still(){
            float attackStartTime = Time.time;
            while(Time.time < attackStartTime + basicAttackTime){
                moveSpeed = 0;
                yield return null;
            }
            moveSpeed = moveSpeedStore;
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


        private IEnumerator WaitForImpact(float seconds1)
        {
            yield return new WaitForSeconds(seconds1);
            swordGotHit1.Play();
            HitHeavy.Play();
            PhotonNetwork.Instantiate(basicImpact.name, new Vector3(isAttackTarget.transform.position.x, isAttackTarget.transform.position.y + 1.2f, isAttackTarget.transform.position.z), Quaternion.identity);
            PhotonNetwork.Instantiate(basicImpact2.name, new Vector3(isAttackTarget.transform.position.x, isAttackTarget.transform.position.y + 1.2f, isAttackTarget.transform.position.z), Quaternion.identity);
        }
        #endregion

        // SHIELD
        #region SHIELD:
        public void FShield()
        {
            if(!isEvading && !isRolling && Time.time - shieldActivated > (shieldCooldown)){
                isShielded = true;
                isCasting = false;
                PhotonNetwork.Instantiate(shieldObject.name, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
                moveSpeed = halfMoveSpeed;
                shieldActivated = Time.time;
                shieldCooldown = 30.0f;
                //Start Cooldown:
                StartCoroutine(updateCooldown(shieldCooldown, uiFillShield));
                Invoke("F_ShieldBreak", 5.0f);
            }
        }

        public void F_ShieldBreak(){
            if(isShielded){
                
                isShielded = false;
                //Play Particle:
                PhotonNetwork.Instantiate(shieldBreakObject.name, new Vector3(transform.position.x, transform.position.y +1.6f, transform.position.z), Quaternion.identity);
                //Set movespeed back to original:
                SE_ShieldBreak.Play();
                moveSpeed = moveSpeedStore;
                //Reset all Cooldowns & Timters:
                GameObject[] shields = GameObject.FindGameObjectsWithTag("Shield"); 
                foreach (GameObject shield in shields){
                    if(shield.GetComponent<PhotonView>().Owner == playerGameObject.GetComponent<PhotonView>().Owner){
                        PhotonNetwork.Destroy(shield);
                        break;
                    }
                }
            }
        }
        #endregion

        // BANDAGE
        #region BANDAGE
        public void FBandage()
        {
            if(!isRolling && !isEvading && !isShielded && Time.time - bandageActivated > (bandageCooldown) && controller.isGrounded){
                // bandageObject.SetActive(true);
                bandageActivated = Time.time;
                //Instantiate game object over network: 
                PhotonNetwork.Instantiate(bandageObject.name, new Vector3(transform.position.x, transform.position.y + 0.81f, transform.position.z), Quaternion.identity);
                // //Activate state & objects:
                anim.SetTrigger("CastBandage");
                anim.ResetTrigger("StopBandage");
                isCasting = true;
                doBandage = true;
                moveSpeed = 0;
                bandageCooldown = 15f;
                // //Play Bandage Function:
                StartCoroutine(Bandage());
                StartCoroutine(Channel(5.0f));
                StartCoroutine(updateCooldown(bandageCooldown, uiFillBandage));
                // Make bandage break after 5 seconds.
                Invoke("F_BandageBreak", 5.0f);
            }
        }

        public void F_BandageBreak(){
        // Animation tirggers:
            anim.ResetTrigger("CastBandage");
            anim.SetTrigger("StopBandage");
        // State settings:
            isCasting = false;
            channelBar.gameObject.SetActive(false);
            channelGroup.SetActive(false);
            moveSpeed = moveSpeedStore;
            doBandage = false; 
        // Remove bandage instantiated object:
            GameObject[] bandages = GameObject.FindGameObjectsWithTag("Bandage");
            foreach (GameObject bandage in bandages){
                if(bandage.GetComponent<PhotonView>().Owner == playerGameObject.GetComponent<PhotonView>().Owner){
                    PhotonNetwork.Destroy(bandage);
                    break;
                }
            }
        // Cancel bandaging:
            StopCoroutine(Channel(5.0f));
            StopCoroutine(Bandage());
        }

        private IEnumerator Bandage(){
            while(doBandage == true){
                GetComponent<Handler_Stats>().RestoreHealth(bandageHealth);
                yield return new WaitForSeconds(1f);
            }
        }
        #endregion

        // SWAP
        #region SWAP
        public void FSwap()
        {
            if(Time.time - swapActivated > (swapCooldown)){
                uiFillSwap.gameObject.SetActive(true);
                swapCooldown = 60f;
                swapActivated = Time.time;
                PhotonNetwork.Instantiate(swapVFX.name, new Vector3(transform.position.x, transform.position.y +0.65f, transform.position.z), Quaternion.identity);
                StartCoroutine(updateCooldown(swapCooldown, uiFillSwap));
                if(isStanceA){
                    StartCoroutine(swapToIconB());
                }
                else if(isStanceB){
                    StartCoroutine(swapToIconA());
                }
                PowerUp1.Play();
                StartCoroutine(WaitForPop());
            }
        }


        private IEnumerator swapToIconA(){
            float dissapearSwap = 7f;
            isStanceA = false;
            isStanceB = true;
            groupStanceB.SetActive(false);
            groupStanceA.SetActive(true);
            swapIconB.SetActive(false);
            swapIconA.SetActive(true);
            while(dissapearSwap >= 0){
                dissapearSwap -= Time.deltaTime;
                swapIconA.SetActive(true);
                yield return null;
            }
            swapIconA.SetActive(false);
        }


        private IEnumerator swapToIconB(){
            float dissapearSwap = 7f;
            isStanceA = false;
            isStanceB = true;
            groupStanceA.SetActive(false);
            groupStanceB.SetActive(true);
            swapIconA.SetActive(false);
            swapIconB.SetActive(true);
            while(dissapearSwap >= 0){
                dissapearSwap -= Time.deltaTime;
                swapIconB.SetActive(true);
                yield return null;
            }
            swapIconB.SetActive(false);
        }

        private IEnumerator WaitForPop()
        {
            yield return new WaitForSeconds(2.3f);
            PowerUpPop.Play();
        }
        #endregion
        
        // CHANNELING & COOLDOWNS
        #region CHANNELING & COOLDOWNS
        public void FSelfDamage()
        {
            GetComponent<Handler_Stats>().TakeDamage(5);
        }


        public void F_CastBreak(){
            isCasting = false;
        }

        //COOLDOWN:
        private IEnumerator updateCooldown(float cooldown, Image cooldownImage){
            float cooldownRemaining = cooldown;
            cooldownImage.gameObject.SetActive(true);
            while(cooldownRemaining >= 0){
                    cooldownRemaining -= Time.deltaTime;
                    cooldownImage.fillAmount = cooldownRemaining / cooldown;
                    yield return null;
            }
            cooldownImage.gameObject.SetActive(false);
        }

        //CHANNELING:
        private IEnumerator Channel(float duration){
            float channelRemainingDuration = duration;
            channelGroup.SetActive(true);
            channelBar.gameObject.SetActive(true);
            while(channelRemainingDuration >= 0){
                isCasting = true;
                channelRemainingDuration -= Time.deltaTime;
                channelBar.fillAmount = channelRemainingDuration / duration;
                yield return null;
            }
            channelBar.gameObject.SetActive(false);
            channelGroup.SetActive(false);
        }
        #endregion
        

        //_______________________________________
        //LATE UPDATE:
        //---------------------------------------
    
        void LateUpdate()
        {
            #region Set Gravity Format:
            //Determine which gravityScale to apply:
            if(isJumping){
                //Add jumping gravity while isJumping
                moveDirection.y += Physics.gravity.y * jumpGravityScale * Time.deltaTime;
            }
            else{
                //Add normal gravity
                moveDirection.y += Physics.gravity.y * gravityScale * Time.deltaTime;
            }
            #endregion
        }

        void FixedUpdate()
        {
            if(!view.IsMine){
                return;
            }
        }
    }
}
