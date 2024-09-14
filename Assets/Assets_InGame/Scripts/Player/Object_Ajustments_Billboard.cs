using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

namespace CJ
{

    public class Object_Adjustments_Billboard : MonoBehaviour
    {

        public Transform cam;

        // Update is called once per frame
        void LateUpdate()
        {
            transform.LookAt(transform.position + cam.forward);
        }
    }
}
