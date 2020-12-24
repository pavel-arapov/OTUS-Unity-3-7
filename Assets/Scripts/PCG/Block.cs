using UnityEngine;
using UnityEngine.Tilemaps;

public class Block : MonoBehaviour
{
    public const int BitmaskLeft = 1; // 0001
    public const int BitmaskRight = 2; // 0010
    public const int BitmaskUp = 4; // 0100
    public const int BitmaskDown = 8; // 1000

    public bool left;
    public bool right;
    public bool up;
    public bool down;
    public Transform playerStart;
    protected const int X = -5;
    protected const int Y = -3;
    public const int Width = 12;
    public const int Height = 8;

    public int bitMask = 0;

    protected Map blockMap;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public int GetBitmask() {
        int result = 0;
        if (left)
            result |= BitmaskLeft;
        if (right)
            result |= BitmaskRight;
        if (up)
            result |= BitmaskUp;
        if (down)
            result |= BitmaskDown;
        bitMask = result;
        return result;
    }

    public virtual Vector3 GetPlayerPosition() {
        return Vector3.zero;
    }

    public virtual Map GetMap() {
        return null;
    }

    public virtual void SetGameObjects(Tilemap mainTilemap, int blockX, int blockY, Transform rootObject) {
    }
}