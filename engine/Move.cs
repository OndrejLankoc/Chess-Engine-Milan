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

        public static List<Move> OrderMoves(List<Move> unorderedMoves, Move?[,] killers, int[,,] history, Board board, int ply)
        {
            Dictionary<Move, int> moveScores = new Dictionary<Move, int>();
            foreach (Move move in unorderedMoves)
            {
                int score = 0;

                // Promotion
                if (move.PromotedPiece != null) score += 10000 + Piece.GetValue(move.PromotedPiece.Type);

                // MVV-LVA
                Piece? agresor = board.GetPiece(move.From);
                Piece? victim = board.GetPiece(move.To);
                if (victim != null && agresor != null) score += 9000 + (int)victim.Type * 10 - (int)agresor.Type;

                if (board.IsMoveQuiet(move))
                {
                    // Killers
                    if (killers[ply, 0] != null && move.Equals(killers[ply, 0]!)) score += 8000;
                    else if (killers[ply, 1] != null && move.Equals(killers[ply, 1]!)) score += 7000;

                    //History
                    int from = move.From.Rank * 8 + move.From.File;
                    int to = move.To.Rank * 8 + move.To.File;
                    score += history[(int)board.SideToMove, from, to];
                }

                moveScores[move] = score;
            }
            
            List<Move> orderedMoves = moveScores.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
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