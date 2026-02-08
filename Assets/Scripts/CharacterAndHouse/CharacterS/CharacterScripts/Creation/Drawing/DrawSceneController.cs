using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class DrawSceneController : MonoBehaviour
{
    [Header("Core")]
    public DrawingCanvas drawingCanvas;
    
    [Header("UI")]
    public Button saveButton;
    public Button clearButton;
    public Button backButton;
    public Dropdown itemTypeDropdown; // "T-Shirt", "Hat", etc.

    [Header("Templates")]
    public Texture2D tshirtTemplate;
    public Texture2D hatTemplate;

    [Header("Drawing Tools")]
    public Transform colorsPanel; 
    public Transform brushesPanel;

    void Start()
    {
        // Auto Find Core
        if (drawingCanvas == null) drawingCanvas = FindFirstObjectByType<DrawingCanvas>(FindObjectsInactive.Include);
        if (saveButton == null) saveButton = GameObject.Find("SaveButton")?.GetComponent<Button>();
        if (clearButton == null) clearButton = GameObject.Find("ClearButton")?.GetComponent<Button>();
        if (backButton == null) backButton = GameObject.Find("BackButton")?.GetComponent<Button>();

        // Auto Find Panels if not assigned
        if (colorsPanel == null) colorsPanel = GameObject.Find("ColorsPanel")?.transform;
        if (brushesPanel == null) brushesPanel = GameObject.Find("BrushesPanel")?.transform;

        // Bind Core Buttons
        if (saveButton) saveButton.onClick.AddListener(OnSave);
        if (clearButton) clearButton.onClick.AddListener(OnClear);
        if (backButton) backButton.onClick.AddListener(OnBack);

        // Bind Item Dropdown
        if (itemTypeDropdown)
        {
            itemTypeDropdown.onValueChanged.AddListener(OnTypeChanged);
            OnTypeChanged(itemTypeDropdown.value);
        }

        // Setup 20-Color Grid
        SetupColorPalette();

        // Auto-Bind Brush Buttons (Expects buttons named "Btn_Pen", "Btn_Marker", etc)
        if (brushesPanel != null)
        {
            BindBrushButton(brushesPanel, "Btn_Pen", DrawingCanvas.BrushType.Pen);
            BindBrushButton(brushesPanel, "Btn_Marker", DrawingCanvas.BrushType.Marker);
            BindBrushButton(brushesPanel, "Btn_Crayon", DrawingCanvas.BrushType.Crayon);
            BindBrushButton(brushesPanel, "Btn_Eraser", DrawingCanvas.BrushType.Eraser);
        }
    }

    void BindBrushButton(Transform root, string btnName, DrawingCanvas.BrushType type)
    {
        Transform t = root.Find(btnName);
        if (t != null)
        {
            Button b = t.GetComponent<Button>();
            if (b != null) b.onClick.AddListener(() => SetBrush(type));
        }
    }

    public void SetColor(Color c)
    {
        if (drawingCanvas) drawingCanvas.SetBrushColor(c);
    }

    public void SetBrush(DrawingCanvas.BrushType type)
    {
        if (drawingCanvas) drawingCanvas.SetBrushType(type);
    }

    void OnTypeChanged(int index)
    {
        string type = itemTypeDropdown.options[index].text;
        Texture2D mask = null;
        if (type.Contains("Hat")) mask = hatTemplate;
        else mask = tshirtTemplate; // Default

        if(drawingCanvas)
        {
             drawingCanvas.SetTemplateMask(mask);
             drawingCanvas.SetBackgroundImage(mask);
        }
    }

    void OnSave()
    {
        if(!drawingCanvas) return;
        
        string typeName = (itemTypeDropdown) ? itemTypeDropdown.options[itemTypeDropdown.value].text : "Custom";
        string fileName = $"{typeName}_{System.DateTime.Now:yyyyMMdd_HHmmss}";
        
        string dir = Path.Combine(Application.persistentDataPath, "CustomClothes");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
        string path = Path.Combine(dir, fileName + ".png");
        
        // Save to PNG
        Texture2D tex = new Texture2D(drawingCanvas.drawingTexture.width, drawingCanvas.drawingTexture.height, TextureFormat.ARGB32, false);
        RenderTexture old = RenderTexture.active;
        RenderTexture.active = drawingCanvas.drawingTexture;
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = old;

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        
        Debug.Log($"[DrawScene] Saved to: {path}");
    }

    void OnClear()
    {
        if(drawingCanvas) drawingCanvas.ClearCanvas();
    }

    void OnBack()
    {
        SceneManager.LoadScene("CharacterScene"); 
    }
    // Toca Boca Style Color Palette (20 Colors)
    private readonly Color[] tocaColors = new Color[]
    {
        // Row 1: Vibrants
        new Color(1f, 0.2f, 0.4f), // Hot Pink
        new Color(1f, 0.5f, 0f),   // Orange
        new Color(1f, 0.9f, 0.2f), // Yellow
        new Color(0.2f, 0.8f, 0.2f), // Lime
        new Color(0.2f, 0.6f, 1f), // Sky Blue

        // Row 2: Pastels
        new Color(1f, 0.7f, 0.8f), // Soft Pink
        new Color(1f, 0.8f, 0.6f), // Peach
        new Color(1f, 1f, 0.8f),   // Cream
        new Color(0.7f, 1f, 0.8f), // Mint
        new Color(0.7f, 0.8f, 1f), // Periwinkle

        // Row 3: Darks & Basics
        new Color(0.5f, 0f, 0.5f), // Purple
        new Color(0f, 0.3f, 0.6f), // Dark Blue
        new Color(0.4f, 0.2f, 0f), // Brown
        new Color(0.2f, 0.2f, 0.2f), // Dark Grey
        new Color(0f, 0f, 0f),     // Black

        // Row 4: Skin Tones & Whites
        new Color(1f, 0.9f, 0.8f), // Fair
        new Color(0.9f, 0.7f, 0.5f), // Tan
        new Color(0.6f, 0.4f, 0.3f), // Dark Skin
        new Color(0.8f, 0.8f, 0.8f), // Light Grey
        new Color(1f, 1f, 1f)      // White
    };

    void SetupColorPalette()
    {
        if (colorsPanel == null) return;

        // Clean existing buttons (if any) to avoid duplicates
        foreach (Transform child in colorsPanel) {
            Destroy(child.gameObject);
        }

        // Tweak Layout Group locally if needed
        GridLayoutGroup grid = colorsPanel.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = colorsPanel.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(40, 40); // Estimate size
        grid.spacing = new Vector2(5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        // Generate Buttons
        foreach (Color c in tocaColors)
        {
            GameObject btnObj = new GameObject("Btn_Color", typeof(Image), typeof(Button));
            btnObj.transform.SetParent(colorsPanel, false);
            
            // Setup Image
            Image img = btnObj.GetComponent<Image>();
            img.color = c;
            
            // Setup Button
            Button btn = btnObj.GetComponent<Button>();
            Color targetColor = c; // Capture closure
            btn.onClick.AddListener(() => SetColor(targetColor));
        }
    }
}
