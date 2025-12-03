using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//SlidePanelSpace'in tüm oyun boyunca sadece 1 tane instance olmasını sağlıyor.

public class SlidePanelSpace : MonoBehaviour
{
    public static SlidePanelSpace Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Sahneye yeni ekleneni sil
            Debug.Log("SlidePanelSpace vardı YENI OLUSTURULAN SILINDI");
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // İlk oluşturulanı koru

        Debug.Log("SlidePanelSpace oluşturuldu");

    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
}
