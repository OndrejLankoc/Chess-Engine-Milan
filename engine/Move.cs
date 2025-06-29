namespace Engine
{
    public class Move
    {
        public Square From { get; set; }
        public Square To { get; set; }
        public Piece? PromotedPiece { get; set; }
        public Move(Square from, Square to, Piece? promotedPiece = null)
        {
            From = from;
            To = to;
            PromotedPiece = promotedPiece;
        }

        public bool TryParse(PieceColor color, string moveString, out Move move)
        {
            if (moveString.Length < 4 || moveString.Length > 5)
            {
                move = null!;
                return false;
            }

            int fromFile = moveString[0] - 'a';
            int fromRank = moveString[1] - '1';
            int toFile = moveString[2] - 'a';
            int toRank = moveString[3] - '1';

            Square from = new Square(fromRank, fromFile);
            Square to = new Square(toRank, toFile);

            if (!from.IsOnBoard() || !to.IsOnBoard())
            {
                move = null!;
                return false;
            }

            Piece? promotedPiece = null;
            if (moveString.Length == 5)
            {
                promotedPiece = moveString[4] switch
                {
                    'Q' => new Piece(PieceType.Queen, color),
                    'R' => new Piece(PieceType.Rook, color),
                    'B' => new Piece(PieceType.Bishop, color),
                    'N' => new Piece(PieceType.Knight, color),
                    _ => new Piece(PieceType.Queen, color)
                };
            }
            move = new Move(from, to, promotedPiece);
            return true;
        }

        public string ToString()
        {
            string fromFile = ((char)('a' + From.File)).ToString();
            string fromRank = (From.Rank + 1).ToString();
            string toFile = ((char)('a' + To.File)).ToString();
            string toRank = (To.Rank + 1).ToString();

            string moveString = $"{fromFile}{fromRank}{toFile}{toRank}";

            if (PromotedPiece != null)
            {
                moveString += PromotedPiece.Type switch
                {
                    PieceType.Queen => "Q",
                    PieceType.Rook => "R",
                    PieceType.Bishop => "B",
                    PieceType.Knight => "N"
                };
            }
            return moveString;
        }
    }

    public class MoveInfo
    {
        public Piece? TakenPiece { get; set; }
        public bool[] CastlingRights { get; set; } 
        public bool[] CastlingRightsAfterMove { get; set; }
        public bool IsCastling { get; set; }
        public Square? EnPassantSquare { get; set; }
        public Square? EnPassantSquareAfterMove { get; set; }
        public bool IsEnPassant { get; set; }
        public bool IsPromotion { get; set; }
        public bool IsPawnMove { get; set; }
        public Piece? PromotedPiece { get; set; }
        public MoveInfo(Piece? takenPiece = null)
        {
            TakenPiece = takenPiece;
            CastlingRights = [true, true, true, true];
            CastlingRightsAfterMove = [true, true, true, true]; // [0] = White Kingside, [1] = White Queenside, [2] = Black Kingside, [3] = Black Queenside
            IsCastling = false;
            EnPassantSquare = null;
            EnPassantSquareAfterMove = null;
            IsEnPassant = false;
            IsPromotion = false;
            IsPawnMove = false;
            PromotedPiece = null;
        }
    }
}