using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    private bool earlyGame = true, midGame = false, endGame = false;
    /* An integer that memorises the number of times this "Think" function was called;
     useful to determine the current state of the match. */
    private int callNumber = 0;
    
    private static Random random = new();
    
    private Move? lastMove = null;
    private Move moveToDo;
    private Move[] allMoves;

    private Stack<Move> movesDoneByMiniMax = new();

    private bool botIsWhite;
    
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    
    public Move Think(Board board, Timer timer)
    {
        allMoves = board.GetLegalMoves();
        botIsWhite = board.IsWhiteToMove;

        if (earlyGame)
            EarlyPolicy(board);
        else
            MidGamePolicy(board);
        CheckIfMoveIsStillLegal(board);
        return moveToDo;
    }

    private void RandomMove() => moveToDo = allMoves[random.Next(allMoves.Length)];

    private void EarlyPolicy(Board board) 
    {
        if (++this.callNumber >= 5)
        { 
            earlyGame = false;
            midGame = true;
        }
        if (!this.lastMove.HasValue)
            RandomMove();

        // Avoid moving the same piece during openings
        else
            do
            {
                RandomMove();
                //Console.WriteLine("Move to do: " + moveToDo.ToString()); // debug only
                //Console.WriteLine("Last move: " + lastMove.ToString()); // debug only
            } while (lastMove.Value.TargetSquare.Name.Equals(moveToDo.StartSquare.Name) && allMoves.Length > 1);
            // if there's only one move available it should be played even if it implies moving the same piece

        lastMove = moveToDo;
    }

    private void MidGamePolicy(Board board) {
        MiniMaxAlfaBetaPruning(board, 3, int.MinValue, int.MaxValue, true);
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

    private int MiniMaxAlfaBetaPruning(Board board, int depth, int alpha, int beta, bool maximizingPlayer) 
    {
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw())
            return Evaluate(board);

        Move[] allPossibleMoves = board.GetLegalMoves();

        if (maximizingPlayer) 
        {
            int maxEvaluation = int.MinValue;
            foreach (Move move in allPossibleMoves)
            {
                board.MakeMove(move);
                movesDoneByMiniMax.Push(move);
                PrettyPrintDebug(depth, 3);
                Console.WriteLine("Pushed move " + move.ToString() + "at depth " + depth);
                int evaluation = MiniMaxAlfaBetaPruning(board, depth - 1, alpha, beta, false);
                board.UndoMove(move);
                var popped = movesDoneByMiniMax.Pop();
                PrettyPrintDebug(depth, 3);
                Console.WriteLine("Popped move: " + popped.ToString(), ", expected: " + move.ToString());
                if (!popped.ToString().Equals(move.ToString()))
                {
                    Console.WriteLine("INCONSISTENCY FOUND!\n\n");
                    System.Environment.Exit(-12);
                }
                if (evaluation >= maxEvaluation)
                {
                    maxEvaluation = evaluation;
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
                movesDoneByMiniMax.Push(move);
                PrettyPrintDebug(depth, 3);
                Console.WriteLine("Pushed move " + move.ToString() + "at depth " + depth);
                int evaluation = MiniMaxAlfaBetaPruning(board, depth - 1, alpha, beta, true);
                board.UndoMove(move);
                var popped = movesDoneByMiniMax.Pop();
                PrettyPrintDebug(depth, 3);
                Console.WriteLine("Popped move: " + popped.ToString(), ", expected: " + move.ToString());
                if (!popped.ToString().Equals(move.ToString()))
                {
                    Console.WriteLine("INCONSISTENCY FOUND!\n\n");
                    System.Environment.Exit(-12);
                }
                minEvaluation = Math.Min(minEvaluation, evaluation);
                beta = Math.Min(beta, evaluation);
            }
            return minEvaluation;
        }
    }

    private int Evaluate(Board board) 
    {
        int whiteMaterial = 0, blackMaterial = 0, i;
        for (i = 0; i < 6; i++)
            whiteMaterial += board.GetAllPieceLists()[i].Count * pieceValues[i + 1];
        for (i = 0; i < 6; i++)
            blackMaterial += board.GetAllPieceLists()[i + 6].Count * pieceValues[i + 1];
        if (botIsWhite)
            return whiteMaterial - blackMaterial;
        return - (whiteMaterial - blackMaterial);
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