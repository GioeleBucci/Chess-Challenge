using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    private static readonly int MAX_DEPTH = 3;
    
    private static Random random = new();
    
    private Move moveToDo;
    private Move[] allMoves;

    //private Stack<Move> movesDoneByMiniMax = new(); //debug only

    private bool botIsWhite;
    
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    
    public Move Think(Board board, Timer timer)
    {
        allMoves = board.GetLegalMoves();
        botIsWhite = board.IsWhiteToMove;
        MidGamePolicy(board);
        return moveToDo;
    }

    private void RandomMove() => moveToDo = allMoves[random.Next(allMoves.Length)];

    private void MidGamePolicy(Board board) {
        MiniMaxAlfaBetaPruning(board, MAX_DEPTH, MAX_DEPTH, int.MinValue, int.MaxValue, true);
        //Console.WriteLine("****************************************************************\n\n");
    }

    private bool CheckIfMoveIsStillLegal(Board board)
    {
        if (!board.GetLegalMoves().Contains(moveToDo))
        {
            //Console.WriteLine("Illegal move detected: " + moveToDo.ToString());
            //RandomMove(); //without this I still get illegal moves...
            return false;
        }
        return true;
    }

    private int MiniMaxAlfaBetaPruning(Board board, int maxDepth, int currentDepth, int alpha, int beta, bool maximizingPlayer) 
    {
        if (currentDepth == 0 || board.IsInCheckmate() || board.IsDraw())
            return Evaluate(board);

        Move[] allPossibleMoves = board.GetLegalMoves();

        if (maximizingPlayer) 
        {
            int maxEvaluation = int.MinValue;
            foreach (Move move in allPossibleMoves)
            {
                board.MakeMove(move);
                //movesDoneByMiniMax.Push(move);
                //PrettyPrintDebug(depth, MAX_DEPTH);
                //Console.WriteLine("Pushed move " + move.ToString() + "at depth " + depth);
                int evaluation = MiniMaxAlfaBetaPruning(board, maxDepth, currentDepth - 1, alpha, beta, false);
                board.UndoMove(move);
                //var popped = movesDoneByMiniMax.Pop();
                //PrettyPrintDebug(depth, MAX_DEPTH);
                //Console.WriteLine("Popped move: " + popped.ToString(), ", expected: " + move.ToString());
                /*
                if (!popped.ToString().Equals(move.ToString()))
                {
                    Console.WriteLine("INCONSISTENCY FOUND!\n\n");
                    System.Environment.Exit(-12);
                }
                */
                alpha = Math.Max(alpha, evaluation);
                if (evaluation >= maxEvaluation)
                {
                    maxEvaluation = evaluation;
                    if (currentDepth == maxDepth)
                        moveToDo = move;
                    //Console.WriteLine("Depth is: " + depth + ", in the MiniMax function the move is: " + moveToDo.ToString() +
                      //" and the move is " + (CheckIfMoveIsStillLegal(board) ? "legal" : "ILLEGAL"));
                }
            }
            return maxEvaluation;
        } else {
            int minEvaluation = int.MaxValue;
            foreach (Move move in allPossibleMoves)
            {
                board.MakeMove(move);
                //movesDoneByMiniMax.Push(move);
                //PrettyPrintDebug(depth, MAX_DEPTH);
                //Console.WriteLine("Pushed move " + move.ToString() + "at depth " + depth);
                int evaluation = MiniMaxAlfaBetaPruning(board, maxDepth, currentDepth - 1, alpha, beta, true);
                board.UndoMove(move);
                //var popped = movesDoneByMiniMax.Pop();
                //PrettyPrintDebug(depth, MAX_DEPTH);
                //Console.WriteLine("Popped move: " + popped.ToString(), ", expected: " + move.ToString());
                /*
                if (!popped.ToString().Equals(move.ToString()))
                {
                    Console.WriteLine("INCONSISTENCY FOUND!\n\n");
                    System.Environment.Exit(-12);
                }
                */
                minEvaluation = Math.Min(minEvaluation, evaluation);
            }
            return minEvaluation;
        }
    }

    private int Evaluate(Board board) 
    {
        int score = 0;
        if (board.IsDraw())
            return score;
        if (board.IsInCheckmate())
            return int.MaxValue;
        int whiteMaterial = 0, blackMaterial = 0, i;
        for (i = 0; i < 6; i++)
            whiteMaterial += board.GetAllPieceLists()[i].Count * pieceValues[i + 1];
        for (i = 0; i < 6; i++)
            blackMaterial += board.GetAllPieceLists()[i + 6].Count * pieceValues[i + 1];
        if (botIsWhite)
            score = whiteMaterial - blackMaterial;
        else 
            score = blackMaterial - whiteMaterial;
        return score;
    }

    private void PrettyPrintDebug(int currentDepth, int maxDepth)
    {
        /* This method is used to print some tabs before writing a line containing the moves pushed
         and popped by the caller. It visually represents the depths at which the moves are done.*/
        for (int j = 0; j < maxDepth; j++)
        {
            for (int xx = Math.Abs(maxDepth - currentDepth); xx > 0; xx--)
                Console.Write("\t");
        }
    }
}