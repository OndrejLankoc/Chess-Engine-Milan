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
    }
}