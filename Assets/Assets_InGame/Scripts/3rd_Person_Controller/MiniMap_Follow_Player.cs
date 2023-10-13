using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CJ
{
    public class MiniMap_Follow_Player : MonoBehaviour
    {
        public Transform playerToFollow; 

        void LateUpdate()
        {
            Vector3 newPosition = playerToFollow.position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;
        }

    }
}
