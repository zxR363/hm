using UnityEngine;
using System.Collections;



// BELIRLI SURE ILE SPRITE DEGISTIRME ISLEMI YAPMAK ISTERSEK 
//(Orn: animasyon vb yada Tablo degistirme)
public class SpriteChanger : MonoBehaviour
{
    [System.Serializable]
    public class SpriteData
    {
        public Sprite sprite;           
        public int orderInLayer;        

        public bool overrideScale = false;
        public Vector3 scale = Vector3.one;

        public bool overridePosition = false;
        public Vector3 position = Vector3.zero;

        public float interval = 2f;     // bu sprite’ın ekranda kalma süresi
    }

    public SpriteData[] sprites;
    private SpriteRenderer spriteRenderer;
    private int currentIndex = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(ChangeSpriteRoutine());
    }

    IEnumerator ChangeSpriteRoutine()
    {
        while (true)
        {
            if (sprites.Length == 0) yield break;

            var data = sprites[currentIndex];

            // Sprite ve order
            spriteRenderer.sprite = data.sprite;
            spriteRenderer.sortingOrder = data.orderInLayer;

            // Eğer override işaretlendiyse uygula
            if (data.overrideScale) transform.localScale = data.scale;
            if (data.overridePosition) transform.localPosition = data.position;

            // Bekleme süresi
            yield return new WaitForSeconds(data.interval);

            // Sonraki sprite
            currentIndex = (currentIndex + 1) % sprites.Length;
        }
    }
}
