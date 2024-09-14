using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using Photon.Pun; 
using CJ;

namespace CJ
{
public class Ability_BasicAttack_Ranged : MonoBehaviour
    {
// ################################################################################################################################

// VARIABLES:

// ################################################################################################################################
        // Reference Scripts
        public Player_Handle_Movement Player_Handle_Movement; // Reference to HandlerMovementOnPlayer  
        public Player_Handle_Target Player_Handle_Target;

        // Game Objects
        public GameObject rangedProjectilePrefab; // Reference to the projectile prefab that contains the particle system
        public GameObject hitEffectPrefab; // Reference to the hit particle effect prefab

        // Transforms
        public Transform projectileSpawnPoint; // Reference to the spawn point for the projectile

        // Booleans

        // Floats
        private float basicAttackTime = 0.5f; // Force player to stand still for 0.seconds
        private int basicAttackDamage;
        public float attackRange = 30f; // Set range of attack
        private float attackCooldown; // Cooldown time between attacks
        private float attackRemainingCooldown; // Tracking remaining cooldown time

        public float rangedAttackSpeed = 50f; // Speed of the ranged projectile
        public float rangedAttackDelay1 = 0.35f; // Delay for first animation
        public float rangedAttackDelay2 = 0.1f; // Delay for second animation
        
        // Audio
        public AudioSource projectileShootSound; // Reference to the projectile shoot sound
        public AudioSource projectileHitSound; // Reference to the projectile hit sound
        
        // Images
        [SerializeField] private Image uiFillAttack; // Cooldown image
    

// ################################################################################################################################

// VOIDS:

// ################################################################################################################################   
        //---------------------------------------------------------------------------------------------------------------------
        // START
        //--------------------------------------------------------------------------------------------------------------------- 
        // Start is called before the first frame update
        void Start()
        {
            Player_Handle_Target = GetComponent<Player_Handle_Target>();

            uiFillAttack.gameObject.SetActive(false);
            attackCooldown = 0f;            
        }

        // Update is called once per frame
        void Update()
        {
            
        }


// ################################################################################################################################

// FUNCTIONS:

// ################################################################################################################################
        // BASIC ATTACK
        public void FBasicAttack()
        {
            if (!Player_Handle_Movement.isJumping && !Player_Handle_Movement.isRolling && !Player_Handle_Movement.isEvading && !Player_Handle_Movement.isShielded)
            { // Check movement states
                if (Time.time - Player_Handle_Movement.attackActivated > (attackCooldown))
                { // Check cooldown
                    if (Player_Handle_Movement.isInRange < attackRange && Player_Handle_Movement.isAttackTarget != GameObject.Find("Empty_Target"))
                    { // Check range & if isAttackTarget
                        Player_Handle_Movement.isCasting = false; // Stop casting
                        Player_Handle_Movement.isInCombat = true; // Bring player in combat
                        Player_Handle_Movement.isAttacking = true; // Prevent double attacks
                        int attackIndex = Random.Range(0, 2); // Choose random attack animation
                        Player_Handle_Movement.anim.SetInteger("BasicAttackIndex", attackIndex);
                        Player_Handle_Movement.attackActivated = Time.time; // Set attack activation time
                        attackCooldown = 1.2f; // Set cooldown -- NOTE!! Will be based on weapon speed!
                        basicAttackDamage = Random.Range(13, 20);

                        float delay = attackIndex == 0 ? rangedAttackDelay1 : rangedAttackDelay2;
                        StartCoroutine(ShootProjectileAtTarget(Player_Handle_Movement.isAttackTarget, projectileSpawnPoint, 1.2f, rangedAttackSpeed, delay, basicAttackDamage));
                        StartCoroutine(BasicAttack_Still()); // Stand still for 0.seconds

                        StartCoroutine(Player_Handle_Movement.updateCooldown(attackCooldown, uiFillAttack)); // Start cooldown timer
                    }
                }
                else
                {
                    return;
                }
            }
        }

        // Used to prevent player from walking when attacking
        private IEnumerator BasicAttack_Still()
        {
            float attackStartTime = Time.time; // Set start of attack time
            while (Time.time < attackStartTime + basicAttackTime)
            { // As long as basicAttackTime hasn't passed
                Player_Handle_Movement.moveSpeed = 0; // Stand still
                yield return null;
            }
            Player_Handle_Movement.moveSpeed = Player_Handle_Movement.moveSpeedStore; // Restore back to original move speed
        }


        private IEnumerator ShootProjectileAtTarget(GameObject target, Transform spawnPoint, float hitHeight, float speed, float delay, int damage)
        {
            yield return new WaitForSeconds(delay);

            Vector3 spawnPosition = spawnPoint.position;
            GameObject projectile = PhotonNetwork.Instantiate(rangedProjectilePrefab.name, spawnPosition, Quaternion.identity);
            Ability_BasicAttack_Ranged_Projectile Ability_BasicAttack_Ranged_Projectile = projectile.GetComponent<Ability_BasicAttack_Ranged_Projectile>();
            Ability_BasicAttack_Ranged_Projectile.Initialize(target, damage, speed, spawnPoint.position.y, hitHeight, hitEffectPrefab, projectileHitSound.clip, Player_Handle_Target); // Pass hitEffectPrefab and hit sound

            // Play shooting sound
            if (projectileShootSound != null)
            {
                projectileShootSound.volume = Random.Range(0.2f, 0.4f);
                projectileShootSound.pitch = Random.Range(0.9f, 1.1f);
                projectileShootSound.Play();
            }

            // Play particle effect if available
            ParticleSystem particleSystem = projectile.GetComponentInChildren<ParticleSystem>();
            if (particleSystem != null)
            {
                var main = particleSystem.main;
                main.startSpeed = speed; // Set the speed of the particle
                particleSystem.Play();
            }

            // Wait until the projectile is destroyed
            while (projectile != null)
            {
                yield return null;
            }
        }
    }
}
