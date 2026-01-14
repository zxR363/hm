using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace AvatarWorld.EditorTools
{
    public class RiggingHelperTool : EditorWindow
    {
        private Transform rootCharacter;
        private Transform skinContainer;

        // Limb References
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;

        [MenuItem("Tools/Avatar World/Rigging Helper Tool")]
        public static void ShowWindow()
        {
            GetWindow<RiggingHelperTool>("Rigging Helper");
        }

        private void OnGUI()
        {
            GUILayout.Label("Character Rigging Helper", EditorStyles.boldLabel);
            GUILayout.Space(10);

            rootCharacter = (Transform)EditorGUILayout.ObjectField("Character Root", rootCharacter, typeof(Transform), true);

            if (GUILayout.Button("Auto-Find Limb Parts"))
            {
                AutoFindParts();
            }

            GUILayout.Space(10);
            GUILayout.Label("Limb References & Validation", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawLimbField("Left Arm", ref leftArm);
            DrawLimbField("Right Arm", ref rightArm);
            DrawLimbField("Left Leg", ref leftLeg);
            DrawLimbField("Right Leg", ref rightLeg);
            EditorGUILayout.EndVertical();

            GUILayout.Space(20);
            GUILayout.Label("1. Pivot Correction", EditorStyles.boldLabel);
            GUILayout.Label("Sets pivot to natural joint location (Shoulder/Hip) without moving sprite.");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Arms Pivot (Top)"))
            {
                SetPivotSmart(leftArm, new Vector2(0.5f, 1f));
                SetPivotSmart(rightArm, new Vector2(0.5f, 1f));
            }
            if (GUILayout.Button("Set Legs Pivot (Top)"))
            {
                SetPivotSmart(leftLeg, new Vector2(0.5f, 1f)); // Legs also pivot from top (Hip) usually
                SetPivotSmart(rightLeg, new Vector2(0.5f, 1f));
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.Label("2. Hand Slots Setup", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Hand Slots"))
            {
                CreateHandSlot(leftArm, "HandSlot_L");
                CreateHandSlot(rightArm, "HandSlot_R");
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Ping Modified Key Objects"))
            {
                if(rootCharacter) EditorGUIUtility.PingObject(rootCharacter);
            }
        }

        private void AutoFindParts()
        {
            if (rootCharacter == null)
            {
                Debug.LogError("Assign Character Root first!");
                return;
            }

            // Based on known structure: Character -> Body -> Skin -> Limbs
            // Or simple recursive search
            skinContainer = FindRecursive(rootCharacter, "Skin");
            
            if (skinContainer != null)
            {
                Debug.Log($"Found Skin Container: {skinContainer.name}");
                leftArm = FindRecursive(skinContainer, "LeftArm");
                rightArm = FindRecursive(skinContainer, "RightArm");
                leftLeg = FindRecursive(skinContainer, "LeftLeg");
                rightLeg = FindRecursive(skinContainer, "RightLeg");
            }
            else
            {
                // Fallback: search anywhere
                leftArm = FindRecursive(rootCharacter, "LeftArm");
                rightArm = FindRecursive(rootCharacter, "RightArm");
                leftLeg = FindRecursive(rootCharacter, "LeftLeg");
                rightLeg = FindRecursive(rootCharacter, "RightLeg");
            }
        }

        private Transform FindRecursive(Transform parent, string name)
        {
            if (parent.name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return parent;
            foreach (Transform child in parent)
            {
                Transform distinct = FindRecursive(child, name);
                if (distinct != null) return distinct;
            }
            return null;
        }

        private void SetPivotSmart(Transform target, Vector2 newPivot)
        {
            if (target == null) return;

            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null) return;

            Undo.RecordObject(rectTransform, "Set Pivot");

            // Logic to keep visual position same while changing pivot
            Vector2 size = rectTransform.rect.size;
            Vector2 deltaPivot = rectTransform.pivot - newPivot;
            Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            
            // Apply rotation to delta
            deltaPosition = rectTransform.rotation * deltaPosition;

            rectTransform.pivot = newPivot;
            rectTransform.localPosition -= deltaPosition; // Adjust position to compensate

            Debug.Log($"Set Pivot for {target.name} to {newPivot}");
        }

        private void CreateHandSlot(Transform arm, string slotName)
        {
            if (arm == null) return;

            Transform existing = arm.Find(slotName);
            if (existing != null)
            {
                Debug.Log($"Slot {slotName} already exists.");
                return;
            }

            GameObject slot = new GameObject(slotName);
            slot.transform.SetParent(arm, false);
            
            // Default position: Bottom Center (assuming Arm sprite is vertical, hand is at bottom)
            RectTransform rt = slot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero; // Start at the bottom tip

            Undo.RegisterCreatedObjectUndo(slot, "Create Hand Slot");
            Debug.Log($"Created {slotName} under {arm.name}");
        }

        private void DrawLimbField(string label, ref Transform limbTransform)
        {
            EditorGUILayout.BeginHorizontal();
            limbTransform = (Transform)EditorGUILayout.ObjectField(label, limbTransform, typeof(Transform), true);

            if (limbTransform != null)
            {
                RectTransform rt = limbTransform.GetComponent<RectTransform>();
                if (rt != null)
                {
                    float pivotY = rt.pivot.y;
                    GUIStyle style = new GUIStyle(EditorStyles.label);
                    // Green if close to 1 (Top), Red if far
                    bool isTop = pivotY > 0.9f;
                    style.normal.textColor = isTop ? Color.green : Color.red;
                    
                    GUILayout.Label($"Pivot Y: {pivotY:F2} " + (isTop ? "(OK)" : "(CHECK)"), style, GUILayout.Width(120));
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
