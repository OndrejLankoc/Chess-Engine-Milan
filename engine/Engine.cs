namespace Engine
{
    public class Engine
    {
        private TranspositionTable tt = new();

        public int Evaluate(Board board)
        {
            int evaluation = 0;
            int[,] pawnSquareTable =
            {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 50, 50, 50, 50, 50, 50, 50, 50 },
                { 10, 10, 20, 30, 30, 20, 10, 10 },
                { 5,  5, 10, 25, 25, 10, 5, 5 },
                { 0, 0, 0, 20, 20, 0, 0, 0 },
                { 5, -5, -10, 0, 0, -10, -5, 5 },
                { 5, 10, 10, -20, -20, 10, 10, 5 },
                { 0, 0, 0, 0, 0, 0, 0, 0 }
            };

            int[,] knightSquareTable =
            {
                { -50, -40, -30, -30, -30, -30, -40, -50 },
                { -40, -20, 0, 0, 0, 0, -20, -40 },
                { -30, 0, 10, 15, 15, 10, 0, -30 },
                { -30, 5, 15, 20, 20, 15, 5, -30 },
                { -30, 0, 15, 20, 20, 15, 0, -30 },
                { -30, 5, 10, 15, 15, 10, 5, -30 },
                { -40, -20, 0, 5, 5, 0, -20, -40 },
                { -50, -40, -30, -30, -30, -30, -40, -50 }
            };

            int[,] bishopSquareTable =
            {
                { -20, -10, -10, -10, -10, -10, -10, -20 },
                { -10, 0, 0, 0, 0, 0, 0, -10 },
                { -10, 0, 5, 10, 10, 5, 0, -10 },
                { -10, 5, 5, 10, 10, 5, 5, -10 },
                { -10, 0, 10, 10, 10, 10, 0, -10 },
                { -10, 10, 10, 10, 10, 10, 10, -10 },
                { -10, 5, 0, 0, 0, 0, 5, -10 },
                { -20, -10, -10, -10, -10, -10, -10, -20 }
            };

            int[,] rookSquareTable =
            {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 5, 10, 10, 10, 10, 10, 10, 5 },
                { -5, 0, 0, 0, 0, 0, 0, -5 },
                { -5, 0, 0, 0, 0, 0, 0, -5 },
                { -5, 0, 0, 0, 0, 0, 0, -5 },
                { -5, 0, 0, 0, 0, 0, 0, -5 },
                { -5, 0, 0, 0, 0, 0, 0, -5 },
                { 0, 0, 0, 5, 5, 0, 0, 0 }
            };

            int[,] queenSquareTable =
            {
                { -20, -10, -10, -5, -5, -10, -10, -20 },
                { -10, 0, 0, 0, 0, 0, 0, -10 },
                { -10, 0, 5, 5, 5, 5, 0, -10 },
                { -5, 0, 5, 5, 5, 5, 0, -5 },
                { 0, 0, 5, 5, 5, 5, 0, -5 },
                { -10, 5, 5, 5, 5, 5, 0, -10 },
                { -10, 0, 5, 0, 0, 0, 0, -10 },
                { -20, -10, -10, -5, -5, -10, -10, -20 }
            };

            int[,] mgKingSquareTable =
            {
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -20, -30, -30, -40, -40, -30, -30, -20 },
                { -10, -20, -20, -20, -20, -20, -20, -10 },
                { 20, 20, 0, 0, 0, 0, 20, 20 },
                { 20, 30, 10, 0, 0, 10, 30, 20 }
            };

            int[,] egKingSquareTable =
            {
                { -50, -40, -30, -20, -20, -30, -40, -50 },
                { -30, -20, -10, 0, 0, -10, -20, -30 },
                { -30, -10, 20, 30, 30, 20, -10, -30 },
                { -30, -10, 30, 40, 40, 30, -10, -30 },
                { -30, -10, 30, 40, 40, 30, -10, -30 },
                { -30, -10, 20, 30, 30, 20, -10, -30 },
                { -30, -30, 0, 0, 0, 0, -30, -30 },
                { -50, -30, -30, -30, -30, -30, -30, -50 }
            };

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Piece piece = board.Squares[rank, file];
                    if (piece != null)
                    {
                        int rankIndex = (piece.Color == PieceColor.White) ? rank : 7 - rank;
                        evaluation += (piece.Color == PieceColor.White ? 1 : -1) *
                        (piece.Type switch
                        {
                            PieceType.Pawn => 100 + pawnSquareTable[rankIndex, file],
                            PieceType.Knight => 320 + knightSquareTable[rankIndex, file],
                            PieceType.Bishop => 330 + bishopSquareTable[rankIndex, file],
                            PieceType.Rook => 500 + rookSquareTable[rankIndex, file],
                            PieceType.Queen => 900 + queenSquareTable[rankIndex, file],
                            PieceType.King => 20000 + (int)((1 - board.EndgamePhase()) * mgKingSquareTable[rankIndex, file] + board.EndgamePhase() * egKingSquareTable[rankIndex, file]),
                            _ => 0
                        });
                    }
                }
            }
            return evaluation;
        }

        public int Search(Board board, int depth, out Move bestMove, List<Move> allMoves, List<MoveInfo> allMovesInfo, int alpha = int.MinValue, int beta = int.MaxValue)
        {
            bestMove = null;
            if (depth == 0)
            {
                return Evaluate(board);
            }

            GameResult result = board.Result(allMovesInfo, allMoves, out _);
            if (result != GameResult.Ongoing)
            {
                return result == GameResult.WhiteWin ? int.MaxValue : result == GameResult.BlackWin ? int.MinValue : 0;
            }

            int alphaOriginal = alpha;
            ulong hash = board.ComputeHash();
            if (tt.TryGet(hash, out TranspositionTableEntry entry) && entry.Depth >= depth)
            {
                switch (entry.Type)
                {
                    case NodeType.Exact:
                        return entry.Score;

                    case NodeType.LowerBound:
                        alpha = Math.Max(alpha, entry.Score);
                        break;

                    case NodeType.UpperBound:
                        beta = Math.Min(beta, entry.Score);
                        break;
                }
                if (alpha >= beta)
                {
                    bestMove = entry.BestMove;
                    return entry.Score;
                }
            }

            List<Move> moves = new List<Move>();
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    Piece piece = board.Squares[rank, file];
                    if (piece != null && piece.Color == board.sideToMove)
                    {
                        moves.AddRange(piece.GetLegalMoves(board, new Square(rank, file), board.castlingRights, board.enPassantSquare));
                    }
                }
            }

            int bestScore = board.sideToMove == PieceColor.White ? int.MinValue + 1 : int.MaxValue - 1;
            foreach (Move move in moves)
            {
                Board nextMove = board.Clone();
                List<Move> newAllMoves = new List<Move>(allMoves);
                List<MoveInfo> newAllMovesInfo = new List<MoveInfo>(allMovesInfo);
                newAllMoves.Add(move);
                newAllMovesInfo.Add(nextMove.MakeMove(move));

                int score = Search(nextMove, depth - 1, out _, newAllMoves, newAllMovesInfo, alpha, beta);
                if (board.sideToMove == PieceColor.White)
                {
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                    beta = Math.Min(beta, bestScore);
                }

                if (alpha >= beta)
                {
                    break;
                }
            }

            NodeType nodeType = NodeType.Exact;
            if (bestScore <= alphaOriginal)
            {
                nodeType = NodeType.UpperBound;
            }
            else if (bestScore >= beta)
            {
                nodeType = NodeType.LowerBound;
            }
            tt.Store(hash, depth, bestScore, nodeType, bestMove);

            return bestScore;
        }
    }
}