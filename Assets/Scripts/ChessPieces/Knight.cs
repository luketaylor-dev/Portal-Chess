using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        var r = new List<Vector2Int>();

        //Top right
        var x = currentX + 1;
        var y = currentY + 2;
        if (x < tileCountX && y < tileCountY)
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].type == ChessPieceType.Portal)
                for (var px = 0; px < tileCountX; px++)
                for (var py = 0; py < tileCountY; py++)
                    if (px != x || py != y)
                        if (board[px, py] != null)
                            if (board[px, py].type == ChessPieceType.Portal)
                            {
                                if (px + 1 < tileCountX)
                                {
                                    //up then right
                                    if (board[px + 1, py] == null)
                                        r.Add(new Vector2Int(px + 1, py));
                                    else if (board[px + 1, py].team != team &&
                                             board[px + 1, py].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px + 1, py));
                                }

                                if (py + 1 < tileCountY)
                                {
                                    //right then up
                                    if (board[px, py + 1] == null)
                                        r.Add(new Vector2Int(px, py + 1));
                                    else if (board[px, py + 1].team != team &&
                                             board[px, py + 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px, py + 1));
                                }
                            }

        x = currentX + 2;
        y = currentY + 1;
        if (x < tileCountX && y < tileCountY)
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].type == ChessPieceType.Portal)
                for (var px = 0; px < tileCountX; px++)
                for (var py = 0; py < tileCountY; py++)
                    if (px != x || py != y)
                        if (board[px, py] != null)
                            if (board[px, py].type == ChessPieceType.Portal)
                            {
                                //up then right
                                if (px + 1 < tileCountX)
                                {
                                    if (board[px + 1, py] == null)
                                        r.Add(new Vector2Int(px + 1, py));
                                    else if (board[px + 1, py].team != team &&
                                             board[px + 1, py].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px + 1, py));
                                }

                                //right then up
                                if (py + 1 < tileCountY)
                                {
                                    if (board[px, py + 1] == null)
                                        r.Add(new Vector2Int(px, py + 1));
                                    else if (board[px, py + 1].team != team &&
                                             board[px, py + 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px, py + 1));
                                }
                            }

        //Top left
        x = currentX - 1;
        y = currentY + 2;
        if (x >= 0 && y < tileCountY)
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].type == ChessPieceType.Portal)
                for (var px = 0; px < tileCountX; px++)
                for (var py = 0; py < tileCountY; py++)
                    if (px != x || py != y)
                        if (board[px, py] != null)
                            if (board[px, py].type == ChessPieceType.Portal)
                            {
                                if (px - 1 >= 0)
                                {
                                    //up then left
                                    if (board[px - 1, py] == null)
                                        r.Add(new Vector2Int(px - 1, py));
                                    else if (board[px - 1, py].team != team &&
                                             board[px - 1, py].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px - 1, py));
                                }

                                if (py + 1 < tileCountY)
                                {
                                    //left then up
                                    if (board[px, py + 1] == null)
                                        r.Add(new Vector2Int(px, py + 1));
                                    else if (board[px, py + 1].team != team &&
                                             board[px, py + 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px, py + 1));
                                }
                            }

        x = currentX - 2;
        y = currentY + 1;
        if (x >= 0 && y < tileCountY)
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].type == ChessPieceType.Portal)
                for (var px = 0; px < tileCountX; px++)
                for (var py = 0; py < tileCountY; py++)
                    if (px != x || py != y)
                        if (board[px, py] != null)
                            if (board[px, py].type == ChessPieceType.Portal)
                            {
                                //up then left
                                if (px - 1 >= 0)
                                {
                                    if (board[px - 1, py] == null)
                                        r.Add(new Vector2Int(px - 1, py));
                                    else if (board[px - 1, py].team != team &&
                                             board[px - 1, py].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px - 1, py));
                                }

                                //left then up
                                if (py + 1 < tileCountY)
                                {
                                    if (board[px, py + 1] == null)
                                        r.Add(new Vector2Int(px, py + 1));
                                    else if (board[px, py + 1].team != team &&
                                             board[px, py + 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px, py + 1));
                                }
                            }

        //Bottom Right
        x = currentX + 1;
        y = currentY - 2;
        if (x < tileCountY && y >= 0)
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].type == ChessPieceType.Portal)
                for (var px = 0; px < tileCountX; px++)
                for (var py = 0; py < tileCountY; py++)
                    if (px != x || py != y)
                        if (board[px, py] != null)
                            if (board[px, py].type == ChessPieceType.Portal)
                            {
                                //down then right
                                if (px + 1 < tileCountX)
                                {
                                    if (board[px + 1, py] == null)
                                        r.Add(new Vector2Int(px + 1, py));
                                    else if (board[px + 1, py].team != team &&
                                             board[px + 1, py].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px + 1, py));
                                }

                                //right then up
                                if (py - 1 >= 0)
                                {
                                    if (board[px, py - 1] == null)
                                        r.Add(new Vector2Int(px, py - 1));
                                    else if (board[px, py - 1].team != team &&
                                             board[px, py - 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px, py - 1));
                                }
                            }

        x = currentX + 2;
        y = currentY - 1;
        if (x < tileCountY && y >= 0)
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].type == ChessPieceType.Portal)
                for (var px = 0; px < tileCountX; px++)
                for (var py = 0; py < tileCountY; py++)
                    if (px != x || py != y)
                        if (board[px, py] != null)
                            if (board[px, py].type == ChessPieceType.Portal)
                            {
                                //down then right
                                if (px + 1 < tileCountX)
                                {
                                    if (board[px + 1, py] == null)
                                        r.Add(new Vector2Int(px + 1, py));
                                    else if (board[px + 1, py].team != team &&
                                             board[px + 1, py].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px + 1, py));
                                }

                                //right then up
                                if (py - 1 >= 0)
                                {
                                    if (board[px, py - 1] == null)
                                        r.Add(new Vector2Int(px, py - 1));
                                    else if (board[px, py - 1].team != team &&
                                             board[px, py - 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px, py - 1));
                                }
                            }

        //Bottom Left
        x = currentX - 1;
        y = currentY - 2;
        if (x >= 0 && y >= 0)
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].type == ChessPieceType.Portal)
                for (var px = 0; px < tileCountX; px++)
                for (var py = 0; py < tileCountY; py++)
                    if (px != x || py != y)
                        if (board[px, py] != null)
                            if (board[px, py].type == ChessPieceType.Portal)
                            {
                                //down then left
                                if (px - 1 >= 0)
                                {
                                    if (board[px - 1, py] == null)
                                        r.Add(new Vector2Int(px - 1, py));
                                    else if (board[px - 1, py].team != team &&
                                             board[px - 1, py].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px - 1, py));
                                }

                                //left then up
                                if (py - 1 >= 0)
                                {
                                    if (board[px, py - 1] == null)
                                        r.Add(new Vector2Int(px, py - 1));
                                    else if (board[px, py - 1].team != team &&
                                             board[px, py - 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px, py - 1));
                                }
                            }

        x = currentX - 2;
        y = currentY - 1;
        if (x >= 0 && y >= 0)
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                r.Add(new Vector2Int(x, y));
            else if (board[x, y].type == ChessPieceType.Portal)
                for (var px = 0; px < tileCountX; px++)
                for (var py = 0; py < tileCountY; py++)
                    if (px != x || py != y)
                        if (board[px, py] != null)
                            if (board[px, py].type == ChessPieceType.Portal)
                            {
                                //down then left
                                if (px - 1 >= 0)
                                {
                                    if (board[px - 1, py] == null)
                                        r.Add(new Vector2Int(px - 1, py));
                                    else if (board[px - 1, py].team != team &&
                                             board[px - 1, py].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px - 1, py));
                                }

                                //left then up
                                if (py - 1 >= 0)
                                {
                                    if (board[px, py - 1] == null)
                                        r.Add(new Vector2Int(px, py - 1));
                                    else if (board[px, py - 1].team != team &&
                                             board[px, py - 1].type != ChessPieceType.Portal)
                                        r.Add(new Vector2Int(px, py - 1));
                                }
                            }

        return r;
    }
}