using UnityEngine;

public class GeneratedBlock: Block
{
    private int leftBit = 1;
    private int rightBit = 2;
    private int topBit = 4;
    private int bottomBit = 8;
    
    public void SetBitmask(int requiredMask) {
        bitMask = requiredMask;
        if ((leftBit & requiredMask) == leftBit)
            left = true;
        // else
            // left = Random.Range(0, 10) > 7 ? true : false;
        if ((rightBit & requiredMask) == rightBit)
            right = true;
        // else
            // right = Random.Range(0, 10) > 7 ? true : false;
        if ((topBit & requiredMask) == topBit)
            up = true;
        // else 
            // up = Random.Range(0, 10) > 7 ? true : false;
        if ((bottomBit & requiredMask) == bottomBit)
            down = true;
        // else
            // down = Random.Range(0, 10) > 7 ? true : false;
    }
    
    public override Map GetMap() {
        var result = new Map(Width, Height);
        // based on given possible mask generate what kind of passes we want to generate
        

        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                // have to generate bricks taking into account the passes we have
                if (y == 0 && up && x > 3 && x < 6) {
                    result.SetWall(x, y, false);
                    continue;
                }

                if (y == Height - 1 && down && x > 3 && x < 6) {
                    result.SetWall(x, y, false);
                    continue;
                }

                if (x == 0 && left && y > 3 && y < 6) {
                    result.SetWall(x, y, false);
                    continue;
                }

                if (x == Width - 1 && right && y > 3 && y > 6) {
                    result.SetWall(x, y, false);
                    continue;
                }

                if ((!up && y == Height - 1) || (!down && y == 0) || (!left && x == 0) || (!right && x == Width - 1)) {
                    result.SetWall(x, y, true);
                }
                else {
                    // rand to put the wall
                    result.SetWall(x, y, Random.Range(0, 7) > 5);
                }
            }
        }

        return result;
    }
}