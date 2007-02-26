/******************************************************************************
* The MIT License
* Copyright (c) 2006 Rusty Howell, Thomas Wiest
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest  twiest@users.sourceforge.net
//		Rusty Howell  rhowell@users.sourceforge.net

using System;
using System.Text;

namespace UvsChess
{
    public class ChessState
    {
        #region Members
        public const string StartState = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private ChessBoard _currentBoard;
        private ChessBoard _previousBoard;
        private ChessMove _previousMove;
        private ChessColor _yourColor;
        private int _fullMoves = 0;
        private int _halfMoves = 0;
        #endregion

        #region Constructors
        public ChessState()
            : this(StartState)
        {

        }

        /// <summary>
        /// See: http://en.wikipedia.org/wiki/Forsyth-Edwards_Notation
        /// </summary>
        /// <param name="fenBoard"></param>
        public ChessState(string fenBoard)
        {
            if ((fenBoard == null) || (fenBoard == string.Empty))
            {
                fenBoard = StartState;
            }
            FromFenBoard(fenBoard);
        }
        #endregion

        #region Properties and Indexers

        public ChessBoard CurrentBoard
        {
            get { return _currentBoard; }
            set { _currentBoard = value; }
        }       

        public ChessBoard PreviousBoard
        {
            get { return _previousBoard; }
            set { _previousBoard = value; }
        }       

        public ChessMove PreviousMove
        {
            get { return _previousMove; }
            set { _previousMove = value; }
        }       

        public ChessColor CurrentPlayerColor
        {
            get { return _yourColor; }
            set { _yourColor = value; }
        }

        /// <summary>
        /// Fullmove number: The number of the full move. It starts at 1, and is incremented after Black's move.
        /// </summary>
        public int FullMoves
        {
            get { return _fullMoves; }
            set 
            { 
                _fullMoves = value; 
                //Program.Log("FullMoves: " + _fullMoves);
            }
        }

        /// <summary>
        /// Halfmove clock: This is the number of halfmoves since the last pawn advance or capture. 
        /// This is used to determine if a draw can be claimed under the fifty move rule.
        /// See: http://en.wikipedia.org/wiki/Fifty_move_rule
        /// </summary>
        public int  HalfMoves
        {
            get { return _halfMoves; }
            set
            {
                _halfMoves = value;
                //Program.Log("HalfMoves: " + _halfMoves);
            }
        }

        public void MakeMove(ChessMove move)
        {
            PreviousMove = move;
            PreviousBoard = CurrentBoard.Clone();
            CurrentBoard.MakeMove(move);

            if (CurrentPlayerColor == ChessColor.White)
            {
                CurrentPlayerColor = ChessColor.Black;
            }
            else
            {
                CurrentPlayerColor = ChessColor.White;
            }
        }

        #endregion

        #region Methods and Operators
        /// <summary>
        /// Creates a deep copy of ChessState
        /// </summary>
        /// <returns></returns>
        public ChessState Clone()
        {
            ChessState newState = new ChessState();

            if (this.CurrentBoard == null)
                newState.CurrentBoard = null;                
            else
                newState.CurrentBoard = this.CurrentBoard.Clone();                

            if (this.PreviousBoard == null)
                newState.PreviousBoard = null;
            else
                newState.PreviousBoard = this.PreviousBoard.Clone();                

            if (this.PreviousMove == null)
                newState.PreviousMove = null;                
            else
                newState.PreviousMove = this.PreviousMove.Clone();

            newState.CurrentPlayerColor = this.CurrentPlayerColor;

            return newState;
        }

        #region FEN board
        /// <summary>
        /// Converts the ChessState to a FEN board
        /// </summary>
        /// <returns>FEN board</returns>
        public string ToFenBoard()
        {
            StringBuilder strBuild = new StringBuilder();
            strBuild.Append(CurrentBoard.ToFenBoard());

            if (CurrentPlayerColor == ChessColor.White)
            {
                strBuild.Append(" w");
            }
            else
            {
                strBuild.Append(" b");
            }

            //the two dashes are place holders for castling and en passant info respectively, (neither are currently supported)
            strBuild.Append(" - - " + HalfMoves.ToString() + " " + FullMoves.ToString());

            return strBuild.ToString();
        }

        /// <summary>
        /// Sets the chess state as described in the FEN board. 
        /// See: http://en.wikipedia.org/wiki/Forsyth-Edwards_Notation
        /// </summary>
        /// <param name="fenBoard"></param>
        public void FromFenBoard(string fenBoard)
        {            
            string[] lines = fenBoard.Split(' ');
            CurrentBoard = new ChessBoard(lines[0]);

            if (lines[1] == "w")
            {
                CurrentPlayerColor = ChessColor.White;
            }
            else if (lines[1] == "b")
            {
                CurrentPlayerColor = ChessColor.Black;
            }
            else
            {
                throw new Exception("Missing active color in FEN board");
            }
            
            HalfMoves = Convert.ToInt32(lines[4]);
            FullMoves = Convert.ToInt32(lines[5]);

            return;
        }

        #endregion

        #endregion
    }
}
