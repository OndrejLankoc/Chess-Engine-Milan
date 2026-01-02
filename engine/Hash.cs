namespace Engine
{
    static class Hash
    {
        public static ulong[,] PieceSquare = new ulong[12, 64];
        public static ulong SideToMove;
        public static ulong[] CastlingRights = new ulong[4];
        public static ulong[] EnPassantFile = new ulong[8];
        public static ulong[,] PawnSquare = new ulong[2, 64];

        static Random rng = new Random(314159);

        static Hash()
        {
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    PieceSquare[i, j] = (ulong)rng.Next() << 32 | (ulong)rng.Next();
                }
            }

            SideToMove = (ulong)rng.Next() << 32 | (ulong)rng.Next();

            for (int i = 0; i < 4; i++)
            {
                CastlingRights[i] = (ulong)rng.Next() << 32 | (ulong)rng.Next();
            }

            for (int i = 0; i < 8; i++)
            {
                EnPassantFile[i] = (ulong)rng.Next() << 32 | (ulong)rng.Next();
            }

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    PawnSquare[i, j] = (ulong)rng.Next() << 32 | (ulong)rng.Next();
                }
            }
        }
    }
}