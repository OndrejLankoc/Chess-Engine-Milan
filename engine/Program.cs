namespace Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board = new Board();
            board.setupBoard();

            PieceColor playerColor = PieceColor.White;

            List<Move> playedMoves = new List<Move>();
            List<MoveInfo> playedMovesInfo = new List<MoveInfo>();

            while (!board.IsGameOver(playedMovesInfo, playedMoves))
            {
                if (board.sideToMove == playerColor)
                {
                    Move move;
                    while (true)
                    {
                        Console.WriteLine("Enter your move");
                        string input = Console.ReadLine();
                        if (!Move.TryParse(board.sideToMove, input, out move))
                        {
                            Console.WriteLine("Invalid move format. Please try again.");
                            continue;
                        }
                        if (!board.IsMoveLegal(move))
                        {
                            Console.WriteLine("Illegal move. Please try again.");
                            continue;
                        }
                        break;
                    }

                    playedMovesInfo.Add(board.MakeMove(move));
                    playedMoves.Add(move);
                }

                else
                {
                    Console.WriteLine("Computer is thinking...");

                    Move bestMove;
                    board.Search(4, out bestMove);

                    playedMovesInfo.Add(board.MakeMove(bestMove));
                    playedMoves.Add(bestMove);

                    Console.WriteLine($"{bestMove.ToString()}");
                }
            }
            Console.WriteLine("GG");
        }
    }
}