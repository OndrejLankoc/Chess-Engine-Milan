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
                        Console.WriteLine("id name Engine");
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
                        engine.Search(board, 4, out Move bestMove, PlayedMoves, PlayedMovesInfo);

                        engine.TT.ClearOldEntries(board.HalfMoveClock);

                        Console.WriteLine($"bestmove {bestMove.ToString()}");
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

            if (input.Contains("startpos"))
            {
                board.SetupBoard();

                if (input.Contains("moves"))
                {
                    string moves = input.Split("moves ")[1];
                    foreach (string move in moves.Split(' '))
                    {
                        Move.TryParse(board.SideToMove, move, out Move parsedMove);

                        if (!board.IsMoveLegal(parsedMove))
                        {
                            Console.WriteLine("Illegal move in UCI input.");
                            Environment.Exit(1);
                        }

                        PlayedMovesInfo.Add(board.MakeMove(parsedMove));
                        PlayedMoves.Add(parsedMove);
                    }
                }
            }

            else if (input.Contains("fen"))
            {
                string fen = input.Split("fen ")[1];
                board.SetupBoard(fen);
            }
        }
    }
}