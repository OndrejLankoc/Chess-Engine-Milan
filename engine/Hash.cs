namespace Engine
{
    static class Hash
    {
        public static ulong[,] PieceSquare = new ulong[12, 64];
        public static ulong SideToMove;
        public static ulong[] CastlingRights = new ulong[4];
        public static ulong[] EnPassantFile = new ulong[8];

        static Random _rng = new Random(314159);

        static Hash()
        {
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    PieceSquare[i, j] = (ulong)_rng.Next() << 32 | (ulong)_rng.Next();
                }
            }

            SideToMove = (ulong)_rng.Next() << 32 | (ulong)_rng.Next();

            for (int i = 0; i < 4; i++)
            {
                CastlingRights[i] = (ulong)_rng.Next() << 32 | (ulong)_rng.Next();
            }

            for (int i = 0; i < 8; i++)
            {
                EnPassantFile[i] = (ulong)_rng.Next() << 32 | (ulong)_rng.Next();
            }
        }
    }
}