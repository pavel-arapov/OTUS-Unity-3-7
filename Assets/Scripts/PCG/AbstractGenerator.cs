using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class AbstractGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public TileData tileData;

    public int width;
    public int height;

    protected Map map;

    protected abstract void GeneratorImpl();

    protected void Apply()
    {
#if UNITY_EDITOR
        if (!Application.IsPlaying(this)) {
            Undo.RecordObject(tilemap, "Procedural Generation");
            PrefabUtility.RecordPrefabInstancePropertyModifications(tilemap);
        }
#endif

        map.ApplyToTilemap(tilemap, tileData, Vector2Int.zero, map.rect);

#if UNITY_EDITOR
        if (!Application.IsPlaying(this))
            EditorUtility.SetDirty(tilemap);
#endif
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        map = new Map(width, height);
        GeneratorImpl();
        Apply();
    }
}
