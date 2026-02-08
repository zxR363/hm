using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CustomClothesGallery : MonoBehaviour
{
    public GameObject itemPrefab; 
    public Transform contentRoot;
    public CharacterModifier modifier;
    private GameObject characterRoot;

    // Type -> BoneName Mapping
    private Dictionary<string, string> typeMap = new Dictionary<string, string>
    {
        { "T-Shirt", "Clothes" },
        { "Hat", "Hat" },
        { "Pants", "Accessory" }
    };

    void OnEnable() { RefreshGallery(); }

    public void RefreshGallery()
    {
        foreach (Transform t in contentRoot) Destroy(t.gameObject);

        string dir = Path.Combine(Application.persistentDataPath, "CustomClothes");
        if (!Directory.Exists(dir)) return;

        foreach (string file in Directory.GetFiles(dir, "*.png"))
        {
            CreateItem(file);
        }
    }

    void CreateItem(string path)
    {
        if(!itemPrefab) return;
        GameObject go = Instantiate(itemPrefab, contentRoot);
        RawImage img = go.GetComponentInChildren<RawImage>();
        Button btn = go.GetComponent<Button>();

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);

        if(img) 
        {
            img.texture = tex;
            img.color = Color.white;
        }

        if(btn) btn.onClick.AddListener(()=> Apply(path));
    }

    void Apply(string path)
    {
        if (modifier == null) modifier = FindFirstObjectByType<CharacterModifier>();
        if (characterRoot == null) characterRoot = GameObject.FindWithTag("Player");

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);

        Sprite s = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f,0.5f));
        s.name = Path.GetFileNameWithoutExtension(path);

        string fileName = Path.GetFileNameWithoutExtension(path); 
        // fileName expects: Type_Timestamp
        string type = fileName.Split('_')[0];
        
        string bone = "Clothes";
        if (typeMap.ContainsKey(type)) bone = typeMap[type];

        if(modifier) modifier.SetBodyPartSprite(characterRoot, bone, s);
        
        Debug.Log($"Applied {bone} from {path}");
    }
}
