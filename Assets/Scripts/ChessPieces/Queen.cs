using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // Down
        for (int i = currentY - 1; i >= 0; i--)
        {
            if (board[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i));
            if (board[currentX, i] != null)
            {
                if (board[currentX, i].team != team && board[currentX, i].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(currentX, i));
                else if (board[currentX, i].type == ChessPieceType.Portal)
                {
                    for (int x = 0; x < tileCountX; x++)
                    {
                        for (int y = 0; y < tileCountY; y++)
                        {
                            if (x != currentX || y != i)
                            {
                                if (board[x, y] != null)
                                {
                                    if (board[x, y].type == ChessPieceType.Portal)
                                    {
                                        for (int newY = board[x, y].currentY; newY >= 0; newY--)
                                        {
                                            if (board[x, newY] == null)
                                            {
                                                r.Add(new Vector2Int(x, newY));
                                            }
                                            else if (board[x, newY].type != ChessPieceType.Portal) //make sure we dont go through the portal repeatdly like infinite fall in portal
                                            {
                                                if (board[x, newY].team != team)
                                                {
                                                    r.Add(new Vector2Int(x, newY));
                                                    goto LoopBreak; //if we hit an enemy we can stop, but we need to leave a few loops so just forcing the exit
                                                }
                                                else
                                                {
                                                    goto LoopBreak;
                                                }
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

        // Up
        for (int i = currentY + 1; i < tileCountY; i++)
        {
            if (board[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i));
            if (board[currentX, i] != null)
            {
                if (board[currentX, i].team != team && board[currentX, i].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(currentX, i));
                else if (board[currentX, i].type == ChessPieceType.Portal)
                {
                    for (int x = 0; x < tileCountX; x++)
                    {
                        for (int y = 0; y < tileCountY; y++)
                        {
                            if (x != currentX || y != i)
                            {
                                if (board[x, y] != null)
                                {
                                    if (board[x, y].type == ChessPieceType.Portal)
                                    {
                                        for (int newY = board[x, y].currentY; newY < tileCountY; newY++)
                                        {
                                            if (board[x, newY] == null)
                                            {
                                                r.Add(new Vector2Int(x, newY));
                                            }
                                            else if (board[x, newY].type != ChessPieceType.Portal) //make sure we dont go through the portal repeatdly like infinite fall in portal
                                            {
                                                if (board[x, newY].team != team)
                                                {
                                                    r.Add(new Vector2Int(x, newY));
                                                    goto LoopBreak; //if we hit an enemy we can stop, but we need to leave a few loops so just forcing the exit
                                                }
                                                else
                                                {
                                                    goto LoopBreak;
                                                }
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

        // Left
        for (int i = currentX - 1; i >= 0; i--)
        {
            if (board[i, currentY] == null)
                r.Add(new Vector2Int(i, currentY));
            if (board[i, currentY] != null)
            {
                if (board[i, currentY].team != team && board[i, currentY].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(i, currentY));
                else if (board[i, currentY].type == ChessPieceType.Portal)
                {
                    for (int x = 0; x < tileCountX; x++)
                    {
                        for (int y = 0; y < tileCountY; y++)
                        {
                            if (x != i || y != currentY)
                            {
                                if (board[x, y] != null)
                                {
                                    if (board[x, y].type == ChessPieceType.Portal)
                                    {
                                        for (int newX = board[x, y].currentX; newX >= 0; newX--)
                                        {
                                            if (board[newX, y] == null)
                                            {
                                                r.Add(new Vector2Int(newX, y));
                                            }
                                            else if (board[newX, y].type != ChessPieceType.Portal) //make sure we dont go through the portal repeatdly like infinite fall in portal
                                            {
                                                if (board[newX, y].team != team)
                                                {
                                                    r.Add(new Vector2Int(newX, y));
                                                    goto LoopBreak; //if we hit an enemy we can stop, but we need to leave a few loops so just forcing the exit
                                                }
                                                else
                                                {
                                                    goto LoopBreak;
                                                }
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

        // Right
        for (int i = currentX + 1; i < tileCountX; i++)
        {
            if (board[i, currentY] == null)
                r.Add(new Vector2Int(i, currentY));
            if (board[i, currentY] != null)
            {
                if (board[i, currentY].team != team && board[i, currentY].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(i, currentY));
                else if (board[i, currentY].type == ChessPieceType.Portal)
                {
                    for (int x = 0; x < tileCountX; x++)
                    {
                        for (int y = 0; y < tileCountY; y++)
                        {
                            if (x != i || y != currentY)
                            {
                                if (board[x, y] != null)
                                {
                                    if (board[x, y].type == ChessPieceType.Portal)
                                    {
                                        for (int newX = board[x, y].currentX; newX < tileCountX; newX++)
                                        {
                                            if (board[newX, y] == null)
                                            {
                                                r.Add(new Vector2Int(newX, y));
                                            }
                                            else if (board[newX, y].type != ChessPieceType.Portal) //make sure we dont go through the portal repeatdly like infinite fall in portal
                                            {
                                                if (board[newX, y].team != team)
                                                {
                                                    r.Add(new Vector2Int(newX, y));
                                                    goto LoopBreak; //if we hit an enemy we can stop, but we need to leave a few loops so just forcing the exit
                                                }
                                                else
                                                {
                                                    goto LoopBreak;
                                                }
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
                                            else
                                            {
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
                                            else
                                            {
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
                                            else
                                            {
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
                                            else
                                            {
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
