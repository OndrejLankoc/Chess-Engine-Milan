namespace Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board = new Board();
            board.setupBoard();

            PieceColor colorToMove = PieceColor.White;
            PieceColor playerColor = PieceColor.White;

            List<Move> playedMoves = new List<Move>();
            List<MoveInfo> playedMovesInfo = new List<MoveInfo>();

            while (!board.IsGameOver(colorToMove, playedMovesInfo, playedMoves))
            {
                if (colorToMove == playerColor)
                {
                    Move move;
                    MoveInfo previousMoveInfo = playedMovesInfo.Count > 0 ? playedMovesInfo[playedMovesInfo.Count - 1] : new MoveInfo();
                    while (true)
                    {
                        Console.WriteLine("Enter your move");
                        string input = Console.ReadLine();
                        if (!Move.TryParse(colorToMove, input, out move))
                        {
                            Console.WriteLine("Invalid move format. Please try again.");
                            continue;
                        }
                        if (!board.IsMoveLegal(move, colorToMove))
                        {
                            Console.WriteLine("Illegal move. Please try again.");
                            continue;
                        }
                        break;
                    }

                    playedMovesInfo.Add(board.MakeMove(move));
                    playedMoves.Add(move);
                    colorToMove = colorToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
                }

                else
                {
                    Console.WriteLine("Computer is thinking...");

                    Move bestMove;
                    MoveInfo previousMoveInfo = playedMovesInfo.Count > 0 ? playedMovesInfo[playedMovesInfo.Count - 1] : new MoveInfo();
                    board.Search(4, colorToMove, out bestMove);

                    playedMovesInfo.Add(board.MakeMove(bestMove));
                    playedMoves.Add(bestMove);
                    colorToMove = colorToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;

                    Console.WriteLine($"{bestMove.ToString()}");
                }
            }
            Console.WriteLine("GG");
        }
    }
}