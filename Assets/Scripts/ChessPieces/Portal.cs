using System.Collections.Generic;
using UnityEngine;

public class Portal : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        var r = new List<Vector2Int>();

        //Get all available spaces
        for (var x = 0; x < tileCountX; x++)
        for (var y = 0; y < tileCountY; y++)
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));

        return r;
    }
}