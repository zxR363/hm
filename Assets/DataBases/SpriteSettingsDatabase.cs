using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SpriteSettingsDatabase", menuName = "Sprite Settings Database")]
public class SpriteSettingsDatabase : ScriptableObject
{
    [System.Serializable]
    public class SpriteSetting
    {
        public Sprite sprite;
        public Vector3 position;
        public Vector3 scale = Vector3.one;
    }

    public List<SpriteSetting> settings = new List<SpriteSetting>();

    public SpriteSetting GetSetting(Sprite sprite)
    {
        return settings.Find(s => s.sprite == sprite);
    }

    public void SaveSetting(Sprite sprite, Vector3 position, Vector3 scale)
    {
        var existing = GetSetting(sprite);
        if (existing != null)
        {
            existing.position = position;
            existing.scale = scale;
        }
        else
        {
            settings.Add(new SpriteSetting
            {
                sprite = sprite,
                position = position,
                scale = scale
            });
        }
    }
}
