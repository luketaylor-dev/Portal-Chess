using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        var r = new List<Vector2Int>();

        //Right
        if (currentX + 1 < tileCountX)
        {
            //Right
            if (board[currentX + 1, currentY] == null)
                r.Add(new Vector2Int(currentX + 1, currentY));
            else if (board[currentX + 1, currentY].team != team &&
                     board[currentX + 1, currentY].type != ChessPieceType.Portal)
                r.Add(new Vector2Int(currentX + 1, currentY));
            else if (board[currentX + 1, currentY].type == ChessPieceType.Portal)
                for (var x = 0; x < tileCountX; x++)
                for (var y = 0; y < tileCountY; y++)
                    if (x != currentX + 1 || y != currentY)
                        if (board[x, y] != null && board[x, y].type == ChessPieceType.Portal)
                            if (x + 1 < tileCountX)
                            {
                                if (board[x + 1, y] == null)
                                    r.Add(new Vector2Int(x + 1, y));
                                else if (board[x + 1, y].team != team && board[x + 1, y].type != ChessPieceType.Portal)
                                    r.Add(new Vector2Int(x + 1, y));
                            }

            //Top right
            if (currentY + 1 < tileCountY)
                if (board[currentX + 1, currentY + 1] == null)
                    r.Add(new Vector2Int(currentX + 1, currentY + 1));
                else if (board[currentX + 1, currentY + 1].team != team &&
                         board[currentX + 1, currentY + 1].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(currentX + 1, currentY + 1));
                else if (board[currentX + 1, currentY + 1].type == ChessPieceType.Portal)
                    for (var x = 0; x < tileCountX; x++)
                    for (var y = 0; y < tileCountY; y++)
                        if (x != currentX + 1 || y != currentY + 1)
                            if (board[x, y] != null && board[x, y].type == ChessPieceType.Portal)
                                if (y + 1 < tileCountY && x + 1 < tileCountX)
                                {
                                    if (board[x + 1, y + 1] == null)
                                        r.Add(new Vector2Int(x + 1, y + 1));
                                    else if (board[x + 1, y + 1].team != team &&
                                             board[x + 1, y + 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(x + 1, y + 1));
                                }

            //Top Bottom
            if (currentY - 1 >= 0)
                if (board[currentX + 1, currentY - 1] == null)
                    r.Add(new Vector2Int(currentX + 1, currentY - 1));
                else if (board[currentX + 1, currentY - 1].team != team &&
                         board[currentX + 1, currentY - 1].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(currentX + 1, currentY - 1));
                else if (board[currentX + 1, currentY - 1].type == ChessPieceType.Portal)
                    for (var x = 0; x < tileCountX; x++)
                    for (var y = 0; y < tileCountY; y++)
                        if (x != currentX + 1 || y != currentY - 1)
                            if (board[x, y] != null && board[x, y].type == ChessPieceType.Portal)
                                if (y - 1 >= 0 && x + 1 < tileCountX)
                                {
                                    if (board[x + 1, y - 1] == null)
                                        r.Add(new Vector2Int(x + 1, y - 1));
                                    else if (board[x + 1, y - 1].team != team &&
                                             board[x + 1, y - 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(x + 1, y - 1));
                                }
        }


        //Left
        if (currentX - 1 >= 0)
        {
            //Left
            if (board[currentX - 1, currentY] == null)
                r.Add(new Vector2Int(currentX - 1, currentY));
            else if (board[currentX - 1, currentY].team != team &&
                     board[currentX - 1, currentY].type != ChessPieceType.Portal)
                r.Add(new Vector2Int(currentX - 1, currentY));
            else if (board[currentX - 1, currentY].type == ChessPieceType.Portal)
                for (var x = 0; x < tileCountX; x++)
                for (var y = 0; y < tileCountY; y++)
                    if (x != currentX - 1 || y != currentY)
                        if (board[x, y] != null && board[x, y].type == ChessPieceType.Portal)
                            if (x - 1 >= 0)
                            {
                                if (board[x - 1, y] == null)
                                    r.Add(new Vector2Int(x - 1, y));
                                else if (board[x - 1, y].team != team && board[x - 1, y].type != ChessPieceType.Portal)
                                    r.Add(new Vector2Int(x - 1, y));
                            }

            //Top left
            if (currentY + 1 < tileCountY)
                if (board[currentX - 1, currentY + 1] == null)
                    r.Add(new Vector2Int(currentX - 1, currentY + 1));
                else if (board[currentX - 1, currentY + 1].team != team &&
                         board[currentX - 1, currentY + 1].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(currentX - 1, currentY + 1));
                else if (board[currentX - 1, currentY + 1].type == ChessPieceType.Portal)
                    for (var x = 0; x < tileCountX; x++)
                    for (var y = 0; y < tileCountY; y++)
                        if (x != currentX - 1 || y != currentY + 1)
                            if (board[x, y] != null && board[x, y].type == ChessPieceType.Portal)
                                if (x - 1 >= 0 && y + 1 < tileCountY)
                                {
                                    if (board[x - 1, y + 1] == null)
                                        r.Add(new Vector2Int(x - 1, y + 1));
                                    else if (board[x - 1, y + 1].team != team &&
                                             board[x - 1, y + 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(x - 1, y + 1));
                                }

            //Top left
            if (currentY - 1 >= 0)
                if (board[currentX - 1, currentY - 1] == null)
                    r.Add(new Vector2Int(currentX - 1, currentY - 1));
                else if (board[currentX - 1, currentY - 1].team != team &&
                         board[currentX - 1, currentY - 1].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(currentX - 1, currentY - 1));
                else if (board[currentX - 1, currentY - 1].type == ChessPieceType.Portal)
                    for (var x = 0; x < tileCountX; x++)
                    for (var y = 0; y < tileCountY; y++)
                        if (x != currentX - 1 || y != currentY - 1)
                            if (board[x, y] != null && board[x, y].type == ChessPieceType.Portal)
                                if (x - 1 >= 0 && y - 1 >= 0)
                                    if (board[x - 1, y - 1] == null)
                                        r.Add(new Vector2Int(x - 1, y - 1));
                                    else if (board[x - 1, y - 1].team != team &&
                                             board[x - 1, y - 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(x - 1, y - 1));
        }

        //Up
        if (currentY + 1 < tileCountY)
            if (board[currentX, currentY + 1] == null)
                r.Add(new Vector2Int(currentX, currentY + 1));
            else if (board[currentX, currentY + 1].team != team &&
                     board[currentX, currentY + 1].type != ChessPieceType.Portal)
                r.Add(new Vector2Int(currentX, currentY + 1));
            else if (board[currentX, currentY + 1].type == ChessPieceType.Portal)
                for (var x = 0; x < tileCountX; x++)
                for (var y = 0; y < tileCountY; y++)
                    if (x != currentX || y != currentY + 1)
                        if (board[x, y] != null && board[x, y].type == ChessPieceType.Portal)
                            if (y + 1 < tileCountY)
                            {
                                if (board[x, y + 1] == null)
                                    r.Add(new Vector2Int(x, y + 1));
                                else if (board[x, y + 1].team != team && board[x, y].type != ChessPieceType.Portal)
                                    r.Add(new Vector2Int(x, y + 1));
                            }

        //Down
        if (currentY - 1 >= 0)
            if (board[currentX, currentY - 1] == null)
                r.Add(new Vector2Int(currentX, currentY - 1));
            else if (board[currentX, currentY - 1].team != team)
                r.Add(new Vector2Int(currentX, currentY - 1));
            else if (board[currentX, currentY - 1].type == ChessPieceType.Portal)
                for (var x = 0; x < tileCountX; x++)
                for (var y = 0; y < tileCountY; y++)
                    if (x != currentX || y != currentY - 1)
                        if (board[x, y] != null && board[x, y].type == ChessPieceType.Portal)
                            if (y - 1 >= 0)
                            {
                                if (board[x, y - 1] == null)
                                    r.Add(new Vector2Int(x, y - 1));
                                else if (board[x, y - 1].team != team && board[x, y].type != ChessPieceType.Portal)
                                    r.Add(new Vector2Int(x, y - 1));
                            }

        return r;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList,
        ref List<Vector2Int> availableMoves, int tileCountX, int tileCountY)
    {
        var r = SpecialMove.None;

        var kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == (team == 0 ? 0 : 7));
        var leftRook = moveList.Find(m => m[0].x == 0 && m[0].y == (team == 0 ? 0 : 7));
        var rightRook = moveList.Find(m => m[0].x == 7 && m[0].y == (team == 0 ? 0 : 7));

        if (kingMove == null && currentX == 4)
        {
            //White team
            if (team == 0)
            {
                //Left Rook
                if (leftRook == null)
                    if (board[0, 0]?.type == ChessPieceType.Rook)
                        if (board[0, 0].team == 0)
                            if (board[3, 0] == null)
                                if (board[2, 0] == null)
                                    if (board[1, 0] == null)
                                    {
                                        availableMoves.Add(new Vector2Int(2, 0));
                                        r = SpecialMove.Castling;
                                    }

                //Right Rook
                if (rightRook == null)
                    if (board[7, 0]?.type == ChessPieceType.Rook)
                        if (board[7, 0].team == 0)
                            if (board[5, 0] == null)
                                if (board[6, 0] == null)
                                {
                                    availableMoves.Add(new Vector2Int(6, 0));
                                    r = SpecialMove.Castling;
                                }
            }
            else
            {
                //Left Rook
                if (leftRook == null)
                    if (board[0, 7]?.type == ChessPieceType.Rook)
                        if (board[0, 7].team == 1)
                            if (board[3, 7] == null)
                                if (board[2, 7] == null)
                                    if (board[1, 7] == null)
                                    {
                                        availableMoves.Add(new Vector2Int(2, 7));
                                        r = SpecialMove.Castling;
                                    }

                //Right Rook
                if (leftRook == null)
                    if (board[7, 7]?.type == ChessPieceType.Rook)
                        if (board[7, 7].team == 1)
                            if (board[5, 7] == null)
                                if (board[6, 7] == null)
                                {
                                    availableMoves.Add(new Vector2Int(6, 7));
                                    r = SpecialMove.Castling;
                                }
            }
        }

        return r;
    }
}