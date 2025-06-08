namespace Engine
{
    public class Square
    {
        public int File { get; set; }
        public int Rank { get; set; }
        public Square(int rank, int file)
        {
            File = file;
            Rank = rank;
        }

        public bool IsOnBoard()
        {
            return File >= 0 && File < 8 && Rank >= 0 && Rank < 8;
        }
    }
}