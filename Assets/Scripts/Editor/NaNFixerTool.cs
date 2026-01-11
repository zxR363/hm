using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NaNFixerTool : EditorWindow
{
    [MenuItem("Tools/Fix NaNs in Scene")]
    public static void ShowWindow()
    {
        GetWindow<NaNFixerTool>("NaN Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("NaN Fixer Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Scan & Fix All NaNs in Active Scene", GUILayout.Height(40)))
        {
            FixNaNsInScene();
        }
        
        GUILayout.Space(5);
        GUILayout.Label("This will check Position, Rotation, Scale, and RectTransform values.", EditorStyles.helpBox);
    }

    private void FixNaNsInScene()
    {
        int fixedCount = 0;
        int checkedCount = 0;
        
        // Find all root objects in the active scene
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] roots = activeScene.GetRootGameObjects();

        foreach (var root in roots)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                checkedCount++;
                if (CheckAndFixTransform(t))
                {
                    fixedCount++;
                }
            }
        }

        if (fixedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log($"<color=green><b>[NaN Fixer]</b> Fixed {fixedCount} objects with NaN values!</color>");
            EditorUtility.DisplayDialog("NaN Fixer", $"Successfully fixed {fixedCount} corrupted objects.", "OK");
        }
        else
        {
            Debug.Log($"[NaN Fixer] Scan complete. No NaNs found in {checkedCount} objects.");
            EditorUtility.DisplayDialog("NaN Fixer", "No NaN values found in the scene.", "OK");
        }
    }

    private bool CheckAndFixTransform(Transform t)
    {
        bool wasFixed = false;
        string path = GetPath(t);

        // 1. Local Position
        try
        {
            if (IsInvalid(t.localPosition))
            {
                Undo.RecordObject(t, "Fix Invalid Position");
                Debug.LogWarning($"[NaN Fixer] Fixed Position on: {path}");
                t.localPosition = Vector3.zero;
                wasFixed = true;
            }
        }
        catch (System.Exception e) 
        {
             // If reading failed, it's definitely corrupt. Try forcing reset.
             Debug.LogError($"[NaN Fixer] Exception reading Position on {path}: {e.Message}. Forcing reset.");
             try { t.localPosition = Vector3.zero; wasFixed = true; } catch {}
        }

        // 2. Local Rotation
        try
        {
            // Check quaternion components directly to be safe
            if (float.IsNaN(t.localRotation.x) || float.IsInfinity(t.localRotation.x) ||
                float.IsNaN(t.localRotation.y) || float.IsInfinity(t.localRotation.y) ||
                float.IsNaN(t.localRotation.z) || float.IsInfinity(t.localRotation.z) ||
                float.IsNaN(t.localRotation.w) || float.IsInfinity(t.localRotation.w))
            {
                 Undo.RecordObject(t, "Fix Invalid Rotation");
                 t.localRotation = Quaternion.identity;
                 wasFixed = true;
            }
            else if (IsInvalid(t.localEulerAngles))
            {
                 Undo.RecordObject(t, "Fix Invalid Euler");
                 t.localEulerAngles = Vector3.zero;
                 wasFixed = true;
            }
        }
        catch 
        {
             try { t.localRotation = Quaternion.identity; wasFixed = true; } catch {}
        }

        // 3. Local Scale
        try
        {
            if (IsInvalid(t.localScale))
            {
                Undo.RecordObject(t, "Fix Invalid Scale");
                Debug.LogWarning($"[NaN Fixer] Fixed Scale on: {path}");
                t.localScale = Vector3.one;
                wasFixed = true;
            }
        }
        catch
        {
             try { t.localScale = Vector3.one; wasFixed = true; } catch {}
        }

        // 4. RectTransform Specifics
        RectTransform rt = t as RectTransform;
        if (rt != null)
        {
            try
            {
                if (IsInvalid(rt.anchoredPosition))
                {
                    Undo.RecordObject(rt, "Fix Invalid AnchoredPos");
                    rt.anchoredPosition = Vector2.zero;
                    wasFixed = true;
                }
                
                if (IsInvalid(rt.sizeDelta))
                {
                    Undo.RecordObject(rt, "Fix Invalid SizeDelta");
                    rt.sizeDelta = Vector2.zero;
                    wasFixed = true;
                }
                
                if (IsInvalid(rt.anchorMin))
                {
                     Undo.RecordObject(rt, "Fix Invalid AnchorMin");
                     rt.anchorMin = new Vector2(0.5f, 0.5f);
                     wasFixed = true;
                }
                
                if (IsInvalid(rt.anchorMax))
                {
                     Undo.RecordObject(rt, "Fix Invalid AnchorMax");
                     rt.anchorMax = new Vector2(0.5f, 0.5f);
                     wasFixed = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NaN Fixer] RectTransform Error on {path}: {e.Message}");
            }
        }

        return wasFixed;
    }

    private bool IsInvalid(Vector3 v)
    {
        return float.IsNaN(v.x) || float.IsInfinity(v.x) || 
               float.IsNaN(v.y) || float.IsInfinity(v.y) || 
               float.IsNaN(v.z) || float.IsInfinity(v.z);
    }

    private bool IsInvalid(Vector2 v)
    {
        return float.IsNaN(v.x) || float.IsInfinity(v.x) || 
               float.IsNaN(v.y) || float.IsInfinity(v.y);
    }

    private string GetPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetPath(t.parent) + "/" + t.name;
    }
}
