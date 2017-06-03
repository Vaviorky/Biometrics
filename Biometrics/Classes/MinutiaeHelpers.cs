using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biometrics.Classes
{
    public static class MinutiaeHelpers
    {

        public static int[] GetArrayOfBlacksSquare3(int x, int y, int[,] intPixels)
        {
            int[] blacks = new int[9];

            //if it's stupid but it works, it ain't stupid
            blacks[0] = intPixels[x - 1, y - 1];
            blacks[1] = intPixels[x, y - 1];
            blacks[2] = intPixels[x + 1, y - 1];
            blacks[3] = intPixels[x + 1, y];
            blacks[4] = intPixels[x + 1, y + 1];
            blacks[5] = intPixels[x, y + 1];
            blacks[6] = intPixels[x - 1, y + 1];
            blacks[7] = intPixels[x - 1, y];
            blacks[8] = intPixels[x - 1, y - 1];

            return blacks;
        }

        public static int[] GetArrayOfBlacksSquare5(int x, int y, int[,] intPixels)
        {
            int[] blacks = new int[17];

            //if it's stupid but it works, it ain't stupid
            blacks[0] = intPixels[x - 2, y - 2];
            blacks[1] = intPixels[x - 1, y - 2];
            blacks[2] = intPixels[x, y - 2];
            blacks[3] = intPixels[x + 1, y - 2];
            blacks[4] = intPixels[x + 2, y - 2];
            blacks[5] = intPixels[x + 2, y - 1];
            blacks[6] = intPixels[x + 2, y];
            blacks[7] = intPixels[x + 2, y + 1];
            blacks[8] = intPixels[x + 2, y + 2];
            blacks[9] = intPixels[x + 1, y + 2];
            blacks[10] = intPixels[x, y + 2];
            blacks[11] = intPixels[x - 1, y + 2];
            blacks[12] = intPixels[x - 2, y + 2];
            blacks[13] = intPixels[x - 2, y + 1];
            blacks[14] = intPixels[x - 2, y];
            blacks[15] = intPixels[x - 2, y - 1];
            blacks[16] = intPixels[x - 2, y - 2];

            return blacks;
        }

        public static int[] GetArrayOfBlacksSquare9(int x, int y, int[,] intPixels)
        {
            int[] blacks = new int[33];

            //if it's stupid but it works, it ain't stupid
            blacks[0] = intPixels[x - 4, y - 4];
            blacks[1] = intPixels[x - 3, y - 4];
            blacks[2] = intPixels[x - 2, y - 4];
            blacks[3] = intPixels[x - 1, y - 4];
            blacks[4] = intPixels[x, y - 4];
            blacks[5] = intPixels[x + 1, y - 4];
            blacks[6] = intPixels[x + 2, y - 4];
            blacks[7] = intPixels[x + 3, y - 4];
            blacks[8] = intPixels[x + 4, y - 4];
            blacks[9] = intPixels[x + 4, y - 3];
            blacks[10] = intPixels[x + 4, y - 2];
            blacks[11] = intPixels[x + 4, y - 1];
            blacks[12] = intPixels[x + 4, y];
            blacks[13] = intPixels[x + 4, y + 1];
            blacks[14] = intPixels[x + 4, y + 2];
            blacks[15] = intPixels[x + 4, y + 3];
            blacks[16] = intPixels[x + 4, y + 4];
            blacks[17] = intPixels[x + 3, y + 4];
            blacks[18] = intPixels[x + 2, y + 4];
            blacks[19] = intPixels[x + 1, y + 4];
            blacks[20] = intPixels[x, y + 4];
            blacks[21] = intPixels[x - 1, y + 4];
            blacks[22] = intPixels[x - 2, y + 4];
            blacks[23] = intPixels[x - 3, y + 4];
            blacks[24] = intPixels[x - 4, y + 4];
            blacks[25] = intPixels[x - 4, y + 3];
            blacks[26] = intPixels[x - 4, y + 2];
            blacks[27] = intPixels[x - 4, y + 1];
            blacks[28] = intPixels[x - 4, y];
            blacks[29] = intPixels[x - 4, y - 1];
            blacks[30] = intPixels[x - 4, y - 2];
            blacks[31] = intPixels[x - 4, y - 3];
            blacks[32] = intPixels[x - 4, y - 4];

            return blacks;
        }


    }
}
