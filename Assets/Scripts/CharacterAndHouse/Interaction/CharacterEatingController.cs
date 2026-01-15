using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.CharacterAndHouse.CharacterScripts.Expression;

namespace AvatarWorld.Interaction
{
    public class CharacterEatingController : MonoBehaviour
    {
        [Header("Settings")]
        public Sprite openMouthSprite; // Mouth Open (Waiting for food)
        public Sprite chewingMouthSprite; // Simple chewing state (or could be animation)
        public float chewDuration = 1.0f; // Time to chew before swallowing
        public AudioClip chewSound;
        
        [Header("Detection Area")]
        public RectTransform mouthDetectionRect; // Drag & Drop target area

        private CharacterExpressionManager expressionManager;
        private Image mouthImage;
        private Sprite defaultMouthSprite;
        private bool isChewing = false;

        private void Start()
        {
            expressionManager = GetComponent<CharacterExpressionManager>();
            if (expressionManager != null)
            {
                mouthImage = expressionManager.mouthImage;
            }
        }

        // Called when food is hovering near mouth
        public void OnFoodNearby(bool isNear)
        {
            if (isChewing) return; // Ignore if busy chewing
            if (mouthImage == null) return;

            if (isNear)
            {
                // Cache default if not already
                if (defaultMouthSprite == null) defaultMouthSprite = mouthImage.sprite;
                
                // Show Open Mouth
                if (openMouthSprite != null) mouthImage.sprite = openMouthSprite;
            }
            else
            {
                // Restore default
                if (defaultMouthSprite != null) mouthImage.sprite = defaultMouthSprite;
                defaultMouthSprite = null; // Clear cache
            }
        }

        public void Eat(ConsumableItem item)
        {
            if (item == null || isChewing) return;

            StartCoroutine(ChewRoutine(item));
        }

        private IEnumerator ChewRoutine(ConsumableItem item)
        {
            isChewing = true;
            
            // 1. Take a bite immediately
            item.TakeBite();

            // 2. Play Sound
            // AudioSource.PlayClipAtPoint(chewSound, transform.position); // Logic placeholder

            // 3. Visual: Chewing Loop
            float timer = 0f;
            bool swapToggle = false;
            
            Sprite originalMouth = (defaultMouthSprite != null) ? defaultMouthSprite : mouthImage.sprite;

            while (timer < chewDuration)
            {
                timer += 0.15f;
                // Simple chew animation: Swap between Open and Chewing/Closed sprites
                mouthImage.sprite = swapToggle ? openMouthSprite : chewingMouthSprite;
                swapToggle = !swapToggle;
                
                yield return new WaitForSeconds(0.15f);
            }

            // 4. Swallow / Finish
            mouthImage.sprite = originalMouth; // Return to normal
            defaultMouthSprite = null;
            isChewing = false;
        }
    }
}
