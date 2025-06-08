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
            return piece.Color != color;
        }

        public MoveInfo MakeMove(Move move, MoveInfo previousMoveInfo)
        {
            Piece piece = GetPiece(move.From);
            if (piece == null || !piece.GetMoves(this, move.From, previousMoveInfo).Contains(move))
            {
                return null;
            }

            MoveInfo moveInfo = new MoveInfo(GetPiece(move.To));
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
                Squares[move.From.Rank, move.From.File] = new Piece(PieceType.Pawn, (move.To.Rank == 7) ? PieceColor.Black : PieceColor.White);
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

        public int Evaluate()
        {
            int evaluation = 0;
            int[,] pawnSquareTable =
            {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 50, 50, 50, 50, 50, 50, 50, 50 },
                { 10, 10, 20, 30, 30, 20, 10, 10 },
                { 5,  5, 10, 25, 25, 10,  5,  5 },
                { 0,  0,  0, 20, 20,  0,  0,  0 },
                { 5, -5,-10,  0,  0,-10, -5,  5},
                { 5, 10, 10,-20,-20, 10, 10,  5 },
                { 0,  0,  0,  0,  0,  0,  0,  0 }
            };

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Piece piece = Squares[rank, file];
                    if (piece != null && piece.Type != PieceType.King)
                    {
                        evaluation += (piece.Color == PieceColor.White ? 1 : -1) *
                        (piece.Type switch
                        {
                            PieceType.Pawn => 100,
                            PieceType.Knight => 320,
                            PieceType.Bishop => 330,
                            PieceType.Rook => 500,
                            PieceType.Queen => 900,
                            _ => 0
                        });
                    }
                }
            }
            return evaluation;
        }
    }
}