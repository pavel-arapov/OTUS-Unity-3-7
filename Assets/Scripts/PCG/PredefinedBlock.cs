using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PredefinedBlock : Block
{
    public GameObject[] elements;
    
    public override Map GetMap() {
        var result = new Map(Width, Height);
        var tilemap = GetComponentInChildren<Tilemap>();
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                var coord = new Vector3Int(X + x, Y + y, 0);
                result.SetWall(x, y, tilemap.GetTile(coord) != null);
            }
        }

        return result;
    }

    public override void SetGameObjects(Tilemap mainTilemap, int blockX, int blockY, Transform rootObject) {
        var tilemap = GetComponentInChildren<Tilemap>();
        foreach (GameObject t in elements) {
            GameObject go = Instantiate(t);
            var tilePos = mainTilemap.layoutGrid.CellToWorld(new Vector3Int(blockX, blockY, 0));
            go.transform.position =
                tilePos + go.transform.position - new Vector3Int(X, Y, 0);
        }
    }

    public override Vector3 GetPlayerPosition() {
        var tilemap = GetComponentInChildren<Tilemap>();
        return playerStart.position - tilemap.layoutGrid.CellToWorld(new Vector3Int(X, Y, 0));
    }
}
