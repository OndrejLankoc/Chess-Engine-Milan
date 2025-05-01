namespace Engine
{
    public class Board
    {
        public Piece[,] Squares = new Piece[8, 8];

        public Piece GetPiece(Square square)
        {
            if (!square.IsOnBoard())
            {
                return null;
            }
            return Squares[square.File, square.Rank];
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
            Squares[move.To.File, move.To.Rank] = piece;
            Squares[move.From.File, move.From.Rank] = null;

            if (piece.Type == PieceType.King)
            {
                moveInfo.CastlingRightsAfterMove[(piece.Color == PieceColor.White) ? 0 : 2] = moveInfo.CastlingRightsAfterMove[(piece.Color == PieceColor.White) ? 1 : 3] = false;
                if (Math.Abs(move.From.File - move.To.File) == 2)
                {
                    int rank = (piece.Color == PieceColor.White) ? 0 : 7;
                    int[] files = (move.To.File > move.From.File) ? new[] { 7, 5 } : new[] { 0, 3 };
                    Squares[files[1], rank] = Squares[files[0], rank];
                    Squares[files[0], rank] = null;
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
                    moveInfo.EnPassantSquareAfterMove = new Square(move.To.File, move.To.Rank + (piece.Color == PieceColor.White ? -1 : 1));
                }

                int rank = (piece.Color == PieceColor.White) ? 7 : 0;
                if (move.To.Rank == rank)
                {
                    Squares[move.To.File, move.To.Rank] = move.PromotedPiece;
                    moveInfo.IsPromotion = true;
                    moveInfo.PromotedPiece = move.PromotedPiece;
                }

                if (moveInfo.EnPassantSquare == move.To)
                {
                    Squares[move.To.File, move.To.Rank + (piece.Color == PieceColor.White ? -1 : 1)] = null;
                    moveInfo.IsEnPassant = true;
                }
            }

            return moveInfo;
        }

        public void UndoMove(Move move, MoveInfo moveInfo)
        {
            Squares[move.From.File, move.From.Rank] = Squares[move.To.File, move.To.Rank];
            Squares[move.To.File, move.To.Rank] = moveInfo.TakenPiece;

            if (moveInfo.IsCastling)
            {
                Square rookSquareTo = new Square((move.To.File == 6) ? 5 : 3, (move.To.Rank == 0) ? 0 : 7);
                Square rookSquareFrom = new Square((move.To.File == 6) ? 7 : 0, (move.To.Rank == 0) ? 0 : 7);
                Squares[rookSquareFrom.File, rookSquareFrom.Rank] = Squares[rookSquareTo.File, rookSquareTo.Rank];
                Squares[rookSquareTo.File, rookSquareTo.Rank] = null;   
            }

            if (moveInfo.IsEnPassant)
            {
                Squares[move.To.File, move.To.Rank + ((move.From.Rank == 4) ? 1 : -1)] = new Piece(PieceType.Pawn, (move.From.Rank == 4) ? PieceColor.Black : PieceColor.White);
            }

            if (moveInfo.IsPromotion)
            {
                Squares[move.From.File, move.From.Rank] = new Piece(PieceType.Pawn, (move.To.Rank == 7) ? PieceColor.White : PieceColor.Black);
            }
        }
    }
}