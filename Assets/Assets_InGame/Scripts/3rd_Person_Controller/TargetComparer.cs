using System.Collections;
using System;
using UnityEngine;

//_____________________________________________________________________________________________________________________
//NOTES
//---------------------------------------------------------------------------------------------------------------------
//NOTE: 
//          (1) Still have to figure out what TargetComparer() does!

namespace CJ
{
    public class TargetComparer : IComparer
    {
        
//_____________________________________________________________________________________________________________________
// GENERAL VARIABLES:
//---------------------------------------------------------------------------------------------------------------------
#region VARIABLES
    private Transform compareTransform; // To temporary store a transform object of target from list (List originates from Sense script)
#endregion VARIABLES

//_____________________________________________________________________________________________________________________
// FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
#region FUNCTIONS
    #region F: TargetComparer
        public TargetComparer(Transform compTransform) 
        {
            compareTransform = compTransform;
        }
    #endregion F: TargetComparer


    #region Comparer
        public int Compare(object x, object y) // Function to calculate the distance between object x and object y
        {
            Collider xCollider = x as Collider; // Set collider X
            Collider yCollider = y as Collider; // Set collider Y

            Vector3 offset = xCollider.transform.position - compareTransform.position; // Gets the difference in distance between xCollider and compareTransform
            float xDistance = offset.sqrMagnitude; // Calculate the distance of X

            offset = yCollider.transform.position - compareTransform.position; // Gets the difference in distance between yCollider and compareTransform
            float  yDistance = offset.sqrMagnitude; // Calculate the distance of Y

            return xDistance.CompareTo(yDistance); // Compare X to Y
        }
    #endregion Comparer
#endregion FUNCTIONS
    }
}
