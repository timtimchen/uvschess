using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;

namespace StudentAI
{
    public class StudentAI : IChessAI
    {
        #region IChessAI Members that are implemented by the Student

        private Random random = new Random();

        /// <summary>
        /// The name of your AI
        /// </summary>
        public string Name
        {
#if DEBUG
            get { return "StudentAI (Debug)"; }
#else
            get { return "StudentAI"; }
#endif
        }

        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board</param>
        /// <param name="myColor">Your color</param>
        /// <returns> Returns the best chess move the player has for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
            ChessMove myNextMove = null;
            ChessColor opponentColor = (myColor == ChessColor.Black) ? ChessColor.White : ChessColor.Black;

            while (!IsMyTurnOver())
            {
                // search and find my pieces
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
                                        ChessPiece[,] boardAfterMove = board.RawBoard;
                                        boardAfterMove[toX, toY] = boardAfterMove[fromX, fromY];
                                        boardAfterMove[fromX, fromY] = ChessPiece.Empty;
                                        // handle the Promotion case
                                        if (boardAfterMove[toX, toY] == ChessPiece.BlackPawn && toY == ChessBoard.NumberOfRows - 1)
                                        {
                                            boardAfterMove[toX, toY] = ChessPiece.BlackQueen;
                                        }
                                        if (boardAfterMove[toX, toY] == ChessPiece.WhitePawn && toY == 0)
                                        {
                                            boardAfterMove[toX, toY] = ChessPiece.WhiteQueen;
                                        }
                                        if (IsInCheck(boardAfterMove, opponentColor))
                                        {
                                            potentialMove.Flag = ChessFlag.Check;
                                            if (IsCheckmate(boardAfterMove, opponentColor))
                                            {
                                                // found a Checkmate movement!
                                                potentialMove.Flag = ChessFlag.Checkmate;
                                                this.Log(myColor.ToString() + " (" + this.Name + ") just moved.");
                                                this.Log(string.Empty);
                                                Profiler.SetDepthReachedDuringThisTurn(2);
                                                return potentialMove;
                                            }
                                        }
                                        potentialMove.ValueOfMove = Math.Abs(EvaluateBoard(boardAfterMove));
                                        if (myNextMove == null || myNextMove.ValueOfMove < potentialMove.ValueOfMove)
                                        {
                                            myNextMove = potentialMove;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // if no possible movement was found, it is a Stalemate state
                if (myNextMove == null)
                {
                    myNextMove = new ChessMove(null, null, ChessFlag.Stalemate);
                }

                this.Log(myColor.ToString() + " (" + this.Name + ") just moved.");
                this.Log(string.Empty);
                // Since I have a move, break out of loop
                break;
            }

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
            ChessPiece[,] boardAfterMove = boardBeforeMove.RawBoard;
            boardAfterMove[moveToCheck.To.X, moveToCheck.To.Y] = boardAfterMove[moveToCheck.From.X, moveToCheck.From.Y];
            boardAfterMove[moveToCheck.From.X, moveToCheck.From.Y] = ChessPiece.Empty;
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
        public bool IsInCheck(ChessPiece[,] rawBoard, ChessColor colorOfPlayer)
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
                    if (rawBoard[x, y] == selfKing)
                    {
                        KingX = x;
                        KingY = y;
                    }
                    else if (rawBoard[x, y] == opponentKnight)
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
                if (i == 1 && rawBoard[KingX + i, KingY] == opponentKing) return true;
                if (rawBoard[KingX + i, KingY] == opponentRook) return true;
                if (rawBoard[KingX + i, KingY] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (rawBoard[KingX + i, KingY] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingX - i >= 0)  // go west
            {
                if (i == 1 && rawBoard[KingX - i, KingY] == opponentKing) return true;
                if (rawBoard[KingX - i, KingY] == opponentRook) return true;
                if (rawBoard[KingX - i, KingY] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (rawBoard[KingX - i, KingY] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingY + i < ChessBoard.NumberOfRows)  // go south
            {
                if (i == 1 && rawBoard[KingX, KingY + i] == opponentKing) return true;
                if (rawBoard[KingX, KingY + i] == opponentRook) return true;
                if (rawBoard[KingX, KingY + i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (rawBoard[KingX, KingY + i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingY - i >= 0)  // go north
            {
                if (i == 1 && rawBoard[KingX, KingY - i] == opponentKing) return true;
                if (rawBoard[KingX, KingY - i] == opponentRook) return true;
                if (rawBoard[KingX, KingY - i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (rawBoard[KingX, KingY - i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingX + i < ChessBoard.NumberOfColumns && KingY + i < ChessBoard.NumberOfRows)  // go southeast
            {
                if (i == 1 && rawBoard[KingX + i, KingY + i] == opponentKing) return true;
                if (i == 1 && colorOfPlayer == ChessColor.Black && rawBoard[KingX + i, KingY + i] == ChessPiece.WhitePawn) return true;
                if (rawBoard[KingX + i, KingY + i] == opponentBishop) return true;
                if (rawBoard[KingX + i, KingY + i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (rawBoard[KingX + i, KingY + i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingX - i >= 0 && KingY + i < ChessBoard.NumberOfRows)  // go southwest
            {
                if (i == 1 && rawBoard[KingX - i, KingY + i] == opponentKing) return true;
                if (i == 1 && colorOfPlayer == ChessColor.Black && rawBoard[KingX - i, KingY + i] == ChessPiece.WhitePawn) return true;
                if (rawBoard[KingX - i, KingY + i] == opponentBishop) return true;
                if (rawBoard[KingX - i, KingY + i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (rawBoard[KingX - i, KingY + i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingX + i < ChessBoard.NumberOfColumns && KingY - i >= 0)  // go northeast
            {
                if (i == 1 && rawBoard[KingX + i, KingY - i] == opponentKing) return true;
                if (i == 1 && colorOfPlayer == ChessColor.White && rawBoard[KingX + i, KingY - i] == ChessPiece.BlackPawn) return true;
                if (rawBoard[KingX + i, KingY - i] == opponentBishop) return true;
                if (rawBoard[KingX + i, KingY - i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (rawBoard[KingX + i, KingY - i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            i = 1;
            while (KingX - i >= 0 && KingY - i >= 0)  // go northwest
            {
                if (i == 1 && rawBoard[KingX - i, KingY - i] == opponentKing) return true;
                if (i == 1 && colorOfPlayer == ChessColor.White && rawBoard[KingX - i, KingY - i] == ChessPiece.BlackPawn) return true;
                if (rawBoard[KingX - i, KingY - i] == opponentBishop) return true;
                if (rawBoard[KingX - i, KingY - i] == opponentQueen) return true;
                // if found a piece in this way which can't attack the king, it is a safe direction, then break
                if (rawBoard[KingX - i, KingY - i] != ChessPiece.Empty) break;
                i++;  // if next grid is empty, continue searching
            }
            // now the King is not in check state
            return false;
        }

        // assuming the player in turn is in a Check state first.
        // search if there is a movement to avoid Checking. If not found, it is Checkmate. 
        public bool IsCheckmate(ChessPiece[,] rawBoard, ChessColor playerColor)
        {
            for (int fromY = 0; fromY < ChessBoard.NumberOfRows; fromY++)
            {
                for (int fromX = 0; fromX < ChessBoard.NumberOfColumns; fromX++)
                {
                    if (rawBoard[fromX, fromY] != ChessPiece.Empty && PieceColor(rawBoard[fromX, fromY]) == playerColor)
                    {
                        // if found one of my pieces, search if there are posible movements of this piece
                        for (int toY = 0; toY < ChessBoard.NumberOfRows; toY++)
                        {
                            for (int toX = 0; toX < ChessBoard.NumberOfColumns; toX++)
                            {
                                ChessLocation moveFrom = new ChessLocation(fromX, fromY);
                                ChessLocation moveTo = new ChessLocation(toX, toY);
                                ChessMove potentialMove = new ChessMove(moveFrom, moveTo, ChessFlag.NoFlag);
                                if (IsValidMove(new ChessBoard(rawBoard), potentialMove, playerColor))
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

        // return a evaluation based on a rawBoard state
        // White color side will be grated a positive integer, while Black side gets a negative
        // So more positive means White is leading, and more negative means Black is leading
        public int EvaluateBoard(ChessPiece[,] rawBoard)
        {
            return random.Next();
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
