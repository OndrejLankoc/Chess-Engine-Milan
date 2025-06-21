namespace Engine
{
    public class Board
    {
        public Piece[,] Squares = new Piece[8, 8];

        public Board Clone()
        {
            Board cloneBoard = new Board();
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    if (Squares[rank, file] != null)
                    {
                        cloneBoard.Squares[rank, file] = new Piece(Squares[rank, file].Type, Squares[rank, file].Color);
                    }
                }
            }
            return cloneBoard;
        }

        public Piece GetPiece(Square square)
        {
            if (!square.IsOnBoard())
            {
                return null;
            }
            return Squares[square.Rank, square.File];
        }

        public bool IsEmpty(Square square)
        {
            return GetPiece(square) == null;
        }

        public bool IsEnemy(Square square, PieceColor color)
        {
            Piece piece = GetPiece(square);
            return piece != null && piece.Color != color;
        }

        public MoveInfo MakeMove(Move move, MoveInfo previousMoveInfo)
        {
            Piece piece = GetPiece(move.From);
            if (piece == null || !piece.GetMoves(this, move.From, previousMoveInfo).Contains(move))
            {
                return null;
            }

            MoveInfo moveInfo = new MoveInfo(GetPiece(move.To))
            {
                CastlingRights = previousMoveInfo.CastlingRightsAfterMove,
                CastlingRightsAfterMove = previousMoveInfo.CastlingRightsAfterMove,
                EnPassantSquare = previousMoveInfo.EnPassantSquareAfterMove
            };
            Squares[move.To.Rank, move.To.File] = piece;
            Squares[move.From.Rank, move.From.File] = null;

            if (piece.Type == PieceType.King)
            {
                moveInfo.CastlingRightsAfterMove[(piece.Color == PieceColor.White) ? 0 : 2] = moveInfo.CastlingRightsAfterMove[(piece.Color == PieceColor.White) ? 1 : 3] = false;
                if (Math.Abs(move.From.File - move.To.File) == 2)
                {
                    int rank = (piece.Color == PieceColor.White) ? 7 : 0;
                    int[] files = (move.To.File > move.From.File) ? new[] { 7, 5 } : new[] { 0, 3 };
                    Squares[rank, files[1]] = Squares[rank, files[0]];
                    Squares[rank, files[0]] = null;
                    moveInfo.IsCastling = true;
                }
            }

            if (piece.Type == PieceType.Rook)
            {
                int i = ((piece.Color == PieceColor.White) ? 0 : 2) + ((move.From.File == 7) ? 0 : 1);
                moveInfo.CastlingRightsAfterMove[i] = false;
            }

            if (piece.Type == PieceType.Pawn)
            {
                if (Math.Abs(move.From.Rank - move.To.Rank) == 2)
                {
                    moveInfo.EnPassantSquareAfterMove = new Square(move.To.Rank + (piece.Color == PieceColor.White ? 1 : -1), move.To.File);
                }

                int rank = (piece.Color == PieceColor.White) ? 0 : 7;
                if (move.To.Rank == rank)
                {
                    Squares[move.To.Rank, move.To.File] = move.PromotedPiece;
                    moveInfo.IsPromotion = true;
                    moveInfo.PromotedPiece = move.PromotedPiece;
                }

                if (moveInfo.EnPassantSquare == move.To)
                {
                    Squares[move.To.Rank + (piece.Color == PieceColor.White ? 1 : -1), move.To.File] = null;
                    moveInfo.IsEnPassant = true;
                }
            }

            return moveInfo;
        }

        public void UndoMove(Move move, MoveInfo moveInfo)
        {
            Squares[move.From.Rank, move.From.File] = Squares[move.To.Rank, move.To.File];
            Squares[move.To.Rank, move.To.File] = moveInfo.TakenPiece;

            if (moveInfo.IsCastling)
            {
                Square rookSquareTo = new Square((move.To.Rank == 0) ? 0 : 7, (move.To.File == 6) ? 5 : 3);
                Square rookSquareFrom = new Square((move.To.Rank == 0) ? 0 : 7, (move.To.File == 6) ? 7 : 0);
                Squares[rookSquareFrom.Rank, rookSquareFrom.File] = Squares[rookSquareTo.Rank, rookSquareTo.File];
                Squares[rookSquareTo.Rank, rookSquareTo.File] = null;
            }

            if (moveInfo.IsEnPassant)
            {
                Squares[move.To.Rank + ((move.From.Rank == 4) ? -1 : 1), move.To.File] = new Piece(PieceType.Pawn, (move.From.Rank == 4) ? PieceColor.White : PieceColor.Black);
            }

            if (moveInfo.IsPromotion)
            {
                Squares[move.From.Rank, move.From.File] = new Piece(PieceType.Pawn, (Squares[move.From.Rank, move.From.File].Color == PieceColor.Black) ? PieceColor.Black : PieceColor.White);
            }
        }

        public Square GetKingPosition(PieceColor color)
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    if (Squares[rank, file] != null && Squares[rank, file].Type == PieceType.King && Squares[rank, file].Color == color)
                    {
                        return new Square(rank, file);
                    }
                }
            }
            return null;
        }

        public bool IsInCheck(PieceColor color)
        {
            Square kingPosition = GetKingPosition(color);
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    if (Squares[rank, file] != null && Squares[rank, file].Color != color)
                    {
                        List<Move> moves = Squares[rank, file].GetAttackMoves(new Square(rank, file));
                        foreach (Move move in moves)
                        {
                            if (move.To == kingPosition)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public double endgamePhase()
        {
            int gamePhase = 0;
            foreach (Piece piece in Squares)
            {
                if (piece != null)
                {
                    switch (piece.Type)
                    {
                        case PieceType.Knight:
                        case PieceType.Bishop:
                            gamePhase += 1;
                            break;

                        case PieceType.Rook:
                            gamePhase += 2;
                            break;

                        case PieceType.Queen:
                            gamePhase += 4;
                            break;
                    }
                }
            }

            double endgamePhase = 1 - (gamePhase / 24.0);
            endgamePhase = Math.Clamp(endgamePhase, 0, 1);
            return endgamePhase;
        }

        public int Evaluate()
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
                    Piece piece = Squares[rank, file];
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
                            PieceType.King => 20000 + (int)((1 - endgamePhase()) * mgKingSquareTable[rankIndex, file] + endgamePhase() * egKingSquareTable[rankIndex, file]),
                            _ => 0
                        });
                    }
                }
            }
            return evaluation;
        }

        public int Search(int depth, PieceColor sideToMove, MoveInfo previousMoveInfo)
        {
            List<Move> moves = new List<Move>();
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    Piece piece = Squares[rank, file];
                    if (piece != null && piece.Color == sideToMove)
                    {
                        moves.AddRange(piece.GetLegalMoves(this, new Square(rank, file), previousMoveInfo));
                    }
                }
            }
            if (moves.Count == 0)
            {
                if (IsInCheck(sideToMove))
                {
                    return sideToMove == PieceColor.White ? int.MinValue + depth : int.MaxValue - depth;
                }
                return 0;
            }

            if (depth == 0)
            {
                return Evaluate();
            }

            int bestScore = sideToMove == PieceColor.White ? int.MinValue : int.MaxValue;
            foreach (Move move in moves)
            {
                Board nextMove = Clone();
                MoveInfo nextMoveInfo = nextMove.MakeMove(move, previousMoveInfo);
                int score = nextMove.Search(depth - 1, sideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White, nextMoveInfo);
                bestScore = sideToMove == PieceColor.White ? Math.Max(bestScore, score) : Math.Min(bestScore, score);
            }
            return bestScore;
        }
    }
}