using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCategoryManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform categoryGridParent;      // Alt klasör butonları için GridLayout
    public Transform optionGridParent;        // Sprite seçenekleri için GridLayout
    public GameObject categoryButtonPrefab;   // Alt klasör adıyla buton prefab
    public GameObject optionItemPrefab;       // Sprite gösterimi için OptionItem prefab
    public CharacterCreationManager creationManager;

    /// <summary>
    /// Belirtilen ana kategori altında yer alan alt klasörleri bulur ve buton oluşturur
    /// Örn: "Clothes_Image" → Casual, Formal, Man
    /// </summary>
    public void PopulateCategoryButtons(string categoryKey)
    {
        ClearGrid(categoryGridParent);

        string fullPath = Path.Combine(Application.dataPath, "Resources", "Images/Character/Style", categoryKey);
        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning($"Category path not found: {fullPath}");
            return;
        }

        string[] folders = Directory.GetDirectories(fullPath);
        foreach (string folder in folders)
        {
            string folderName = Path.GetFileName(folder);

            GameObject btn = Instantiate(categoryButtonPrefab, categoryGridParent);
            Debug.Log("BUTONLAR OLUSTURULUYOR="+folderName);
            //btn.GetComponentInChildren<Text>().text = folderName;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                PopulateOptionGrid(categoryKey, folderName);
            });
        }

        Debug.Log($"Category buttons created for: {categoryKey} → {folders.Length} folders");
    }

    /// <summary>
    /// Seçilen alt klasördeki sprite’ları OptionGrid’e yükler
    /// Örn: "Clothes_Image", "Formal"
    /// </summary>
    public void PopulateOptionGrid(string categoryKey, string styleKey)
    {
        ClearGrid(optionGridParent);

        string resourcePath = $"Images/CharacterStyle/{categoryKey}/{styleKey}";
        List<Sprite> sprites = creationManager.LoadSpritesFromResources(resourcePath);

        Debug.Log($"Loading {sprites.Count} sprites from {resourcePath}");

        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject item = Instantiate(optionItemPrefab, optionGridParent);
            OptionItem option = item.GetComponent<OptionItem>();
            option.Setup(sprites[i], i, creationManager, styleKey);

            item.SetActive(true);
            item.GetComponent<Button>().onClick.AddListener(option.OnClick);
        }
    }

    /// <summary>
    /// Grid içeriğini temizler
    /// </summary>
    void ClearGrid(Transform grid)
    {
        foreach (Transform child in grid)
            Destroy(child.gameObject);
    }
}