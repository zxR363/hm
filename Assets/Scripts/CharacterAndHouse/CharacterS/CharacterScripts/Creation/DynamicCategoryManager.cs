using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//KARAKTER SECIM EKRANINDAKI
public class DynamicCategoryManager : MonoBehaviour
{
    [Header("KARAKTER SECIM EKRANI Dynamic Category Manager")]
    [Header("UI References")]
    public Transform categoryGridParent;      // Alt klasör butonları için GridLayout
    public Transform optionGridParent;        // Sprite seçenekleri için GridLayout
    public GameObject categoryButtonPrefab;   // Alt klasör adıyla buton prefab
    public GameObject optionItemPrefab;       // Sprite gösterimi için OptionItem prefab
    public CharacterCreationManager creationManager;

    public GameObject colorSelectButtonPrefab;

    //CategoryButonlarının olduğu seçimlerde ilk Buton otomatik olarak aktif ediliyor. Bu sayede OptionItem'lar otomatik gelmiş oluyor
    private bool initialCategoryButtonFlag = false;

    [Header("Color ToneSliderArea")]
    public GameObject toneSliderArea; // Inspector'dan bağlanacak
    public Slider toneSlider;

    private Transform colorRootInstanceObj;
    private bool colorRootInstanceObjSkinFlag;
    private Color selectedColor;

    //----------------TONESLIDERAREA

    [Header("CategoryButtons Circle Background Colors")]
    //Kategori olarak açılan butonların dinamik şekilde 
    // color seçilmesi için tanımlanan renkler
    public Color[] categoryColors; // Inspector’dan tanımlanabilir


    private void Start()
    {
        toneSlider.onValueChanged.AddListener(OnToneSliderChanged);
    }



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

        
        for(int i=0;i<folders.Length;i++)
        {
            string folder = folders[i];
            string folderName = Path.GetFileName(folder);

            GameObject btn = Instantiate(categoryButtonPrefab, categoryGridParent);

            //Her bir buton'a Icon'ları ekleniyor.Spesifik olarak her 
            // klasörün altında "0.png" resmi o klasörün iconu

            string previewPath = $"Images/Character/Style/{categoryKey}/{folderName}/icon";
            Sprite previewIcon = Resources.Load<Sprite>(previewPath);

            if (previewIcon != null)
            {
                Image img = btn.GetComponentInChildren<Image>();
                if (img != null)
                {
                    if(i < categoryColors.Length)
                    {
                        img.color = categoryColors[i];
                        Debug.Log("COLOR="+img.color+"   "+categoryColors.Length) ;
                    }
                    else
                    {
                        img.color = Color.white;
                    }        

                    Color fixedColor = img.color;
                    fixedColor.a = 1f;
                    img.color = fixedColor;           
                }

                // 🔥 Alt objede bulunan Image bileşenini bul
                Transform imageChild = btn.transform.Find("Image"); // "Icon" alt objenin adı olmalı
                if (imageChild != null)
                {
                    Image img1 = imageChild.GetComponentInChildren<Image>();
                    if (img1 != null)
                    {
                        img1.sprite = previewIcon;
                    }
                }
            }
            else
            {
                Debug.Log($"Preview icon not found: {previewPath}");
            }
            //Her bir buton'a Icon'ları ekleniyor.Spesifik olarak her 
            // klasörün altında "0.png" resmi o klasörün iconu

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
        ClearGrid(optionGridParent);

        string resourcePath = $"Images/Character/Style/{categoryKey}/{styleKey}";
        List<Sprite> sprites = creationManager.GetOrLoadSprites(resourcePath);

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

    //-------------- TONE SLIDER AREA---------------

    public void setActiveCategorySelectedToneSliderArea(bool activeOrNot,Transform colorRootInstance,bool skinFlag)
    {
        colorRootInstanceObjSkinFlag = skinFlag;
        colorRootInstanceObj = colorRootInstance;

        // Skin ise ayrı bir logic, Diğer Gameobject'ler ise farklı bir logic
        if(colorRootInstanceObjSkinFlag == true)
        {
            foreach (Transform child in colorRootInstanceObj)
            {
                Image childImage = child.GetComponent<Image>();
                if (childImage != null)
                    selectedColor = childImage.color;
            }
        }
        else
        {
            Image tmpImage = colorRootInstance.GetComponent<Image>();
            if (tmpImage != null)
            {
                selectedColor = tmpImage.color;
            }
        }

        toneSlider.minValue = 0f;
        toneSlider.maxValue = 1f;
        toneSlider.value = 0.5f; // 🎯 ortadan başlat

        UpdateSliderVisual(selectedColor);
        toneSliderArea.SetActive(activeOrNot);
    }

    public void OnOptionItemClicked(OptionItem item)
    {
        selectedColor = item.GetColor(); // OptionItem içinde tanımlı olmalı
        toneSlider.minValue = 0f;
        toneSlider.maxValue = 1f;
        toneSlider.value = 0.5f; // 🎯 ortadan başlat

        Debug.Log("OPTINCLICKED COLOR="+selectedColor);

        UpdateSliderVisual(selectedColor);
        ApplyTone(toneSlider.value); // slider değeriyle tonu uygula
    }

    public void OnToneSliderChanged(float value)
    {
        ApplyTone(value);
    }

    private void ApplyTone(float toneValue)
    {
        if (colorRootInstanceObj == null) return;

        Color tonedColor = AdjustColorTone(selectedColor, toneValue);

        // 🎯 Renderer varsa (3D prefab)
        Renderer rend = colorRootInstanceObj.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material.color = tonedColor;
            return;
        }

        // 🎯 Image varsa (UI prefab)
        // Skin ise ayrı bir logic, Diğer Gameobject'ler ise farklı bir logic
        if(colorRootInstanceObjSkinFlag == true)
        {
            foreach (Transform child in colorRootInstanceObj)
            {
                Image childImage = child.GetComponent<Image>();
                if (childImage != null)
                    childImage.color = tonedColor;
            }
        }
        else
        {
            Image img = colorRootInstanceObj.GetComponentInChildren<Image>();
            if (img != null)
            {
                img.color = tonedColor;
            }
        }
        

    }

    private Color AdjustColorTone(Color baseColor, float toneValue)
    {
        toneValue = Mathf.Clamp01(toneValue);
        const float toneStrength = 0.25f; // %10 sapma

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

    public void UpdateSliderVisual(Color baseColor)
    {
            // Fill Area/Fill objesini bul
        Image sliderFillImage = toneSlider.transform.Find("Fill Area/Fill").GetComponent<Image>();

        // Background alanı
        Image sliderBackgroundImage = toneSlider.transform.Find("Background").GetComponent<Image>();

        Texture2D gradientTex = GenerateToneGradient(baseColor);
        Sprite gradientSprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));

        // Fill alanına uygula
        sliderFillImage.sprite = gradientSprite;
        sliderFillImage.type = Image.Type.Simple;
        sliderFillImage.preserveAspect = false;

        // Background alanına da uygula 🎯
        sliderBackgroundImage.sprite = gradientSprite;
        sliderBackgroundImage.type = Image.Type.Simple;
        sliderBackgroundImage.preserveAspect = false;

    }

    private Texture2D GenerateToneGradient(Color baseColor)
    {
        int width = 128;
        Texture2D tex = new Texture2D(width, 1);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < width; x++)
        {
            float t = x / (float)(width - 1);
            Color toned = AdjustColorTone(baseColor, t);
            tex.SetPixel(x, 0, toned);
        }

        tex.Apply();
        return tex;
    }


    //-------------- TONE SLIDER AREA---------------

}