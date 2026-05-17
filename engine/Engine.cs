namespace Engine
{
    public class Engine
    {
        public TranspositionTable TT = new();
        public PawnTTEntry[] PawnTT = new PawnTTEntry[1 << 16];
        private const int KillersSize = 32;
        public Move?[,] KillerMoves = new Move?[KillersSize, 2];
        public int[,,] History = new int[2, 64, 64];
        private const int Mate = 100000;
        private const int MateThreshold = Mate - 1000;

        public int Evaluate(Board board)
        {
            int evaluation = 0;
            double gamePhase = board.EndgamePhase();
            int[,] mgPawnSquareTable =
            {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 98, 134, 61, 95, 68, 126, 34, -11 },
                { -6, 7, 6, 31, 68, 65, 25, -20 },
                { -14, 13, 25, 60, 62, 12, 17, -23 },
                { -27, -2, 11, 47, 52, 6, 10, -25 },
                { -26, -4, -4, 3, 3, 3, 33, -12 },
                { -35, -1, -5, -23, -15, 24, 38, -22 },
                { 0, 0, 0, 0, 0, 0, 0, 0 }
            };

            int[,] egPawnSquareTable =
            {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 178, 173, 158, 134, 147, 132, 165, 187 },
                { 94, 100, 85, 67, 56, 53, 82, 84 },
                { 32, 24, 13, 5, -2, 4, 17, 17 },
                { 13, 9, -3, -7, -7, -8, 3, -1 },
                { 4, 7, -6, 1, 0, -5, -1, -8 },
                { 13, 8, 8, 10, 13, 0, 2, -7 },
                { 0, 0, 0, 0, 0, 0, 0, 0 }
            };
            
            int[,] mgKnightSquareTable =
            {
                { -167, -89, -34, -49, 61, -97, -15, -107 },
                { -73, -41, 72, 36, 23, 62, 7, -17 },
                { -47, 60, 37, 65, 84, 129, 73, 44 },
                { -9, 17, 19, 43, 37, 69, 18, 22 },
                { -13, 4, 16, 13, 28, 19, 21, -8 },
                { -23, -9, 12, 10, 19, 17, 25, -16 },
                { -29, -53, -12, -3, -1, 18, -14, -19 },
                { -105, -21, -58, -33, -17, -28, -19, -23 }
            };

            int[,] egKnightSquareTable =
            {
                { -58, -38, -13, -28, -31, -27, -63, -99 },
                { -25, -8, -25, -2, -9, -25, -24, -52 },
                { -24, -20, 10, 9, -1, -9, -19, -41 },
                { -17, 3, 22, 22, 22, 11, 8, -18 },
                { -18, -6, 16, 25, 16, 17, 4, -18 },
                { -23, -3, -1, 15, 10, -3, -20, -22 },
                { -42, -20, -10, -5, -2, -20, -23, -44 },
                { -29, -51, -23, -15, -22, -18, -50, -64 }
            };

            int[,] mgBishopSquareTable =
            {
                { -29, 4, -82, -37, -25, -42, 7, -8 },
                { -26, 16, -18, -13, 30, 59, 18, -47 },
                { -16, 37, 43, 40, 35, 50, 37, -2 },
                { -4, 5, 19, 50, 37, 37, 7, -2 },
                { -6, 13, 13, 26, 34, 12, 10, 4 },
                { 0, 15, 15, 15, 14, 27, 18, 10 },
                { 4, 15, 16, 0, 7, 21, 33, 1 },
                { -33, -3, -14, -21, -13, -12, -39, -21 }
            };

            int[,] egBishopSquareTable =
            {
                { -14, -21, -11, -8, -7, -9, -17, -24 },
                { -8, -4, 7, -12, -3, -13, -4, -14 },
                { 2, -8, 0, -1, -2, 6, 0, 4 },
                { -3, 9, 12, 9, 14, 10, 3, 2 },
                { -6, 3, 13, 19, 7, 10, -3, -9 },
                { -12, -3, 8, 10, 13, 3, -7, -15 },
                { -14, -18, -7, -1, 4, -9, -15, -27 },
                { -23, -9, -23, -5, -9, -16, -5, -17 }
            };

            int[,] mgRookSquareTable =
            {
                { 32, 42, 32, 51, 63, 9, 31, 43 },
                { 27, 32, 58, 62, 80, 67, 26, 44 },
                { -5, 19, 26, 36, 17, 45, 61, 16 },
                { -24, -11, 7, 26, 24, 35, -8, -20 },
                { -36, -26, -12, -1, 9, -7, 6, -23 },
                { -45, -25, -16, -17, 3, 0, -5, -33 },
                { -44, -16, -20, -9, -1, 11, -6, -71 },
                { -19, -13, 1, 17, 16, 7, -37, -26 }
            };

            int[,] egRookSquareTable =
            {
                { 13, 10, 18, 15, 12, 12, 8, 5 },
                { 11, 13, 13, 11, -3, 3, 8, 3 },
                { 7, 7, 7, 5, 4, -3, -5, -3 },
                { 4, 3, 13, 1, 2, 1, -1, 2 },
                { 3, 5, 8, 4, -5, -6, -8, -11 },
                { -4, 0, -5, -1, -7, -12, -8, -16 },
                { -6, -6, 0, 2, -9, -9, -11, -3 },
                { -9, 2, 3, -1, -5, -13, 4, -20 }
            };

            int[,] mgQueenSquareTable =
            {
                { -28, 0, 29, 12, 59, 44, 43, 45 },
                { -24, -39, -5, 1, -16, 57, 28, 54 },
                { -13, -17, 7, 8, 29, 56, 47, 57 },
                { -27, -27, -16, -16, -1, 17, -2, 1 },
                { -9, -26, -9, -10, -2, -4, 3, -3 },
                { -14, 2, -11, -2, -5, 2, 14, 5 },
                { -35, -8, 11, 2, 8, 15, -3, 1 },
                { -1, -18, -9, 10, -15, -25, -31, -50 }
            };

            int[,] egQueenSquareTable =
            {
                { -9, 22, 22, 27, 27, 19, 10, 20 },
                { -17, 20, 32, 41, 58, 25, 30, 0 },
                { -20, 6, 9, 49, 47, 35, 19, 9 },
                { 3, 22, 24, 45, 57, 40, 57, 36 },
                { -18, 28, 19, 47, 31, 34, 39, 23 },
                { -16, -27, 15, 6, 9, 17, 10, 5 },
                { -22, -23, -30, -16, -16, -23, -36, -32 },
                { -33, -28, -22, -43, -5, -32, -20, -41 }
            };
            
            int[,] mgKingSquareTable =
            {
                { -65, 23, 16, -15, -56, -34, 2, 13 },
                { 29, -1, -20, -7, -8, -4, -38, -29 },
                { -9, 24, 2, -16, -20, 6, 22, -22 },
                { -17, -20, -12, -27, -30, -25, -14, -36 },
                { -49, -1, -27, -39, -46, -44, -33, -51 },
                { -14, -14, -22, -46, -44, -30, -15, -27 },
                { 1, 7, -8, -64, -43, -16, 9, 8 },
                { -15, 36, 12, -54, 8, -28, 24, 14 }
            };

            int[,] egKingSquareTable =
            {
                { -74, -35, -18, -18, -11, 15, 4, -17 },
                { -12, 17, 14, 17, 17, 38, 23, 11 },
                { 10, 17, 23, 15, 20, 45, 44, 13 },
                { -8, 22, 24, 27, 26, 33, 26, 3 },
                { -18, -4, 21, 24, 27, 23, 9, -11 },
                { -19, -3, 11, 21, 23, 16, 7, -9 },
                { -27, -11, 4, 13, 14, 4, -5, -17 },
                { -53, -34, -21, -11, -28, -14, -24, -43 }
            };

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Piece? piece = board.GetPiece(new Square(rank, file));
                    if (piece != null)
                    {
                        int rankIndex = (piece.Color == PieceColor.White) ? rank : 7 - rank;
                        evaluation += (piece.Color == PieceColor.White ? 1 : -1) *
                        (Piece.GetValue(piece.Type) + piece.Type switch
                        {
                            PieceType.Pawn => (int)((1 - gamePhase) * mgPawnSquareTable[rankIndex, file] + gamePhase * egPawnSquareTable[rankIndex, file]),
                            PieceType.Knight => (int)((1 - gamePhase) * mgKnightSquareTable[rankIndex, file] + gamePhase * egKnightSquareTable[rankIndex, file]),
                            PieceType.Bishop => (int)((1 - gamePhase) * mgBishopSquareTable[rankIndex, file] + gamePhase * egBishopSquareTable[rankIndex, file]),
                            PieceType.Rook => (int)((1 - gamePhase) * mgRookSquareTable[rankIndex, file] + gamePhase * egRookSquareTable[rankIndex, file]),
                            PieceType.Queen => (int)((1 - gamePhase) * mgQueenSquareTable[rankIndex, file] + gamePhase * egQueenSquareTable[rankIndex, file]),
                            PieceType.King => (int)((1 - gamePhase) * mgKingSquareTable[rankIndex, file] + gamePhase * egKingSquareTable[rankIndex, file]),
                            _ => 0
                        });
                    }
                }
            }

            evaluation += Mobility(board, gamePhase);
            evaluation += PawnStructure(board);
            evaluation += KingSafety(board, gamePhase);
            return evaluation;
        }

        public int Search(Board board, int depth, out Move bestMove, List<Move> allMoves, List<MoveInfo> allMovesInfo, int ply = 0, int alpha = int.MinValue, int beta = int.MaxValue, bool nullMoveAllowed = true)
        {
            bestMove = null;
            List<Move> moves = new List<Move>();
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    Piece? piece = board.GetPiece(new Square(rank, file));
                    if (piece != null && piece.Color == board.SideToMove)
                    {
                        moves.AddRange(piece.GetLegalMoves(board, new Square(rank, file)));
                    }
                }
            }

            GameResult result = board.Result(allMovesInfo, allMoves, out _, moves.Count);
            if (result != GameResult.Ongoing)
            {
                return result == GameResult.WhiteWin ? Mate : result == GameResult.BlackWin ? -Mate : 0;
            }

            if (depth <= 0 || moves.Count == 0)
            {
                return Quiescence(board, allMoves, allMovesInfo, ply, alpha, beta);
            }

            moves = Move.OrderMoves(moves, KillerMoves, History, board, ply);

            int alphaOriginal = alpha;
            if (TT.TryGet(board.BoardHash, out TranspositionTableEntry entry))
            {
                if (entry.Depth >= depth)
                {
                    switch (entry.Type)
                    {
                        case NodeType.Exact:
                            if (board.HalfMoveClock >= 6 && ply <= 2)
                            {
                                moves.RemoveAll(m => m.Equals(entry.BestMove));
                                moves.Insert(0, entry.BestMove);
                                break;
                            }
                            bestMove = entry.BestMove;
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

                else
                {
                    if (entry.Type == NodeType.Exact)
                    {
                        moves.RemoveAll(m => m.Equals(entry.BestMove));
                        moves.Insert(0, entry.BestMove);
                    }

                    else if (entry.Type == NodeType.LowerBound)
                    {
                        moves.RemoveAll(m => m.Equals(entry.BestMove));
                        moves.Insert(0, entry.BestMove);
                    }
                    else if (entry.Type == NodeType.UpperBound)
                    {
                        moves.RemoveAll(m => m.Equals(entry.BestMove));
                        moves.Add(entry.BestMove);
                    }
                }
            }

            if (depth >= 4 && !board.IsInCheck(board.SideToMove) && nullMoveAllowed)
            {
                if (board.EndgamePhase() < 0.7)
                {
                    int r = 2;
                    MoveInfo nullMoveInfo = board.MakeMove();

                    if (board.SideToMove == PieceColor.White)
                    {
                        int score = Search(board, depth - 1 - r, out _, allMoves, allMovesInfo, ply + 1, alpha, alpha + 1, false);
                        board.UndoMove(nullMoveInfo);
                        if (score <= alpha) return alpha;
                    }
                    else
                    {
                        int score = Search(board, depth - 1 - r, out _, allMoves, allMovesInfo, ply + 1, beta - 1, beta, false);
                        board.UndoMove(nullMoveInfo);
                        if (score >= beta) return beta;
                    }
                }
            }

            int bestScore = board.SideToMove == PieceColor.White ? int.MinValue : int.MaxValue;
            foreach (Move move in moves)
            {
                int r = 0;
                int index = moves.IndexOf(move);
                int score = 0;
                allMoves.Add(move);
                allMovesInfo.Add(board.MakeMove(move));

                if (depth >= 3 && index >= 5 && board.IsMoveQuiet(move))
                {
                    if (!board.IsInCheck(board.SideToMove))
                    {
                        r = 1;
                        score = Search(board, depth - r, out _, allMoves, allMovesInfo, ply + 1, alpha, beta);
                        if (score > alpha && score < beta) r = 0;
                    }
                }
                if (r == 0) score = Search(board, depth - 1, out _, allMoves, allMovesInfo, ply + 1, alpha, beta);
                if (Math.Abs(score) >= MateThreshold) score += score > 0 ? -1 : 1;

                board.UndoMove(move, allMovesInfo.Last());
                allMoves.RemoveAt(allMoves.Count - 1);
                allMovesInfo.RemoveAt(allMovesInfo.Count - 1);

                if (board.SideToMove == PieceColor.White)
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
                    if (board.IsMoveQuiet(move))
                    {
                        StoreKillerMove(move, ply);
                        StoreHistoryMove(move, board.SideToMove, depth);
                    }
                    break;
                }
            }

            if (depth >= 2)
            {
                NodeType nodeType = NodeType.Exact;
                if (bestScore <= alphaOriginal)
                {
                    nodeType = NodeType.UpperBound;
                }
                else if (bestScore >= beta)
                {
                    nodeType = NodeType.LowerBound;
                }

                TT.Store(board.BoardHash, bestScore, depth, nodeType, bestMove, board.FullMoveNumber);
            }

            return bestScore;
        }

        private int Quiescence(Board board, List<Move> allMoves, List<MoveInfo> allMovesInfo, int ply, int alpha, int beta)
        {
            List<Move> moves = new List<Move>();
            
            if (board.IsInCheck(board.SideToMove))
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    for (int file = 0; file < 8; file++)
                    {
                        Piece? piece = board.GetPiece(new Square(rank, file));
                        if (piece != null && piece.Color == board.SideToMove) moves.AddRange(piece.GetLegalMoves(board, new Square(rank, file)));
                    }
                }
                if (moves.Count == 0) return board.SideToMove == PieceColor.White ? -Mate : Mate;
            }

            else
            {
                int standPat = Evaluate(board);
                if (board.SideToMove == PieceColor.White)
                {
                    alpha = Math.Max(alpha, standPat);
                    if (alpha >= beta) return alpha;
                }
                else
                {
                    beta = Math.Min(beta, standPat);
                    if (alpha >= beta) return beta;
                }

                for (int rank = 0; rank < 8; rank++)
                {
                    for (int file = 0; file < 8; file++)
                    {
                        Piece? piece = board.GetPiece(new Square(rank, file));
                        if (piece != null && piece.Color == board.SideToMove) moves.AddRange(piece.GetCaptureMoves(board, new Square(rank, file)));
                    }
                }

                if (moves.Count == 0)
                {
                    for (int rank = 0; rank < 8; rank++)
                    {
                        for (int file = 0; file < 8; file++)
                        {
                            Piece? piece = board.GetPiece(new Square(rank, file));
                            if (piece == null || piece.Color != board.SideToMove) continue;
                            if (piece.GetLegalMoves(board, new Square(rank, file)).Count > 0) return standPat;
                        }
                    }

                    return 0;
                }
            }

            moves = Move.OrderMoves(moves, KillerMoves, History, board, Math.Min(ply, KillersSize - 1));

            foreach (Move move in moves)
            {
                allMoves.Add(move);
                allMovesInfo.Add(board.MakeMove(move));

                int score = Quiescence(board, allMoves, allMovesInfo, ply + 1, alpha, beta);
                if (Math.Abs(score) >= MateThreshold) score = score > 0 ? score - 1 : score + 1;

                board.UndoMove(move, allMovesInfo.Last());
                allMoves.RemoveAt(allMoves.Count - 1);
                allMovesInfo.RemoveAt(allMovesInfo.Count - 1);

                if (board.SideToMove == PieceColor.White) alpha = Math.Max(alpha, score);
                else beta = Math.Min(beta, score);

                if (alpha >= beta) break;
            }

            return board.SideToMove == PieceColor.White ? alpha : beta;
        }

        private int Mobility(Board board, double gamePhase)
        {
            int mobility = 0;
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Square startSquare = new Square(rank, file);
                    Piece? piece = board.GetPiece(startSquare);
                    if (piece != null && piece.Type != PieceType.King && piece.Type != PieceType.Pawn)
                    {
                        List<Move> moves = piece.GetMoves(board, startSquare);
                        int movesCount = moves.Count;

                        if (movesCount > 0)
                        {
                            int mgMobilityWeight = piece.Type switch
                            {
                                PieceType.Knight => 5,
                                PieceType.Bishop => 4,
                                PieceType.Rook => 2,
                                PieceType.Queen => 1
                            };
                            int egMobilityWeight = piece.Type switch
                            {
                                PieceType.Knight => 3,
                                PieceType.Bishop => 3,
                                PieceType.Rook => 3,
                                PieceType.Queen => 2
                            };

                            int mobilityWeight = (int)((1 - gamePhase) * mgMobilityWeight + gamePhase * egMobilityWeight);
                            mobility += (piece.Color == PieceColor.White ? 1 : -1) * movesCount * mobilityWeight;
                        }
                    }
                }
            }

            return mobility;
        }

        private int PawnStructure(Board board)
        {
            int score = 0;
            int index = (int)(board.PawnHash & (ulong)(PawnTT.Length - 1));

            if (PawnTT[index].Key == board.PawnHash) return PawnTT[index].Score;

            int[] whiteFiles = new int[8];
            int[] blackFiles = new int[8];
            int whiteIslands = 0;
            int blackIslands = 0;

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Piece? piece = board.GetPiece(new Square(rank, file));
                    if (piece != null && piece.Type == PieceType.Pawn)
                    {
                        if (piece.Color == PieceColor.White)
                        {
                            whiteFiles[file]++;
                        }
                        else
                        {
                            blackFiles[file]++;
                        }

                        int direction = piece.Color == PieceColor.White ? 1 : -1;
                        Piece? leftBehindPiece = board.GetPiece(new Square(rank + direction, file - 1));
                        Piece? rightBehindPiece = board.GetPiece(new Square(rank + direction, file + 1));
                        bool leftConnected = leftBehindPiece != null && leftBehindPiece.Type == PieceType.Pawn && leftBehindPiece.Color == piece.Color;
                        bool rightConnected = rightBehindPiece != null && rightBehindPiece.Type == PieceType.Pawn && rightBehindPiece.Color == piece.Color;

                        if (leftConnected && rightConnected) score += 12 * direction;
                        else if (leftConnected || rightConnected) score += 5 * direction;

                        Piece? pieceOnRight = board.GetPiece(new Square(rank, file + 1));
                        if (pieceOnRight != null && pieceOnRight.Type == PieceType.Pawn && pieceOnRight.Color == piece.Color) score += 3 * direction;
                    }
                }
            }

            for (int file = 0; file < 8; file++)
            {
                score -= 12 * Math.Max(0, whiteFiles[file] - 1);
                score += 12 * Math.Max(0, blackFiles[file] - 1);

                bool[] isolation = { false, false, false, false }; // [0] = White from left, [1] = White from right, [2] = Black from left, [3] = Black from right

                if (file > 0)
                {
                    if (whiteFiles[file] > 0 && whiteFiles[file - 1] == 0) isolation[0] = true;
                    if (blackFiles[file] > 0 && blackFiles[file - 1] == 0) isolation[2] = true;
                }
                else isolation[0] = isolation[2] = true;

                if (file < 7)
                {
                    if (whiteFiles[file] > 0 && whiteFiles[file + 1] == 0) isolation[1] = true;
                    if (blackFiles[file] > 0 && blackFiles[file + 1] == 0) isolation[3] = true;
                }
                else isolation[1] = isolation[3] = true;

                if (isolation[0] && isolation[1]) score -= 15 * whiteFiles[file];
                if (isolation[2] && isolation[3]) score += 15 * blackFiles[file];

                int[] passedBonus = { 0, 100, 60, 35, 20, 10, 0, 0 };
                bool blackPassed = false;
                for (int rank = 1; rank < 7; rank++)
                {
                    Piece? piece = board.GetPiece(new Square(rank, file));
                    if (piece == null || piece.Type != PieceType.Pawn) continue;
                    if (piece.Color == PieceColor.Black && blackPassed) continue;

                    if (board.IsPawnPassed(new Square(rank, file)))
                    {
                        if (piece.Color == PieceColor.White) score += passedBonus[rank];
                        else
                        {
                            score -= passedBonus[7 - rank];
                            blackPassed = true;
                        }
                    }
                }

                if (whiteFiles[file] > 0 && (file == 0 || whiteFiles[file - 1] == 0)) whiteIslands++;
                if (blackFiles[file] > 0 && (file == 0 || blackFiles[file - 1] == 0)) blackIslands++;
            }

            score -= 12 * Math.Max(0, whiteIslands - 1);
            score += 12 * Math.Max(0, blackIslands - 1);

            PawnTT[index].Key = board.PawnHash;
            PawnTT[index].Score = score;
            return score;
        }
        
        private int KingSafety(Board board, double gamePhase)
        {
            int score = 0;

            score += PawnShield(board, gamePhase);
            score += KingZoneAttack(board);

            return score;
        }

        private int PawnShield(Board board, double gamePhase)
        {
            int score = 0;
            int[] pawnShieldBonus = { 15, 9, 3, -20 };
            int[] neighboringPawnShieldBonus = { 8, 4, 2, -10 };
            int startFile = -1;
            int endFile = -1;

            Square whiteKing = board.GetKingPosition(PieceColor.White);
            Square blackKing = board.GetKingPosition(PieceColor.Black);

            if (whiteKing.File == 3 || whiteKing.File == 4) score -= 15;
            else if (whiteKing.File <= 2)
            {
                startFile = 0;
                endFile = 2;
            }
            else if (whiteKing.File >= 5)
            {
                startFile = 5;
                endFile = 7;

            }

            if (startFile != -1 && endFile != -1)
            {
                for (int file = startFile; file <= endFile; file++)
                {
                    for (int rank = Math.Min(6, whiteKing.Rank); rank >= 4; rank--)
                    {
                        Piece? piece = board.GetPiece(new Square(rank, file));
                        if (piece != null && piece.Type == PieceType.Pawn && piece.Color == PieceColor.White)
                        {
                            score += file == whiteKing.File ? pawnShieldBonus[6 - rank] : neighboringPawnShieldBonus[6 - rank];
                            break;
                        }

                        if (rank == 4) score += file == whiteKing.File ? pawnShieldBonus[3] : neighboringPawnShieldBonus[3];
                    }
                }

                startFile = -1;
                endFile = -1;
            }

            if (blackKing.File == 3 || blackKing.File == 4) score += 15;
            else if (blackKing.File <= 2)
            {
                startFile = 0;
                endFile = 2;
            }
            else if (blackKing.File >= 5)
            {
                startFile = 5;
                endFile = 7;
            }

            if (startFile != -1 && endFile != -1)
            {
                for (int file = startFile; file <= endFile; file++)
                {
                    for (int rank = Math.Max(1, blackKing.Rank); rank <= 3; rank++)
                    {
                        Piece? piece = board.GetPiece(new Square(rank, file));
                        if (piece != null && piece.Type == PieceType.Pawn && piece.Color == PieceColor.Black)
                        {
                            score -= file == blackKing.File ? pawnShieldBonus[rank - 1] : neighboringPawnShieldBonus[rank - 1];
                            break;
                        }

                        if (rank == 3) score -= file == blackKing.File ? pawnShieldBonus[3] : neighboringPawnShieldBonus[3];
                    }
                }
            }

            return (int)(score * (1 - gamePhase));
        }

        private int KingZoneAttack(Board board)
        {
            int score = 0;

            int[] attackerValue = { 0, 20, 20, 40, 80 }; // P, N, B, R, Q
            int[] attackWeight = { 0, 0, 50, 75, 88, 94, 97, 99 }; // 0, 1, 2, 3, 4, 5, 6, 7+ attackers

            int whiteAttackers = 0;
            int blackAttackers = 0;
            int whiteValueOfAttacks = 0;
            int blackValueOfAttacks = 0;

            List<Square> whiteKingZone = new List<Square>();
            List<Square> blackKingZone = new List<Square>();
            Square whiteKing = board.GetKingPosition(PieceColor.White);
            Square blackKing = board.GetKingPosition(PieceColor.Black);

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    Square whiteSquare = new Square(whiteKing.Rank + i, whiteKing.File + j);
                    if (whiteSquare.IsOnBoard()) whiteKingZone.Add(whiteSquare);

                    Square blackSquare = new Square(blackKing.Rank + i, blackKing.File + j);
                    if (blackSquare.IsOnBoard()) blackKingZone.Add(blackSquare);
                }
            }

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Piece? piece = board.GetPiece(new Square(rank, file));
                    if (piece == null || piece.Type == PieceType.King || piece.Type == PieceType.Pawn) continue;

                    bool isAttacker = false;
                    List<Move> moves = piece.GetMoves(board, new Square(rank, file));
                    foreach (Move move in moves)
                    {
                        if (piece.Color == PieceColor.White && blackKingZone.Contains(move.To))
                        {
                            isAttacker = true;
                            whiteValueOfAttacks += attackerValue[(int)piece.Type];
                        }
                        else if (piece.Color == PieceColor.Black && whiteKingZone.Contains(move.To))
                        {
                            isAttacker = true;
                            blackValueOfAttacks += attackerValue[(int)piece.Type];
                        }
                    }

                    if (isAttacker && piece.Color == PieceColor.White) whiteAttackers++;
                    else if (isAttacker && piece.Color == PieceColor.Black) blackAttackers++;
                }
            }

            score += (int)(whiteValueOfAttacks * attackWeight[Math.Min(whiteAttackers, attackWeight.Length - 1)] / 100);
            score -= (int)(blackValueOfAttacks * attackWeight[Math.Min(blackAttackers, attackWeight.Length - 1)] / 100);

            return score;
        }

        public void StoreKillerMove(Move move, int ply)
        {
            if (KillerMoves[ply, 0] != null && move.Equals(KillerMoves[ply, 0]!)) return;
            KillerMoves[ply, 1] = KillerMoves[ply, 0];
            KillerMoves[ply, 0] = move;
        }

        public void StoreHistoryMove(Move move, PieceColor color, int depth)
        {
            int from = move.From.Rank * 8 + move.From.File;
            int to = move.To.Rank * 8 + move.To.File;
            History[(int)color, from, to] += depth * depth;
        }

        public void ClearOldData(int halfMoveClock)
        {
            TT.ClearOldEntries(halfMoveClock);
            KillerMoves = new Move?[32, 2];
            for (int color = 0; color < 2; color++)
            {
                for (int from = 0; from < 64; from++)
                {
                    for (int to = 0; to < 64; to++)
                    {
                        History[color, from, to] /= 2;
                    }
                }
            }
        }
    }
}