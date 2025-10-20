using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class RoomPanel : MonoBehaviour
{
    public RoomType roomType;

    public Transform objectContainer;
    public List<RoomObjectData> trackedObjects = new List<RoomObjectData>();

    private float updateInterval = 0.5f;
    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            UpdateAllObjectsRecursive(objectContainer);
            timer = 0f;
        }
    }

    //Odadaki objeleri kaydediyor (Oda felan değiştiğinde kaybolmaması için)
    public void RegisterObject(GameObject obj)
    {
        RoomObjectData data = new RoomObjectData
        {
            objectID = obj.name,
            instance = obj
        };

        // UI objesi mi?
        if (obj.TryGetComponent<RectTransform>(out var rect))
        {
            data.customStates["anchoredX"] = rect.anchoredPosition.x.ToString();
            data.customStates["anchoredY"] = rect.anchoredPosition.y.ToString();
        }
        else
        {
            data.position = obj.transform.position;
            data.rotation = obj.transform.rotation;
        }

        // // Özel stateleri kaydet
        // if (obj.TryGetComponent<ObjectState>(out var state))
        // {
        //     data.customStates["isOpen"] = state.IsOpen.ToString();
        //     data.customStates["temperature"] = state.Temperature.ToString();
        // }

        Debug.Log("OBJECTID = "+obj.name+"   EKLENDI  " +rect.anchoredPosition.x.ToString()+" "+rect.anchoredPosition.y.ToString());

        trackedObjects.Add(data);
    }

    public void NotifyObjectChanged(GameObject obj)
    {
        var data = trackedObjects.FirstOrDefault(d => d.instance == obj);
        if (data == null) return;

        // UI objesi mi?
        if (obj.TryGetComponent<RectTransform>(out var rect))
        {
            data.customStates["anchoredX"] = rect.anchoredPosition.x.ToString();
            data.customStates["anchoredY"] = rect.anchoredPosition.y.ToString();
        }
        else
        {
            data.position = obj.transform.position;
            data.rotation = obj.transform.rotation;
        }

        // Stateleri güncelle
        if (obj.TryGetComponent<ObjectState>(out var state))
        {
            data.customStates["isOpen"] = state.IsOpen.ToString();
            data.customStates["temperature"] = state.Temperature.ToString();
        }

        Debug.Log("Objede değişiklik algılandı: " + data.objectID);
    }

    //Mevcut objeleri geri yüklüyor.
    public void UpdateAllObjectsRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            var obj = child.gameObject;
            var data = trackedObjects.FirstOrDefault(d => d.instance == obj);

            if (data == null)
            {
                RegisterObject(obj);
            }
            else
            {
                // UI objesi mi?
                if (obj.TryGetComponent<RectTransform>(out var rect))
                {
                    data.customStates["anchoredX"] = rect.anchoredPosition.x.ToString();
                    data.customStates["anchoredY"] = rect.anchoredPosition.y.ToString();
                }
                else
                {
                    data.position = obj.transform.position;
                    data.rotation = obj.transform.rotation;
                }

                // // Stateleri güncelle
                // if (obj.TryGetComponent<ObjectState>(out var state))
                // {
                //     data.customStates["isOpen"] = state.IsOpen.ToString();
                //     data.customStates["temperature"] = state.Temperature.ToString();
                // }
            }

            // Alt objeleri de taramak için recursive çağrı
            if (child.childCount > 0)
                UpdateAllObjectsRecursive(child);
        }
    }

    public void ClearObjects()
    {
        foreach (var data in trackedObjects)
        {
            if (data.instance != null)
                Destroy(data.instance);
        }

        trackedObjects.Clear();
    }

}