using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredefinedBlockGenerator : AbstractGenerator
{
    int widthInBlocks;
    int heightInBlocks;
    PredefinedBlock[] blocks;
    Dictionary<Vector2Int, int> bitmasks;

    public Transform character;

    void PutBlock(int blockX, int blockY, PredefinedBlock block) {
        Map blockMap = block.GetMap();
        map.PutMap(blockX * PredefinedBlock.Width, blockY * PredefinedBlock.Height, blockMap, blockMap.rect);
    }

    void SetPosition(Transform transform, int blockX, int blockY, Vector3 posInBlock) {
        int tileX = blockX * PredefinedBlock.Width;
        int tileY = blockY * PredefinedBlock.Height;
        var tilePos = tilemap.layoutGrid.CellToWorld(new Vector3Int(tileX, tileY, 0));
        transform.position = tilePos + posInBlock;
    }

    static List<PredefinedBlock>
        GetBlocksMatchingMask(PredefinedBlock[] blocks, int requirePresent, int requireAbsent) {
        var result = new List<PredefinedBlock>();
        foreach (var block in blocks) {
            int bitmask = block.GetBitmask();
            if ((bitmask & requirePresent) == requirePresent && (bitmask & requireAbsent) == 0)
                result.Add(block);
        }

        return result;
    }

    static PredefinedBlock GetRandomBlockMatchingMask(PredefinedBlock[] blocks, int requirePresent, int requireAbsent) {
        var list = GetBlocksMatchingMask(blocks, requirePresent, requireAbsent);
        return list[Random.Range(0, list.Count)];
    }

    int GetRequireAbsentMask(Vector2Int pos) {
        int requireAbsent = 0;
        if (pos.x == 0)
            requireAbsent |= PredefinedBlock.BitmaskLeft;
        else if (pos.x == widthInBlocks - 1)
            requireAbsent |= PredefinedBlock.BitmaskRight;
        if (pos.y == 0)
            requireAbsent |= PredefinedBlock.BitmaskDown;
        else if (pos.y == heightInBlocks - 1)
            requireAbsent |= PredefinedBlock.BitmaskUp;
        return requireAbsent;
    }

    void SpawnNode(Vector2Int pos) {
        int requirePresent = 0;
        int requireAbsent = GetRequireAbsentMask(pos);
        int bitmask;

        List<int> empty = new List<int>();

        if (bitmasks.TryGetValue(new Vector2Int(pos.x - 1, pos.y), out bitmask)) {
            if ((bitmask & PredefinedBlock.BitmaskRight) != 0)
                requirePresent |= PredefinedBlock.BitmaskLeft;
            else
                requireAbsent |= PredefinedBlock.BitmaskLeft;
        }
        else if ((requireAbsent & PredefinedBlock.BitmaskLeft) == 0)
            empty.Add(PredefinedBlock.BitmaskLeft);

        if (bitmasks.TryGetValue(new Vector2Int(pos.x + 1, pos.y), out bitmask)) {
            if ((bitmask & PredefinedBlock.BitmaskLeft) != 0)
                requirePresent |= PredefinedBlock.BitmaskRight;
            else
                requireAbsent |= PredefinedBlock.BitmaskRight;
        }
        else if ((requireAbsent & PredefinedBlock.BitmaskRight) == 0)
            empty.Add(PredefinedBlock.BitmaskRight);

        if (bitmasks.TryGetValue(new Vector2Int(pos.x, pos.y - 1), out bitmask)) {
            if ((bitmask & PredefinedBlock.BitmaskUp) != 0)
                requirePresent |= PredefinedBlock.BitmaskDown;
            else
                requireAbsent |= PredefinedBlock.BitmaskDown;
        }
        else if ((requireAbsent & PredefinedBlock.BitmaskDown) == 0)
            empty.Add(PredefinedBlock.BitmaskDown);

        if (bitmasks.TryGetValue(new Vector2Int(pos.x, pos.y + 1), out bitmask)) {
            if ((bitmask & PredefinedBlock.BitmaskDown) != 0)
                requirePresent |= PredefinedBlock.BitmaskUp;
            else
                requireAbsent |= PredefinedBlock.BitmaskUp;
        }
        else if ((requireAbsent & PredefinedBlock.BitmaskUp) == 0)
            empty.Add(PredefinedBlock.BitmaskUp);

        if (empty.Count > 0)
            requirePresent |= empty[Random.Range(0, empty.Count)];


        PredefinedBlock block = GetRandomBlockMatchingMask(blocks, requirePresent, requireAbsent);
        PutBlock(pos.x, pos.y, block);
        bitmasks[pos] = block.GetBitmask();
    }

    protected override void GeneratorImpl() {
        map.Fill(map.rect, false);

        // TODO: use prefabs instead
        blocks = FindObjectsOfType<PredefinedBlock>();

        widthInBlocks = width / PredefinedBlock.Width;
        heightInBlocks = height / PredefinedBlock.Height;

        int startX = Random.Range(0, widthInBlocks);
        int startY = Random.Range(0, heightInBlocks);
        PredefinedBlock startBlock =
            GetRandomBlockMatchingMask(blocks, 0, GetRequireAbsentMask(new Vector2Int(startX, startY)));
        PutBlock(startX, startY, startBlock);

        SetPosition(character, startX, startY, startBlock.GetPlayerPosition());

        bitmasks = new Dictionary<Vector2Int, int>();
        bitmasks[new Vector2Int(startX, startY)] = startBlock.GetBitmask();

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(startX, startY));
        do {
            Vector2Int pos = stack.Pop();


            int bitmask = bitmasks[pos];

            if ((bitmask & PredefinedBlock.BitmaskLeft) != 0 &&
                !bitmasks.ContainsKey(new Vector2Int(pos.x - 1, pos.y))) {
                stack.Push(new Vector2Int(pos.x - 1, pos.y));
                SpawnNode(new Vector2Int(pos.x - 1, pos.y));
            }

            if ((bitmask & PredefinedBlock.BitmaskRight) != 0 &&
                !bitmasks.ContainsKey(new Vector2Int(pos.x + 1, pos.y))) {
                stack.Push(new Vector2Int(pos.x + 1, pos.y));
                SpawnNode(new Vector2Int(pos.x + 1, pos.y));
            }

            if ((bitmask & PredefinedBlock.BitmaskDown) != 0 &&
                !bitmasks.ContainsKey(new Vector2Int(pos.x, pos.y - 1))) {
                stack.Push(new Vector2Int(pos.x, pos.y - 1));
                SpawnNode(new Vector2Int(pos.x, pos.y - 1));
            }

            if ((bitmask & PredefinedBlock.BitmaskUp) != 0 &&
                !bitmasks.ContainsKey(new Vector2Int(pos.x, pos.y + 1))) {
                stack.Push(new Vector2Int(pos.x, pos.y + 1));
                SpawnNode(new Vector2Int(pos.x, pos.y + 1));
            }
        } while (stack.Count > 0);
    }
}