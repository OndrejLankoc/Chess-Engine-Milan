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
                        Console.WriteLine("id name Milan 2026-11-17");
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

            if (input.Contains("movetime"))
            {
                int moveTime = int.Parse(input.Split("movetime ")[1].Split(' ')[0]);
                TimeSpan timeLimitSoft = TimeSpan.FromMilliseconds(moveTime * 0.85);
                TimeSpan timeLimitHard = TimeSpan.FromMilliseconds(moveTime);

                bestMove = Engine.IterativeDeepening(Board, timeLimitSoft, timeLimitHard, PlayedMoves, PlayedMovesInfo);
            }
            else if (input.Contains("wtime"))
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

                TimeSpan[] timeLimits = GetTimeLimits(timeLeft, increment);
                bestMove = Engine.IterativeDeepening(Board, timeLimits[0], timeLimits[1], PlayedMoves, PlayedMovesInfo);
            }
            else
            {
                int depth = input.Contains("depth") ? int.Parse(input.Split("depth ")[1].Split(' ')[0]) : 6;
                bestMove = Engine.IterativeDeepening(Board, depth, PlayedMoves, PlayedMovesInfo);
            }

            Engine.ClearOldData();
            Console.WriteLine($"bestmove {bestMove.ToString()}");
        }

        private TimeSpan[] GetTimeLimits(int timeLeft, int increment)
        {
            TimeSpan[] timeLimits = new TimeSpan[2]; // [0] = soft limit, [1] = hard limit

            double divisor;
            switch (timeLeft)
            {
                case <= 10_000:
                    divisor = 20;
                    break;
                
                case <= 30_000:
                    divisor = 35;
                    break;
                
                case <= 120_000:
                    divisor = 45;
                    break;
                
                default:
                    divisor = 60;
                    break;
            }

            double reserve = Math.Clamp(timeLeft * 0.1, 500, 5_000);
            if (reserve > timeLeft) reserve = timeLeft;
            double timeForMove = timeLeft / divisor + increment * 0.8;

            timeLimits[0] = TimeSpan.FromMilliseconds(Math.Min(timeForMove, timeLeft - reserve));
            timeLimits[1] = TimeSpan.FromMilliseconds(Math.Min(timeForMove * 2.5, timeLeft - reserve));
            return timeLimits;
        }
    }
}