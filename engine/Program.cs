namespace Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            Uci uci = new Uci();
            uci.Run();

            // Board board = new Board();
            // board.SetupBoard();
            // Engine engine = new Engine();

            // PieceColor playerColor = PieceColor.White;

            // List<Move> playedMoves = new List<Move>();
            // List<MoveInfo> playedMovesInfo = new List<MoveInfo>();

            // while (board.Result(playedMovesInfo, playedMoves, out _) == GameResult.Ongoing)
            // {
            //     if (board.SideToMove == playerColor)
            //     {
            //         Move move;
            //         while (true)
            //         {
            //             Console.WriteLine("Enter your move");
            //             string input = Console.ReadLine();
            //             if (!Move.TryParse(board.SideToMove, input, out move))
            //             {
            //                 Console.WriteLine("Invalid move format. Please try again.");
            //                 continue;
            //             }
            //             if (!board.IsMoveLegal(move))
            //             {
            //                 Console.WriteLine("Illegal move. Please try again.");
            //                 continue;
            //             }
            //             break;
            //         }

            //         playedMovesInfo.Add(board.MakeMove(move));
            //         playedMoves.Add(move);
            //     }

            //     else
            //     {
            //         Console.WriteLine("Computer is thinking...");

            //         engine.Search(board, 4, out Move bestMove, playedMoves, playedMovesInfo);

            //         playedMovesInfo.Add(board.MakeMove(bestMove));
            //         playedMoves.Add(bestMove);

            //         engine.ClearOldData(board.FullMoveNumber);

            //         Console.WriteLine($"{bestMove.ToString()}");
            //     }
            // }
            // GameResult result = board.Result(playedMovesInfo, playedMoves, out string reason);
            // Console.WriteLine($"GG\n{reason} - {result}");
        }
    }
}