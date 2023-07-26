using System;
using System.Diagnostics;
using System.IO;

public class Stockfish : IDisposable {
    private Process stockfishProcess;

    public Stockfish(string path) {
        stockfishProcess = new Process();
        stockfishProcess.StartInfo.FileName = path;
        stockfishProcess.StartInfo.UseShellExecute = false;
        stockfishProcess.StartInfo.RedirectStandardInput = true;
        stockfishProcess.StartInfo.RedirectStandardOutput = true;
        stockfishProcess.StartInfo.CreateNoWindow = true;

        stockfishProcess.Start();
    }

    /// <summary>
    /// Returns the best move according to Stockfish. 
    /// </summary>
    public string BestMove(string fenPosition, int searchDepth) {
        if (stockfishProcess == null || stockfishProcess.HasExited)
            throw new InvalidOperationException("Stockfish process is not running (check the program path)");

        stockfishProcess.StandardInput.WriteLine("uci");
        stockfishProcess.StandardInput.WriteLine("position fen " + fenPosition);
        stockfishProcess.StandardInput.WriteLine("go depth " + searchDepth);

        string bestMove = string.Empty;
        while (true) {
            string line = stockfishProcess.StandardOutput.ReadLine();
            if (line.StartsWith("bestmove")) {
                bestMove = line.Split(' ')[1];
                break;
            }
        }
        return bestMove;
    }

    public void Dispose() {
        if (stockfishProcess != null && !stockfishProcess.HasExited) {
            stockfishProcess.StandardInput.WriteLine("quit");
            stockfishProcess.WaitForExit(500);
            stockfishProcess.Dispose();
        }
    }
}
