namespace Engine
{
    public enum GameResult
    {
        WhiteWin,
        BlackWin,
        Draw,
        Ongoing
    }
    public class Board
    {
        public Piece?[,] Squares = new Piece?[8, 8];
        public bool[] CastlingRights = { true, true, true, true }; // [0] = White Kingside, [1] = White Queenside, [2] = Black Kingside, [3] = Black Queenside
        public Square? EnPassantSquare;
        public PieceColor SideToMove = PieceColor.White;
        public int HalfMoveClock = 0;
        public int FullMoveNumber = 1;
        public ulong BoardHash;
        public ulong PawnHash;

        public void SetupBoard(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            try
            {
                string[] fenParts = fen.Split(' ');
                if (fenParts.Length != 6)
                {
                    throw new ArgumentException("Invalid FEN string: incorrect number of parts.");
                }

                string[] ranks = fenParts[0].Split('/');
                if (ranks.Length != 8)
                {
                    throw new ArgumentException("Invalid FEN string: incorrect number of ranks.");
                }

                for (int rank = 0; rank < 8; rank++)
                {
                    int skippedFiles = 0;
                    for (int file = 0; file + skippedFiles < 8; file++)
                    {
                        if (int.TryParse(ranks[rank][file].ToString(), out int emptySquares))
                        {
                            skippedFiles += emptySquares - 1;
                            continue;
                        }

                        PieceType type = char.ToLower(ranks[rank][file]) switch
                        {
                            'p' => PieceType.Pawn,
                            'n' => PieceType.Knight,
                            'b' => PieceType.Bishop,
                            'r' => PieceType.Rook,
                            'q' => PieceType.Queen,
                            'k' => PieceType.King,
                            _ => throw new ArgumentException($"Invalid piece type: {ranks[rank][file]}")
                        };
                        PieceColor color = char.IsLower(ranks[rank][file]) ? PieceColor.Black : PieceColor.White;
                        Squares[rank, file + skippedFiles] = new Piece(type, color);
                    }
                }

                SideToMove = fenParts[1] == "w" ? PieceColor.White : fenParts[1] == "b" ? PieceColor.Black : throw new ArgumentException($"Invalid side to move: {fenParts[1]}");

                CastlingRights[0] = fenParts[2].Contains('K');
                CastlingRights[1] = fenParts[2].Contains('Q');
                CastlingRights[2] = fenParts[2].Contains('k');
                CastlingRights[3] = fenParts[2].Contains('q');

                if (fenParts[3] != "-")
                {
                    if (!Square.TryParse(fenParts[3], out EnPassantSquare))
                    {
                        throw new ArgumentException($"Invalid en passant square: {fenParts[3]}");
                    }
                }
                else
                {
                    EnPassantSquare = null;
                }

                if (!int.TryParse(fenParts[4], out HalfMoveClock) || HalfMoveClock < 0)
                {
                    throw new ArgumentException($"Invalid half-move clock: {fenParts[4]}");
                }

                if (!int.TryParse(fenParts[5], out FullMoveNumber) || FullMoveNumber < 1)
                {
                    throw new ArgumentException($"Invalid full-move number: {fenParts[5]}");
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            BoardHash = ComputeHash();
            PawnHash = ComputePawnHash();
        }

        public Board Clone()
        {
            Board cloneBoard = new Board
            {
                SideToMove = SideToMove,
                EnPassantSquare = EnPassantSquare,
                CastlingRights = (bool[])CastlingRights.Clone(),
                HalfMoveClock = HalfMoveClock,
                FullMoveNumber = FullMoveNumber,
                BoardHash = BoardHash,
                PawnHash = PawnHash
            };

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Square square = new Square(rank, file);
                    if (GetPiece(square) != null)
                    {
                        cloneBoard.Squares[rank, file] = new Piece(GetPiece(square)!.Type, GetPiece(square)!.Color);
                    }
                }
            }
            return cloneBoard;
        }

        public bool Equals(Board secondBoard)
        {
            if (secondBoard.BoardHash != BoardHash || secondBoard.PawnHash != PawnHash)
            {
                return false;
            }
            
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Piece? a = GetPiece(new Square(rank, file));
                    Piece? b = secondBoard.GetPiece(new Square(rank, file));

                    if (a == null && b == null)
                    {
                        continue;
                    }
                    if (a == null || b == null || a.Type != b.Type || a.Color != b.Color)
                    {
                        return false;
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (CastlingRights[i] != secondBoard.CastlingRights[i])
                {
                    return false;
                }
            }

            if (SideToMove != secondBoard.SideToMove)
            {
                return false;
            }

            if (EnPassantSquare == null && secondBoard.EnPassantSquare != null ||
                EnPassantSquare != null && secondBoard.EnPassantSquare == null ||
                EnPassantSquare != null && secondBoard.EnPassantSquare != null &&
                (EnPassantSquare.File != secondBoard.EnPassantSquare.File || EnPassantSquare.Rank != secondBoard.EnPassantSquare.Rank))
            {
                return false;
            }

            return true;
        }

        public Piece? GetPiece(Square square)
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
            Piece? piece = GetPiece(square);
            return piece != null && piece.Color != color;
        }

        public MoveInfo MakeMove(Move move)
        {
            Piece? piece = GetPiece(move.From);
            if (piece == null)
            {
                return null;
            }

            MoveInfo moveInfo = new MoveInfo(GetPiece(move.To))
            {
                CastlingRights = (bool[])CastlingRights.Clone(),
                EnPassantSquare = EnPassantSquare,
                HalfMoveClock = HalfMoveClock,
                BoardHash = BoardHash,
                PawnHash = PawnHash
            };
            Squares[move.To.Rank, move.To.File] = piece;
            Squares[move.From.Rank, move.From.File] = null;

            int pieceType = (piece.Color == PieceColor.Black ? 6 : 0) + (int)piece.Type;
            int squareIndex = move.From.Rank * 8 + move.From.File;
            BoardHash ^= Hash.PieceSquare[pieceType, squareIndex];

            if (piece.Type == PieceType.Pawn) PawnHash ^= Hash.PawnSquare[piece.Color == PieceColor.White ? 0 : 1, squareIndex];

            if (moveInfo.TakenPiece != null)
            {
                int takenPieceType = (moveInfo.TakenPiece.Color == PieceColor.Black ? 6 : 0) + (int)moveInfo.TakenPiece.Type;
                squareIndex = move.To.Rank * 8 + move.To.File;
                BoardHash ^= Hash.PieceSquare[takenPieceType, squareIndex];

                if (moveInfo.TakenPiece.Type == PieceType.Pawn)
                {
                    PawnHash ^= Hash.PawnSquare[moveInfo.TakenPiece.Color == PieceColor.White ? 0 : 1, squareIndex];
                }
            }
            squareIndex = move.To.Rank * 8 + move.To.File;
            BoardHash ^= Hash.PieceSquare[pieceType, squareIndex];

            if (piece.Type == PieceType.Pawn) PawnHash ^= Hash.PawnSquare[piece.Color == PieceColor.White ? 0 : 1, squareIndex];

            if (piece.Type == PieceType.King)
            {
                int[] castlingIndex = (piece.Color == PieceColor.White) ? new[] { 0, 1 } : new[] { 2, 3 };
                BoardHash ^= CastlingRights[castlingIndex[0]] ? Hash.CastlingRights[castlingIndex[0]] : 0;
                BoardHash ^= CastlingRights[castlingIndex[1]] ? Hash.CastlingRights[castlingIndex[1]] : 0;
                CastlingRights[castlingIndex[0]] = CastlingRights[castlingIndex[1]] = false;

                if (Math.Abs(move.From.File - move.To.File) == 2)
                {
                    int rank = (piece.Color == PieceColor.White) ? 7 : 0;
                    int[] files = (move.To.File > move.From.File) ? new[] { 7, 5 } : new[] { 0, 3 };
                    Squares[rank, files[1]] = Squares[rank, files[0]];
                    Squares[rank, files[0]] = null;
                    moveInfo.IsCastling = true;

                    pieceType = (piece.Color == PieceColor.Black ? 6 : 0) + (int)PieceType.Rook;
                    squareIndex = rank * 8 + files[0];
                    BoardHash ^= Hash.PieceSquare[pieceType, squareIndex];
                    squareIndex = rank * 8 + files[1];
                    BoardHash ^= Hash.PieceSquare[pieceType, squareIndex];
                }
            }

            if (piece.Type == PieceType.Rook)
            {
                int i = -1;
                if (move.From.File == 7 && (move.From.Rank == 7 || move.From.Rank == 0))
                {
                    i += 1;
                }
                if (move.From.File == 0 && (move.From.Rank == 7 || move.From.Rank == 0))
                {
                    i += 2;
                }
                if (i >= 0)
                {
                    i += piece.Color == PieceColor.White ? 0 : 2;
                    BoardHash ^= CastlingRights[i] ? Hash.CastlingRights[i] : 0;
                    CastlingRights[i] = false;
                }
            }

            if (piece.Type == PieceType.Pawn)
            {
                int rank = (piece.Color == PieceColor.White) ? 0 : 7;
                if (move.To.Rank == rank)
                {
                    Squares[move.To.Rank, move.To.File] = move.PromotedPiece;
                    moveInfo.IsPromotion = true;
                    moveInfo.PromotedPiece = move.PromotedPiece;

                    pieceType = (piece.Color == PieceColor.Black ? 6 : 0) + (int)PieceType.Pawn;
                    squareIndex = move.To.Rank * 8 + move.To.File;
                    BoardHash ^= Hash.PieceSquare[pieceType, squareIndex];
                    pieceType = (piece.Color == PieceColor.Black ? 6 : 0) + (int)move.PromotedPiece!.Type;
                    BoardHash ^= Hash.PieceSquare[pieceType, squareIndex];

                    PawnHash ^= Hash.PawnSquare[piece.Color == PieceColor.White ? 0 : 1, squareIndex];
                }

                if (EnPassantSquare != null && EnPassantSquare.File == move.To.File && EnPassantSquare.Rank == move.To.Rank)
                {
                    Piece capturedPawn = GetPiece(new Square(move.To.Rank + (piece.Color == PieceColor.White ? 1 : -1), move.To.File))!;
                    int capturedPawnType = (capturedPawn.Color == PieceColor.Black ? 6 : 0) + (int)capturedPawn.Type;
                    squareIndex = (move.To.Rank + (piece.Color == PieceColor.White ? 1 : -1)) * 8 + move.To.File;
                    BoardHash ^= Hash.PieceSquare[capturedPawnType, squareIndex];

                    PawnHash ^= Hash.PawnSquare[capturedPawn.Color == PieceColor.White ? 0 : 1, squareIndex];

                    Squares[move.To.Rank + (piece.Color == PieceColor.White ? 1 : -1), move.To.File] = null;
                    moveInfo.IsEnPassant = true;
                }
            }

            if (EnPassantSquare != null)
            {
                BoardHash ^= Hash.EnPassantFile[EnPassantSquare.File];
            }
            EnPassantSquare = null;

            if (piece.Type == PieceType.Pawn && Math.Abs(move.From.Rank - move.To.Rank) == 2)
            {
                EnPassantSquare = new Square(move.To.Rank + (piece.Color == PieceColor.White ? 1 : -1), move.To.File);
                BoardHash ^= Hash.EnPassantFile[EnPassantSquare.File];
            }

            List<Square> cornerSquares = new List<Square>
            {
                new Square(7, 7),
                new Square(7, 0),
                new Square(0, 7),
                new Square(0, 0)
            };
            if (moveInfo.TakenPiece != null && moveInfo.TakenPiece.Type == PieceType.Rook && cornerSquares.Contains(move.To))
            {
                int i = cornerSquares.IndexOf(move.To);
                BoardHash ^= CastlingRights[i] ? Hash.CastlingRights[i] : 0;
                CastlingRights[i] = false;
            }

            FullMoveNumber = (SideToMove == PieceColor.Black) ? FullMoveNumber + 1 : FullMoveNumber;
            HalfMoveClock = (piece.Type == PieceType.Pawn || moveInfo.TakenPiece != null) ? 0 : HalfMoveClock + 1;
            SideToMove = (SideToMove == PieceColor.White) ? PieceColor.Black : PieceColor.White;
            BoardHash ^= Hash.SideToMove;

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
                Squares[move.From.Rank, move.From.File] = new Piece(PieceType.Pawn, (Squares[move.From.Rank, move.From.File]!.Color == PieceColor.Black) ? PieceColor.Black : PieceColor.White);
            }

            CastlingRights = moveInfo.CastlingRights;
            EnPassantSquare = moveInfo.EnPassantSquare;
            SideToMove = (SideToMove == PieceColor.White) ? PieceColor.Black : PieceColor.White;
            HalfMoveClock = moveInfo.HalfMoveClock;
            FullMoveNumber = (SideToMove == PieceColor.Black) ? FullMoveNumber - 1 : FullMoveNumber;
            BoardHash = moveInfo.BoardHash;
            PawnHash = moveInfo.PawnHash;
        }

        public Square GetKingPosition(PieceColor color)
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Square square = new Square(rank, file);
                    if (GetPiece(square) != null && GetPiece(square)!.Type == PieceType.King && GetPiece(square)!.Color == color)
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
            return IsSquareAttacked(kingPosition, color == PieceColor.White ? PieceColor.Black : PieceColor.White);
        }

        public bool IsSquareAttacked(Square square, PieceColor attackerColor)
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Square attackerSquare = new Square(rank, file);
                    Piece? attacker = GetPiece(attackerSquare);
                    if (attacker != null && attacker.Color == attackerColor)
                    {
                        List<Move> attackMoves = attacker.GetAttackMoves(attackerSquare, this);
                        foreach (Move attackMove in attackMoves)
                        {
                            if (attackMove.To.Rank == square.Rank && attackMove.To.File == square.File)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public GameResult Result(List<MoveInfo> listOfAllMovesInfo, List<Move> listOfAllMoves, out string reason, int legalMovesCount = -1)
        {
            reason = null!;
            if (legalMovesCount == -1)
            {
                List<Move> moves = new List<Move>();
                for (int rank = 0; rank < 8; rank++)
                {
                    for (int file = 0; file < 8; file++)
                    {
                        Piece? piece = GetPiece(new Square(rank, file));
                        if (piece != null && piece.Color == SideToMove)
                        {
                            moves.AddRange(piece.GetLegalMoves(this, new Square(rank, file)));
                        }
                    }
                }
                legalMovesCount = moves.Count;
            }

            if (legalMovesCount == 0)
            {
                if (IsInCheck(SideToMove))
                {
                    reason = "Checkmate";
                    return SideToMove == PieceColor.White ? GameResult.BlackWin : GameResult.WhiteWin;
                }

                reason = "Stalemate";
                return GameResult.Draw;
            }

            int pieceCount = 0;
            foreach (Piece? piece in Squares)
            {
                if (piece != null && piece.Type != PieceType.King && piece.Type != PieceType.Knight && piece.Type != PieceType.Bishop)
                {
                    pieceCount = 4;
                    break;
                }
                else if (piece != null)
                {
                    pieceCount++;
                }
            }
            if (pieceCount <= 3)
            {
                reason = "Insufficient material";
                return GameResult.Draw;
            }

            if (FullMoveNumber >= 4 && HalfMoveClock >= 6)
            {
                Board previousPositions = Clone();
                int positionCount = 1;

                for (int i = listOfAllMoves.Count - 1; i >= 0; i--)
                {
                    Move move = listOfAllMoves[i];
                    MoveInfo moveInfo = listOfAllMovesInfo[i];
                    previousPositions.UndoMove(move, moveInfo);
                    if (Equals(previousPositions))
                    {
                        positionCount++;
                        if (positionCount >= 3)
                        {
                            reason = "Threefold repetition";
                            return GameResult.Draw;
                        }
                    }
                }
            }

            if (HalfMoveClock >= 100)
            {
                reason = "Fifty-move rule";
                return GameResult.Draw;
            }

            return GameResult.Ongoing;
        }

        public bool IsMoveLegal(Move move)
        {
            Piece? piece = GetPiece(move.From);
            if (piece == null || piece.Color != SideToMove)
            {
                return false;
            }

            List<Move> legalMoves = piece.GetLegalMoves(this, move.From);
            foreach (Move legalMove in legalMoves)
            {
                if (move.Equals(legalMove))
                {
                    return true;
                }
            }
            return false;
        }

        public double EndgamePhase()
        {
            int gamePhase = 0;
            foreach (Piece? piece in Squares)
            {
                if (piece != null)
                {
                    switch (piece.Type)
                    {
                        case PieceType.Knight:
                        case PieceType.Bishop:
                            gamePhase += 1;
                            break;

                        case PieceType.Rook:
                            gamePhase += 2;
                            break;

                        case PieceType.Queen:
                            gamePhase += 4;
                            break;
                    }
                }
            }

            double endgamePhase = 1 - (gamePhase / 24.0);
            endgamePhase = Math.Clamp(endgamePhase, 0, 1);
            return endgamePhase;
        }

        public bool IsPawnPassed(Square pawnSquare)
        {
            Piece? pawn = GetPiece(pawnSquare);
            if (pawn == null || pawn.Type != PieceType.Pawn)
            {
                return false;
            }
            int direction = (pawn.Color == PieceColor.White) ? -1 : 1;

            for (int rank = pawnSquare.Rank + direction; rank > 0 && rank < 7; rank += direction)
            {
                Piece? inFrontPiece = GetPiece(new Square(rank, pawnSquare.File));
                if (inFrontPiece != null && inFrontPiece.Type == PieceType.Pawn && inFrontPiece.Color != pawn.Color) return false;

                if (pawnSquare.File > 0)
                {
                    Piece? leftPiece = GetPiece(new Square(rank, pawnSquare.File - 1));
                    if (leftPiece != null && leftPiece.Type == PieceType.Pawn && leftPiece.Color != pawn.Color) return false;
                }
                if (pawnSquare.File < 7)
                {
                    Piece? rightPiece = GetPiece(new Square(rank, pawnSquare.File + 1));
                    if (rightPiece != null && rightPiece.Type == PieceType.Pawn && rightPiece.Color != pawn.Color) return false;
                }
            }

            return true;
        }

        public ulong ComputeHash()
        {
            ulong hash = 0;

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    Piece? piece = GetPiece(new Square(rank, file));
                    if (piece != null)
                    {
                        int type = piece.Color == PieceColor.Black ? 6 : 0;
                        type += (int)piece.Type;
                        int square = rank * 8 + file;
                        hash ^= Hash.PieceSquare[type, square];
                    }
                }
            }

            if (SideToMove == PieceColor.White)
            {
                hash ^= Hash.SideToMove;
            }

            for (int i = 0; i < 4; i++)
            {
                if (CastlingRights[i])
                {
                    hash ^= Hash.CastlingRights[i];
                }
            }

            if (EnPassantSquare != null)
            {
                hash ^= Hash.EnPassantFile[EnPassantSquare.File];
            }

            return hash;
        }

        public ulong ComputePawnHash()
        {
            ulong hash = 0;

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    Piece? piece = GetPiece(new Square(rank, file));
                    if (piece != null && piece.Type == PieceType.Pawn)
                    {
                        int color = piece.Color == PieceColor.White ? 0 : 1;
                        int square = rank * 8 + file;
                        hash ^= Hash.PawnSquare[color, square];
                    }
                }
            }

            return hash;
        }
    }
}