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
        public Piece[,] Squares = new Piece[8, 8];
        public bool[] castlingRights = { true, true, true, true }; // [0] = White Kingside, [1] = White Queenside, [2] = Black Kingside, [3] = Black Queenside
        public Square? enPassantSquare;
        public PieceColor sideToMove = PieceColor.White;
        public int halfMoveClock = 0;
        public int fullMoveNumber = 1;

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

                sideToMove = fenParts[1] == "w" ? PieceColor.White : fenParts[1] == "b" ? PieceColor.Black : throw new ArgumentException($"Invalid side to move: {fenParts[1]}");

                castlingRights[0] = fenParts[2].Contains('K');
                castlingRights[1] = fenParts[2].Contains('Q');
                castlingRights[2] = fenParts[2].Contains('k');
                castlingRights[3] = fenParts[2].Contains('q');

                if (fenParts[3] != "-")
                {
                    if (!Square.TryParse(fenParts[3], out enPassantSquare))
                    {
                        throw new ArgumentException($"Invalid en passant square: {fenParts[3]}");
                    }
                }

                if (!int.TryParse(fenParts[4], out halfMoveClock) || halfMoveClock < 0)
                {
                    throw new ArgumentException($"Invalid half-move clock: {fenParts[4]}");
                }

                if (!int.TryParse(fenParts[5], out fullMoveNumber) || fullMoveNumber < 1)
                {
                    throw new ArgumentException($"Invalid full-move number: {fenParts[5]}");
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }

        public Board Clone()
        {
            Board cloneBoard = new Board();
            cloneBoard.sideToMove = sideToMove;
            cloneBoard.enPassantSquare = enPassantSquare;
            cloneBoard.castlingRights = (bool[])castlingRights.Clone();

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    if (Squares[rank, file] != null)
                    {
                        cloneBoard.Squares[rank, file] = new Piece(Squares[rank, file].Type, Squares[rank, file].Color);
                    }
                }
            }
            return cloneBoard;
        }

        public bool Equals(Board secondBoard)
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Piece a = Squares[rank, file];
                    Piece b = secondBoard.Squares[rank, file];

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
            return true;
        }

        public Piece GetPiece(Square square)
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
            Piece piece = GetPiece(square);
            return piece != null && piece.Color != color;
        }

        public MoveInfo MakeMove(Move move)
        {
            Piece piece = GetPiece(move.From);
            if (piece == null)
            {
                return null;
            }

            MoveInfo moveInfo = new MoveInfo(GetPiece(move.To));
            moveInfo.CastlingRights = (bool[])castlingRights.Clone();
            moveInfo.EnPassantSquare = enPassantSquare;
            moveInfo.HalfMoveClock = halfMoveClock;
            Squares[move.To.Rank, move.To.File] = piece;
            Squares[move.From.Rank, move.From.File] = null;

            if (piece.Type == PieceType.King)
            {
                castlingRights[(piece.Color == PieceColor.White) ? 0 : 2] = castlingRights[(piece.Color == PieceColor.White) ? 1 : 3] = false;
                if (Math.Abs(move.From.File - move.To.File) == 2)
                {
                    int rank = (piece.Color == PieceColor.White) ? 7 : 0;
                    int[] files = (move.To.File > move.From.File) ? new[] { 7, 5 } : new[] { 0, 3 };
                    Squares[rank, files[1]] = Squares[rank, files[0]];
                    Squares[rank, files[0]] = null;
                    moveInfo.IsCastling = true;
                }
            }

            if (piece.Type == PieceType.Rook)
            {
                int i = ((piece.Color == PieceColor.White) ? 0 : 2) + ((move.From.File == 7) ? 0 : 1);
                castlingRights[i] = false;
            }

            if (piece.Type == PieceType.Pawn)
            {
                int rank = (piece.Color == PieceColor.White) ? 0 : 7;
                if (move.To.Rank == rank)
                {
                    Squares[move.To.Rank, move.To.File] = move.PromotedPiece;
                    moveInfo.IsPromotion = true;
                    moveInfo.PromotedPiece = move.PromotedPiece;
                }

                if (enPassantSquare != null && enPassantSquare.File == move.To.File && enPassantSquare.Rank == move.To.Rank)
                {
                    Squares[move.To.Rank + (piece.Color == PieceColor.White ? 1 : -1), move.To.File] = null;
                    moveInfo.IsEnPassant = true;
                }

                enPassantSquare = null;

                if (Math.Abs(move.From.Rank - move.To.Rank) == 2)
                {
                    enPassantSquare = new Square(move.To.Rank + (piece.Color == PieceColor.White ? 1 : -1), move.To.File);
                }

                moveInfo.IsPawnMove = true;
            }

            fullMoveNumber = (sideToMove == PieceColor.Black) ? fullMoveNumber + 1 : fullMoveNumber;
            halfMoveClock = (moveInfo.IsPawnMove || moveInfo.TakenPiece != null) ? 0 : halfMoveClock + 1;
            sideToMove = (sideToMove == PieceColor.White) ? PieceColor.Black : PieceColor.White;
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
                Squares[move.From.Rank, move.From.File] = new Piece(PieceType.Pawn, (Squares[move.From.Rank, move.From.File].Color == PieceColor.Black) ? PieceColor.Black : PieceColor.White);
            }

            castlingRights = moveInfo.CastlingRights;
            enPassantSquare = moveInfo.EnPassantSquare;
            sideToMove = (sideToMove == PieceColor.White) ? PieceColor.Black : PieceColor.White;
            halfMoveClock = moveInfo.HalfMoveClock;
            fullMoveNumber = (sideToMove == PieceColor.Black) ? fullMoveNumber - 1 : fullMoveNumber;
        }

        public Square GetKingPosition(PieceColor color)
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    if (Squares[rank, file] != null && Squares[rank, file].Type == PieceType.King && Squares[rank, file].Color == color)
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
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    if (Squares[rank, file] != null && Squares[rank, file].Color != color)
                    {
                        List<Move> moves = Squares[rank, file].GetAttackMoves(new Square(rank, file));
                        foreach (Move move in moves)
                        {
                            if (move.To.Rank == kingPosition.Rank && move.To.File == kingPosition.File)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public GameResult Result(List<MoveInfo> listOfAllMovesInfo, List<Move> listOfAllMoves, out string reason)
        {
            reason = null;
            List<Move> moves = new List<Move>();
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    Piece piece = Squares[rank, file];
                    if (piece != null && piece.Color == sideToMove)
                    {
                        moves.AddRange(piece.GetLegalMoves(this, new Square(rank, file), castlingRights, enPassantSquare));
                    }
                }
            }
            if (moves.Count == 0)
            {
                if (IsInCheck(sideToMove))
                {
                    reason = "Checkmate";
                    return sideToMove == PieceColor.White ? GameResult.BlackWin : GameResult.WhiteWin;
                }
            }

            int pieceCount = 0;
            foreach (Piece piece in Squares)
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

            if (fullMoveNumber >= 4)
            {
                Board previousPositions = Clone();
                int positionCount = 1;

                for (int i = listOfAllMoves.Count - 1; i >= 0; i -= 2)
                {
                    Move move = listOfAllMoves[i];
                    MoveInfo moveInfo = listOfAllMovesInfo[i];
                    previousPositions.UndoMove(move, moveInfo);
                    if (Equals(previousPositions) && moveInfo.CastlingRights.SequenceEqual(castlingRights) && moveInfo.EnPassantSquare == enPassantSquare)
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

            if (halfMoveClock >= 100)
            {
                reason = "Fifty-move rule";
                return GameResult.Draw;
            }

            return GameResult.Ongoing;
        }

        public bool IsMoveLegal(Move move)
        {
            Piece piece = GetPiece(move.From);
            if (piece == null || piece.Color != sideToMove)
            {
                return false;
            }

            List<Move> legalMoves = piece.GetLegalMoves(this, move.From, castlingRights, enPassantSquare);
            foreach (Move legalMove in legalMoves)
            {
                if (move.Equals(legalMove))
                {
                    return true;
                }
            }
            return false;
        }

        public double endgamePhase()
        {
            int gamePhase = 0;
            foreach (Piece piece in Squares)
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

        public int Evaluate()
        {
            int evaluation = 0;
            int[,] pawnSquareTable =
            {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 50, 50, 50, 50, 50, 50, 50, 50 },
                { 10, 10, 20, 30, 30, 20, 10, 10 },
                { 5,  5, 10, 25, 25, 10, 5, 5 },
                { 0, 0, 0, 20, 20, 0, 0, 0 },
                { 5, -5, -10, 0, 0, -10, -5, 5 },
                { 5, 10, 10, -20, -20, 10, 10, 5 },
                { 0, 0, 0, 0, 0, 0, 0, 0 }
            };

            int[,] knightSquareTable =
            {
                { -50, -40, -30, -30, -30, -30, -40, -50 },
                { -40, -20, 0, 0, 0, 0, -20, -40 },
                { -30, 0, 10, 15, 15, 10, 0, -30 },
                { -30, 5, 15, 20, 20, 15, 5, -30 },
                { -30, 0, 15, 20, 20, 15, 0, -30 },
                { -30, 5, 10, 15, 15, 10, 5, -30 },
                { -40, -20, 0, 5, 5, 0, -20, -40 },
                { -50, -40, -30, -30, -30, -30, -40, -50 }
            };

            int[,] bishopSquareTable =
            {
                { -20, -10, -10, -10, -10, -10, -10, -20 },
                { -10, 0, 0, 0, 0, 0, 0, -10 },
                { -10, 0, 5, 10, 10, 5, 0, -10 },
                { -10, 5, 5, 10, 10, 5, 5, -10 },
                { -10, 0, 10, 10, 10, 10, 0, -10 },
                { -10, 10, 10, 10, 10, 10, 10, -10 },
                { -10, 5, 0, 0, 0, 0, 5, -10 },
                { -20, -10, -10, -10, -10, -10, -10, -20 }
            };

            int[,] rookSquareTable =
            {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 5, 10, 10, 10, 10, 10, 10, 5 },
                { -5, 0, 0, 0, 0, 0, 0, -5 },
                { -5, 0, 0, 0, 0, 0, 0, -5 },
                { -5, 0, 0, 0, 0, 0, 0, -5 },
                { -5, 0, 0, 0, 0, 0, 0, -5 },
                { -5, 0, 0, 0, 0, 0, 0, -5 },
                { 0, 0, 0, 5, 5, 0, 0, 0 }
            };

            int[,] queenSquareTable =
            {
                { -20, -10, -10, -5, -5, -10, -10, -20 },
                { -10, 0, 0, 0, 0, 0, 0, -10 },
                { -10, 0, 5, 5, 5, 5, 0, -10 },
                { -5, 0, 5, 5, 5, 5, 0, -5 },
                { 0, 0, 5, 5, 5, 5, 0, -5 },
                { -10, 5, 5, 5, 5, 5, 0, -10 },
                { -10, 0, 5, 0, 0, 0, 0, -10 },
                { -20, -10, -10, -5, -5, -10, -10, -20 }
            };

            int[,] mgKingSquareTable =
            {
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -30, -40, -40, -50, -50, -40, -40, -30 },
                { -20, -30, -30, -40, -40, -30, -30, -20 },
                { -10, -20, -20, -20, -20, -20, -20, -10 },
                { 20, 20, 0, 0, 0, 0, 20, 20 },
                { 20, 30, 10, 0, 0, 10, 30, 20 }
            };

            int[,] egKingSquareTable =
            {
                { -50, -40, -30, -20, -20, -30, -40, -50 },
                { -30, -20, -10, 0, 0, -10, -20, -30 },
                { -30, -10, 20, 30, 30, 20, -10, -30 },
                { -30, -10, 30, 40, 40, 30, -10, -30 },
                { -30, -10, 30, 40, 40, 30, -10, -30 },
                { -30, -10, 20, 30, 30, 20, -10, -30 },
                { -30, -30, 0, 0, 0, 0, -30, -30 },
                { -50, -30, -30, -30, -30, -30, -30, -50 }
            };

            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    Piece piece = Squares[rank, file];
                    if (piece != null)
                    {
                        int rankIndex = (piece.Color == PieceColor.White) ? rank : 7 - rank;
                        evaluation += (piece.Color == PieceColor.White ? 1 : -1) *
                        (piece.Type switch
                        {
                            PieceType.Pawn => 100 + pawnSquareTable[rankIndex, file],
                            PieceType.Knight => 320 + knightSquareTable[rankIndex, file],
                            PieceType.Bishop => 330 + bishopSquareTable[rankIndex, file],
                            PieceType.Rook => 500 + rookSquareTable[rankIndex, file],
                            PieceType.Queen => 900 + queenSquareTable[rankIndex, file],
                            PieceType.King => 20000 + (int)((1 - endgamePhase()) * mgKingSquareTable[rankIndex, file] + endgamePhase() * egKingSquareTable[rankIndex, file]),
                            _ => 0
                        });
                    }
                }
            }
            return evaluation;
        }

        public int Search(int depth, out Move bestMove, List<Move> allMoves, List<MoveInfo> allMovesInfo, int alpha = int.MinValue, int beta = int.MaxValue)
        {
            bestMove = null;
            if (depth == 0)
            {
                return Evaluate();
            }

            GameResult result = Result(allMovesInfo, allMoves, out _);
            if (result != GameResult.Ongoing)
            {
                return result == GameResult.WhiteWin ? int.MaxValue : result == GameResult.BlackWin ? int.MinValue : 0;
            }

            List<Move> moves = new List<Move>();
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    Piece piece = Squares[rank, file];
                    if (piece != null && piece.Color == sideToMove)
                    {
                        moves.AddRange(piece.GetLegalMoves(this, new Square(rank, file), castlingRights, enPassantSquare));
                    }
                }
            }

            int bestScore = sideToMove == PieceColor.White ? int.MinValue : int.MaxValue;
            foreach (Move move in moves)
            {
                Board nextMove = Clone();
                nextMove.MakeMove(move);
                int score = nextMove.Search(depth - 1, out _, allMoves, allMovesInfo, alpha, beta);
                if (sideToMove == PieceColor.White)
                {
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                    beta = Math.Min(beta, bestScore);
                }

                if (alpha >= beta)
                {
                    break;
                }
            }
            return bestScore;
        }
    }
}