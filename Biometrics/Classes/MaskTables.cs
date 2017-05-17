namespace Biometrics.Classes
{
    public static class MaskTables
    {
        public static int[,] Blur =
        {
            {1, 1, 1},
            {1, 1, 1},
            {1, 1, 1}
        };

        public static int[,] PrewittHorizontal =
        {
            {-1, -1, -1},
            {0, 0, 0},
            {1, 1, 1}
        };

        public static int[,] PrewittVertical =
        {
            {-1, 0, 1},
            {-1, 0, 1},
            {-1, 0, 1}
        };

        public static int[,] SobelVertical =
        {
            {-1, 0, 1},
            {-2, 0, 2},
            {-1, 0, 1}
        };

        public static int[,] SobelHorizontal =
        {
            {1, 2, 1},
            {0, 0, 0},
            {-1, -2, -1}
        };

        public static int[,] LaplaceMiddle4 =
        {
            {0, -1, 0},
            {-1, 4, -1},
            {0, -1, 0}
        };

        public static int[,] LaplaceMiddle8 =
        {
            {-1, -1, -1},
            {-1, 8, -1},
            {-1, -1, -1}
        };

        public static int[,] LaplaceDiagonal =
        {
            {-1, 0, -1},
            {0, 4, 0},
            {-1, 0, -1}
        };

        public static int[,] EastCorner =
        {
            {-1, 1, 1},
            {-1, -2, 1},
            {-1, 1, 1}
        };

        public static int[,] WestCorner =
        {
            {-1, -1, 1},
            {-1, -2, 1},
            {1, 1, 1}
        };

        public static int[,] NorthWestCorner =
        {
            {1, 1, 1},
            {1, -2, -1},
            {1, -1, -1}
        };

        public static int[,] SouthEastCorner =
        {
            {1, 1, -1},
            {1, -2, -1},
            {1, 1, -1}
        };
    }
}