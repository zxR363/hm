using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public static class RoomLoader
{
    public static void Load(RoomType roomType,List<RoomPanel> roomPanels)
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

        RoomManager.Instance.StartCoroutine(DelayedRoomSwitch(roomType,roomPanels, transitionCanvas, originalOrder));
    }

    private static IEnumerator DelayedRoomSwitch(RoomType roomType,List<RoomPanel> roomPanels, Canvas transitionCanvas, int originalOrder)
    {
        yield return new WaitForSeconds(3f);

        foreach (var panel in roomPanels)
        {
            if(panel.roomType == roomType)
            {
                LoadRoomObjects(panel);
            }
            panel.gameObject.SetActive(panel.roomType == roomType);
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
        Debug.Log("LoadRoomObject GERI YUKLENDI");
        foreach (var data in roomPanel.trackedObjects)
        {
            if (data.instance != null) continue; // Zaten sahnede varsa atla

            // Prefab'ı Resources klasöründen yükle
            GameObject prefab = Resources.Load<GameObject>(data.objectID);
            if (prefab == null)
            {
                Debug.LogWarning("Prefab bulunamadı: " + data.objectID);
                continue;
            }

            GameObject obj = GameObject.Instantiate(prefab, roomPanel.objectContainer);
            data.instance = obj;

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

            Debug.Log("Obje geri yüklendi: " + data.objectID);
        }
    }



}
