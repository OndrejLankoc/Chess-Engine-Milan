public enum PiecesType
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
    public PiecesType Type { get; set; }
    public PieceColor Color { get; set; }

    public Piece(PiecesType type, PieceColor color)
    {
        Type = type;
        Color = color;
    }

    List<Move> GetMoves(Board board, Square positionOfPiece)
    {
        switch (Type)
        {
            case PiecesType.Pawn:
                return GetPawnMoves(board, positionOfPiece);
            case PiecesType.Knight:
                return GetKnightMoves(board, positionOfPiece);
            case PiecesType.Bishop:
                return GetBishopMoves(board, positionOfPiece);
            case PiecesType.Rook:
                return GetRookMoves(board, positionOfPiece);
            case PiecesType.Queen:
                return GetQueenMoves(board, positionOfPiece);
            case PiecesType.King:
                return GetKingMoves(board, positionOfPiece);
            default:
                return new List<Move>();
        }
    }

    private List<Move> GetPawnMoves(Board board, Square positionOfPiece)
    {
        List<Move> moves = new List<Move>();

        int direction = 0;
        if (Color == PieceColor.White)
        {
            direction =1;
        }
        else
        {
            direction = -1;
        }

        Square forwardByOne = new Square(positionOfPiece.File, positionOfPiece.Rank + direction);
        if (forwardByOne.IsOnBoard() && board.IsEmpty(forwardByOne))
        {
            moves.Add(new Move(positionOfPiece, forwardByOne));

            if ((Color == PieceColor.White && positionOfPiece.Rank == 1) || (Color == PieceColor.Black && positionOfPiece.Rank == 6))
            {
                Square forwardByTwo = new Square(positionOfPiece.File, positionOfPiece.Rank + 2 * direction);
                if (forwardByTwo.IsOnBoard() && board.IsEmpty(forwardByTwo))
                {
                    moves.Add(new Move(positionOfPiece, forwardByTwo));
                }
            }
        }

        Square captureLeft = new Square(positionOfPiece.File - 1, positionOfPiece.Rank + direction);
        if (captureLeft.IsOnBoard() && board.IsEnemy(captureLeft, Color))
        {
            moves.Add(new Move(positionOfPiece, captureLeft));
        }

        Square captureRight = new Square(positionOfPiece.File + 1, positionOfPiece.Rank + direction);
        if (captureRight.IsOnBoard() && board.IsEnemy(captureRight, Color))
        {
            moves.Add(new Move(positionOfPiece, captureRight));
        }

        return moves;
    }

    private List<Move> GetKnightMoves(Board board, Square positionOfPiece)
    {
        List<Move> moves = new List<Move>();

        int[,] relativePositions = { {-1, 2}, {1, 2}, {2, 1}, {2, -1}, {1, -2}, {-1, -2}, {-2, -1}, {-2, 1} };
        for (int i = 0; i < relativePositions.GetLength(0); i++)
        {
            Square destination = new Square(positionOfPiece.File + relativePositions[i, 0], positionOfPiece.Rank + relativePositions[i, 1]);
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
        
        int[,] relativePositions = { {1, 1}, {1, -1}, {-1, -1}, {-1, 1} };
        for (int i = 0; i < relativePositions.GetLength(0); i++)
        {
            Square destination = new Square(positionOfPiece.File + relativePositions[i, 0], positionOfPiece.Rank + relativePositions[i, 1]);
            int multiplier = 1;
            while (destination.IsOnBoard() && (board.IsEmpty(destination) || board.IsEnemy(destination, Color)))
            {
                moves.Add(new Move(positionOfPiece, destination));

                if (board.IsEnemy(destination, Color))
                {
                    break;
                }

                multiplier++;
                destination = new Square(positionOfPiece.File + relativePositions[i, 0] * multiplier, positionOfPiece.Rank + relativePositions[i, 1] * multiplier); 
            }
        }

        return moves;
    }

    private List<Move> GetRookMoves(Board board, Square positionOfPiece)
    {
        List<Move> moves = new List<Move>();

        int[,] relativePositions = { {0, 1}, {1, 0}, {0, -1}, {-1, 0} };
        for (int i = 0; i < relativePositions.GetLength(0); i++)
        {
            Square destination = new Square(positionOfPiece.File + relativePositions[i, 0], positionOfPiece.Rank + relativePositions[i, 1]);
            int multiplier = 1;
            while (destination.IsOnBoard() && (board.IsEmpty(destination) || board.IsEnemy(destination, Color)))
            {
                moves.Add(new Move(positionOfPiece, destination));

                if (board.IsEnemy(destination, Color))
                {
                    break;
                }

                multiplier++;
                destination = new Square(positionOfPiece.File + relativePositions[i, 0] * multiplier, positionOfPiece.Rank + relativePositions[i, 1] * multiplier); 
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

    private List<Move> GetKingMoves(Board board, Square positionOfPiece)
    {
        List<Move> moves = new List<Move>();

        int[] relativePosition = { -1, 0, 1 };
        for (int i = 0; i < relativePosition.Length; i++)
        {
            for (int j = 0; j < relativePosition.Length; j++)
            {
                Square destination = new Square(positionOfPiece.File + i, positionOfPiece.Rank + j);
                if (destination.IsOnBoard() && (board.IsEmpty(destination) || board.IsEnemy(destination, Color)))
                {
                    moves.Add(new Move(positionOfPiece, destination));
                }
            }
        }

        return moves;
    }

}