using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CJ
{
    public class Handler_Stats : MonoBehaviour
    {
        public GameObject playerObject;
        public float shieldReduction = 2.0f;

        public Sprite myPortrait;
        public Sprite myClass;

        public float myHealth;
        //private float lerpTimer;
        public float myMaxHealth = 100f;
        //public float chipSpeed = 2f;
        public Image frontHealthBar;
        public Image floatingHealthBar;
        //public Image backHealthBar;
        public Image frontEnergyBar;
        //public Image backEnergyBar;

        void Start()
        {
            myHealth = myMaxHealth;   
        }

        void Update()
        {
            myHealth = Mathf.Clamp(myHealth, 0, myMaxHealth);
            UpdateHealthUI();
        }

        public void UpdateHealthUI()
        {
            // float fillFront = frontHealthBar.fillAmount;

            float hFraction = myHealth/myMaxHealth;

            frontHealthBar.fillAmount = hFraction;
            floatingHealthBar.fillAmount = hFraction;
        }

        public void TakeDamage(float damage)
        {
            myHealth -= damage;
            //
            //lerpTimer = 0f;
        }

        public void RestoreHealth(float healAmount)
        {
            myHealth += healAmount;
            //lerpTimer = 0f;
        }
    }
}

