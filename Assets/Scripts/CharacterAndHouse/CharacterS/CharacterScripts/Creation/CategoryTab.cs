using System.Collections.Generic;
using UnityEngine;

public enum SubCategoryType
{
    Folder,         // A specific folder of items (e.g. "BoyHair")
    ColorPalette,   // Opens the color palette
    DirectItems     // Loads items from a specific path (or default)
}

[System.Serializable]
public class SubCategoryDef
{
    public string subCategoryName;      // Display Name (e.g. "Casual")
    public Sprite icon;                 // Icon for the button
    public SubCategoryType type;        // Type of action
    public string path;                 // Path for resources (e.g. "Images/Character/Style/Hair/BoyHair")
    public Object paletteObject;        // Custom palette for this specific sub-option
}

public class CategoryTab : MonoBehaviour
{
    [Header("Configuration")]
    public string categoryId; // e.g. "Hair", "Clothes"
    public bool autoSelectFirst = false; // If true, the first sub-category is selected automatically
    
    [Header("Sub Options")]
    [Header("Sub Options")]
    public List<SubCategoryDef> subCategories = new List<SubCategoryDef>();
    
    [Header("Custom Colors")]
    [Tooltip("Drag a ScriptableObject (SkinColorList or ItemsColorList) here to load colors automatically.")]
    public Object paletteObject; // Drag-drop slot
}
