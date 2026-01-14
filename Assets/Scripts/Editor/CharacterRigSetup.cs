using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class CharacterRigSetup : EditorWindow
{
    [MenuItem("Tools/Avatar World/Setup Character Rig")]
    public static void ShowWindow()
    {
        GetWindow<CharacterRigSetup>("Rig Setup");
    }

    private GameObject targetCharacter;

    private void OnGUI()
    {
        GUILayout.Label("Character Rigging Tool", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        targetCharacter = (GameObject)EditorGUILayout.ObjectField("Target Character", targetCharacter, typeof(GameObject), true);

        if (GUILayout.Button("Create Rig Hierarchy"))
        {
            if (targetCharacter != null)
            {
                CreateRig(targetCharacter);
            }
            else
            {
                Debug.LogError("Lütfen bir karakter objesi seçin!");
            }
        }
    }

    private void CreateRig(GameObject root)
    {
        // 1. Ana Parçaları Oluştur (Body, Head, Arms, Legs)
        GameObject bodyRoot = CreateBone(root.transform, "Body");
        GameObject headRoot = CreateBone(bodyRoot.transform, "Head");
        
        // --- Otomatik Taşıma (İsimlere Göre) ---
        MoveChild(root.transform, "Skin", bodyRoot.transform);   // Genelde gövde
        MoveChild(root.transform, "Clothes", bodyRoot.transform); // Kıyafet
        
        // Kafa Parçaları -> Head Altmanına
        MoveChild(root.transform, "Hair", headRoot.transform);
        MoveChild(root.transform, "Face", headRoot.transform); // Varsa
        
        // Detaylı Yüz Parçaları
        MoveChild(root.transform, "Eyes", headRoot.transform);
        MoveChild(root.transform, "EyeBrown", headRoot.transform);
        MoveChild(root.transform, "Mouth", headRoot.transform); // Varsa
        MoveChild(root.transform, "Beard", headRoot.transform);
        MoveChild(root.transform, "Nose", headRoot.transform); // Noise diye geçiyor projede
        MoveChild(root.transform, "Noise", headRoot.transform); 
        MoveChild(root.transform, "Freckle", headRoot.transform);
        
        // Aksesuarlar
        MoveChild(root.transform, "Hat", headRoot.transform);
        MoveChild(root.transform, "Accessory", headRoot.transform); // Gözlük vb. ise kafada

        Debug.Log($"{root.name} Rigging Tamamlandı! Lütfen Pivotları kontrol edin.");
    }

    private GameObject CreateBone(Transform parent, string name)
    {
        // Varsa bul, yoksa yarat
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;

        GameObject bone = new GameObject(name);
        bone.transform.SetParent(parent, false);
        
        // RectTransform ekle (UI tabanlı olduğu için)
        if (parent.GetComponent<RectTransform>() != null)
        {
            RectTransform rect = bone.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero; // Ebeveynine uysun diye sıfırlamıyoruz, nokta pivot.
        }

        return bone.gameObject;
    }

    private void MoveChild(Transform currentRoot, string childName, Transform newParent)
    {
        Transform t = currentRoot.Find(childName);
        if (t != null)
        {
            t.SetParent(newParent, true); // World position stays same
            Debug.Log($"Moved {childName} to {newParent.name}");
        }
        else
        {
            // Debug.LogWarning($"Could not find {childName} in {currentRoot.name}");
        }
    }
}
#endif
