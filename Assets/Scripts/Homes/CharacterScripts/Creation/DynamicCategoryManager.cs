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

    public GameObject colorSelectButtonPrefab;

    //CategoryButonlarının olduğu seçimlerde ilk Buton otomatik olarak aktif ediliyor. Bu sayede OptionItem'lar otomatik gelmiş oluyor
    private bool initialCategoryButtonFlag = false;

    /// <summary>
    /// Belirtilen ana kategori altında yer alan alt klasörleri bulur ve buton oluşturur
    /// Örn: "Clothes_Image" → Casual, Formal, Man
    /// </summary>
    public void PopulateCategoryButtons(string categoryKey)
    {
        ClearGrid(categoryGridParent);

        initialCategoryButtonFlag = false;

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
            Debug.Log("BUTONLAR OLUSTURULUYOR KATEGORI="+categoryKey);
            btn.SetActive(true);

            Button buttonComponent = btn.GetComponent<Button>();
            if (buttonComponent == null)
            {
                Debug.LogError("CategoryButtonTemplate prefab'ında Button bileşeni eksik!");
                return;
            }

            buttonComponent.onClick.AddListener(() =>
            {
                PopulateOptionGrid(categoryKey, folderName);
            });

            //CategoryButonlarının olduğu seçimlerde ilk Buton otomatik olarak aktif ediliyor. Bu sayede OptionItem'lar otomatik gelmiş oluyor
            if(initialCategoryButtonFlag == false)
            {
                initialCategoryButtonFlag = true;
                PopulateOptionGrid(categoryKey, folderName);
            }

        }

        Debug.Log($"Category buttons created for: {categoryKey} → {folders.Length} folders");
    }

    /// <summary>
    /// Seçili kategorideki colorları OptionGrid’e yükler
    /// </summary>
    public void PopulateOptionColorPalette()
    {
        ClearGrid(categoryGridParent);

        //-----------------COLOR
         // 🔥 İlk olarak Color Select butonunu ekle
        GameObject colorBtn = Instantiate(colorSelectButtonPrefab, categoryGridParent);

        Button colorButton = colorBtn.GetComponent<Button>();
        colorBtn.SetActive(true);
        if (colorButton != null)
        {
            colorButton.onClick.AddListener(() =>
            {
                creationManager.Populate_ColorPalette_Options();
            });
        }
        //-----------------COLOR
    }

    /// <summary>
    /// Seçilen alt klasördeki sprite’ları OptionGrid’e yükler
    /// Örn: "Clothes_Image", "Formal"
    /// </summary>
    public void PopulateOptionGrid(string categoryKey, string styleKey)
    {
        Debug.Log("OptionGrid TETİKLENDİ");

        ClearGrid(optionGridParent);

        string resourcePath = $"Images/Character/Style/{categoryKey}/{styleKey}";
        List<Sprite> sprites = creationManager.LoadSpritesFromResources(resourcePath);

        Debug.Log($"Loading {sprites.Count} sprites from {resourcePath}");

        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject item = Instantiate(optionItemPrefab, optionGridParent);
            OptionItem option = item.GetComponent<OptionItem>();
            option.Setup(sprites[i], i, creationManager, styleKey);


            //!!!!!!!!!!!!!!----OZEL DURUM-----!!!!!!!!!
            // Yeni bir CharacterPreview Item seçilirse
            // (Aynı Item ise rengi koruması için yapılıyor)             
            option.updateNewItemUpdateColorPalette(creationManager.colorRoot);
            //!!!!!!!!!!!!!!----OZEL DURUM-----!!!!!!!!!
            // Yeni bir CharacterPreview Item seçilirse
            // (Aynı Item ise rengi koruması için yapılıyor)

            item.SetActive(true);
            item.GetComponent<Button>().onClick.AddListener(option.OnClick);
        }
    }

    /// <summary>
    /// Grid içeriğini temizler
    /// </summary>
    public void ClearGrid(Transform grid)
    {
        foreach (Transform child in grid)
            Destroy(child.gameObject);
    }
}