using ChessChallenge.API;
using System;

namespace ChessChallenge.Example {
    public class MyBot : IChessBot {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 320, 330, 500, 900, 20000 };
        Move bestMoveRoot = Move.NullMove;
        int bestEval; //debug only
        int positionsAnalyzed = 0; //debug only
        int prunes = 0; //debug only
        bool isWhite;
        DebugHelper debugHelper = new DebugHelper();

        bool IsTerminalState(Board board) {
            return board.IsDraw() || board.IsInCheckmate();
        }


        int NegaMax(Board board, int depth, int alpha, int beta, int ply) {
            bool nonRoot = ply > 0;
            int best = -30000;

            //check for repetitions
            if (nonRoot && board.IsRepeatedPosition())
                return 0;
            if (depth == 0 || IsTerminalState(board))
                return Evaluate(board);

            Move[] moves = board.GetLegalMoves();
            Move bestMove = Move.NullMove;

            for (int i = 0; i < moves.Length; i++) {
                Move move = moves[i];
                board.MakeMove(move);
                int score = -NegaMax(board, depth - 1, -alpha, -beta, ply + 1);
                board.UndoMove(move);

                //new best move
                if (score > best) {
                    best = score;
                    bestMove = move;
                    if (ply == 0) {
                        bestMoveRoot = move;
                        board.MakeMove(move);
                        Console.WriteLine("Best eval: " + Evaluate(board));
                        board.UndoMove(move);
                    }
                    //pruning
                    alpha = Math.Max(alpha, score);
                    if (alpha >= beta) break;
                }
            }
            return best;
        }

        ulong KnightPos15 = 0b0000_0000_0000_0000_0011_1100_0011_1100_0011_1100_0011_1100_0000_0000_0000_0000;

        int Evaluate(Board board) {
            int evalScore = 0;

            //check if game is over
            if (board.IsDraw())
                return 0;
            if (board.IsInCheckmate()) {
                if (isWhite && board.IsWhiteToMove || !isWhite && !board.IsWhiteToMove)
                    return int.MinValue; //got mated
                else return int.MaxValue; //mated the adversary
            }

            PieceList[] pieceList = board.GetAllPieceLists();
            for (int i = 0; i < 12; i++) {
                int temp = pieceList[i].Count * pieceValues[i % 6 + 1];
                if (i > 5)
                    temp *= -1;
                evalScore += temp;
            }
            //check knights (experimental
            ulong whiteKnightsBonus = board.GetPieceBitboard(PieceType.Knight, true) & KnightPos15;
            ulong blackKnightsMalus = board.GetPieceBitboard(PieceType.Knight, false) & KnightPos15;
            evalScore += BitboardHelper.GetNumberOfSetBits(whiteKnightsBonus) * 15 - BitboardHelper.GetNumberOfSetBits(blackKnightsMalus) * 15;
            if (!isWhite) evalScore *= -1;
            return evalScore;
        }

        public Move Think(Board board, Timer timer) {
            debugHelper.BestEngineMove(board, 10, false, "C:\\Users\\gioel\\Desktop\\stockfish\\stockfish-windows-x86-64-avx2.exe");
            debugHelper.PlayingAs(board);
            NegaMax(board, 4, -30000, 30000, 0);
            return bestMoveRoot;
        }   
    }
}
