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

            if (!Square.TryParse(moveString.Substring(0, 2), out Square from) || !Square.TryParse(moveString.Substring(2, 2), out Square to))
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

        public static List<Move> MVV_LVA(List<Move> unorderedMoves, Board board)
        {
            List<PieceType> agresors = Enum.GetValues(typeof(PieceType)).Cast<PieceType>().ToList();
            List<PieceType> victims = new List<PieceType>(agresors);
            victims.Remove(PieceType.King);
            victims = victims.OrderByDescending(pt => Piece.GetValue(pt)).ToList();
            agresors = agresors.OrderBy(pt => Piece.GetValue(pt)).ToList();
            List<Move> orderedMoves = new List<Move>();

            foreach (PieceType victim in victims)
            {
                foreach (PieceType agresor in agresors)
                {
                    foreach (Move move in unorderedMoves)
                    {
                        if (board.GetPiece(move.From) != null && board.GetPiece(move.From).Type == agresor &&
                            board.GetPiece(move.To) != null && board.GetPiece(move.To).Type == victim)
                        {
                            orderedMoves.Add(move);
                        }
                    }
                }
            }

            foreach (Move move in unorderedMoves)
            {
                if (!orderedMoves.Any(m => m.Equals(move)))
                {
                    orderedMoves.Add(move);
                }
            }

            return orderedMoves;
        }
    }

    public class MoveInfo
    {
        public Piece? TakenPiece { get; set; }
        public bool[] CastlingRights { get; set; } 
        public bool IsCastling { get; set; }
        public bool IsEnPassant { get; set; }
        public Square? EnPassantSquare { get; set; }
        public int HalfMoveClock { get; set; }
        public bool IsPromotion { get; set; }
        public Piece? PromotedPiece { get; set; }
        public ulong BoardHash { get; set; }
        public ulong PawnHash { get; set; }
        public MoveInfo(Piece? takenPiece = null)
        {
            TakenPiece = takenPiece;
            IsCastling = false;
            CastlingRights = [true, true, true, true];
            IsEnPassant = false;
            EnPassantSquare = null;
            HalfMoveClock = 0;
            IsPromotion = false;
            PromotedPiece = null;
        }
    }
}