using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // Top Right
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(x, y));
                else if (board[x, y].type == ChessPieceType.Portal)
                {
                    for (int px = 0; px < tileCountX; px++)
                    {
                        for (int py = 0; py < tileCountY; py++)
                        {
                            if (px != x || py != y)
                            {
                                if (board[px, py] != null && board[px, py].type == ChessPieceType.Portal)
                                {
                                    for (int nx = px + 1, ny = py + 1; nx < tileCountX && ny < tileCountY; nx++, ny++)
                                    {
                                        if (board[nx, ny] == null)
                                            r.Add(new Vector2Int(nx, ny));
                                        else
                                        {
                                            if (board[nx, ny].type != ChessPieceType.Portal && board[nx, ny].team != team)
                                            {
                                                r.Add(new Vector2Int(nx, ny));
                                                goto LoopBreak;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            LoopBreak:
                break;
            }
        }

        // Top Left
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(x, y));
                else if (board[x, y].type == ChessPieceType.Portal)
                {
                    for (int px = 0; px < tileCountX; px++)
                    {
                        for (int py = 0; py < tileCountY; py++)
                        {
                            if (px != x || py != y)
                            {
                                if (board[px, py] != null && board[px, py].type == ChessPieceType.Portal)
                                {
                                    for (int nx = px - 1, ny = py + 1; nx >= 0 && ny < tileCountY; nx--, ny++)
                                    {
                                        if (board[nx, ny] == null)
                                            r.Add(new Vector2Int(nx, ny));
                                        else
                                        {
                                            if (board[nx, ny].type != ChessPieceType.Portal && board[nx, ny].team != team)
                                            {
                                                r.Add(new Vector2Int(nx, ny));
                                                goto LoopBreak;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            LoopBreak:
                break;
            }
        }

        // Bottom Right
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(x, y));
                else if (board[x, y].type == ChessPieceType.Portal)
                {
                    for (int px = 0; px < tileCountX; px++)
                    {
                        for (int py = 0; py < tileCountY; py++)
                        {
                            if (px != x || py != y)
                            {
                                if (board[px, py] != null && board[px, py].type == ChessPieceType.Portal)
                                {
                                    for (int nx = px + 1, ny = py - 1; nx < tileCountX && ny >= 0; nx++, ny--)
                                    {
                                        if (board[nx, ny] == null)
                                            r.Add(new Vector2Int(nx, ny));
                                        else
                                        {
                                            if (board[nx, ny].type != ChessPieceType.Portal && board[nx, ny].team != team)
                                            {
                                                r.Add(new Vector2Int(nx, ny));
                                                goto LoopBreak;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            LoopBreak:
                break;
            }
        }
        // Bottom Left
        for (int x = currentX - 1, y = currentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team && board[x, y].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(x, y));

                else if (board[x, y].type == ChessPieceType.Portal)
                {
                    for (int px = 0; px < tileCountX; px++)
                    {
                        for (int py = 0; py < tileCountY; py++)
                        {
                            if (px != x || py != y)
                            {
                                if (board[px, py] != null && board[px, py].type == ChessPieceType.Portal)
                                {
                                    for (int nx = px - 1, ny = py - 1; nx >= 0 && ny >= 0; nx--, ny--)
                                    {
                                        if (board[nx, ny] == null)
                                            r.Add(new Vector2Int(nx, ny));
                                        else
                                        {
                                            if (board[nx, ny].type != ChessPieceType.Portal && board[nx, ny].team != team)
                                            {
                                                r.Add(new Vector2Int(nx, ny));
                                                goto LoopBreak;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            LoopBreak:
                break;

            }
        }
        return r;
    }
}
