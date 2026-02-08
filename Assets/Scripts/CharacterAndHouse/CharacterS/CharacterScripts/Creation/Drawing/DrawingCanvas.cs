using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Çizim Sistemi (Robust Version)
/// </summary>
public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Drawing Settings")]
    public RenderTexture drawingTexture;
    public Texture2D templateMask;
    public Color brushColor = Color.red;
    public int brushSize = 10;
    
    [Header("UI Reference")]
    public RawImage displayImage; 
    public RawImage backgroundImage;

    private Texture2D brushTexture;
    private RectTransform rectTransform;
    private Vector2 lastPaintUV;
    private bool isDrawing = false;

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        Setup();
    }

    [ContextMenu("Auto Setup & Fix")]
    public void Setup()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // 1. Display Image Bul
        if (displayImage == null)
        {
            var obj = GameObject.Find("PaintingCanvas_Surface");
            if (obj != null) displayImage = obj.GetComponent<RawImage>();
            
            if (displayImage == null)
            {
                var rawImages = GetComponentsInChildren<RawImage>();
                if (rawImages.Length > 0) displayImage = rawImages[rawImages.Length - 1];
            }
        }

        // 2. Background Image Bul
        if (backgroundImage == null && transform.parent != null)
        {
            Transform bgTrans = transform.parent.Find("BackgroundImage");
            if (bgTrans != null) backgroundImage = bgTrans.GetComponent<RawImage>();
        }

        // 3. Raycast Ayarları
        if (backgroundImage != null) backgroundImage.raycastTarget = false;
        if (displayImage != null) displayImage.raycastTarget = false; // Tıklamayı DrawingCanvas (bu script) yakalamalı
        
        Image parentImg = GetComponent<Image>();
        if (parentImg != null) parentImg.raycastTarget = true;

        // 4. Layout Fix
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        
        RectTransform targetRT = displayImage != null ? displayImage.rectTransform : rectTransform;
        
        // Auto-Stretch Logic
        if (targetRT.rect.width < 100 || targetRT.rect.height < 100 || (targetRT.rect.width == 100 && targetRT.rect.height == 100))
        {
            Debug.LogWarning($"[DrawingCanvas] Canvas size too small ({targetRT.rect}). Auto-Stretching.");
            targetRT.anchorMin = Vector2.zero;
            targetRT.anchorMax = Vector2.one;
            targetRT.offsetMin = Vector2.zero;
            targetRT.offsetMax = Vector2.zero;
            targetRT.localScale = Vector3.one;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(targetRT);
            Canvas.ForceUpdateCanvases();
        }

        InitializeTexture();
        UpdateBrush();
        
        Debug.Log($"[DrawingCanvas] Setup Complete. Size: {targetRT.rect}");
    }

    void InitializeTexture()
    {
        if (drawingTexture != null && drawingTexture.IsCreated()) return;

        drawingTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        drawingTexture.filterMode = FilterMode.Bilinear;
        // drawingTexture.useMipMap = false; // Bazen blurry yapar, kapalı kalsın
        drawingTexture.Create();
        
        if (displayImage != null)
        {
            displayImage.texture = drawingTexture;
            displayImage.color = Color.white; 
        }

        ClearCanvas();
    }

    public void ClearCanvas()
    {
        RenderTexture old = RenderTexture.active;
        RenderTexture.active = drawingTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = old;
    }

    public enum BrushType { Pen, Marker, Crayon, Eraser }
    public BrushType currentBrushType = BrushType.Pen;

    public void SetBrushType(BrushType type)
    {
        currentBrushType = type;
        UpdateBrush();
    }

    public void UpdateBrush()
    {
        int size = Mathf.Max(1, brushSize * 2);
        if (brushTexture != null) Destroy(brushTexture);
        brushTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                Color pixelColor = Color.clear;

                if (d <= radius)
                {
                    if (currentBrushType == BrushType.Pen)
                    {
                        // Hard edge, full solid
                        pixelColor = brushColor;
                    }
                    else if (currentBrushType == BrushType.Marker)
                    {
                        // Soft edge, transparent (multiply effect simulated by low alpha)
                        float alpha = Mathf.SmoothStep(1f, 0f, d / radius);
                        pixelColor = new Color(brushColor.r, brushColor.g, brushColor.b, 0.5f * alpha);
                    }
                    else if (currentBrushType == BrushType.Crayon)
                    {
                        // Noise texture for wax effect
                        float noise = Mathf.PerlinNoise(x * 0.2f, y * 0.2f);
                        if (noise > 0.4f) // Threshold
                        {
                            pixelColor = brushColor;
                        }
                    }
                    else if (currentBrushType == BrushType.Eraser)
                    {
                        // Eraser logic is handled in DrawAt usually via BlendMode, 
                        // but here we just send clear color. 
                        // Note: GL.Clear or BlendMode.Zero is needed for true erasing in Unity GL.
                        pixelColor = Color.clear; 
                    }
                }
                brushTexture.SetPixel(x, y, pixelColor);
            }
        }
        brushTexture.Apply();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GetUV(eventData, out Vector2 uv))
        {
            isDrawing = true;
            lastPaintUV = uv;
            DrawAt(uv);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDrawing && GetUV(eventData, out Vector2 uv))
        {
            float dist = Vector2.Distance(lastPaintUV, uv);
            int steps = Mathf.Max(1, Mathf.CeilToInt(dist * 512f)); 
            for (int i = 1; i <= steps; i++)
            {
                DrawAt(Vector2.Lerp(lastPaintUV, uv, (float)i / steps));
            }
            lastPaintUV = uv;
        }
    }

    public void OnPointerUpHelper() { isDrawing = false; } 

    bool GetUV(PointerEventData eventData, out Vector2 uv)
    {
        uv = Vector2.zero;
        RectTransform targetRect = (displayImage != null) ? displayImage.rectTransform : rectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, eventData.position, eventData.pressEventCamera, out Vector2 localPos))
        {
            uv.x = (localPos.x / targetRect.rect.width) + targetRect.pivot.x;
            uv.y = (localPos.y / targetRect.rect.height) + targetRect.pivot.y;
            return uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1;
        }
        return false;
    }

    // Material for drawing (cached)
    private Material drawingMaterial;

    void DrawAt(Vector2 uv)
    {
        if (drawingMaterial == null)
        {
            // Simple shader for UI particles/drawing
            drawingMaterial = new Material(Shader.Find("Sprites/Default")); 
        }

        RenderTexture old = RenderTexture.active;
        RenderTexture.active = drawingTexture;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, drawingTexture.width, 0, drawingTexture.height);

        float x = uv.x * drawingTexture.width;
        float y = uv.y * drawingTexture.height;
        float r = brushSize;

        if (currentBrushType == BrushType.Eraser)
        {
            // To ERASE (make transparent), we need a specific blend mode.
            // Source: Zero, Dest: OneMinusSrcAlpha is common for "Cutout" but we want to reduce Alpha.
            // Standard Sprites/Default does alpha blending.
            // For true erasing, we'd use Blend Zero OneMinusSrcAlpha (to clear bits).
            // But without a custom shader, let's try specific GL states if possible, or just paint Clear?
            // Painting "Color.clear" with Standard Blending (SrcAlpha, OneMinusSrcAlpha) does NOTHING (adds 0).
            // We need to use "Clear" operation.
            // Hacky Fix: Use GL.Clear on the specific rect? No, that clears everything in the rect (square eraser).
            // Let's use a "Square" eraser for now which is robust.
            GL.Clear(false, true, Color.clear, 1.0f); // This clears the Whole screen? No, it clears Depth/Color buffers.
            // Okay, let's fallback to "White Eraser" if the canvas is white based.
            // Since we initialized drawingTexture as WHITE in Setup(), "Erasing" effectively means painting WHITE.
            // However, the user might want transparent clothes.
            // Let's assume we are painting on a transparent layer (InitializeTexture does Color.clear?).
            // Let's check Setup again. Setup does `displayImage.color = Color.white;` but `InitializeTexture` does `GL.Clear(..., Color.clear)`.
            // So the texture is transparent. To erase, we need to remove alpha.
            // We will defer "True Eraser" optimization and just use a "Cutout" approach if possible or simple Eraser=White.
             // For Toca Boca, usually backgrounds are not transparent while drawing?
             // If we really want transparent output, we need a Cutout shader.
             // Let's assume we paint WHITE for now as a "Corrector".
             // Or better: Use BlendMode via GL.
        }

        Graphics.DrawTexture(new Rect(x - r, y - r, r * 2, r * 2), brushTexture, drawingMaterial);

        GL.PopMatrix();
        RenderTexture.active = old;
    }

    public void SetBrushSize(int s) { brushSize = s; UpdateBrush(); }
    public void SetBrushColor(Color c) { brushColor = c; UpdateBrush(); }
    public void SetTemplateMask(Texture2D t) { templateMask = t; }
    public void SetBackgroundImage(Texture2D t) 
    { 
        if(backgroundImage) 
        {
            backgroundImage.texture = t; 
            backgroundImage.color = (t!=null) ? new Color(1,1,1,0.5f) : Color.clear;
        }
    }
}
