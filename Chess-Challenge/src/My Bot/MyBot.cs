using ChessChallenge.API;
using System;

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
    
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    
    public Move Think(Board board, Timer timer)
    {
        allMoves = board.GetLegalMoves();

        if (earlyGame)
            EarlyPolicy(board);
        else
            MidGamePolicy(board);
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
            } while (lastMove.Value.TargetSquare.Name.Equals(moveToDo.StartSquare.Name));

        lastMove = moveToDo;
    }

    private void MidGamePolicy(Board board) {
        int highestValueCapture = 0;
        foreach (var move in allMoves) 
        {
            Piece capturedPiece = board.GetPiece(move.TargetSquare);
            int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

            if (capturedPieceValue >= highestValueCapture)
            {
                moveToDo = move;
                highestValueCapture = capturedPieceValue;
            }
        }
    }
}