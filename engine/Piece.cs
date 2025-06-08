namespace Engine
{
    public enum PieceType
{
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}
    public enum PieceColor
    {
        White,
        Black
    }

    public class Piece
    {
        public PieceType Type { get; set; }
        public PieceColor Color { get; set; }
        public Piece(PieceType type, PieceColor color)
        {
            Type = type;
            Color = color;
        }

        public List<Move> GetLegalMoves(Board originalBoard, Square positionOfPiece, MoveInfo previousMoveInfo)
        {
            List<Move> legalMoves = new List<Move>();
            foreach (Move move in GetMoves(originalBoard, positionOfPiece, previousMoveInfo))
            {
                Board boardAfterMove = originalBoard.Clone();
                boardAfterMove.MakeMove(move, previousMoveInfo);
                if (!boardAfterMove.IsInCheck(Color))
                {
                    legalMoves.Add(move);
                }
            }
            return legalMoves;
        }

        public List<Move> GetMoves(Board board, Square positionOfPiece, MoveInfo previousMoveInfo)
        {
            switch (Type)
            {
                case PieceType.Pawn:
                    return GetPawnMoves(board, positionOfPiece, previousMoveInfo);
                case PieceType.Knight:
                    return GetKnightMoves(board, positionOfPiece);
                case PieceType.Bishop:
                    return GetBishopMoves(board, positionOfPiece);
                case PieceType.Rook:
                    return GetRookMoves(board, positionOfPiece);
                case PieceType.Queen:
                    return GetQueenMoves(board, positionOfPiece);
                case PieceType.King:
                    return GetKingMoves(board, positionOfPiece, previousMoveInfo);
                default:
                    return new List<Move>();
            }
        }

        private List<Move> GetPawnMoves(Board board, Square positionOfPiece, MoveInfo previousMoveInfo)
        {
            List<Move> moves = new List<Move>();

            int direction = (Color == PieceColor.White) ? -1 : 1;
            Square forwardByOne = new Square(positionOfPiece.Rank + direction, positionOfPiece.File);
            if (forwardByOne.IsOnBoard() && board.IsEmpty(forwardByOne))
            {
                if (forwardByOne.Rank == (Color == PieceColor.White ? 0 : 7))
                {
                    moves.Add(new Move(positionOfPiece, forwardByOne, new Piece(PieceType.Queen, Color)));
                    moves.Add(new Move(positionOfPiece, forwardByOne, new Piece(PieceType.Rook, Color)));
                    moves.Add(new Move(positionOfPiece, forwardByOne, new Piece(PieceType.Bishop, Color)));
                    moves.Add(new Move(positionOfPiece, forwardByOne, new Piece(PieceType.Knight, Color)));
                }
                else
                {
                    moves.Add(new Move(positionOfPiece, forwardByOne));
                }

                if ((Color == PieceColor.White && positionOfPiece.Rank == 6) || (Color == PieceColor.Black && positionOfPiece.Rank == 1))
                {
                    Square forwardByTwo = new Square(positionOfPiece.Rank + 2 * direction, positionOfPiece.File);
                    if (forwardByTwo.IsOnBoard() && board.IsEmpty(forwardByTwo))
                    {
                        moves.Add(new Move(positionOfPiece, forwardByTwo));
                    }
                }
            }

            Square captureLeft = new Square(positionOfPiece.Rank + direction, positionOfPiece.File - 1);
            if (captureLeft.IsOnBoard() && (board.IsEnemy(captureLeft, Color) || captureLeft == previousMoveInfo.EnPassantSquareAfterMove))
            {
                if (captureLeft.Rank == (Color == PieceColor.White ? 0 : 7))
                {
                    moves.Add(new Move(positionOfPiece, captureLeft, new Piece(PieceType.Queen, Color)));
                    moves.Add(new Move(positionOfPiece, captureLeft, new Piece(PieceType.Rook, Color)));
                    moves.Add(new Move(positionOfPiece, captureLeft, new Piece(PieceType.Bishop, Color)));
                    moves.Add(new Move(positionOfPiece, captureLeft, new Piece(PieceType.Knight, Color)));
                }
                else
                {
                    moves.Add(new Move(positionOfPiece, captureLeft));
                }
            }

            Square captureRight = new Square(positionOfPiece.Rank + direction, positionOfPiece.File + 1);
            if (captureRight.IsOnBoard() && (board.IsEnemy(captureRight, Color) || captureRight == previousMoveInfo.EnPassantSquareAfterMove))
            {
                if (captureRight.Rank == (Color == PieceColor.White ? 0 : 7))
                {
                    moves.Add(new Move(positionOfPiece, captureRight, new Piece(PieceType.Queen, Color)));
                    moves.Add(new Move(positionOfPiece, captureRight, new Piece(PieceType.Rook, Color)));
                    moves.Add(new Move(positionOfPiece, captureRight, new Piece(PieceType.Bishop, Color)));
                    moves.Add(new Move(positionOfPiece, captureRight, new Piece(PieceType.Knight, Color)));
                }
                else
                {
                    moves.Add(new Move(positionOfPiece, captureRight));
                }
            }

            return moves;
        }

        private List<Move> GetKnightMoves(Board board, Square positionOfPiece)
        {
            List<Move> moves = new List<Move>();

            int[,] relativePositions = { { -1, 2 }, { 1, 2 }, { 2, 1 }, { 2, -1 }, { 1, -2 }, { -1, -2 }, { -2, -1 }, { -2, 1 } };
            for (int i = 0; i < relativePositions.GetLength(0); i++)
            {
                Square destination = new Square(positionOfPiece.Rank + relativePositions[i, 1], positionOfPiece.File + relativePositions[i, 0]);
                if (destination.IsOnBoard() && (board.IsEmpty(destination) || board.IsEnemy(destination, Color)))
                {
                    moves.Add(new Move(positionOfPiece, destination));
                }
            }

            return moves;
        }

        private List<Move> GetBishopMoves(Board board, Square positionOfPiece)
        {
            List<Move> moves = new List<Move>();

            int[,] relativePositions = { { 1, 1 }, { 1, -1 }, { -1, -1 }, { -1, 1 } };
            for (int i = 0; i < relativePositions.GetLength(0); i++)
            {
                Square destination = new Square(positionOfPiece.Rank + relativePositions[i, 1], positionOfPiece.File + relativePositions[i, 0]);
                int multiplier = 1;
                while (destination.IsOnBoard() && (board.IsEmpty(destination) || board.IsEnemy(destination, Color)))
                {
                    moves.Add(new Move(positionOfPiece, destination));

                    if (board.IsEnemy(destination, Color))
                    {
                        break;
                    }

                    multiplier++;
                    destination = new Square(positionOfPiece.Rank + relativePositions[i, 1] * multiplier, positionOfPiece.File + relativePositions[i, 0] * multiplier);
                }
            }

            return moves;
        }

        private List<Move> GetRookMoves(Board board, Square positionOfPiece)
        {
            List<Move> moves = new List<Move>();

            int[,] relativePositions = { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };
            for (int i = 0; i < relativePositions.GetLength(0); i++)
            {
                Square destination = new Square(positionOfPiece.Rank + relativePositions[i, 1], positionOfPiece.File + relativePositions[i, 0]);
                int multiplier = 1;
                while (destination.IsOnBoard() && (board.IsEmpty(destination) || board.IsEnemy(destination, Color)))
                {
                    moves.Add(new Move(positionOfPiece, destination));

                    if (board.IsEnemy(destination, Color))
                    {
                        break;
                    }

                    multiplier++;
                    destination = new Square(positionOfPiece.Rank + relativePositions[i, 1] * multiplier, positionOfPiece.File + relativePositions[i, 0] * multiplier);
                }
            }

            return moves;
        }

        private List<Move> GetQueenMoves(Board board, Square positionOfPiece)
        {
            List<Move> moves = new List<Move>();

            moves.AddRange(GetBishopMoves(board, positionOfPiece));
            moves.AddRange(GetRookMoves(board, positionOfPiece));

            return moves;
        }

        private List<Move> GetKingMoves(Board board, Square positionOfPiece, MoveInfo previousMoveInfo)
        {
            List<Move> moves = new List<Move>();

            int[] relativePosition = { -1, 0, 1 };
            for (int i = 0; i < relativePosition.Length; i++)
            {
                for (int j = 0; j < relativePosition.Length; j++)
                {
                    Square destination = new Square(positionOfPiece.Rank + j, positionOfPiece.File + i);
                    if (destination.IsOnBoard() && (board.IsEmpty(destination) || board.IsEnemy(destination, Color)))
                    {
                        moves.Add(new Move(positionOfPiece, destination));
                    }
                }
            }

            int rights = (Color == PieceColor.White) ? 0 : 2;
            int rank = (Color == PieceColor.White) ? 7 : 0;
            if (previousMoveInfo.CastlingRightsAfterMove[rights])
            {
                if (board.IsEmpty(new Square(rank, 5)) && board.IsEmpty(new Square(rank, 6)))
                {
                    moves.Add(new Move(positionOfPiece, new Square(rank, 6)));
                }
            }
            if (previousMoveInfo.CastlingRightsAfterMove[rights + 1])
            {
                if (board.IsEmpty(new Square(rank, 3)) && board.IsEmpty(new Square(rank, 2)) && board.IsEmpty(new Square(rank, 1)))
                {
                    moves.Add(new Move(positionOfPiece, new Square(rank, 2)));
                }
            }

            return moves;
        }

        public List<Move> GetAttackMoves(Square positionOfPiece)
        {
            List<Move> attackMoves = new List<Move>();
            switch (Type)
            {
                case PieceType.Pawn:
                    int direction = (Color == PieceColor.White) ? -1 : 1;
                    attackMoves.Add(new Move(positionOfPiece, new Square(positionOfPiece.Rank + direction, positionOfPiece.File - 1)));
                    attackMoves.Add(new Move(positionOfPiece, new Square(positionOfPiece.Rank + direction, positionOfPiece.File + 1)));
                    break;

                case PieceType.Knight:
                    int[,] relativePositions = { { -1, 2 }, { 1, 2 }, { 2, 1 }, { 2, -1 }, { 1, -2 }, { -1, -2 }, { -2, -1 }, { -2, 1 } };
                    for (int i = 0; i < relativePositions.GetLength(0); i++)
                    {
                        attackMoves.Add(new Move(positionOfPiece, new Square(positionOfPiece.Rank + relativePositions[i, 1], positionOfPiece.File + relativePositions[i, 0])));
                    }
                    break;

                case PieceType.Bishop:
                    int[,] bishopRelativePositions = { { 1, 1 }, { 1, -1 }, { -1, -1 }, { -1, 1 } };
                    attackMoves.AddRange(GetAttackMovesOfBRQ(positionOfPiece, bishopRelativePositions));
                    break;

                case PieceType.Rook:
                    int[,] rookRelativePositions = { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };
                    attackMoves.AddRange(GetAttackMovesOfBRQ(positionOfPiece, rookRelativePositions));
                    break;

                case PieceType.Queen:
                    int[,] queenRelativePositions = { { 1, 1 }, { 1, -1 }, { -1, -1 }, { -1, 1 }, { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };
                    attackMoves.AddRange(GetAttackMovesOfBRQ(positionOfPiece, queenRelativePositions));
                    break;

                case PieceType.King:
                    int[] kingRelativePositions = { -1, 0, 1 };
                    for (int i = 0; i < kingRelativePositions.Length; i++)
                    {
                        for (int j = 0; j < kingRelativePositions.Length; j++)
                        {
                            Square destination = new Square(positionOfPiece.Rank + kingRelativePositions[j], positionOfPiece.File + kingRelativePositions[i]);
                            if (destination.IsOnBoard())
                            {
                                attackMoves.Add(new Move(positionOfPiece, destination));
                            }
                        }
                    }
                    break;
            }
            return attackMoves;
        }

        private List<Move> GetAttackMovesOfBRQ(Square positionOfPiece, int[,] relativePositions)
        {
            List<Move> attackMoves = new List<Move>();
            for (int i = 0; i < relativePositions.GetLength(0); i++)
            {
                Square destination = new Square(positionOfPiece.Rank + relativePositions[i, 1], positionOfPiece.File + relativePositions[i, 0]);
                int multiplier = 1;
                while (destination.IsOnBoard())
                {
                    attackMoves.Add(new Move(positionOfPiece, destination));
                    multiplier++;
                    destination = new Square(positionOfPiece.Rank + relativePositions[i, 1] * multiplier, positionOfPiece.File + relativePositions[i, 0] * multiplier);
                }
            }
            return attackMoves;
        }
    }
}