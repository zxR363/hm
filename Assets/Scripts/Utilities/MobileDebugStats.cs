using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MobileDebugStats : MonoBehaviour
{
    private TextMeshProUGUI debugText;
    private RectTransform canvasRect;
    private RectTransform scrollViewRect;
    private RectTransform viewportRect;
    private RectTransform room2PanelRect;
    private RectTransform wallRawImageRect;

    private void Start()
    {
        // Setup simple GUI Text if not assigned
        GameObject go = new GameObject("MobileDebug_Text");
        go.transform.SetParent(this.transform.root, false); // Attach to root Canvas
        
        // Ensure it's on top
        Canvas c = go.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 999;
        go.AddComponent<GraphicRaycaster>();

        debugText = go.AddComponent<TextMeshProUGUI>();
        debugText.rectTransform.anchorMin = Vector2.zero; // Bottom Left
        debugText.rectTransform.anchorMax = Vector2.one;  // Full Screen
        debugText.rectTransform.offsetMin = new Vector2(50, 50);
        debugText.rectTransform.offsetMax = new Vector2(-50, -50);
        debugText.color = Color.green;
        debugText.fontSize = 28;
        debugText.fontStyle = FontStyles.Bold;
        debugText.raycastTarget = false;

        // Auto-Find References (Hardcoded for this diagnosis)
        canvasRect = this.transform.root.GetComponent<RectTransform>();
        
        GameObject sv = GameObject.Find("HouseScrollView");
        if(sv) scrollViewRect = sv.GetComponent<RectTransform>();

        if (scrollViewRect)
        {
            Transform vp = scrollViewRect.Find("Viewport");
            if (vp) viewportRect = vp.GetComponent<RectTransform>();
            
            if (vp)
            {
                Transform content = vp.Find("Content");
                if (content)
                {
                    Transform r2 = content.Find("Room2Panel");
                    if (r2) room2PanelRect = r2.GetComponent<RectTransform>();
                    
                    if(r2)
                    {
                         Transform bg = r2.Find("RoomBackground/Blocks/Wall/WallRawImage");
                         // Try alternate path if user hierarchy differs
                         if(bg == null) bg = r2.Find("RoomBackground/Wall/WallRawImage");
                         
                         if(bg) wallRawImageRect = bg.GetComponent<RectTransform>();
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (debugText == null) return;

        string msg = "=== LAYOUT DIAGNOSIS ===\n";
        msg += $"Screen: {Screen.width} x {Screen.height}\n";
        
        if (canvasRect) msg += $"Canvas: {canvasRect.rect.width:F0} x {canvasRect.rect.height:F0} (Scale: {canvasRect.localScale.x:F3})\n";
        
        if (scrollViewRect)
        {
            msg += $"ScrollView: {scrollViewRect.rect.width:F0} x {scrollViewRect.rect.height:F0}";
            // Check Anchors
            bool isStretch = scrollViewRect.anchorMin == Vector2.zero && scrollViewRect.anchorMax == Vector2.one;
            msg += isStretch ? " [STRETCH OK]\n" : " [ANCHOR FAIL!]\n";
        }
        else msg += "ScrollView NOT FOUND\n";

        if (viewportRect) msg += $"Viewport: {viewportRect.rect.width:F0} (Parent of Content)\n";
        
        if (room2PanelRect)
        {
            msg += $"Room2Panel: {room2PanelRect.rect.width:F0} (Target: {viewportRect?.rect.width:F0})\n";
            var le = room2PanelRect.GetComponent<LayoutElement>();
            if (le) msg += $"  -> LayoutElement PrefWidth: {le.preferredWidth:F0}\n";
            else msg += "  -> NO LAYOUT ELEMENT!\n";
            
            var mvs = room2PanelRect.GetComponent<MatchViewportSize>();
            msg += $"  -> MatchViewportSize: {(mvs && mvs.enabled ? "ACTIVE" : "MISSING/OFF")}\n";
        }
        else msg += "Room2Panel NOT FOUND\n";

        if (wallRawImageRect)
        {
            msg += $"WallRawImage: {wallRawImageRect.rect.width:F0} (Parent: {wallRawImageRect.parent.GetComponent<RectTransform>().rect.width:F0})\n";
            msg += $"  -> Anchors: {wallRawImageRect.anchorMin} / {wallRawImageRect.anchorMax}\n";
            msg += $"  -> AnchoredPos: {wallRawImageRect.anchoredPosition} | SizeDelta: {wallRawImageRect.sizeDelta}\n";
            msg += $"  -> WorldPos: {wallRawImageRect.position}\n";

            var raw = wallRawImageRect.GetComponent<RawImage>();
            if (raw)
            {
                msg += $"  -> [RawImage]: Enabled={raw.enabled}, Color={raw.color}\n";
                if (raw.texture) msg += $"  -> Texture: '{raw.texture.name}' ({raw.texture.width}x{raw.texture.height})\n";
                else msg += "  -> Texture: NULL (!!!)\n";
                
                msg += $"  -> Material: {(raw.material ? raw.material.name : "Null")}\n";
            }
            
            // Check for Masking parents
            var mask = wallRawImageRect.GetComponentInParent<Mask>();
            var rectMask = wallRawImageRect.GetComponentInParent<RectMask2D>();
            if (mask || rectMask) msg += $"  -> INSIDE MASK: {(mask ? mask.name : rectMask.name)}\n";
        }
        else msg += "WallRawImage NOT FOUND (Check Path)\n";

        debugText.text = msg;
    }
}
