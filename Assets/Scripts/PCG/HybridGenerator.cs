using System.Collections.Generic;
using UnityEngine;

/**
 * Домашнее задание
Добавить в вашу игру генератор уровня
Цель: Реализовать генератор уровней в проекте для закрепления навыков работы с генератором уровней
1. Скачайте проект из материалов к занятию
2. Создайте новый класс генератора уровней, объединяющий возможности блочного и клеточного генераторов. Половина блоков в сгенерированном уровне должны быть
выбраны из заготовленных заранее, тогда как другая половина должна быть сгенерирована с помощью клеточного автомата.
3. Уделите внимание правильной стыковке блоков (не должно быть соединений вида "проход в стену")
4. Добавьте в генератор поддержку объектов в предзаготовленных блоках. Сделайте так, чтобы в некоторых предзаготовленных блоках были NPC и платформы.
Критерии оценки: 2 балла - создан генератор - гибрид блочного и клеточного
1 балл - блоки в сгенерированном уровне стыкуются правильно
1 балл - добавлена поддержка NPC в предзаготовленных блоках
1 балл - добавлена поддержка платформ в предазготовленных блоках
 */
public class HybridGenerator : AbstractGenerator
{
    int widthInBlocks;
    int heightInBlocks;
    Block[] blocks;
    Dictionary<Vector2Int, int> bitmasks;

    public Transform character;
    public Transform generatedBlocks;
    public Transform generatedElements;

    void PutBlock(int blockX, int blockY, Block block) {
        Map blockMap = block.GetMap();
        map.PutMap(blockX * Block.Width, blockY * Block.Height, blockMap, blockMap.rect);
        block.SetGameObjects(tilemap, blockX * Block.Width, blockY * Block.Height, generatedElements);
    }

    void SetPosition(Transform transform, int blockX, int blockY, Vector3 posInBlock) {
        int tileX = blockX * Block.Width;
        int tileY = blockY * Block.Height;
        var tilePos = tilemap.layoutGrid.CellToWorld(new Vector3Int(tileX, tileY, 0));
        transform.position = tilePos + posInBlock;
    }

    static List<Block> GetBlocksMatchingMask(Block[] blocks, int requirePresent, int requireAbsent) {
        var result = new List<Block>();
        foreach (var block in blocks) {
            int bitmask = block.GetBitmask();
            if ((bitmask & requirePresent) == requirePresent && (bitmask & requireAbsent) == 0)
                result.Add(block);
        }

        return result;
    }

    /**
     * Looking for a Block that might fulfill the passes identified
     * Block might be chosen from predefined or generated dynamically
     */
    Block GetRandomBlockMatchingMask(Block[] blocks, int requirePresent, int requireAbsent) {
        var rand = Random.Range(0, 2);
        if (rand == 1) {
            GameObject gameObject = new GameObject("GeneratedBlock");
            var block = gameObject.AddComponent<GeneratedBlock>();
            block.transform.parent = generatedBlocks;
            block.SetBitmask(requirePresent);
            return block;
        }
        else {
            return GetRandomPredefinedBlockMatchingMask(blocks, requirePresent, requireAbsent);
        }
    }

    static Block GetRandomPredefinedBlockMatchingMask(Block[] blocks, int requirePresent, int requireAbsent) {
        var list = GetBlocksMatchingMask(blocks, requirePresent, requireAbsent);
        return list[Random.Range(0, list.Count)];
    }

    int GetRequireAbsentMask(Vector2Int pos) {
        int requireAbsent = 0;
        if (pos.x == 0)
            requireAbsent |= Block.BitmaskLeft;
        else if (pos.x == widthInBlocks - 1)
            requireAbsent |= Block.BitmaskRight;
        if (pos.y == 0)
            requireAbsent |= Block.BitmaskDown;
        else if (pos.y == heightInBlocks - 1)
            requireAbsent |= Block.BitmaskUp;
        return requireAbsent;
    }

    void SpawnNode(Vector2Int pos) {
        int requirePresent = 0;
        int requireAbsent = GetRequireAbsentMask(pos);
        int bitmask;

        List<int> empty = new List<int>();

        if (bitmasks.TryGetValue(new Vector2Int(pos.x - 1, pos.y), out bitmask)) {
            if ((bitmask & Block.BitmaskRight) != 0)
                requirePresent |= Block.BitmaskLeft;
            else
                requireAbsent |= Block.BitmaskLeft;
        }
        else if ((requireAbsent & Block.BitmaskLeft) == 0)
            empty.Add(Block.BitmaskLeft);

        if (bitmasks.TryGetValue(new Vector2Int(pos.x + 1, pos.y), out bitmask)) {
            if ((bitmask & Block.BitmaskLeft) != 0)
                requirePresent |= Block.BitmaskRight;
            else
                requireAbsent |= Block.BitmaskRight;
        }
        else if ((requireAbsent & Block.BitmaskRight) == 0)
            empty.Add(Block.BitmaskRight);

        if (bitmasks.TryGetValue(new Vector2Int(pos.x, pos.y - 1), out bitmask)) {
            if ((bitmask & Block.BitmaskUp) != 0)
                requirePresent |= Block.BitmaskDown;
            else
                requireAbsent |= Block.BitmaskDown;
        }
        else if ((requireAbsent & Block.BitmaskDown) == 0)
            empty.Add(Block.BitmaskDown);

        if (bitmasks.TryGetValue(new Vector2Int(pos.x, pos.y + 1), out bitmask)) {
            if ((bitmask & Block.BitmaskDown) != 0)
                requirePresent |= Block.BitmaskUp;
            else
                requireAbsent |= Block.BitmaskUp;
        }
        else if ((requireAbsent & Block.BitmaskUp) == 0)
            empty.Add(Block.BitmaskUp);

        if (empty.Count > 0)
            requirePresent |= empty[Random.Range(0, empty.Count)];

        Block block = GetRandomBlockMatchingMask(blocks, requirePresent, requireAbsent);
        PutBlock(pos.x, pos.y, block);

        bitmasks[pos] = block.GetBitmask();
    }

    protected override void GeneratorImpl() {
        map.Fill(map.rect, false);

        blocks = FindObjectsOfType<PredefinedBlock>();

        widthInBlocks = width / Block.Width;
        heightInBlocks = height / Block.Height;

        int startX = Random.Range(0, widthInBlocks);
        int startY = Random.Range(0, heightInBlocks);
        Block startBlock =
            GetRandomPredefinedBlockMatchingMask(blocks, 0, GetRequireAbsentMask(new Vector2Int(startX, startY)));
        PutBlock(startX, startY, startBlock);

        SetPosition(character, startX, startY, startBlock.GetPlayerPosition());

        bitmasks = new Dictionary<Vector2Int, int>();
        bitmasks[new Vector2Int(startX, startY)] = startBlock.GetBitmask();

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(startX, startY));
        do {
            Vector2Int pos = stack.Pop();
            int bitmask = bitmasks[pos];

            if ((bitmask & Block.BitmaskLeft) != 0 && !bitmasks.ContainsKey(new Vector2Int(pos.x - 1, pos.y))) {
                stack.Push(new Vector2Int(pos.x - 1, pos.y));
                SpawnNode(new Vector2Int(pos.x - 1, pos.y));
            }

            if ((bitmask & Block.BitmaskRight) != 0 && !bitmasks.ContainsKey(new Vector2Int(pos.x + 1, pos.y))) {
                stack.Push(new Vector2Int(pos.x + 1, pos.y));
                SpawnNode(new Vector2Int(pos.x + 1, pos.y));
            }

            if ((bitmask & Block.BitmaskDown) != 0 && !bitmasks.ContainsKey(new Vector2Int(pos.x, pos.y - 1))) {
                stack.Push(new Vector2Int(pos.x, pos.y - 1));
                SpawnNode(new Vector2Int(pos.x, pos.y - 1));
            }

            if ((bitmask & Block.BitmaskUp) != 0 && !bitmasks.ContainsKey(new Vector2Int(pos.x, pos.y + 1))) {
                stack.Push(new Vector2Int(pos.x, pos.y + 1));
                SpawnNode(new Vector2Int(pos.x, pos.y + 1));
            }
        } while (stack.Count > 0);
    }
}