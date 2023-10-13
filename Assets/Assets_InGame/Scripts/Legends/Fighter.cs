using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CJ
{
    public class Fighter : MonoBehaviour
    {

        #region Stance Swap:
        public GameObject swapIconA;
        public GameObject swapIconB;
        public Sprite swapA;
        public Sprite swapB;
        public GameObject fighterProfile;
        public Sprite fighterSprite;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            #region Stance Swap:
            swapIconA.GetComponent<SpriteRenderer>().sprite = swapA;
            swapIconB.GetComponent<SpriteRenderer>().sprite = swapB;
            #endregion
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
