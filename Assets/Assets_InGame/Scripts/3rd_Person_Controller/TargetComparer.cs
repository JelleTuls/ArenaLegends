using System.Collections;
using System;
using UnityEngine;

namespace CJ{

    public class TargetComparer : IComparer
    {
        private Transform compareTransform;

        public TargetComparer(Transform compTransform)
        {
            compareTransform = compTransform;
        }

        public int Compare(object x, object y)
        {
            Collider xCollider = x as Collider;
            Collider yCollider = y as Collider;

            Vector3 offset = xCollider.transform.position - compareTransform.position;
            float xDistance = offset.sqrMagnitude;

            offset = yCollider.transform.position - compareTransform.position;
            float  yDistance = offset.sqrMagnitude;

            return xDistance.CompareTo(yDistance);
        }
    }
}
