namespace Engine
{
    public class Uci
    {
        private Board board = new Board();
        private Engine engine = new Engine();

        public List<Move> PlayedMoves = new List<Move>();
        public List<MoveInfo> PlayedMovesInfo = new List<MoveInfo>();

        public void Run()
        {
            while (true)
            {
                string input = Console.ReadLine();

                if (input == "") continue;

                switch (input)
                {
                    case "uci":
                        Console.WriteLine("id name Milan 2026-06-11");
                        Console.WriteLine("id author OL");
                        Console.WriteLine("uciok");
                        break;

                    case "isready":
                        Console.WriteLine("readyok");
                        break;

                    case string s when s.StartsWith("position"):
                        HandlePosition(s);
                        break;

                    case string s when s.StartsWith("go"):
                        int depth = 6;
                        if (s.Contains("depth")) int.TryParse(s.Split("depth ")[1].Split(' ')[0], out depth);

                        // engine.Search(board, depth, out Move bestMove, PlayedMoves, PlayedMovesInfo);
                        Move bestMove = engine.IterativeDeepening(board, depth, PlayedMoves, PlayedMovesInfo);

                        engine.ClearOldData();

                        Console.WriteLine($"bestmove {bestMove.ToString()}");
                        break;

                    case "ucinewgame":
                        engine = new Engine();
                        break;

                    case "quit":
                        return;
                }                
            }
        }

        private void HandlePosition(string input)
        {
            PlayedMoves.Clear();
            PlayedMovesInfo.Clear();
            board = new Board();

            if (input.Contains("startpos"))
            {
                board.SetupBoard();
            }
            else if (input.Contains("fen"))
            {
                string fen = input.Split("fen ")[1];
                fen = fen.Split(" moves")[0];
                board.SetupBoard(fen);
            }

            if (input.Contains("moves") && board.BoardHash != 0)
            {
                string moves = input.Split("moves ")[1];
                foreach (string move in moves.Split(' '))
                {
                    Move.TryParse(board.SideToMove, move, out Move parsedMove);

                    if (!board.IsMoveLegal(parsedMove))
                    {
                        Console.WriteLine("Illegal move in UCI input.");
                        Console.WriteLine($"Move: {move}");
                        Environment.Exit(1);
                    }

                    PlayedMovesInfo.Add(board.MakeMove(parsedMove));
                    PlayedMoves.Add(parsedMove);
                }
            }
        }
    }
}