using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CharacterAndHouse.CharacterScripts.Expression
{
    public enum EmotionType
    {
        Neutral,
        Happy,
        Sad,
        Angry,
        Surprised
    }

    [System.Serializable]
    public class EmotionPreset
    {
        public EmotionType emotion;
        public Sprite eyeLook; // Assuming both eyes use same sprite or mirrored
        public Sprite mouthLook;
        public Sprite browLook;
    }

    public class CharacterExpressionManager : MonoBehaviour
    {
        [Header("Target Parts (Auto-Found)")]
        public Image eyesImage;
        public Image mouthImage;
        public Image browsImage; // Optional

        [Header("Settings")]
        public bool enableBlinking = true;
        public float minBlinkTime = 2.0f;
        public float maxBlinkTime = 6.0f;
        public float blinkDuration = 0.15f;

        [Header("Resources")]
        public Sprite blinkSprite; // Sprite used during blink
        public List<EmotionPreset> emotionalStates;

        private EmotionType currentEmotion = EmotionType.Neutral;
        private Sprite currentEyeSprite; // To remember what to go back to after blink

        private void Start()
        {
            FindBodyParts();
            
            // Start Blinking Loop
            if(enableBlinking)
                StartCoroutine(BlinkRoutine());
        }

        private void FindBodyParts()
        {
            // Trying to find parts by standard Toca-style names
            if (eyesImage == null) eyesImage = FindPartRecursively(transform, "Eyes")?.GetComponent<Image>();
            if (mouthImage == null) mouthImage = FindPartRecursively(transform, "Mouth")?.GetComponent<Image>();
            if (browsImage == null) browsImage = FindPartRecursively(transform, "EyeBrown")?.GetComponent<Image>(); // Or "Brows"
        }

        private Transform FindPartRecursively(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindPartRecursively(child, name);
                if (found != null) return found;
            }
            return null;
        }

        public void SetEmotion(EmotionType emotion)
        {
            currentEmotion = emotion;
            EmotionPreset preset = emotionalStates.Find(x => x.emotion == emotion);

            if (preset != null)
            {
                if (eyesImage != null && preset.eyeLook != null)
                {
                    eyesImage.sprite = preset.eyeLook;
                    currentEyeSprite = preset.eyeLook;
                }
                
                if (mouthImage != null && preset.mouthLook != null)
                    mouthImage.sprite = preset.mouthLook;

                if (browsImage != null && preset.browLook != null)
                    browsImage.sprite = preset.browLook;
            }
            else
            {
                // If no preset, maybe just revert to default/neutral if possible?
                // For now, do nothing or log warning
            }
        }

        private IEnumerator BlinkRoutine()
        {
            while (enableBlinking)
            {
                // Wait random time
                float waitTime = Random.Range(minBlinkTime, maxBlinkTime);
                yield return new WaitForSeconds(waitTime);

                // Perform Blink
                if (eyesImage != null && blinkSprite != null && currentEmotion != EmotionType.Surprised) // Don't blink if surprised?
                {
                    Sprite preBlinkSprite = eyesImage.sprite;
                    eyesImage.sprite = blinkSprite;
                    
                    yield return new WaitForSeconds(blinkDuration);

                    // Restore (unless emotion changed mid-blink, strictly we should use currentEyeSprite)
                    eyesImage.sprite = currentEyeSprite != null ? currentEyeSprite : preBlinkSprite;
                }
            }
        }

        private void Update()
        {
            // Debug / Testing Inputs
            // if (Input.GetKeyDown(KeyCode.Alpha1)) SetEmotion(EmotionType.Happy);
            // if (Input.GetKeyDown(KeyCode.Alpha2)) SetEmotion(EmotionType.Sad);
            // if (Input.GetKeyDown(KeyCode.Alpha3)) SetEmotion(EmotionType.Angry);
            // if (Input.GetKeyDown(KeyCode.Alpha4)) SetEmotion(EmotionType.Surprised);
            // if (Input.GetKeyDown(KeyCode.Alpha0)) SetEmotion(EmotionType.Neutral);
        }

        // Helper to test from Inspector context menu
        [ContextMenu("Test Happy")]
        public void TestHappy() => SetEmotion(EmotionType.Happy);
        
        [ContextMenu("Test Sad")]
        public void TestSad() => SetEmotion(EmotionType.Sad);
    }
}
