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

        public bool Equals(Move secondMove)
        {
            if (From.Rank != secondMove.From.Rank || From.File != secondMove.From.File || To.Rank != secondMove.To.Rank || To.File != secondMove.To.File)
            {
                return false;
            }

            if (PromotedPiece == null && secondMove.PromotedPiece == null)
            {
                return true;
            }
            if (PromotedPiece == null || secondMove.PromotedPiece == null || PromotedPiece.Type != secondMove.PromotedPiece.Type || PromotedPiece.Color != secondMove.PromotedPiece.Color)
            {
                return false;
            }
            return true;
        }

        public static bool TryParse(PieceColor color, string moveString, out Move move)
        {
            if (moveString.Length < 4 || moveString.Length > 5)
            {
                move = null!;
                return false;
            }

            int fromFile = moveString[0] - 'a';
            int fromRank = '8' - moveString[1];
            int toFile = moveString[2] - 'a';
            int toRank = '8' - moveString[3];

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
            string fromRank = (8 - From.Rank).ToString();
            string toFile = ((char)('a' + To.File)).ToString();
            string toRank = (8 - To.Rank).ToString();

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
        public bool IsCastling { get; set; }
        public bool IsEnPassant { get; set; }
        public bool IsPromotion { get; set; }
        public bool IsPawnMove { get; set; }
        public Piece? PromotedPiece { get; set; }
        public MoveInfo(Piece? takenPiece = null)
        {
            TakenPiece = takenPiece;
            IsCastling = false;
            IsEnPassant = false;
            IsPromotion = false;
            IsPawnMove = false;
            PromotedPiece = null;
        }
    }
}