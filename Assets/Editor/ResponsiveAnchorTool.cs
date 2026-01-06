using UnityEngine;
using UnityEditor;

public class ResponsiveAnchorTool : EditorWindow
{
    private RectTransform itemsContainer;

    [MenuItem("Tools/Nibi World: My Avatar Life/Responsive Anchor Tool (Native)")]
    public static void ShowWindow()
    {
        GetWindow<ResponsiveAnchorTool>("Responsive Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("LAYOUT FREEZER (NATIVE)", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("1. Mevcut Görüntüyü Kilitle: Eşyaları kutuya çiviler.\n2. Zinciri Analiz Et: telefonda neden boşluk kaldığını bulur.", MessageType.Info);
        GUILayout.Space(10);

        itemsContainer = (RectTransform)EditorGUILayout.ObjectField("Hedef Kutu (Content/Canvas)", itemsContainer, typeof(RectTransform), true);

        GUILayout.Space(20);
        
        // DIAGNOSE
        GUI.enabled = itemsContainer != null;
        if (GUILayout.Button("ZİNCİRİ ANALİZ ET (Hata Bul)", GUILayout.Height(40)))
        {
             DiagnoseChain();
        }
        
        GUILayout.Space(10);
        // FIX SCALER
        if (GUILayout.Button("CANVAS AYARLARINI DÜZELT (Mobile Fix)", GUILayout.Height(30)))
        {
             if (EditorUtility.DisplayDialog("Canvas Fix", "Canvas Scaler ayarları 'Scale With Screen Size' (1920x1080) olarak ayarlanacak.\nBu işlem telefonda arayüzün doğru boyutlanmasını sağlar.", "Düzelt", "İptal"))
             {
                 FixCanvasScaler();
             }
        }
        
        GUILayout.Space(10);

        // STRETCH CONTAINER
        if (GUILayout.Button("KUTUYU EKRANA YAY (Fix Container)", GUILayout.Height(30)))
        {
             if (EditorUtility.DisplayDialog("Uyarı", "Bu işlem 'Hedef Kutu'nun çapasını (Anchor) 'Stretch All' (tam ekran) yapar.\nOffsetleri sıfırlar.\nEğer bu bir ScrollView Content ise dikkatli kullanın (Height'i bozulabilir).", "Yap", "İptal"))
             {
                 FixContainerStretch();
             }
        }
        
        GUILayout.Space(10);

        // ONE CLICK FIX
        if (GUILayout.Button("MEVCUT GÖRÜNTÜYÜ KİLİTLE (Bake Children)", GUILayout.Height(50)))
        {
             if (EditorUtility.DisplayDialog("Onay", "Tüm alt objeler şu anki konumlarına göre kilitlenecek (Anchor). Emin misiniz?", "Kilitle", "İptal"))
             {
                 ApplyPureBake();
             }
        }
        
        GUILayout.Space(20);
        if (GUILayout.Button("Seçili Objeleri Kilitle"))
        {
            BakeSelectedAnchors();
        }
        GUI.enabled = true;
    }
    
    // --- FEATURE 1: DIAGNOSE ---
    private void DiagnoseChain()
    {
        if (itemsContainer == null) return;
        
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("--- RESPONSIVE CHAIN ANALYSIS ---");
        
        RectTransform current = itemsContainer;
        // bool chainBroken = false; // Unused
        
        while (current != null)
        {
            string status = "OK";
            // Check Anchor Stretch
            bool fillsWidth = Mathf.Abs(current.anchorMin.x - 0) < 0.01f && Mathf.Abs(current.anchorMax.x - 1) < 0.01f;
            bool fillsHeight = Mathf.Abs(current.anchorMin.y - 0) < 0.01f && Mathf.Abs(current.anchorMax.y - 1) < 0.01f;
            
            // Allow ScrollView Content to be wider, but Height MUST match for full screen bg
            bool isContent = current == itemsContainer;
            
            if (!fillsHeight)
            {
                status = "FAIL (Dikey Yayılmıyor!)";
                // chainBroken = true;
            }
            else if (!fillsWidth && !isContent)
            {
                 status = "FAIL (Yatay Yayılmıyor!)";
                 // chainBroken = true;
            }
            
            report.AppendLine($"[{current.name}]: {status}");
            report.AppendLine($"   Anchors: ({current.anchorMin} - {current.anchorMax})");
            report.AppendLine($"   Offsets: ({current.offsetMin} - {current.offsetMax})");
            
            if (current.GetComponent<Canvas>() != null) break; 
            current = current.parent as RectTransform;
        }
        
        // Analyze Important Children (Walls)
        report.AppendLine("\n--- WALL CHECK ---");
        var walls = itemsContainer.GetComponentsInChildren<UnityEngine.UI.RawImage>();
        foreach(var w in walls)
        {
             RectTransform rt = w.GetComponent<RectTransform>();
             float zRot = rt.localRotation.eulerAngles.z;
             report.AppendLine($"[{w.name}] Rot: {zRot:F2}");
             if (Mathf.Abs(zRot) > 0.5f) report.AppendLine("   -> WARNING: Rotated object will NOT stretch!");
             else report.AppendLine("   -> OK: Rotation small enough to stretch.");
             
             bool stretches = Mathf.Abs(rt.anchorMin.x - rt.anchorMax.x) > 0.1f; 
             if (!stretches) report.AppendLine("   -> WARNING: Uses Point Anchor (Not stretching).");
             else report.AppendLine("   -> OK: Uses Stretch Anchor.");
        }
        
        Debug.Log(report.ToString());
        EditorUtility.DisplayDialog("Analiz Raporu", "Konsola detaylı rapor yazıldı. Lütfen Console penceresini kontrol edin.", "Tamam");
    }

    // --- FEATURE 2: FIX CANVAS SCALER ---
    private void FixCanvasScaler()
    {
        // Find Canvas
        Canvas canvas = null;
        if (itemsContainer != null) canvas = itemsContainer.GetComponentInParent<Canvas>();
        else canvas = FindObjectOfType<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("Canvas bulunamadı!");
            return;
        }
        
        UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        
        Undo.RecordObject(scaler, "Fix Canvas Scaler");
        
        // Standard Mobile Settings
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f; // Balance
        
        Debug.Log($"[ResponsiveTool] Canvas '{canvas.name}' ayarlandı: ScaleWithScreenSize, 1920x1080, Match 0.5");
        EditorUtility.DisplayDialog("Tamam", "Canvas Ayarları Mobile Uygun Hale Getirildi!", "Süper");
    }

    // --- FEATURE 3: STRETCH CONTAINER ---
    private void FixContainerStretch()
    {
        Undo.RecordObject(itemsContainer, "Stretch Container");

        // STRATEGY: Unparent Children -> Stretch Container -> Reparent Children
        // This prevents children from jumping when the container resizes/moves.
        
        int childCount = itemsContainer.childCount;
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++) children[i] = itemsContainer.GetChild(i);

        // A. Detach
        foreach (var t in children) Undo.SetTransformParent(t, null, "Detach for Stretch");

        // B. Force Stretch (Min 0,0 - Max 1,1)
        itemsContainer.anchorMin = Vector2.zero;
        itemsContainer.anchorMax = Vector2.one;
        itemsContainer.pivot = new Vector2(0.5f, 0.5f);
        
        // Zero Offsets (Fill parent)
        itemsContainer.offsetMin = Vector2.zero;
        itemsContainer.offsetMax = Vector2.zero;
        
        // C. Re-Attach
        foreach (var t in children) Undo.SetTransformParent(t, itemsContainer.transform, "Reattach after Stretch");
        
        // Update Canvas
        Canvas.ForceUpdateCanvases();
        Debug.Log("[ResponsiveTool] Container ekrana yayildi (Stretch) ve çocukların konumu korundu.");
        EditorUtility.DisplayDialog("Tamam", "Kutu Ekrana Yayildi.\nÇocukların konumu korundu.\n\nŞimdi 'MEVCUT GÖRÜNTÜYÜ KİLİTLE' diyerek sabitleyebilirsiniz.", "Tamam");
    }

    // --- FEATURE 4: BAKE ANCHORS (MAIN) ---
    private void ApplyPureBake()
    {
        Undo.RecordObject(itemsContainer, "Bake Layout");
        
        // Ensure Layout is fresh before baking
        Canvas.ForceUpdateCanvases();
        
        // Just find all children and bake them to their CURRENT parents
        RectTransform[] children = itemsContainer.GetComponentsInChildren<RectTransform>(true);
        ConvertListToAnchors(children);
        if (itemsContainer == null)
        {
            Debug.LogError("[ResponsiveTool] Hedef Kutu seçilmeli!");
            return;
        }

        Debug.Log($"[ResponsiveTool] Tek Tuşla Kilitleme Başlıyor: {itemsContainer.name}");
        Canvas.ForceUpdateCanvases();

        // 1. Get ALL RectTransforms
        var allChildren = itemsContainer.GetComponentsInChildren<RectTransform>(true);
        
        // 2. Filter out the Root itself and non-children
        var listToBake = new System.Collections.Generic.List<RectTransform>();
        foreach (var rt in allChildren)
        {
            // CRITICAL FIX: Do NOT bake the container itself.
            // If we bake 'Content', it shrinks to Viewport size, crushing all rooms.
            if (rt == itemsContainer) continue;
            
            listToBake.Add(rt);
        }

        var sortedChildren = listToBake.ToArray();
        // ConvertListToAnchors is not needed if we iterate sequentially, 
        // but let's stick to the direct loop for simplicity and reliability.
        // It's better to Bake Parent First? or Child First?
        // Since we rely on Parent Rect size, we MUST ensure Parent is correct.
        // If we don't bake Root, then Root is correct (User set it).
        // For children of children, we should iterate top-down.
        // GetComponentsInChildren returns Top-Down (Parent then Child). This is perfect.

        Debug.Log($"[ResponsiveTool] {sortedChildren.Length} alt obje kilitleniyor...");

        foreach (var child in sortedChildren)
        {
            BakeSingleAnchor(child);
        }

        Debug.Log("[ResponsiveTool] İşlem Tamamlandı! ✨");
    }

    private void BakeSelectedAnchors()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("[ResponsiveTool] Hiçbir obje seçilmedi!");
            return;
        }

        Debug.Log($"[ResponsiveTool] Seçili {Selection.gameObjects.Length} obje işleniyor...");
        Canvas.ForceUpdateCanvases();
        
        foreach (var go in Selection.gameObjects)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null) // Removed '&& rt.parent == itemsContainer' check
            { 
                 // We bake it relative to its OWN parent.
                 BakeSingleAnchor(rt);
            }
            else
            {
                Debug.LogWarning($"[Skip] '{go.name}' bir RectTransform değil.");
            }
        }
    }

    private void ConvertListToAnchors(RectTransform[] list)
    {
        foreach (var child in list)
        {
            if (child == itemsContainer) continue;
            if (child.GetComponent<Canvas>() != null) continue;
            BakeSingleAnchor(child);
        }
    }

    private void BakeSingleAnchor(RectTransform child)
    {
        Undo.RecordObject(child, "Bake Anchor");
        
        RectTransform parent = child.parent as RectTransform;
        if (parent == null) return;

        // Cleanup
        var s1 = child.GetComponent("StablePositionOnce") as MonoBehaviour;
        if (s1 != null) Undo.DestroyObjectImmediate(s1);
        var s2 = child.GetComponent("StablePosition") as MonoBehaviour;
        if (s2 != null) Undo.DestroyObjectImmediate(s2);

        // INTENT CHECK: Is it already trying to stretch?
        // If anchors are separated, it's a "Stretch" object. We should preserve that intent.
        bool isAlreadyStretched = (Mathf.Abs(child.anchorMin.x - child.anchorMax.x) > 0.01f) || 
                                  (Mathf.Abs(child.anchorMin.y - child.anchorMax.y) > 0.01f);

        // ROTATION CHECK
        float zRot = child.localRotation.eulerAngles.z;
        if (zRot > 180) zRot -= 360;
        
        // If it's already stretched, we ignore "minor" rotations and force Stretch Mode.
        // Only treat as "Rotated Point" if it is NOT stretched AND has significant rotation.
        bool isRotated = Mathf.Abs(zRot) > 1.0f && !isAlreadyStretched; 

        // Micro-rotation fix (Snap to 0 if almost 0, regardless of mode)
        if (Mathf.Abs(zRot) > 0.0001f && Mathf.Abs(zRot) < 1.0f)
        {
            Vector3 euler = child.localRotation.eulerAngles;
            euler.z = 0;
            child.localRotation = Quaternion.Euler(euler);
        }

        // PRESERVE Z (Depth)
        float originalZ = child.localPosition.z;

        if (isRotated)
        {
            // --- MODE A: ROTATED ITEM (Point Anchor) ---
            Debug.Log($"[Bake] '{child.name}' treated as ROTATED (Point).");
            
            Vector3 worldPivot = child.position;
            Vector2 localPosInParent = parent.InverseTransformPoint(worldPivot);
            Vector2 normalizedPivot = Rect.PointToNormalized(parent.rect, localPosInParent);
            
            if (float.IsNaN(normalizedPivot.x)) return;

            // Pin min/max to the same point (Pivot)
            child.anchorMin = normalizedPivot;
            child.anchorMax = normalizedPivot;
            
            child.anchoredPosition = Vector2.zero;
            
            // Restore Z
            Vector3 finalPos = child.localPosition;
            finalPos.z = originalZ;
            child.localPosition = finalPos;
        }
        else
        {
            // --- MODE B: AXIS-ALIGNED ITEM (Stretch Anchor) ---
            // This path is now preferred for Walls/Panels
            
            // CRITICAL FIX: If we treat it as Stretch, we MUST enforce 0 rotation.
            // Otherwise, we calculate an AABB, and then Rotate the AABB, causing a shift.
            child.localRotation = Quaternion.identity;
            
            // LOG BEFORE
            string logBefore = $"[BEFORE] Pos: {child.localPosition}, Size: {child.rect.size}, Anchors: {child.anchorMin}-{child.anchorMax}, Offsets: {child.offsetMin}-{child.offsetMax}, Pivot: {child.pivot}, Scale: {child.localScale}";

            // 1. Capture Original World Corners (The Truth)
            Vector3[] worldCorners = new Vector3[4];
            child.GetWorldCorners(worldCorners);
            
            // 2. Calculate Visual Bounds in Parent Space
            Vector2 corner0 = parent.InverseTransformPoint(worldCorners[0]); 
            Vector2 corner2 = parent.InverseTransformPoint(worldCorners[2]);
            
            // Safety: Ensure Min is Min and Max is Max (Handle rotation flipping)
            Vector2 visualMin = Vector2.Min(corner0, corner2);
            Vector2 visualMax = Vector2.Max(corner0, corner2);
            Vector2 visualSize = visualMax - visualMin;
            
            // 3. Pivot-Relative Expansion (Inverse Scale Compensation)
            // We must expand the rect AROUND THE PIVOT to ensure the Visual Bounds match.
            // VisualMin = PivotPos - (DistLeft * Scale)
            // RectMin   = PivotPos - DistLeft
            // So: RectMin = PivotPos - ((PivotPos - VisualMin) / Scale)
            
            Vector2 pivotLocal = (Vector2)parent.InverseTransformPoint(child.position); // Pivot is typically at child.position
            
            Vector2 distMin = pivotLocal - visualMin; // Distance from Pivot to Left/Bottom visual edge
            Vector2 distMax = visualMax - pivotLocal; // Distance from Pivot to Right/Top visual edge
            
            Vector3 scale = child.localScale;
            // Avoid divide by zero / negative scale checks
            if (Mathf.Abs(scale.x) < 0.001f) scale.x = 0.001f;
            if (Mathf.Abs(scale.y) < 0.001f) scale.y = 0.001f;
            
            Vector2 requiredDistMin = new Vector2(distMin.x / scale.x, distMin.y / scale.y);
            Vector2 requiredDistMax = new Vector2(distMax.x / scale.x, distMax.y / scale.y);
            
            // 4. Calculate New Local Min/Max (Unscaled Rect)
            Vector2 newMinLocal = pivotLocal - requiredDistMin;
            Vector2 newMaxLocal = pivotLocal + requiredDistMax;

            // 5. Convert to Anchors
            // 5. Convert to Anchors (Manual Calc to avoid clamping)
            // Rect.PointToNormalized clamps to 0-1, which breaks scroll content > parent.
            float pWidth = parent.rect.width;
            float pHeight = parent.rect.height;

            float anchorMinX = (Mathf.Abs(pWidth) > 0.001f) ? (newMinLocal.x - parent.rect.x) / pWidth : 0.5f;
            float anchorMinY = (Mathf.Abs(pHeight) > 0.001f) ? (newMinLocal.y - parent.rect.y) / pHeight : 0.5f;
            
            float anchorMaxX = (Mathf.Abs(pWidth) > 0.001f) ? (newMaxLocal.x - parent.rect.x) / pWidth : 0.5f;
            float anchorMaxY = (Mathf.Abs(pHeight) > 0.001f) ? (newMaxLocal.y - parent.rect.y) / pHeight : 0.5f;

            // 6. Smart Rounding (Prevent micro-decimals, preserve logic)
            Vector2 minAnchor = new Vector2(SmartRound(anchorMinX), SmartRound(anchorMinY));
            Vector2 maxAnchor = new Vector2(SmartRound(anchorMaxX), SmartRound(anchorMaxY));

            Debug.Log($"[Math] Pivot:{pivotLocal}, Scale:{scale} -> NewMin:{newMinLocal} => Anchors:{minAnchor}-{maxAnchor}");

            if (float.IsNaN(minAnchor.x)) return;

            child.anchorMin = minAnchor;
            child.anchorMax = maxAnchor;
            child.offsetMin = Vector2.zero;
            child.offsetMax = Vector2.zero;
            
            // 7. Restore Z
            Vector3 finalPos = child.localPosition;
            finalPos.z = originalZ;
            child.localPosition = finalPos;
            
            // LOG AFTER
            string logAfter = $"[AFTER] Pos: {child.localPosition}, Size: {child.rect.size}, Anchors: {child.anchorMin}-{child.anchorMax}, Offsets: {child.offsetMin}-{child.offsetMax}";
            Debug.Log($"Bake Report '{child.name}':\n{logBefore}\n{logAfter}");
        }
    }

    private float SmartRound(float val)
    {
        // Candidates: 0, 0.5, 1.0, and maybe integers?
        // If val is -0.001, we want 0.
        // If val is -1.001, we want -1.
        // If val is 0.499, we want 0.5.
        
        float[] candidates = { 0f, 0.5f, 1f };
        float threshold = 0.02f; // tight threshold

        // check standard candidates 0-1
        foreach (float c in candidates)
        {
            if (Mathf.Abs(val - c) < threshold) return c;
        }

        // check integer snapping (for tiling or outside content)
        float rounded = Mathf.Round(val);
        if (Mathf.Abs(val - rounded) < threshold) return rounded;

        // Otherwise return exact
        return val;
    }
}
