using UnityEngine;

namespace CJ
{
    public class Projectile : MonoBehaviour
    {
        private GameObject target;
        private int damage;
        private float speed;
        private float startHeight;
        private float hitHeight;
        private GameObject hitEffectPrefab;

        public void Initialize(GameObject target, int damage, float speed, float startHeight, float hitHeight, GameObject hitEffectPrefab)
        {
            this.target = target;
            this.damage = damage;
            this.speed = speed;
            this.startHeight = startHeight;
            this.hitHeight = hitHeight;
            this.hitEffectPrefab = hitEffectPrefab;
        }

        private void Update()
        {
            if (target == null)
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

        private void OnHitTarget()
        {
            target.GetComponent<Handler_Stats>().TakeDamage(damage);

            // Instantiate the hit effect at the target's position
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, target.transform.position + new Vector3(0, hitHeight, 0), Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
