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

    /// <summary>
    /// Karakter üzerindeki bir parçanın Sprite'ını değiştirir.
    /// Örn: "Hat", "Clothes", "Hair"
    /// </summary>
    public void SetBodyPartSprite(GameObject characterRoot, string partName, Sprite newSprite)
    {
        if (characterRoot == null || newSprite == null) return;

        Transform partT = FindPart(characterRoot.transform, partName);
        
        // Özel Durum: "Hat" vs "Hats" isim karmaşasını yönet
        if (partT == null && partName == "Hat") partT = FindPart(characterRoot.transform, "Hats");
        
        if (partT != null)
        {
            Image img = partT.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = newSprite;
                
                // Eğer parça üzerinde özel ayar uygulayıcı varsa çalıştır (Örn: Native Size, Scale vb.)
                ImageSettingsApplier applier = partT.GetComponent<ImageSettingsApplier>();
                if (applier != null) applier.ApplySettings();
            }
        }
        else
        {
            Debug.LogWarning($"[CharacterModifier] Part not found: {partName}");
        }
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
        // 1. Doğrudan çocuk mu?
        Transform t = root.Find(partName);
        if (t != null) return t;

        // 2. Derinlemesine ara
        if (useRecursionForParts)
        {
            foreach (Transform child in root)
            {
                Transform result = FindPart(child, partName);
                if (result != null) return result;
            }
        }

        return null;
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
