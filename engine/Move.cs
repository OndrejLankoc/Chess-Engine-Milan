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
    }

    public class MoveInfo
    {
        public Piece TakenPiece { get; set; }
        public bool[] CastlingRights { get; set; } 
        public bool[] CastlingRightsAfterMove { get; set; }
        public bool IsCastling { get; set; }
        public Square? EnPassantSquare { get; set; }
        public Square? EnPassantSquareAfterMove { get; set; }
        public bool IsEnPassant { get; set; }
        public bool IsPromotion { get; set; }
        public Piece? PromotedPiece { get; set; }
        public MoveInfo(Piece takenPiece)
        {
            TakenPiece = takenPiece;
            CastlingRights = [true, true, true, true];
            CastlingRightsAfterMove = [true, true, true, true]; // [0] = White Kingside, [1] = White Queenside, [2] = Black Kingside, [3] = Black Queenside
            IsCastling = false;
            EnPassantSquare = null;
            EnPassantSquareAfterMove = null;
            IsEnPassant = false;
            IsPromotion = false;
            PromotedPiece = null;
        }
    }
}