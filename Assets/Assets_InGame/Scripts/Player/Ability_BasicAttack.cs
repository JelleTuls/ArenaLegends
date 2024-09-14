using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using Photon.Pun; 
using CJ;

namespace CJ
{
public class Ability_BasicAttack : MonoBehaviour
    {
// ################################################################################################################################

// VARIABLES:

// ################################################################################################################################
        // Reference Scripts
        public Player_Handle_Movement Player_Handle_Movement; // Reference to HandlerMovementOnPlayer  
        public Player_Handle_Target Player_Handle_Target;

        // Game Objects
        public GameObject basicAttackSlash1; // Slash VFX effect 1
        public GameObject basicAttackSlash2; // Slash VFX effect 2
        public GameObject basicImpact; // Impact VFX effect 1
        public GameObject basicImpact2; // Impact VFX effect 2

        public GameObject rangedProjectilePrefab; // Reference to the projectile prefab that contains the particle system
        public GameObject hitEffectPrefab; // Reference to the hit particle effect prefab

        // Transforms
        public Transform projectileSpawnPoint; // Reference to the spawn point for the projectile

        // Booleans
        [SerializeField] private bool isRangedClass;

        // Floats
        private float basicAttackTime = 0.5f; // Force player to stand still for 0.seconds
        private int basicAttackDamage;
        public float attackRange; // Set range of attack
        private float attackCooldown; // Cooldown time between attacks
        private float attackRemainingCooldown; // Tracking remaining cooldown time

        public float rangedAttackSpeed = 10f; // Speed of the ranged projectile
        public float rangedAttackDelay1 = 0.1f; // Delay for first animation
        public float rangedAttackDelay2 = 0.2f; // Delay for second animation

        [SerializeField] private float float_WaitForSlash1;
        [SerializeField] private float float_WaitForSlash2;
        [SerializeField] private float float_WaitForImpact;
        
        // Audio
        public AudioSource basicAttackSound1; // Sound effect 1
        public AudioSource basicAttackSound2; // Sound effect 2
        public AudioSource swordGotHit1; // Impact (Clash) sound effect
        public AudioSource HitHeavy; // Heavy Impact sound effect

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

        //---------------------------------------------------------------------------------------------------------------------
        // UPDATE
        //---------------------------------------------------------------------------------------------------------------------
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

                        if (isRangedClass)
                        {
                            float delay = attackIndex == 0 ? rangedAttackDelay1 : rangedAttackDelay2;
                            StartCoroutine(ShootProjectileAtTarget(Player_Handle_Movement.isAttackTarget, projectileSpawnPoint, 1.2f, rangedAttackSpeed, delay, basicAttackDamage));
                            StartCoroutine(BasicAttack_Still()); // Stand still for 0.seconds
                        }
                        else
                        {
                            StartCoroutine(BasicAttack_Still()); // Stand still for 0.seconds
                            StartCoroutine(WaitForSlash(float_WaitForSlash1, float_WaitForSlash2)); // Timing for slash VFX effect
                            StartCoroutine(WaitForImpact(float_WaitForImpact, basicAttackDamage)); // Timing for impact VFX effect
                            Player_Handle_Movement.isAttackTarget.GetComponent<Player_Handle_Stats>().TakeDamage(basicAttackDamage); // Damage attack target
                        }

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

        // Basic Attack Slash effects & Sound effects.
        private IEnumerator WaitForSlash(float seconds1, float seconds2)
        {
            if (Player_Handle_Movement.anim.GetInteger("BasicAttackIndex") == 0)
            {
                basicAttackSound1.volume = Random.Range(0.2f, 0.4f);
                basicAttackSound1.pitch = Random.Range(0.9f, 1.1f);
                basicAttackSound1.Play();
                yield return new WaitForSeconds(seconds1);
                basicAttackSlash1.SetActive(true);
            }
            if (Player_Handle_Movement.anim.GetInteger("BasicAttackIndex") == 1)
            {
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
        private IEnumerator WaitForImpact(float seconds1, int damage)
        {
            yield return new WaitForSeconds(seconds1);
            swordGotHit1.Play();
            HitHeavy.Play();
            PhotonNetwork.Instantiate(basicImpact.name, new Vector3(Player_Handle_Movement.isAttackTarget.transform.position.x, Player_Handle_Movement.isAttackTarget.transform.position.y + 1.2f, Player_Handle_Movement.isAttackTarget.transform.position.z), Quaternion.identity);
            PhotonNetwork.Instantiate(basicImpact2.name, new Vector3(Player_Handle_Movement.isAttackTarget.transform.position.x, Player_Handle_Movement.isAttackTarget.transform.position.y + 1.2f, Player_Handle_Movement.isAttackTarget.transform.position.z), Quaternion.identity);
            Player_Handle_Target.SpawnDamageNumber(Player_Handle_Movement.isAttackTarget, damage);
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
