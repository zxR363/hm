using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AvatarWorld.Interaction
{
    public class ConsumableItem : MonoBehaviour
    {
        [Header("Consumable Settings")]
        public List<Sprite> biteStates; // Sprites for each stage (Full -> Bitten 1 -> Bitten 2)
        public AudioClip eatSound;

        private int currentStage = 0;
        private Image itemImage;

        private void Start()
        {
            itemImage = GetComponent<Image>();
            if (itemImage == null) itemImage = GetComponentInChildren<Image>();
        }

        public void TakeBite()
        {
            currentStage++;

            if (currentStage >= biteStates.Count)
            {
                // Finished
                Destroy(gameObject);
            }
            else
            {
                // Next Sprite
                if (itemImage != null)
                {
                    itemImage.sprite = biteStates[currentStage];
                    itemImage.sprite = biteStates[currentStage];
                    // itemImage.SetNativeSize(); // REMOVED: Resizes to texture original (1024x) which is too big. Keep world scale.
                }
            }
        }

        public bool IsFinished()
        {
            return currentStage >= biteStates.Count;
        }
    }
}
