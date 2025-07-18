namespace Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board = new Board();
            board.SetupBoard("rnbqkbnr/pppp1ppp/4p3/8/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2");

            PieceColor playerColor = PieceColor.White;

            List<Move> playedMoves = new List<Move>();
            List<MoveInfo> playedMovesInfo = new List<MoveInfo>();

            while (board.Result(playedMovesInfo, playedMoves, out _) == GameResult.Ongoing)
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
                    board.Search(4, out bestMove, playedMoves, playedMovesInfo);

                    playedMovesInfo.Add(board.MakeMove(bestMove));
                    playedMoves.Add(bestMove);

                    Console.WriteLine($"{bestMove.ToString()}");
                }
            }
            GameResult result = board.Result(playedMovesInfo, playedMoves, out string reason);
            Console.WriteLine($"{result} - {reason}");
        }
    }
}