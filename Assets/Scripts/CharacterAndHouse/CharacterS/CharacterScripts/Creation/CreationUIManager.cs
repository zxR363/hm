using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creation Panel UI Yöneticisi
/// Toca Boca tarzı UI yapısını yönetir: Kategoriler (Tabs), Liste (Grid) ve Renk Paleti.
/// Logic (CharacterModifier) ile direkt konuşmaz, Controller üzerinden haberleşir.
/// </summary>
public class CreationUIManager : MonoBehaviour
{
    [Header("Layout Containers")]
    public Transform categoryTabParent;   // Üst Ana Kategoriler (Saç, Elbise, vb.)
    public Transform subCategoryParent;   // Alt Kategoriler (Kız, Erkek vb.) - Opsiyonel
    public Transform itemGridParent;      // Eşyaların listelendiği Grid
    public GameObject colorPalettePanel;  // Altta açılan renk paneli (Slider dahil)
    
    [Header("Prefabs")]
    public GameObject categoryButtonPrefab;
    public GameObject optionItemPrefab;
    
    [Header("References")]
    public ScrollRect contentScrollRect;
    
    // Basit bir callback yapısı
    private System.Action<string, string> onCategorySelected;
    
    public void Initialize()
    {
        // ClearGrid(categoryTabParent); // ❌ KALDIRILDI: Varolan tabları silmemeli! Bunları CreateCategoryTabs yönetecek.
        ClearGrid(itemGridParent);
        if(subCategoryParent) ClearGrid(subCategoryParent);
        
        SetColorPaletteActive(false);
    }

    /// <summary>
    /// Ana Kategori Butonlarını oluşturur veya varolanları bağlar.
    /// </summary>
    public void CreateCategoryTabs(List<string> categories, System.Action<int> onTabSelected)
    {
        // 1. Önce Hiyerarşide zaten butonlar var mı kontrol et.
        // Eğer varsa, onları yok etmeden sadece eventlerini bağla.
        if (categoryTabParent.childCount > 0)
        {
            Debug.Log($"[UI Manager] Binding to {categoryTabParent.childCount} existing tabs.");
            int count = Mathf.Min(categories.Count, categoryTabParent.childCount);
            
            for (int i = 0; i < count; i++)
            {
                Transform child = categoryTabParent.GetChild(i);
                Button btn = child.GetComponent<Button>();
                
                // Eğer buton bileşeni yoksa ekle (ama genelde vardır)
                if (btn == null) btn = child.gameObject.AddComponent<Button>();
                
                // Event'i temizle ve yenisini ekle
                int index = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => onTabSelected(index));
                
                // İsimlendirme kolaylığı (Debug için)
                child.name = $"Tab_{categories[i]}";
            }
            return;
        }

        // 2. Yoksa (Boşsa) Sıfırdan Üret (Eski Logic)
        ClearGrid(categoryTabParent);
        
        // 2. Yoksa (Boşsa) Sıfırdan Üret (Eski Logic)
        ClearGrid(categoryTabParent);
        
        for (int i = 0; i < categories.Count; i++)
        {
            int index = i;
            GameObject btn = Instantiate(categoryButtonPrefab, categoryTabParent);
            btn.SetActive(true);
            
            Button b = btn.GetComponent<Button>();
            if(b != null)
            {
                b.onClick.AddListener(() => onTabSelected(index));
            }
        }
    }
    
    // 🔥 Public Helpers for Controller
    public void ClearItemsGrid()
    {
        ClearGrid(itemGridParent);
    }

    public void ClearSubCategoryGrid()
    {
        ClearGrid(subCategoryParent);
    }

    /// <summary>
    /// Eşya Grid'ini doldurur.
    /// </summary>
    public void PopulateGrid(List<Sprite> sprites, System.Action<int> onItemClick, bool isColorPalette = false)
    {
        ClearGrid(itemGridParent);

        for (int i = 0; i < sprites.Count; i++)
        {
            int index = i;
            GameObject item = Instantiate(optionItemPrefab, itemGridParent);
            OptionItem option = item.GetComponent<OptionItem>();
            
            // OptionItem'ın Setup fonksiyonunu daha generic hale getireceğiz veya burada direkt erişeceğiz
            // Şimdilik varsayım: option.SetupGeneric(sprite, onClick);
            
            // HACK: Mevcut OptionItem yapısına uydurmak için (Refactor sonrası burası temizlenecek)
            // option.iconImage.sprite = sprites[i];
            
            // Geçici olarak Button ekleyip event verelim, OptionItem refactor edilince orayı kullanırız
            Button btn = item.GetComponent<Button>();
            if(btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => onItemClick(index));
            }
            
            // Helper: Icon set et
            Image img = item.transform.Find("Image")?.GetComponent<Image>(); // Varsayılan hiyerarşi
            if(img == null) img = item.GetComponentInChildren<Image>();
            
            if(img != null)
            {
                if (sprites[i] != null)
                {
                    img.sprite = sprites[i];
                    img.preserveAspect = true;
                    img.color = Color.white;
                }
            }
            
            item.SetActive(true);
            ResetRectTransform(item.GetComponent<RectTransform>());
        }

        // 🔥 SCROLL FIX: Manuel reset yerine helper kullan
        CheckAndFixScroll();
    }

    [Header("Settings")]
    public Sprite colorItemBaseSprite; // Inspector'dan '1' sprite'ını buraya ata

    public void PopulateColorGrid(List<Color> colors, System.Action<int> onColorClick)
    {
        ClearGrid(itemGridParent);

        for (int i = 0; i < colors.Count; i++)
        {
            int index = i;
            GameObject item = Instantiate(optionItemPrefab, itemGridParent);
            
            Button btn = item.GetComponent<Button>();
            if(btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => onColorClick(index));
            }
            
            // Icon bulma (PopulateGrid ile aynı mantık)
            Image img = item.transform.Find("Image")?.GetComponent<Image>();
            if(img == null) img = item.GetComponentInChildren<Image>();

            if(img != null)
            {
                // 🔥 User Request: Source Image olarak '1' (veya atanan sprite) kullan
                if (colorItemBaseSprite != null)
                {
                    img.sprite = colorItemBaseSprite;
                }

                Color displayColor = colors[i];
                displayColor.a = 1f; // 🔥 UI'da görünmesi için alpha'yı 1 yap
                img.color = displayColor;
            }
            
            item.SetActive(true);
            ResetRectTransform(item.GetComponent<RectTransform>());
        }
        
        // 🔥 SCROLL FIX: Manuel reset yerine helper kullan
        CheckAndFixScroll();
    }

    public void PopulateSubCategories(List<Sprite> icons, System.Action<int> onSubClick)
    {
        // 1. Clear Main Grid (User expectation: Sub Menu clears/replaces content until selected)
        ClearGrid(itemGridParent);
        
        // 2. Clear & Populate Sub Category Parent (CategoryGrid)
        // Ensure subCategoryParent is assigned in Inspector!
        if(subCategoryParent == null) 
        {
            Debug.LogError("[UI Manager] SubCategoryParent is NULL! Please assign 'CategoryGrid/Viewport/Content' in Inspector.");
            return;
        }

        ClearGrid(subCategoryParent);

        // Ensure Layout Exists
        // 🔥 User Request: Vertical List for SubCategories (1 Column)
        SetupResponsiveGrid(subCategoryParent as RectTransform, 1, 20f, 20);

        for (int i = 0; i < icons.Count; i++)
        {
            int index = i;
            // Use optionItemPrefab for now as buttons are similar in style to grid items
            GameObject item = Instantiate(optionItemPrefab, subCategoryParent);
            
            // Clean up OptionItem if present to avoid errors (since we just want a button)
            // Or better, just use Button component.
            Button btn = item.GetComponent<Button>();
            if(btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => onSubClick(index));
            }
            
            // Icon Logic
            Image img = item.transform.Find("Image")?.GetComponent<Image>();
            if(img == null) img = item.GetComponentInChildren<Image>();
            
            if(img != null && icons[i] != null)
            {
                img.sprite = icons[i];
                img.preserveAspect = true;
                img.color = Color.white;
            }
            
            item.SetActive(true);
            ResetRectTransform(item.GetComponent<RectTransform>());
        }
    }

    /// <summary>
    /// Renk Paletini (Tone Slider) açar/kapar.
    /// Vertical Layout yapısı sayesinde Grid otomatik yukarı kayar.
    /// </summary>
    public void SetColorPaletteActive(bool isActive)
    {
        if (colorPalettePanel != null)
        {
            colorPalettePanel.SetActive(isActive);
            
            // 🔥 Responsive Cell Size Hesapla
            SetupResponsiveGrid(itemGridParent.GetComponent<RectTransform>(), 4);

            // 🔥 Layout'u zorla yenile
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemGridParent.parent as RectTransform);
        }
    }

    /// <summary>
    /// Slider değiştiğinde çağrılacak event'i bağlar.
    /// </summary>
    public void BindToneSlider(System.Action<float> onValueChange)
    {
        if (colorPalettePanel == null) return;
        Slider sl = colorPalettePanel.GetComponentInChildren<Slider>();
        if (sl != null)
        {
            sl.onValueChanged.RemoveAllListeners();
            sl.onValueChanged.AddListener((val) => onValueChange(val));
        }
    }

    /// <summary>
    /// UI Elemanlarının pozisyonlarını ve boyutlarını kod ile sabitler.
    /// Renk Paleti ve Scroll View arasındaki dikey ilişkiyi yönetir.
    /// </summary>
    public void FixLayoutPositions()
    {
        // 1. Gridler için responsive ayarları yap
        SetupResponsiveGrid(itemGridParent as RectTransform, 4); // Items -> 4 Column
        
        // Kategori Tabs da Grid ise onu da ayarla (Genelde Horizontal Layout olur ama Grid ise destekleyelim)
        if(categoryTabParent.GetComponent<GridLayoutGroup>() != null)
        {
             SetupResponsiveGrid(categoryTabParent as RectTransform, 4); 
        }

        // 2. Vertical Layout Logic (Slider & ScrollView)
        // Slider kapalıysa zaten SetColorPaletteActive(false) offset'i 0 yaptı.
        // Burası sadece açılış veya ekran değişiminde güvenli liman.
        if (contentScrollRect != null)
        {
             // ScrollView her zaman Full Stretch (offsetler dinamik yönetilecek)
             RectTransform rt = contentScrollRect.GetComponent<RectTransform>();
             rt.anchorMin = Vector2.zero;
             rt.anchorMax = Vector2.one;
             rt.sizeDelta = Vector2.zero; 
             // offsetMin.y SetColorPaletteActive içinde yönetiliyor.
        }

        Canvas.ForceUpdateCanvases();
        
        CheckAndFixScroll();
    }

    private void CheckAndFixScroll()
    {
        if (itemGridParent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemGridParent as RectTransform);
            // Viewport rebuild
            if (itemGridParent.parent != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(itemGridParent.parent as RectTransform);
        }

        // 🔥 CRITICAL FIX: ScrollRect settings
        FixScrollRectSettings();
    }

    private void FixScrollRectSettings()
    {
        if (contentScrollRect == null) return;

        // 🔥 User Request: Sadece Dikey Scroll olsun
        contentScrollRect.horizontal = false; 
        contentScrollRect.vertical = true;

        contentScrollRect.movementType = ScrollRect.MovementType.Elastic;
        contentScrollRect.elasticity = 0.1f; // Biraz daha sert snap
        contentScrollRect.inertia = true;
        contentScrollRect.decelerationRate = 0.135f; // Standart
        contentScrollRect.scrollSensitivity = 25f; // Daha hassas
        
        // ContentSizeFitter Check
        ContentSizeFitter csf = itemGridParent.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = itemGridParent.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void ClearGrid(Transform grid)
    {
        if(grid == null) return;
        foreach (Transform child in grid) 
        {
            child.gameObject.SetActive(false); // 🔥 Immediate Visual Removal
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Generic Responsive Grid Hesaplayıcı.
    /// Verilen parent altındaki GridLayoutGroup'u bulur ve Width'e göre CellSize hesaplar.
    /// </summary>
    private void SetupResponsiveGrid(RectTransform gridParent, int columns = 4, float spacing = 20f, int padding = 20)
    {
        if (gridParent == null) return;
        
        GridLayoutGroup glg = gridParent.GetComponent<GridLayoutGroup>();
        if (glg == null) return;
        
        // Genişliği nereden alacağız? Parent'ı (Viewport) veya kendisi.
        // Genelde Content (gridParent) width'i stretch ise Parent width ile aynıdır.
        RectTransform referenceRt = gridParent.parent as RectTransform;
        if(referenceRt == null) referenceRt = gridParent;

        float totalWidth = referenceRt.rect.width;
        
        // Fallback checks
        if(totalWidth <= 0) 
        {
             // Belki root canvas scale factor yüzünden henüz hesaplanmadı. Safe value.
             totalWidth = Screen.width; 
             if(contentScrollRect != null && contentScrollRect.GetComponent<RectTransform>().rect.width > 0)
                totalWidth = contentScrollRect.GetComponent<RectTransform>().rect.width;
        }
        
        // Ensure reasonable defaults
        if(totalWidth < 100) totalWidth = 800; 

        float availableWidth = totalWidth - (padding * 2) - (spacing * (columns - 1));
        float cellWidth = availableWidth / columns;
        
        // Ensure reasonable min size
        if (cellWidth < 50) cellWidth = 50;

        glg.cellSize = new Vector2(cellWidth, cellWidth);
        glg.spacing = new Vector2(spacing, spacing);
        glg.padding = new RectOffset(padding, padding, padding, padding);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = columns;
        glg.childAlignment = TextAnchor.UpperCenter;
        
        // Content Size Fitter şart
        ContentSizeFitter csf = gridParent.GetComponent<ContentSizeFitter>();
        if(csf == null) csf = gridParent.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; // Width parent tarafından belirlenir (Stretch)
        
        // Anchor Fix (Top Stretch)
        gridParent.anchorMin = new Vector2(0, 1);
        gridParent.anchorMax = new Vector2(1, 1);
        gridParent.pivot = new Vector2(0.5f, 1);
        gridParent.sizeDelta = new Vector2(0, 0); // Width 0 (Stretch to Viewport)
        gridParent.anchoredPosition = new Vector2(0, 0);

        Debug.Log($"[ResponsiveGrid] Setup for {gridParent.name}: Width={totalWidth} -> Cell={cellWidth} (Cols={columns})");
    }
    
    private void ResetRectTransform(RectTransform rt)
    {
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
        }
    }

    // --- TONE SLIDER VISUALS ---

    public void UpdateSliderVisual(Slider slider, Color baseColor, CharacterModifier modifierRefForTone)
    {
        if(slider == null) return;

        // Fill Area/Fill objesini bul
        Transform fillT = slider.transform.Find("Fill Area/Fill");
        Transform bgT = slider.transform.Find("Background");
        
        if (fillT == null || bgT == null) return;

        Image sliderFillImage = fillT.GetComponent<Image>();
        Image sliderBackgroundImage = bgT.GetComponent<Image>();

        Texture2D gradientTex = GenerateToneGradient(baseColor, modifierRefForTone);
        Sprite gradientSprite = Sprite.Create(gradientTex, new Rect(0, 0, gradientTex.width, gradientTex.height), new Vector2(0.5f, 0.5f));

        // Fill alanına uygula
        if (sliderFillImage != null)
        {
            sliderFillImage.sprite = gradientSprite;
            sliderFillImage.type = Image.Type.Simple;
            sliderFillImage.preserveAspect = false;
        }

        // Background alanına da uygula 🎯
        if (sliderBackgroundImage != null)
        {
            sliderBackgroundImage.sprite = gradientSprite;
            sliderBackgroundImage.type = Image.Type.Simple;
            sliderBackgroundImage.preserveAspect = false;
        }
    }

    private Texture2D GenerateToneGradient(Color baseColor, CharacterModifier modifier)
    {
        int width = 128;
        Texture2D tex = new Texture2D(width, 1);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < width; x++)
        {
            float t = x / (float)(width - 1);
            Color toned = modifier != null ? modifier.AdjustColorTone(baseColor, t) : baseColor;
            tex.SetPixel(x, 0, toned);
        }

        tex.Apply();
        return tex;
    }
    // --- DEBUG & FIX DOCTOR ---
    
    [Header("Debug Settings")]
    public bool debugScroll = true;

    private void LateUpdate()
    {
        if (debugScroll && contentScrollRect != null)
        {
            RectTransform content = contentScrollRect.content;
            RectTransform viewport = contentScrollRect.viewport;
            
            if (content != null && viewport != null)
            {
                // Only log if something is fishy (Content smaller than Viewport, or Input blocked)
                // or if manually dragging (velocity != 0)
                if (contentScrollRect.velocity.sqrMagnitude > 0.1f)
                {
                    Debug.Log($"[ScrollDebug] Moving.. Vel: {contentScrollRect.velocity} | Y-Pos: {content.anchoredPosition.y} | ContentH: {content.rect.height} | ViewH: {viewport.rect.height}");
                }
                
                // Detection: Content Height vs Viewport Height
                if (content.rect.height < viewport.rect.height && content.childCount > 0)
                {
                     // Debug.LogWarningOnce($"[ScrollDebug] Issue: Content Height ({content.rect.height}) < Viewport ({viewport.rect.height}). Scroll won't enable!");
                     // Auto-Fix attempt in Update? No, spammy.
                }
            }
        }
    }

    /// <summary>
    /// Call this via context menu to force-check everything
    /// </summary>
    [ContextMenu("Run Scroll Doctor")]
    public void RunScrollDoctor()
    {
        Debug.Log("--- SCROLL DOCTOR ---");
        
        if (contentScrollRect == null) { Debug.LogError("❌ ScrollRect is null!"); return; }
        Debug.Log("✅ ScrollRect assigned.");
        
        if (!contentScrollRect.vertical) { Debug.LogError("❌ Vertical Scrolling is disabled!"); contentScrollRect.vertical = true; }
        else Debug.Log("✅ Vertical enabled.");

        if (contentScrollRect.viewport == null) Debug.LogError("❌ Viewport is null!");
        else Debug.Log($"✅ Viewport: {contentScrollRect.viewport.name} (H: {contentScrollRect.viewport.rect.height})");

        RectTransform content = contentScrollRect.content;
        if (content == null) { Debug.LogError("❌ Content is null!"); return; }
        Debug.Log($"✅ Content: {content.name} (H: {content.rect.height})");

        // Hierarchy Check
        if (!content.IsChildOf(contentScrollRect.viewport)) Debug.LogError($"❌ Content is NOT child of Viewport! Parent is: {content.parent.name}");
        else Debug.Log("✅ Content is child of Viewport.");

        // Size Check
        if (content.rect.height <= contentScrollRect.viewport.rect.height) Debug.LogWarning("⚠️ Content Height <= Viewport Height. Scroll disabled.");
        else Debug.Log("✅ Height Check Passed.");
        
        // Raycast Check attempt
        if (!content.gameObject.activeInHierarchy) Debug.LogError("❌ Content is disable!");
        
        // Mask Check
        if (contentScrollRect.viewport.GetComponent<Mask>() == null && contentScrollRect.viewport.GetComponent<RectMask2D>() == null)
            Debug.LogWarning("⚠️ Viewport has no Mask!");
    }
}
