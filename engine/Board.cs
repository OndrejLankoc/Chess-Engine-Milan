class Board
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
}