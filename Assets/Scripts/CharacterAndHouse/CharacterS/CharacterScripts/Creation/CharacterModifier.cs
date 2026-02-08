using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// KARAKTER DEGISTIRME MOTORU (LOGIC CORE)
/// Sadece karakter üzerindeki parçaları bulur ve değiştirir. UI'dan tamamen bağımsızdır.
/// </summary>
public class CharacterModifier : MonoBehaviour
{
    [Header("Settings")]
    public bool useRecursionForParts = true;

    public void SetBodyPartSprite(GameObject characterRoot, string partName, Sprite newSprite)
    {
        if (characterRoot == null) return;
        if (newSprite == null) 
        {
            Debug.LogWarning($"[Modifier] SetBodyPartSprite Fail: Sprite to apply for '{partName}' is NULL.");
            return;
        }

        Transform partT = FindPart(characterRoot.transform, partName);
        
        if (partT != null)
        {
            Debug.Log($"[Modifier] Applying sprite '{newSprite.name}' to object '{partName}' at path: {GetGameObjectPath(partT.gameObject)}");

            // 🔥 v17 Deep Recursion: Get ALL images in children (including sleeves, nested parts)
            Image[] allImages = partT.GetComponentsInChildren<Image>(true);
            foreach (Image img in allImages)
            {
                img.sprite = newSprite;
                
                // Eğer parça üzerinde özel ayar uygulayıcı varsa çalıştır
                ImageSettingsApplier applier = img.GetComponent<ImageSettingsApplier>();
                if (applier != null) applier.ApplySettings();

                Debug.Log($"[Modifier] Deep Apply: Successfully updated sprite on '{GetGameObjectPath(img.gameObject)}'");
            }
        }
        else
        {
            Debug.LogError($"[Modifier] Target object NOT FOUND for part '{partName}'. (Hierarchy check failed for root: {characterRoot.name})");
        }
    }

    // Helper for diagnostic logging
    private string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }

    /// <summary>
    /// Karakter üzerindeki bir parçanın ve alt parçalarının rengini değiştirir.
    /// Örn: "Skin", "Hair" (Root ve childlar dahil)
    /// </summary>
    public void SetBodyPartColor(GameObject characterRoot, string partName, Color newColor)
    {
        if (characterRoot == null) return;

        Transform partT = FindPart(characterRoot.transform, partName);
        if (partT == null) return;

        // 1. Root objenin rengini değiştir
        Image rootImg = partT.GetComponent<Image>();
        if (rootImg != null)
        {
             // Mevcut alfa değerini koru
             float currentAlpha = rootImg.color.a;
             Color appliedColor = newColor;
             appliedColor.a = currentAlpha;
             rootImg.color = appliedColor;
        }

        // 2. Renderer varsa (3D/Mesh) değiştir
        Renderer rend = partT.GetComponentInChildren<Renderer>();
        if (rend != null) rend.material.color = newColor;

        // 3. Tüm çocukların rengini değiştir (Özellikle Skin için önemli)
        foreach (Transform child in partT)
        {
            Image childImg = child.GetComponent<Image>();
            if (childImg != null)
            {
                // Her bir child'ın kendi alfasını koru
                float childAlpha = childImg.color.a;
                Color childColor = newColor;
                childColor.a = childAlpha;
                childImg.color = childColor;
            }
        }
    }

    /// <summary>
    /// Recursive olarak hiyerarşide parça arar.
    /// </summary>
    public Transform FindPart(Transform root, string partName)
    {
        // 🔥 v18 Intelligent Resolution: Try original name first, then aliases
        Transform result = root.Find(partName);
        if (result != null) return result;

        // Try Known Aliases
        string alias = null;
        if (partName == "Hat") alias = "Hats";
        else if (partName == "Hats") alias = "Hat";
        else if (partName == "Noise") alias = "Nose";
        else if (partName == "Nose") alias = "Noise";
        else if (partName == "EyeBrown") alias = "Eyebrows";
        else if (partName == "Eyebrows") alias = "EyeBrown";

        if (alias != null)
        {
            result = root.Find(alias);
            if (result != null) return result;
        }

        // Deep Search (Recursive)
        if (useRecursionForParts)
        {
            foreach (Transform child in root)
            {
                result = FindPart(child, partName);
                if (result != null) return result;
            }
        }
        return null;
    }

    // --- JSON PERSISTENCE (v13 - Pure JSON) ---

    private Dictionary<string, string> partPaths = new Dictionary<string, string>();
    private Dictionary<string, Sprite[]> folderCache = new Dictionary<string, Sprite[]>(); // 🔥 v19: Performance Cache

    public void RegisterPartPath(string partName, string folderPath)
    {
        partPaths[partName] = folderPath;
    }

    private static readonly string[] CharacterParts = {
        "Skin", "Hair", "Eyes", "EyeBrown", "Noise", "Freckles", "Mouth",
        "Clothes", "Hat", "Accessory"
    };

    public CharacterSaveData CaptureVisualState(GameObject root, string charId)
    {
        CharacterSaveData data = new CharacterSaveData { charId = charId };
        foreach (string partName in CharacterParts)
        {
            Transform partT = FindPart(root.transform, partName);

            if (partT == null) continue;

            Image img = partT.GetComponent<Image>();
            CharacterPartSaveData partData = new CharacterPartSaveData { partName = partName };

            if (img != null)
            {
                if (img.sprite != null) partData.spriteName = img.sprite.name;
                partData.colorHex = "#" + ColorUtility.ToHtmlStringRGBA(img.color);
                
                // Ensure we get the correct path if registered, otherwise default
                string path = partPaths.ContainsKey(partName) ? partPaths[partName] : GetDefaultPath(partName);
                partData.folderPath = path;

                Debug.Log($"[Modifier] Capturing state for '{partName}': Sprite={partData.spriteName}, Color={partData.colorHex}, Path={path}");
            }
            data.parts.Add(partData);
        }
        return data;
    }

    public void ApplyVisualState(GameObject root, CharacterSaveData data)
    {
        if (root == null || data == null) return;
        
        // 🔥 v19: Clear cache before full character reload
        folderCache.Clear();

        foreach (var partData in data.parts)
        {
            // 🔥 Path Sync (v17): Ensure registry is synced even if sprite is currently empty
            RegisterPartPath(partData.partName, partData.folderPath);

            Transform partT = FindPart(root.transform, partData.partName);
            if (partT == null) continue;

            // 1. Color (Recursive)
            if (ColorUtility.TryParseHtmlString(partData.colorHex, out Color parsedColor))
            {
                SetBodyPartColor(root, partData.partName, parsedColor);
            }

            // 2. Sprite
            if (!string.IsNullOrEmpty(partData.spriteName))
            {
                // 🔥 v19: Use Robust Sliced Loader
                Sprite s = LoadSpriteRobustly(partData.folderPath, partData.spriteName);
                
                if (s != null) 
                {
                    SetBodyPartSprite(root, partData.partName, s);
                    Debug.Log($"[Modifier] Robust Load Success: Found {partData.spriteName} in {partData.folderPath}");
                }
                else 
                {
                    Debug.LogWarning($"[Modifier] Sliced Load Fail (v19): Could not find '{partData.spriteName}' in '{partData.folderPath}'. Check if file exists or path is correct.");
                }
            }
        }
    }

    /// <summary>
    /// 🔥 v19: Robustly loads a sprite that might be part of a Multiple Sprite Sheet.
    /// </summary>
    private Sprite LoadSpriteRobustly(string folderPath, string spriteName)
    {
        if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(spriteName)) return null;

        // 1. Ensure folder content is in cache
        if (!folderCache.ContainsKey(folderPath))
        {
            // 🔥 v29: Support for Disk-Based Custom Sprites
            if (folderPath.Contains("CustomClothes"))
            {
                LoadCustomSpritesFromDisk(folderPath);
            }
            else
            {
                Sprite[] allInFolder = Resources.LoadAll<Sprite>(folderPath);
                if (allInFolder == null || allInFolder.Length == 0)
                {
                    Debug.LogWarning($"[Modifier] folderCache Fail: No sprites found (or folder missing) at Resources/{folderPath}");
                    folderCache[folderPath] = new Sprite[0];
                }
                else
                {
                    folderCache[folderPath] = allInFolder;
                }
            }
        }

        // 2. Search in cached sprites
        Sprite[] sprites = folderCache[folderPath];
        foreach (Sprite s in sprites)
        {
            if (s.name == spriteName) return s;
        }

        return null;
    }

    // 🔥 v29: Runtime PNG to Sprite Loading
    private void LoadCustomSpritesFromDisk(string folderPath)
    {
        try
        {
            // folderPath logic: It might be a persistentDataPath or a partial one.
            // We assume ClothesExporter.BasePath and simplify.
            string diskPath = folderPath; 
            
            // If it's a relative/convenience path, map it to absolute
            if (!diskPath.Contains(":") && !diskPath.StartsWith("/"))
            {
                diskPath = System.IO.Path.Combine(Application.persistentDataPath, folderPath);
            }

            if (System.IO.Directory.Exists(diskPath))
            {
                string[] files = System.IO.Directory.GetFiles(diskPath, "*.png");
                List<Sprite> sprites = new List<Sprite>();

                foreach (string file in files)
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(file);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(bytes))
                    {
                        tex.name = System.IO.Path.GetFileNameWithoutExtension(file);
                        Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        s.name = tex.name;
                        sprites.Add(s);
                    }
                }
                folderCache[folderPath] = sprites.ToArray();
                Debug.Log($"[Modifier] Loaded {sprites.Count} custom sprites from {diskPath}");
            }
            else
            {
                folderCache[folderPath] = new Sprite[0];
                Debug.LogWarning($"[Modifier] Custom directory not found: {diskPath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Modifier] Disk Load Error: {e.Message}");
            folderCache[folderPath] = new Sprite[0];
        }
    }

    public static string GetDefaultPath(string category)
    {
        if(category == "BoyHair") return "Images/Character/Style/Hair_Image/BoyHair";
        if(category == "GirlHair") return "Images/Character/Style/Hair_Image/GirlHair";
        if(category == "Beard") return "Images/Character/Style/Beard_Image";
        if(category == "Eyes") return "Images/Character/Style/Eyes_Image";
        if(category == "Nose" || category == "Noise") return "Images/Character/Style/Noise_Image"; 
        if(category == "Eyebrows" || category == "EyeBrown") return "Images/Character/Style/EyeBrown_Image";
        if(category == "Freckles") return "Images/Character/Style/Freckle_Image";
        if(category == "Mouth") return "Images/Character/Style/Mouth_Image";
        if(category == "Clothes") return "Images/Character/Style/Clothes_Image";
        if(category == "Hats" || category == "Hat") return "Images/Character/Style/Hats_Image";
        if(category == "Accessory") return "Images/Character/Style/Accessory_Image";
        return $"Images/Character/Style/{category}";
    }

    // --- COLOR HELPERS ---

    public Color AdjustColorTone(Color baseColor, float toneValue)
    {
        toneValue = Mathf.Clamp01(toneValue);
        const float toneStrength = 0.25f; // %25 sapma

        // RGB → HSV
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);

        // Tonlama: 0.5 = nötr, <0.5 = açık, >0.5 = koyu
        if (toneValue < 0.5f)
        {
            float t = (0.5f - toneValue) * 2f;
            v = Mathf.Clamp01(v + (1f - v) * toneStrength * t); // 🎯 açma
        }
        else
        {
            float t = (toneValue - 0.5f) * 2f;
            v = Mathf.Clamp01(v * (1f - toneStrength * t)); // 🎯 koyulaştırma
        }

        // HSV → RGB
        Color tonedColor = Color.HSVToRGB(h, s, v);
        tonedColor.a = 1f;

        return tonedColor;
    }
}
