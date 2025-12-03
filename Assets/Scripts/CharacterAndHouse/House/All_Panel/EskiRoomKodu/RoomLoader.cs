using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public static class RoomLoader
{
    public static void Load(RoomType roomType, List<RoomPanel> roomPanels)
    {
        // 1️⃣ Rastgele görsel seç
        Sprite randomSprite = GetRandomTransitionSprite();

        // 1️⃣ transitionBackgroundImage’in Canvas bileşenini bul
        TransitionPanel transitionPanel = GameObject.FindObjectOfType<TransitionPanel>();
        Canvas transitionCanvas = null;
        int originalOrder = -100;

        if (transitionPanel != null)
        {
            Image panelImage = transitionPanel.GetComponentInChildren<Image>();
            if (panelImage != null && randomSprite != null)
            {
                panelImage.sprite = randomSprite;
            }

            transitionCanvas = transitionPanel.gameObject.GetComponent<Canvas>();
            if (transitionCanvas != null)
            {
                originalOrder = transitionCanvas.sortingOrder;
                transitionCanvas.sortingOrder = 100; // Geçiş sırasında en üstte
            }
        }
        
        // 2️⃣ SlidePanel kapansın
        SlidePanelController slidePanel = GameObject.FindObjectOfType<SlidePanelController>();
        if (slidePanel != null && slidePanel.IsOpen)
            slidePanel.ClosePanel();

        // 3️⃣ Ambient ses ve görsel değişimi
        UpdateAmbient(roomType);

        RoomManager.Instance.StartCoroutine(DelayedRoomSwitch(roomType, roomPanels, transitionCanvas, originalOrder));
    }

    private static IEnumerator DelayedRoomSwitch(RoomType roomType, List<RoomPanel> roomPanels, Canvas transitionCanvas, int originalOrder)
    {
        yield return new WaitForSeconds(3f);

        foreach (var panel in roomPanels)
        {
            bool isTargetRoom = panel.roomType == roomType;
            
            if (isTargetRoom)
            {
                LoadRoomObjects(panel);
            }
            
            // ScrollView (Yan yana odalar) sistemi için tüm odaların açık olması gerekir.
            // Sadece target odayı değil, hepsini aktif yapıyoruz.
            if (!panel.gameObject.activeSelf)
            {
                panel.gameObject.SetActive(true);
            }
        }

        // 6️⃣ transitionBackgroundImage sortingOrder geri alınsın
        if (transitionCanvas != null)
            transitionCanvas.sortingOrder = originalOrder;

        Debug.Log("Room loaded after transition: " + roomType);
    }

    private static void UpdateAmbient(RoomType roomType)
    {
        GameObject audioObj = GameObject.Find("AmbientAudio");
        if (audioObj != null)
        {
            AudioSource ambientAudio = audioObj.GetComponent<AudioSource>();
            AudioClip newClip = Resources.Load<AudioClip>("Audio/Ambient_" + roomType.ToString());
            if (ambientAudio != null && newClip != null)
            {
                ambientAudio.clip = newClip;
                ambientAudio.Play();
            }
        }

        GameObject bgObj = GameObject.Find("BackgroundImage");
        if (bgObj != null)
        {
            Image background = bgObj.GetComponent<Image>();
            Sprite newSprite = Resources.Load<Sprite>("Backgrounds/" + roomType.ToString());
            if (background != null && newSprite != null)
                background.sprite = newSprite;
        }
    }

    //RASTGELE RESIM SECIYOR
    private static Sprite GetRandomTransitionSprite()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/TransitionImages");
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("No transition images found in Resources/Images/TransitionImages");
            return null;
        }

        int index = Random.Range(0, sprites.Length);
        return sprites[index];
    }


    //ROOM ICERISINDE SAKLANMIS TUM OBJELERI YUKLEMEK ICIN KULLANILIYOR
    public static void LoadRoomObjects(RoomPanel roomPanel)
    {
        Debug.Log($"Loading objects for {roomPanel.roomType}");
        
        // Get saved data from the panel (which loaded it from JSON)
        List<RoomObjectData> savedObjects = roomPanel.GetSavedData();
        
        if (savedObjects == null || savedObjects.Count == 0)
        {
            Debug.Log("No saved objects found.");
            return;
        }

        // Clear existing objects to avoid duplicates? 
        // Or check if they exist?
        // For simplicity in this refactor, we clear and respawn.
        roomPanel.ClearObjects();

        foreach (var data in savedObjects)
        {
            // Prefab'ı Resources klasöründen yükle
            GameObject prefab = Resources.Load<GameObject>(data.objectID);
            if (prefab == null)
            {
                // Try finding by name if ID is just name
                prefab = Resources.Load<GameObject>($"Prefabs/{data.objectID}"); // Example path, adjust if needed
                
                if (prefab == null)
                {
                    Debug.LogWarning("Prefab bulunamadı: " + data.objectID);
                    continue;
                }
            }

            GameObject obj = GameObject.Instantiate(prefab, roomPanel.objectContainer);
            obj.name = data.objectID; // Ensure name matches ID
            
            // Add RoomObject component if missing
            if (obj.GetComponent<RoomObject>() == null)
            {
                obj.AddComponent<RoomObject>();
            }

            // UI objesi mi?
            if (obj.TryGetComponent<RectTransform>(out var rect))
            {
                if (data.customStates.TryGetValue("anchoredX", out var xStr) &&
                    data.customStates.TryGetValue("anchoredY", out var yStr))
                {
                    float x = float.Parse(xStr);
                    float y = float.Parse(yStr);
                    rect.anchoredPosition = new Vector2(x, y);
                }
            }
            else
            {
                obj.transform.position = data.position;
                obj.transform.rotation = data.rotation;
            }

            // Stateleri geri yükle
            if (obj.TryGetComponent<ObjectState>(out var state))
            {
                if (data.customStates.TryGetValue("isOpen", out var openStr))
                    state.IsOpen = bool.Parse(openStr);

                if (data.customStates.TryGetValue("temperature", out var tempStr))
                    state.Temperature = float.Parse(tempStr);
            }
            
            // Register with panel (RoomObject Start() will also do this, but we can do it here to be safe/explicit)
            // roomPanel.RegisterObject(obj); 
            // Note: RoomObject.Start() calls RegisterObject. If we call it here, it might be duplicate or fine.
            // RoomPanel.RegisterObject checks for duplicates.
            
            Debug.Log("Obje geri yüklendi: " + data.objectID);
        }
    }
}
