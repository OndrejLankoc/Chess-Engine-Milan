namespace Engine
{
    public class Uci
    {
        public Board Board = new Board();
        public Engine Engine = new Engine();

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
                        Console.WriteLine("id name Milan 2026-06-13_2");
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
                        HandleGo(s);
                        break;

                    case "ucinewgame":
                        Engine = new Engine();
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
            Board = new Board();

            if (input.Contains("startpos"))
            {
                Board.SetupBoard();
            }
            else if (input.Contains("fen"))
            {
                string fen = input.Split("fen ")[1];
                fen = fen.Split(" moves")[0];
                Board.SetupBoard(fen);
            }

            if (input.Contains("moves") && Board.BoardHash != 0)
            {
                string moves = input.Split("moves ")[1];
                foreach (string move in moves.Split(' '))
                {
                    Move.TryParse(Board.SideToMove, move, out Move parsedMove);

                    if (!Board.IsMoveLegal(parsedMove))
                    {
                        Console.WriteLine("Illegal move in UCI input.");
                        Console.WriteLine($"Move: {move}");
                        Environment.Exit(1);
                    }

                    PlayedMovesInfo.Add(Board.MakeMove(parsedMove));
                    PlayedMoves.Add(parsedMove);
                }
            }
        }

        private void HandleGo(string input)
        {
            Move bestMove = null;

            if (input.Contains("wtime"))
            {
                int timeLeft, increment;
                if (Board.SideToMove == PieceColor.White)
                {
                    timeLeft = int.Parse(input.Split("wtime ")[1].Split(' ')[0]);
                    increment = input.Contains("winc") ? int.Parse(input.Split("winc ")[1].Split(' ')[0]) : 0;
                }
                else
                {
                    timeLeft = int.Parse(input.Split("btime ")[1].Split(' ')[0]);
                    increment = input.Contains("binc") ? int.Parse(input.Split("binc ")[1].Split(' ')[0]) : 0;
                }

                TimeSpan timeLimitSoft = TimeSpan.FromMilliseconds(timeLeft / 20 + increment / 2);
                TimeSpan timeLimitHard = TimeSpan.FromMilliseconds(timeLeft / 6 + increment);

                bestMove = Engine.IterativeDeepening(Board, timeLimitSoft, timeLimitHard, PlayedMoves, PlayedMovesInfo);
            }
            else
            {
                int depth = input.Contains("depth") ? int.Parse(input.Split("depth ")[1].Split(' ')[0]) : 6;
                bestMove = Engine.IterativeDeepening(Board, depth, PlayedMoves, PlayedMovesInfo);
            }

            Engine.ClearOldData();
            Console.WriteLine($"bestmove {bestMove.ToString()}");
        }
    }
}