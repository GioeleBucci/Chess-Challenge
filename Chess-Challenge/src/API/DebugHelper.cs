
namespace ChessChallenge.API {
    using ChessChallenge.Application.APIHelpers;
    using ChessChallenge.Chess;
    using System;
        
    /// <summary>
    /// Custom class that offers tools for debugging purposes
    /// </summary>
    public class DebugHelper {
        bool flag = true;
        Stockfish engine;
        /// <summary>
        /// Outputs the color you're playing as
        /// </summary>
        public void PlayingAs(Board board) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!flag) return;
            flag = false;
            string color = board.IsWhiteToMove ? "White" : "Black";
            Console.WriteLine("Playing as " + color + "\nPress any key to continue");
            Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Outputs the move and the respective evaluation according to Eval function.
        /// </summary>
        public void LogCurrentMove(Board board, Move move, Func<Board,int> Eval) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Move: " + move.StartSquare.Name + move.TargetSquare.Name);
            board.MakeMove(move);
            Console.WriteLine("Evaluation score: " + Eval(board) + "\nPress any key to continue");
            board.UndoMove(move);
            Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Start Stockfish if not already running
        /// </summary>
        private void StockfishStart(string path) {
            if(engine == null)
                engine = new Stockfish(path);
        }

        /// <summary>
        /// Returns the best move according to Stockfish.
        /// If halt is true the program will wait for a key press.
        /// </summary>
        public void BestEngineMove(Board board, int searchDepth, bool halt, string stockfishPath) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            StockfishStart(stockfishPath);
            string output = engine.BestMove(board.GetFenString(), searchDepth);
            Console.WriteLine("Best engine move: " + output);
            if (halt) Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
