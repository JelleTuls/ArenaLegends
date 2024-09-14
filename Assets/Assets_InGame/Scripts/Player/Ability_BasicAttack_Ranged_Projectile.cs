using UnityEngine;
using CJ;

namespace CJ
{
    public class Ability_BasicAttack_Ranged_Projectile : MonoBehaviour
    {
// ################################################################################################################################

// VARIABLES:

// ################################################################################################################################
        private GameObject target;
        private int damage;
        private float speed;
        private float startHeight;
        private float hitHeight;
        private GameObject hitEffectPrefab;
        private AudioClip hitSound;
        private AudioSource audioSource;
        private bool hasHit;

        public Player_Handle_Target Player_Handle_Target; 

// ################################################################################################################################

// VOIDS:

// ################################################################################################################################
         
        //---------------------------------------------------------------------------------------------------------------------
        // START
        //---------------------------------------------------------------------------------------------------------------------
        void Start()
        {
            
        }

        //---------------------------------------------------------------------------------------------------------------------
        // UPDATE 
        //---------------------------------------------------------------------------------------------------------------------
        // !!!!!!
        // Update starts playing when the projectile objects spawns!!!
        // The projectile object is spawned by the ShootProjectileAtTarget function in Ability_BasicAttack.cs
        private void Update()
        {
            if (target == null || hasHit)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 targetPosition = new Vector3(target.transform.position.x, target.transform.position.y + hitHeight, target.transform.position.z);
            Vector3 direction = (targetPosition - transform.position).normalized;

            // Move towards the target
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Rotate to face the target
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
            }

            // Check if the projectile reached the target
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                OnHitTarget();
            }
        }

// ################################################################################################################################

// FUNCTIONS:

// ################################################################################################################################
        //---------------------------------------------------------------------------------------------------------------------
        // INITIALIZE
        //---------------------------------------------------------------------------------------------------------------------
        // The Initialize function is being called in Ability_BasicAttack.cs -- ShootProjectileAtTarget();
        public void Initialize(GameObject target, int damage, float speed, float startHeight, float hitHeight, GameObject hitEffectPrefab, AudioClip hitSound, Player_Handle_Target Player_Handle_Target)
        {
            this.target = target;
            this.damage = damage;
            this.speed = speed;
            this.startHeight = startHeight;
            this.hitHeight = hitHeight;
            this.hitEffectPrefab = hitEffectPrefab;
            this.hitSound = hitSound;
            this.Player_Handle_Target = Player_Handle_Target; // Assign sense reference when initialized
            hasHit = false;

            audioSource = GetComponent<AudioSource>();
        }

        //---------------------------------------------------------------------------------------------------------------------
        // ON HIT TARGET
        //---------------------------------------------------------------------------------------------------------------------
        // Function being called when projectile reaches <0.1f from the target.
        // This function spawns the hitEffectPrefab, its sound effect, and calls the damage number pop-up
        private void OnHitTarget()
        {
            if (hasHit) return;

            hasHit = true;

            target.GetComponent<Player_Handle_Stats>().TakeDamage(damage);

            // Instantiate the hit effect at the target's position
            if (hitEffectPrefab != null)
            {
                GameObject hitEffectInstance = Instantiate(hitEffectPrefab, target.transform.position + new Vector3(0, hitHeight, 0), Quaternion.identity);
                // Play the hit sound
                AudioSource hitAudioSource = hitEffectInstance.GetComponent<AudioSource>();
                if (hitAudioSource != null && hitSound != null)
                {
                    hitAudioSource.clip = hitSound;
                    hitAudioSource.volume = Random.Range(0.2f, 0.4f);
                    hitAudioSource.pitch = Random.Range(0.9f, 1.1f);
                    hitAudioSource.Play();
                }
            }

            // Ensure the Player_Handle_Target is valid before calling its function
            Debug.Log(damage);
            Debug.Log(target);
            Player_Handle_Target.SpawnDamageNumber(target, damage);

            Destroy(gameObject);
        }
    }
}
