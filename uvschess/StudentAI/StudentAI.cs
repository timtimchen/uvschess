using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;

namespace StudentAI
{
    public class StudentAI : IChessAI
    {
        #region IChessAI Members that are implemented by the Student

        public const int KING_VALUE = 10000;
        public const int QUEEN_VALUE = 1000;
        public const int ROOK_VALUE = 500;
        public const int BISHOP_VALUE = 300;
        public const int KNIGHT_VALUE = 300;
        public const int PAWN_VALUE = 100;
        public const int INFINITY = 100000;

        /// <summary>
        /// The name of your AI
        /// </summary>
        public string Name
        {
#if DEBUG
            get { return "Sophy&Joe(Debug)"; }
#else
            get { return "Sophy&Joe"; }
#endif
        }

        private int maxDepth = 2;

        private int visitedNode = 0;

        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board</param>
        /// <param name="myColor">Your color</param>
        /// <returns> Returns the best chess move the player has for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
            visitedNode = 0;
            ChessMove myNextMove = new ChessMove(null, null, ChessFlag.Stalemate);
            myNextMove.ValueOfMove = -INFINITY;
            ChessColor opponentColor = (myColor == ChessColor.Black) ? ChessColor.White : ChessColor.Black;
            for (int fromY = 0; fromY < ChessBoard.NumberOfRows; fromY++)
            {
                for (int fromX = 0; fromX < ChessBoard.NumberOfColumns; fromX++)
                {
                    if (board[fromX, fromY] != ChessPiece.Empty && PieceColor(board[fromX, fromY]) == myColor)
                    {
                        // if found one of my pieces, search if there are posible movements of this piece
                        for (int toY = 0; toY < ChessBoard.NumberOfRows; toY++)
                        {
                            for (int toX = 0; toX < ChessBoard.NumberOfColumns; toX++)
                            {
                                ChessLocation moveFrom = new ChessLocation(fromX, fromY);
                                ChessLocation moveTo = new ChessLocation(toX, toY);
                                ChessMove potentialMove = new ChessMove(moveFrom, moveTo, ChessFlag.NoFlag);
                                if (IsValidMove(board, potentialMove, myColor))
                                {
                                    // if found a posible movement, evaluate the board after making this move
                                    ChessBoard boardAfterMove = board.Clone();
                                    boardAfterMove.MakeMove(potentialMove);
                                    if (IsInCheck(boardAfterMove, opponentColor))
                                    {
                                        potentialMove.Flag = ChessFlag.Check;
                                    }
                                    if (IsCheckmate(boardAfterMove, opponentColor))
                                    {
                                        if (potentialMove.Flag == ChessFlag.Check)
                                        {
                                            // found a Checkmate movement!
                                            potentialMove.Flag = ChessFlag.Checkmate;
                                            this.Log(myColor.ToString() + " (" + this.Name + ") just moved. Visited Nodes: " + visitedNode);
                                            this.Log(string.Empty);
                                            Profiler.SetDepthReachedDuringThisTurn(2);
                                            return potentialMove;
                                        }
                                        else
                                        {
                                            // this move would make a Slatemate. Avoid it.
                                            break;
                                        }
                                    }
                                    potentialMove.ValueOfMove = minValue(maxDepth, boardAfterMove, myColor);
                                    if (myNextMove.ValueOfMove < potentialMove.ValueOfMove)
                                    {
                                        myNextMove = potentialMove;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            this.Log(myColor.ToString() + " (" + this.Name + ") just moved. Visited Nodes: " + visitedNode + ". Score: " + myNextMove.ValueOfMove);
            this.Log(string.Empty);

            Profiler.SetDepthReachedDuringThisTurn(2);
            return myNextMove;
        }

        /// <summary>
        /// Validates a move. The framework uses this to validate the opponents move.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        public bool IsValidMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            ChessPiece movingPiece = boardBeforeMove[moveToCheck.From];
            ChessPiece targetPlace = boardBeforeMove[moveToCheck.To];
            if (movingPiece == ChessPiece.Empty ||
                PieceColor(movingPiece) != colorOfPlayerMoving ||
                (targetPlace != ChessPiece.Empty && PieceColor(targetPlace) == colorOfPlayerMoving))
            {
                return false;
            }
            // validate the specified move by each piece rule
            switch (movingPiece)
            {
                case ChessPiece.BlackPawn:
                    if (moveToCheck.To.Y - moveToCheck.From.Y < 1 ||
                        moveToCheck.To.Y - moveToCheck.From.Y > 2 ||
                        (moveToCheck.To.Y - moveToCheck.From.Y == 2 && (moveToCheck.From.Y != 1 || moveToCheck.To.X != moveToCheck.From.X || boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y + 1] != ChessPiece.Empty)) ||
                        Math.Abs(moveToCheck.To.X - moveToCheck.From.X) > 1 ||
                        (moveToCheck.To.X == moveToCheck.From.X && targetPlace != ChessPiece.Empty) ||  //move forward case
                                                                                                        //otherwise, capturing case
                        (Math.Abs(moveToCheck.To.X - moveToCheck.From.X) == 1 && targetPlace == ChessPiece.Empty))
                    {
                        return false;
                    }
                    break;
                case ChessPiece.WhitePawn:
                    if (moveToCheck.From.Y - moveToCheck.To.Y < 1 ||
                        moveToCheck.From.Y - moveToCheck.To.Y > 2 ||
                        (moveToCheck.From.Y - moveToCheck.To.Y == 2 && (moveToCheck.From.Y != 6 || moveToCheck.To.X != moveToCheck.From.X || boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y - 1] != ChessPiece.Empty)) ||
                        Math.Abs(moveToCheck.To.X - moveToCheck.From.X) > 1 ||
                        (moveToCheck.To.X == moveToCheck.From.X && targetPlace != ChessPiece.Empty) ||  //move forward case
                                                                                                        //otherwise, capturing case
                       (Math.Abs(moveToCheck.To.X - moveToCheck.From.X) == 1 && targetPlace == ChessPiece.Empty))
                    {
                        return false;
                    }
                    break;
                case ChessPiece.BlackRook:
                case ChessPiece.WhiteRook:
                    if ((moveToCheck.To.X != moveToCheck.From.X && moveToCheck.To.Y != moveToCheck.From.Y))
                    {
                        return false;
                    }
                    // vertical move
                    if (moveToCheck.To.X == moveToCheck.From.X)
                    {
                        int moveDirection = (moveToCheck.To.Y - moveToCheck.From.Y > 0) ? 1 : -1;
                        for (int i = moveToCheck.From.Y; i != moveToCheck.To.Y; i += moveDirection)
                        {
                            if (i != moveToCheck.From.Y && boardBeforeMove[moveToCheck.From.X, i] != ChessPiece.Empty)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        // horizontal move (moveToCheck.To.Y == moveToCheck.From.Y)
                        int moveDirection = (moveToCheck.To.X - moveToCheck.From.X > 0) ? 1 : -1;
                        for (int i = moveToCheck.From.X; i != moveToCheck.To.X; i += moveDirection)
                        {
                            if (i != moveToCheck.From.X && boardBeforeMove[i, moveToCheck.From.Y] != ChessPiece.Empty)
                            {
                                return false;
                            }
                        }
                    }
                    break;
                case ChessPiece.BlackKnight:
                case ChessPiece.WhiteKnight:
                    if (!((Math.Abs(moveToCheck.To.X - moveToCheck.From.X) == 1 && Math.Abs(moveToCheck.To.Y - moveToCheck.From.Y) == 2) ||
                          (Math.Abs(moveToCheck.To.X - moveToCheck.From.X) == 2 && Math.Abs(moveToCheck.To.Y - moveToCheck.From.Y) == 1)))
                    {
                        return false;
                    }
                    break;
                case ChessPiece.BlackBishop:
                case ChessPiece.WhiteBishop:
                    if (Math.Abs(moveToCheck.To.X - moveToCheck.From.X) != Math.Abs(moveToCheck.To.Y - moveToCheck.From.Y))
                    {
                        return false;
                    }
                    int horizontalDirection = (moveToCheck.To.X - moveToCheck.From.X > 0) ? 1 : -1;
                    int verticalDirection = (moveToCheck.To.Y - moveToCheck.From.Y > 0) ? 1 : -1;
                    int x = moveToCheck.From.X;
                    int y = moveToCheck.From.Y;
                    while (x != moveToCheck.To.X)
                    {
                        if (x != moveToCheck.From.X && boardBeforeMove[x, y] != ChessPiece.Empty)
                        {
                            return false;
                        }
                        x += horizontalDirection;
                        y += verticalDirection;
                    }
                    break;
                case ChessPiece.BlackQueen:
                case ChessPiece.WhiteQueen:
                    if (moveToCheck.To.X != moveToCheck.From.X &&
                        moveToCheck.To.Y != moveToCheck.From.Y &&
                        Math.Abs(moveToCheck.To.X - moveToCheck.From.X) != Math.Abs(moveToCheck.To.Y - moveToCheck.From.Y))
                    {
                        return false;
                    }
                    // vertical move
                    if (moveToCheck.To.X == moveToCheck.From.X)
                    {
                        int moveDirection = (moveToCheck.To.Y - moveToCheck.From.Y > 0) ? 1 : -1;
                        for (int i = moveToCheck.From.Y; i != moveToCheck.To.Y; i += moveDirection)
                        {
                            if (i != moveToCheck.From.Y && boardBeforeMove[moveToCheck.From.X, i] != ChessPiece.Empty)
                            {
                                return false;
                            }
                        }
                    }
                    else if (moveToCheck.To.Y == moveToCheck.From.Y)
                    {
                        // horizontal move
                        int moveDirection = (moveToCheck.To.X - moveToCheck.From.X > 0) ? 1 : -1;
                        for (int i = moveToCheck.From.X; i != moveToCheck.To.X; i += moveDirection)
                        {
                            if (i != moveToCheck.From.X && boardBeforeMove[i, moveToCheck.From.Y] != ChessPiece.Empty)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        //  diagonal move
                        int horizontalDirection2 = (moveToCheck.To.X - moveToCheck.From.X > 0) ? 1 : -1;
                        int verticalDirection2 = (moveToCheck.To.Y - moveToCheck.From.Y > 0) ? 1 : -1;
                        int x2 = moveToCheck.From.X;
                        int y2 = moveToCheck.From.Y;
                        while (x2 != moveToCheck.To.X)
                        {
                            if (x2 != moveToCheck.From.X && boardBeforeMove[x2, y2] != ChessPiece.Empty)
                            {
                                return false;
                            }
                            x2 += horizontalDirection2;
                            y2 += verticalDirection2;
                        }
                    }
                    break;
                case ChessPiece.BlackKing:
                case ChessPiece.WhiteKing:
                    if (Math.Abs(moveToCheck.To.X - moveToCheck.From.X) > 1 || Math.Abs(moveToCheck.To.Y - moveToCheck.From.Y) > 1)
                    {
                        return false;
                    }
                    break;
            }
            // if passed the rule validation, make the movement and test if the King is being attacked (in check state)
            ChessBoard boardAfterMove = boardBeforeMove.Clone();
            boardAfterMove.MakeMove(moveToCheck);
            // if it is not in Check, that is a valid move
            return !(IsInCheck(boardAfterMove, colorOfPlayerMoving));
        }

        //should always check the location is NOT a ChessPiece.Empty first
        public ChessColor PieceColor(ChessPiece piece)
        {
            if (piece == ChessPiece.BlackBishop ||
                piece == ChessPiece.BlackKing ||
                piece == ChessPiece.BlackKnight ||
                piece == ChessPiece.BlackPawn ||
                piece == ChessPiece.BlackQueen ||
                piece == ChessPiece.BlackRook)
            {
                return ChessColor.Black;
            }
            // otherwise
            return ChessColor.White;
        }

        // test if a particular color king being checked
        public bool IsInCheck(ChessBoard board, ChessColor colorOfPlayer)
        {
            ChessPiece opponentKing = (colorOfPlayer == ChessColor.Black) ? ChessPiece.WhiteKing : ChessPiece.BlackKing;
            ChessPiece opponentQueen = (colorOfPlayer == ChessColor.Black) ? ChessPiece.WhiteQueen : ChessPiece.BlackQueen;
            ChessPiece opponentRook = (colorOfPlayer == ChessColor.Black) ? ChessPiece.WhiteRook : ChessPiece.BlackRook;
            ChessPiece opponentKnight = (colorOfPlayer == ChessColor.Black) ? ChessPiece.WhiteKnight : ChessPiece.BlackKnight;
            ChessPiece opponentBishop = (colorOfPlayer == ChessColor.Black) ? ChessPiece.WhiteBishop : ChessPiece.BlackBishop;
            ChessPiece selfKing = (colorOfPlayer == ChessColor.Black) ? ChessPiece.BlackKing : ChessPiece.WhiteKing;
            // get King's position and opponent Knights position
            int KingX = -1;
            int KingY = -1;
            int oppKnightX1 = -1;
            int oppKnightY1 = -1;
            int oppKnightX2 = -1;
            int oppKnightY2 = -1;
            for (int y = 0; y < ChessBoard.NumberOfRows; y++)
            {
                for (int x = 0; x < ChessBoard.NumberOfColumns; x++)
                {
                    if (board[x, y] == selfKing)
                    {
                        KingX = x;
                        KingY = y;
                    }
                    else if (board[x, y] == opponentKnight)
                    {
                        if (oppKnightX1 == -1)
                        {
                            oppKnightX1 = x;
                            oppKnightY1 = y;
                        }
                        else
                        {
                            oppKnightX2 = x;
                            oppKnightY2 = y;
                        }
                    }
                }
            }
            // check if King in check of Knight
            if (oppKnightX1 != -1)
            {
                if ((Math.Abs(oppKnightX1 - KingX) == 2 && Math.Abs(oppKnightY1 - KingY) == 1) ||
                    (Math.Abs(oppKnightX1 - KingX) == 1 && Math.Abs(oppKnightY1 - KingY) == 2))
                {
                    return true;
                }
                if (oppKnightX2 != -1)
                {
                    if ((Math.Abs(oppKnightX2 - KingX) == 2 && Math.Abs(oppKnightY2 - KingY) == 1) ||
                        (Math.Abs(oppKnightX2 - KingX) == 1 && Math.Abs(oppKnightY2 - KingY) == 2))
                    {
                        return true;
                    }
                }
            }
            // check eight directions if there is possible check
            int i; // counting steps
            i = 1;
            while (KingX + i < ChessBoard.NumberOfColumns)  // go east
            {
                if (i == 1 && board[KingX + i, KingY] == opponentKing) return true;
                if (board[KingX + i, KingY] == opponentRook) return true;
                if (board[KingX + i, KingY] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[KingX + i, KingY] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingX - i >= 0)  // go west
            {
                if (i == 1 && board[KingX - i, KingY] == opponentKing) return true;
                if (board[KingX - i, KingY] == opponentRook) return true;
                if (board[KingX - i, KingY] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[KingX - i, KingY] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingY + i < ChessBoard.NumberOfRows)  // go south
            {
                if (i == 1 && board[KingX, KingY + i] == opponentKing) return true;
                if (board[KingX, KingY + i] == opponentRook) return true;
                if (board[KingX, KingY + i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[KingX, KingY + i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingY - i >= 0)  // go north
            {
                if (i == 1 && board[KingX, KingY - i] == opponentKing) return true;
                if (board[KingX, KingY - i] == opponentRook) return true;
                if (board[KingX, KingY - i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[KingX, KingY - i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingX + i < ChessBoard.NumberOfColumns && KingY + i < ChessBoard.NumberOfRows)  // go southeast
            {
                if (i == 1 && board[KingX + i, KingY + i] == opponentKing) return true;
                if (i == 1 && colorOfPlayer == ChessColor.Black && board[KingX + i, KingY + i] == ChessPiece.WhitePawn) return true;
                if (board[KingX + i, KingY + i] == opponentBishop) return true;
                if (board[KingX + i, KingY + i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[KingX + i, KingY + i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingX - i >= 0 && KingY + i < ChessBoard.NumberOfRows)  // go southwest
            {
                if (i == 1 && board[KingX - i, KingY + i] == opponentKing) return true;
                if (i == 1 && colorOfPlayer == ChessColor.Black && board[KingX - i, KingY + i] == ChessPiece.WhitePawn) return true;
                if (board[KingX - i, KingY + i] == opponentBishop) return true;
                if (board[KingX - i, KingY + i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[KingX - i, KingY + i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingX + i < ChessBoard.NumberOfColumns && KingY - i >= 0)  // go northeast
            {
                if (i == 1 && board[KingX + i, KingY - i] == opponentKing) return true;
                if (i == 1 && colorOfPlayer == ChessColor.White && board[KingX + i, KingY - i] == ChessPiece.BlackPawn) return true;
                if (board[KingX + i, KingY - i] == opponentBishop) return true;
                if (board[KingX + i, KingY - i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[KingX + i, KingY - i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingX - i >= 0 && KingY - i >= 0)  // go northwest
            {
                if (i == 1 && board[KingX - i, KingY - i] == opponentKing) return true;
                if (i == 1 && colorOfPlayer == ChessColor.White && board[KingX - i, KingY - i] == ChessPiece.BlackPawn) return true;
                if (board[KingX - i, KingY - i] == opponentBishop) return true;
                if (board[KingX - i, KingY - i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[KingX - i, KingY - i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            // now the King is not in check state
            return false;
        }

        // assuming the player in turn is in a Check state first.
        // search if there is a movement to avoid Checking. If not found, it is Checkmate. 
        public bool IsCheckmate(ChessBoard board, ChessColor playerColor)
        {
            for (int fromY = 0; fromY < ChessBoard.NumberOfRows; fromY++)
            {
                for (int fromX = 0; fromX < ChessBoard.NumberOfColumns; fromX++)
                {
                    if (board[fromX, fromY] != ChessPiece.Empty && PieceColor(board[fromX, fromY]) == playerColor)
                    {
                        // if found one of my pieces, search if there are posible movements of this piece
                        for (int toY = 0; toY < ChessBoard.NumberOfRows; toY++)
                        {
                            for (int toX = 0; toX < ChessBoard.NumberOfColumns; toX++)
                            {
                                ChessLocation moveFrom = new ChessLocation(fromX, fromY);
                                ChessLocation moveTo = new ChessLocation(toX, toY);
                                ChessMove potentialMove = new ChessMove(moveFrom, moveTo, ChessFlag.NoFlag);
                                if (IsValidMove(board, potentialMove, playerColor))
                                {
                                    // if found a posible movement, it is not Checkmate
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Evaluates the chess board Score based on a rawBoard state.
        ///  positive values means the player is leading
        /// </summary>
        /// <param name="board">a chess raw board</param>
        /// <param name="color">player's color</param>
        /// <returns> an Integer as evaluation score</returns>
        public int EvaluateBoard (ChessBoard board, ChessColor color)
        {
            int valueFacotr = (color == ChessColor.Black) ? -1 : 1;
            int sumScore = 0;
            int positionValue;
            int blackBishop = 0; //Bishop counter
            int whiteBishop = 0;
            for (int y = 0; y < ChessBoard.NumberOfRows; y++)
            {
                for (int x = 0; x < ChessBoard.NumberOfColumns; x++)
                {
                    if (board[x, y] != ChessPiece.Empty)
                    {
                        switch (board[x, y])
                        {
                            case ChessPiece.BlackKing:
                                sumScore -= KING_VALUE;
                                break;
                            case ChessPiece.BlackQueen:
                                sumScore -= QUEEN_VALUE;
                                break;
                            case ChessPiece.BlackRook:
                                sumScore -= ROOK_VALUE;
                                break;
                            case ChessPiece.BlackBishop:
                                sumScore -= BISHOP_VALUE;
                                blackBishop++;
                                if (blackBishop == 2) sumScore -= 30;  // double Bishop bonus
                                break;
                            case ChessPiece.BlackKnight:
                                sumScore -= KNIGHT_VALUE;
                                positionValue = (7 - x > x ? x : 7 - x) * 4 + (7 - y > y ? y : 7 - y) * 4;  //center Knight bonus
                                sumScore -= positionValue;
                                break;
                            case ChessPiece.BlackPawn:
                                sumScore -= PAWN_VALUE;
                                positionValue = (x == 1 || x == 6) ? 4 : 8;
                                sumScore -= (y - 1) * positionValue; // move Pawn bonus
                                break;
                            case ChessPiece.WhiteKing:
                                sumScore += KING_VALUE;
                                break;
                            case ChessPiece.WhiteQueen:
                                sumScore += QUEEN_VALUE;
                                break;
                            case ChessPiece.WhiteRook:
                                sumScore += ROOK_VALUE;
                                break;
                            case ChessPiece.WhiteBishop:
                                sumScore += BISHOP_VALUE;
                                whiteBishop++;
                                if (whiteBishop == 2) sumScore += 30;  // double Bishop bonus
                                break;
                            case ChessPiece.WhiteKnight:
                                sumScore += KNIGHT_VALUE;
                                positionValue = (7 - x > x ? x : 7 - x) * 4 + (7 - y > y ? y : 7 - y) * 4;  //center Knight bonus
                                sumScore += positionValue;
                                break;
                            case ChessPiece.WhitePawn:
                                sumScore += PAWN_VALUE;
                                positionValue = (x == 1 || x == 6) ? 4 : 8;
                                sumScore += (6 - y) * positionValue; // move Pawn bonus
                                break;
                        }
                    }
                }
            }
            return sumScore * valueFacotr;
        }

        public int minValue(int depth, ChessBoard board, ChessColor myColor)
        {
            visitedNode++;
            int result = INFINITY;
            ChessColor opponentColor = (myColor == ChessColor.Black) ? ChessColor.White : ChessColor.Black;
            if (IsInCheck(board, opponentColor) && IsCheckmate(board, opponentColor))
            {
                return INFINITY;
            }
            if (depth <= 0)
            {
                return EvaluateBoard(board, myColor);
            }
            for (int fromY = 0; fromY < ChessBoard.NumberOfRows; fromY++)
            {
                for (int fromX = 0; fromX < ChessBoard.NumberOfColumns; fromX++)
                {
                    if (board[fromX, fromY] != ChessPiece.Empty && PieceColor(board[fromX, fromY]) == opponentColor)
                    {
                        // if found one pieces, search if there are posible movements of this piece
                        for (int toY = 0; toY < ChessBoard.NumberOfRows; toY++)
                        {
                            for (int toX = 0; toX < ChessBoard.NumberOfColumns; toX++)
                            {
                                if (IsMyTurnOver()) return -INFINITY;
                                ChessLocation moveFrom = new ChessLocation(fromX, fromY);
                                ChessLocation moveTo = new ChessLocation(toX, toY);
                                ChessMove potentialMove = new ChessMove(moveFrom, moveTo, ChessFlag.NoFlag);
                                if (IsValidMove(board, potentialMove, opponentColor))
                                {
                                    // if found a posible movement, evaluate the board after making this move
                                    ChessBoard boardAfterMove = board.Clone();
                                    boardAfterMove.MakeMove(potentialMove);
                                    int temp = maxValue(depth - 1, boardAfterMove, myColor);
                                    if (temp < result) result = temp;  //min node
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public int maxValue(int depth, ChessBoard board, ChessColor myColor)
        {
            visitedNode++;
            int result = -INFINITY;
            if (IsInCheck(board, myColor) && IsCheckmate(board, myColor))
            {
                return -INFINITY;
            }
            if (depth <= 0)
            {
                return EvaluateBoard(board, myColor);
            }
            for (int fromY = 0; fromY < ChessBoard.NumberOfRows; fromY++)
            {
                for (int fromX = 0; fromX < ChessBoard.NumberOfColumns; fromX++)
                {
                    if (board[fromX, fromY] != ChessPiece.Empty && PieceColor(board[fromX, fromY]) == myColor)
                    {
                        // if found one pieces, search if there are posible movements of this piece
                        for (int toY = 0; toY < ChessBoard.NumberOfRows; toY++)
                        {
                            for (int toX = 0; toX < ChessBoard.NumberOfColumns; toX++)
                            {
                                if (IsMyTurnOver()) return INFINITY;
                                ChessLocation moveFrom = new ChessLocation(fromX, fromY);
                                ChessLocation moveTo = new ChessLocation(toX, toY);
                                ChessMove potentialMove = new ChessMove(moveFrom, moveTo, ChessFlag.NoFlag);
                                if (IsValidMove(board, potentialMove, myColor))
                                {
                                    // if found a posible movement, evaluate the board after making this move
                                    ChessBoard boardAfterMove = board.Clone();
                                    boardAfterMove.MakeMove(potentialMove);
                                    int temp = minValue(depth - 1, boardAfterMove, myColor);
                                    if (temp > result) result = temp;  //max node
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// This method generates all valid moves for myColor based on the currentBoard
        /// </summary>
        /// <param name="currentBoard">This is the current board to generate the moves for.</param>
        /// <param name="myColor">This is the color of the player to generate the moves for.</param>
        /// <returns>List of ChessMoves</returns>
        List<ChessMove> GetAllMoves(ChessBoard board, ChessColor playerColor)
        {
            List<ChessMove> allMoves = new List<ChessMove>();
            ChessLocation kingLocation = new ChessLocation(0, 0);
            List<ChessLocation> myPieces = new List<ChessLocation>();

            ChessPiece opponentKing = (playerColor == ChessColor.Black) ? ChessPiece.WhiteKing : ChessPiece.BlackKing;
            ChessPiece opponentQueen = (playerColor == ChessColor.Black) ? ChessPiece.WhiteQueen : ChessPiece.BlackQueen;
            ChessPiece opponentRook = (playerColor == ChessColor.Black) ? ChessPiece.WhiteRook : ChessPiece.BlackRook;
            ChessPiece opponentBishop = (playerColor == ChessColor.Black) ? ChessPiece.WhiteBishop : ChessPiece.BlackBishop;
            ChessPiece myQueen = (playerColor == ChessColor.White) ? ChessPiece.WhiteQueen : ChessPiece.BlackQueen;
            ChessPiece myRook = (playerColor == ChessColor.White) ? ChessPiece.WhiteRook : ChessPiece.BlackRook;
            ChessPiece myBishop = (playerColor == ChessColor.White) ? ChessPiece.WhiteBishop : ChessPiece.BlackBishop;
            List<ChessLocation> opponentKnightLocation = new List<ChessLocation>();

            // Got through the entire board to get all Pieces locations
            for (int Y = 0; Y < ChessBoard.NumberOfRows; Y++)
            {
                for (int X = 0; X < ChessBoard.NumberOfColumns; X++)
                {
                    if (playerColor == ChessColor.White)
                    {
                        switch (board[X, Y])
                        {
                            case ChessPiece.WhitePawn:
                            case ChessPiece.WhiteRook:
                            case ChessPiece.WhiteKnight:
                            case ChessPiece.WhiteBishop:
                            case ChessPiece.WhiteQueen:
                                myPieces.Add(new ChessLocation(X, Y));
                                break;
                            case ChessPiece.WhiteKing:
                                kingLocation = new ChessLocation(X, Y);
                                break;
                            case ChessPiece.BlackKnight:
                                opponentKnightLocation.Add(new ChessLocation(X, Y));
                                break;
                        }
                    }
                    else // myColor is black
                    {
                        switch (board[X, Y])
                        {
                            case ChessPiece.BlackPawn:
                            case ChessPiece.BlackRook:
                            case ChessPiece.BlackKnight:
                            case ChessPiece.BlackBishop:
                            case ChessPiece.BlackQueen:
                                myPieces.Add(new ChessLocation(X, Y));
                                break;
                            case ChessPiece.BlackKing:
                                kingLocation.X = X;
                                kingLocation.Y = Y;
                                break;
                            case ChessPiece.WhiteKnight:
                                opponentKnightLocation.Add(new ChessLocation(X, Y));
                                break;
                        }
                    }
                }
            }
            // check King's safety
            bool kingInCheckOfKnight = false;
            bool kingInCheckOfOthers = false;
            ChessLocation attackingKnight = new ChessLocation(0, 0);
            // check if King in check of Knight
            foreach (ChessLocation oppKnight in opponentKnightLocation)
            {
                if ((Math.Abs(oppKnight.X - kingLocation.X) == 2 && Math.Abs(oppKnight.Y - kingLocation.Y) == 1) ||
                    (Math.Abs(oppKnight.X - kingLocation.X) == 1 && Math.Abs(oppKnight.Y - kingLocation.Y) == 2))
                {
                    kingInCheckOfKnight = true;
                    attackingKnight.X = oppKnight.X;
                    attackingKnight.Y = oppKnight.Y;
                }
            }
            // check eight directions if there is possible check
            int i; // counting steps
            i = 1;
            while (kingLocation.X + i < ChessBoard.NumberOfColumns)  // go east
            {
                if (i == 1 && board[kingLocation.X + i, kingLocation.Y] == opponentKing)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X + i, kingLocation.Y] == opponentRook)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X + i, kingLocation.Y] == opponentQueen)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                // if found a piece in this way which can't attack the king, get more details
                if (board[kingLocation.X + i, kingLocation.Y] != ChessPiece.Empty)
                {
                    if (PieceColor(board[kingLocation.X + i, kingLocation.Y]) == playerColor)
                    {
                        int j = 1;
                        while (kingLocation.X + i + j < ChessBoard.NumberOfColumns)
                        {
                            if (board[kingLocation.X + i + j, kingLocation.Y] == opponentRook || board[kingLocation.X + i + j, kingLocation.Y] == opponentQueen)
                            {
                                for (int k = 0; k < myPieces.Count; k++)  //find the safe guard and remove it from the waiting list
                                {
                                    if (myPieces[k].X == kingLocation.X + i && myPieces[k].Y == kingLocation.Y)
                                    {
                                        myPieces.RemoveAt(k);
                                        break;
                                    }
                                }
                                if (board[kingLocation.X + i, kingLocation.Y] == myRook || board[kingLocation.X + i, kingLocation.Y] == myQueen)
                                {
                                    for (int k = 1; k < i; k++)
                                    {
                                        allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X + i, kingLocation.Y), new ChessLocation(kingLocation.X + i - k, kingLocation.Y)));
                                    }
                                    for (int k = 1; k <= j; k++)
                                    {
                                        allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X + i, kingLocation.Y), new ChessLocation(kingLocation.X + i + k, kingLocation.Y)));
                                    }
                                }
                                break;
                            }
                            if (board[kingLocation.X + i + j, kingLocation.Y] != ChessPiece.Empty) break;
                            j++;
                        }
                    }
                    break;
                }
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (kingLocation.X - i >= 0)  // go west
            {
                if (i == 1 && board[kingLocation.X - i, kingLocation.Y] == opponentKing)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X - i, kingLocation.Y] == opponentRook)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X - i, kingLocation.Y] == opponentQueen)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                // if found a piece in this way which can't attack the king, check more details
                if (board[kingLocation.X - i, kingLocation.Y] != ChessPiece.Empty)
                {
                    if (PieceColor(board[kingLocation.X - i, kingLocation.Y]) == playerColor)
                    {
                        int j = 1;
                        while (kingLocation.X - i - j < ChessBoard.NumberOfColumns)
                        {
                            if (board[kingLocation.X - i - j, kingLocation.Y] == opponentRook || board[kingLocation.X - i - j, kingLocation.Y] == opponentQueen)
                            {
                                for (int k = 0; k < myPieces.Count; k++)  //find the safe guard and remove it from the waiting list
                                {
                                    if (myPieces[k].X == kingLocation.X - i && myPieces[k].Y == kingLocation.Y)
                                    {
                                        myPieces.RemoveAt(k);
                                        break;
                                    }
                                }
                                if (board[kingLocation.X - i, kingLocation.Y] == myRook || board[kingLocation.X - i, kingLocation.Y] == myQueen)
                                {
                                    for (int k = 1; k < i; k++)
                                    {
                                        allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X - i, kingLocation.Y), new ChessLocation(kingLocation.X - i + k, kingLocation.Y)));
                                    }
                                    for (int k = 1; k <= j; k++)
                                    {
                                        allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X - i, kingLocation.Y), new ChessLocation(kingLocation.X - i - k, kingLocation.Y)));
                                    }
                                }
                                break;
                            }
                            if (board[kingLocation.X - i - j, kingLocation.Y] != ChessPiece.Empty) break;
                            j++;
                        }
                    }
                    break;
                }
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (kingLocation.Y + i < ChessBoard.NumberOfRows)  // go south
            {
                if (i == 1 && board[kingLocation.X, kingLocation.Y + i] == opponentKing)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X, kingLocation.Y + i] == opponentRook)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X, kingLocation.Y + i] == opponentQueen)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                // if found a piece in this way which can't attack the king, check more details
                if (board[kingLocation.X, kingLocation.Y + i] != ChessPiece.Empty)
                {
                    if (PieceColor(board[kingLocation.X, kingLocation.Y + i]) == playerColor)
                    {
                        int j = 1;
                        while (kingLocation.X < ChessBoard.NumberOfColumns + i + j)
                        {
                            if (board[kingLocation.X, kingLocation.Y + i + j] == opponentRook || board[kingLocation.X, kingLocation.Y + i + j] == opponentQueen)
                            {
                                for (int k = 0; k < myPieces.Count; k++)  //find the safe guard and remove it from the waiting list
                                {
                                    if (myPieces[k].X == kingLocation.X && myPieces[k].Y == kingLocation.Y + i)
                                    {
                                        myPieces.RemoveAt(k);
                                        break;
                                    }
                                }
                                if (board[kingLocation.X, kingLocation.Y + i] == myRook || board[kingLocation.X, kingLocation.Y + i] == myQueen)
                                {
                                    for (int k = 1; k < i; k++)
                                    {
                                        allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X, kingLocation.Y + i), new ChessLocation(kingLocation.X, kingLocation.Y + i - k)));
                                    }
                                    for (int k = 1; k <= j; k++)
                                    {
                                        allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X, kingLocation.Y + i), new ChessLocation(kingLocation.X, kingLocation.Y + i + k)));
                                    }
                                }
                                if (board[kingLocation.X, kingLocation.Y + i] == ChessPiece.BlackPawn)
                                {
                                    allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X, kingLocation.Y + i), new ChessLocation(kingLocation.X, kingLocation.Y + i - 1)));
                                    if (kingLocation.Y + i == 1)
                                    {
                                        allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X, kingLocation.Y + i), new ChessLocation(kingLocation.X, kingLocation.Y + i - 2)));
                                    }
                                }
                                break;
                            }
                            if (board[kingLocation.X, kingLocation.Y + i + j] != ChessPiece.Empty) break;
                            j++;
                        }
                    }
                    break;
                }
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (kingLocation.Y - i >= 0)  // go north
            {
                if (i == 1 && board[kingLocation.X, kingLocation.Y - i] == opponentKing)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X, kingLocation.Y - i] == opponentRook)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X, kingLocation.Y - i] == opponentQueen)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                // if found a piece in this way which can't attack the king, check more details
                if (board[kingLocation.X, kingLocation.Y - i] != ChessPiece.Empty)
                {
                    if (PieceColor(board[kingLocation.X, kingLocation.Y - i]) == playerColor)
                    {
                        int j = 1;
                        while (kingLocation.X < ChessBoard.NumberOfColumns - i - j)
                        {
                            if (board[kingLocation.X, kingLocation.Y - i - j] == opponentRook || board[kingLocation.X, kingLocation.Y - i - j] == opponentQueen)
                            {
                                for (int k = 0; k < myPieces.Count; k++)  //find the safe guard and remove it from the waiting list
                                {
                                    if (myPieces[k].X == kingLocation.X && myPieces[k].Y == kingLocation.Y - i)
                                    {
                                        myPieces.RemoveAt(k);
                                        break;
                                    }
                                }
                                if (board[kingLocation.X, kingLocation.Y - i] == myRook || board[kingLocation.X, kingLocation.Y - i] == myQueen)
                                {
                                    for (int k = 1; k < i; k++)
                                    {
                                        allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X, kingLocation.Y - i), new ChessLocation(kingLocation.X, kingLocation.Y - i + k)));
                                    }
                                    for (int k = 1; k <= j; k++)
                                    {
                                        allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X, kingLocation.Y - i), new ChessLocation(kingLocation.X, kingLocation.Y - i - k)));
                                    }
                                }
                                if (board[kingLocation.X, kingLocation.Y - i] == ChessPiece.WhitePawn)
                                {
                                    allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X, kingLocation.Y - i), new ChessLocation(kingLocation.X, kingLocation.Y - i - 1)));
                                    if (kingLocation.Y - i == 6)
                                    {
                                        allMoves.Add(new ChessMove(new ChessLocation(kingLocation.X, kingLocation.Y - i), new ChessLocation(kingLocation.X, kingLocation.Y - i - 2)));
                                    }
                                }
                                break;
                            }
                            if (board[kingLocation.X, kingLocation.Y - i - j] != ChessPiece.Empty) break;
                            j++;
                        }
                    }
                    break;
                }
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (kingLocation.X + i < ChessBoard.NumberOfColumns && kingLocation.Y + i < ChessBoard.NumberOfRows)  // go southeast
            {
                if (i == 1 && board[kingLocation.X + i, kingLocation.Y + i] == opponentKing)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (i == 1 && playerColor == ChessColor.Black && board[kingLocation.X + i, kingLocation.Y + i] == ChessPiece.WhitePawn)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X + i, kingLocation.Y + i] == opponentBishop)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X + i, kingLocation.Y + i] == opponentQueen)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[kingLocation.X + i, kingLocation.Y + i] != ChessPiece.Empty)
                {
                    break;
                }
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (kingLocation.X - i >= 0 && kingLocation.Y + i < ChessBoard.NumberOfRows)  // go southwest
            {
                if (i == 1 && board[kingLocation.X - i, kingLocation.Y + i] == opponentKing)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (i == 1 && playerColor == ChessColor.Black && board[kingLocation.X - i, kingLocation.Y + i] == ChessPiece.WhitePawn)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X - i, kingLocation.Y + i] == opponentBishop)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X - i, kingLocation.Y + i] == opponentQueen)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[kingLocation.X - i, kingLocation.Y + i] != ChessPiece.Empty)
                {
                    break;
                }
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (kingLocation.X + i < ChessBoard.NumberOfColumns && kingLocation.Y - i >= 0)  // go northeast
            {
                if (i == 1 && board[kingLocation.X + i, kingLocation.Y - i] == opponentKing)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (i == 1 && playerColor == ChessColor.White && board[kingLocation.X + i, kingLocation.Y - i] == ChessPiece.BlackPawn)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X + i, kingLocation.Y - i] == opponentBishop)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X + i, kingLocation.Y - i] == opponentQueen)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[kingLocation.X + i, kingLocation.Y - i] != ChessPiece.Empty)
                {
                    break;
                }
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (kingLocation.X - i >= 0 && kingLocation.Y - i >= 0)  // go northwest
            {
                if (i == 1 && board[kingLocation.X - i, kingLocation.Y - i] == opponentKing)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (i == 1 && playerColor == ChessColor.White && board[kingLocation.X - i, kingLocation.Y - i] == ChessPiece.BlackPawn)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X - i, kingLocation.Y - i] == opponentBishop)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                if (board[kingLocation.X - i, kingLocation.Y - i] == opponentQueen)
                {
                    kingInCheckOfOthers = true;
                    break;
                }
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (board[kingLocation.X - i, kingLocation.Y - i] != ChessPiece.Empty)
                {
                    break;
                }
                i++;  // if next grid is empty, continue searching
            }

            if (!kingInCheckOfKnight)
            {

            }
            else
            {

            }
            // move king

            return allMoves;
        }

        #endregion
















        #region IChessAI Members that should be implemented as automatic properties and should NEVER be touched by students.
        /// <summary>
        /// This will return false when the framework starts running your AI. When the AI's time has run out,
        /// then this method will return true. Once this method returns true, your AI should return a 
        /// move immediately.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        public AIIsMyTurnOverCallback IsMyTurnOver { get; set; }

        /// <summary>
        /// Call this method to print out debug information. The framework subscribes to this event
        /// and will provide a log window for your debug messages.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AILoggerCallback Log { get; set; }

        /// <summary>
        /// Call this method to catch profiling information. The framework subscribes to this event
        /// and will print out the profiling stats in your log window.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="key"></param>
        public AIProfiler Profiler { get; set; }

        /// <summary>
        /// Call this method to tell the framework what decision print out debug information. The framework subscribes to this event
        /// and will provide a debug window for your decision tree.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AISetDecisionTreeCallback SetDecisionTree { get; set; }
        #endregion
    }
}
