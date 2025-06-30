namespace Engine
{
    public class Square
    {
        public int Rank { get; set; }
        public int File { get; set; }
        public Square(int rank, int file)
        {
            Rank = rank;
            File = file;
        }

        public bool IsOnBoard()
        {
            return File >= 0 && File < 8 && Rank >= 0 && Rank < 8;
        }

        public static bool TryParse(string squareString, out Square square)
        {
            if (squareString.Length != 2)
            {
                square = null!;
                return false;
            }

            int file = squareString[0] - 'a';
            int rank = '8' - squareString[1];
            square = new Square(rank, file);

            if (!square.IsOnBoard())
            {
                square = null!;
                return false;
            }
            return true;
        }
    }
}