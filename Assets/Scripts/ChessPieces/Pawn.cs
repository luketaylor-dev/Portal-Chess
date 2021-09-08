using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;
        if (currentY != 0 && currentY != tileCountY - 1)
        {
            // One in front
            if (board[currentX, currentY + direction] == null)
                r.Add(new Vector2Int(currentX, currentY + direction));

            //Portal One in front
            if (board[currentX, currentY + direction] != null)
            {
                if (board[currentX, currentY + direction].type == ChessPieceType.Portal)
                {
                    for (int x = 0; x < tileCountX; x++)
                    {
                        for (int y = 0; y < tileCountY; y++)
                        {
                            if (x != currentX || y != currentY + direction)
                            {
                                if (board[x, y]?.type == ChessPieceType.Portal)
                                {
                                    if (y != 0 && y != tileCountY - 1)
                                    {
                                        if (board[x, y + direction] == null)
                                        {
                                            r.Add(new Vector2Int(x, y + direction));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Two in front
            if (board[currentX, currentY + direction] == null)
            {
                if (team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] == null)
                {
                    r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
                }
                if (team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
                {
                    r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
                }

                //Portal
                if (team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] != null)
                {
                    if (board[currentX, currentY + (direction * 2)].type == ChessPieceType.Portal)
                    {
                        for (int x = 0; x < tileCountX; x++)
                        {
                            for (int y = 0; y < tileCountY; y++)
                            {
                                if (x != currentX || y != currentY + (direction * 2))
                                {
                                    if (board[x, y]?.type == ChessPieceType.Portal)
                                    {
                                        if (y != 0 && y != tileCountY - 1)
                                        {
                                            if (board[x, y + direction] == null)
                                            {
                                                r.Add(new Vector2Int(x, y + direction));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] != null)
                {
                    if (board[currentX, currentY + (direction * 2)].type == ChessPieceType.Portal)
                    {
                        for (int x = 0; x < tileCountX; x++)
                        {
                            for (int y = 0; y < tileCountY; y++)
                            {
                                if (x != currentX || y != currentY + (direction * 2))
                                {
                                    if (board[x, y]?.type == ChessPieceType.Portal)
                                    {
                                        if (y != 0 && y != tileCountY - 1)
                                        {
                                            if (board[x, y + direction] == null)
                                            {
                                                r.Add(new Vector2Int(x, y + direction));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Kill move
            if (currentX != tileCountX - 1)
            {
                if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team && board[currentX + 1, currentY + direction].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(currentX + 1, currentY + direction));
                //Portal
                if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].type == ChessPieceType.Portal)
                {
                    for (int x = 0; x < tileCountX; x++)
                    {
                        for (int y = 0; y < tileCountY; y++)
                        {
                            if (x != currentX + 1 || y != currentY + direction)
                            {
                                if (board[x, y]?.type == ChessPieceType.Portal)
                                {
                                    if (y != 0 && y != tileCountY - 1)
                                    {
                                        if (x != tileCountX - 1)
                                        {
                                            if (board[x + 1, y + direction] != null && board[x + 1, y + direction].team != team && board[x + 1, y + direction].type != ChessPieceType.Portal)
                                            {
                                                r.Add(new Vector2Int(x + 1, y + direction));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (currentX != 0)
            {
                if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team && board[currentX - 1, currentY + direction].type != ChessPieceType.Portal)
                    r.Add(new Vector2Int(currentX - 1, currentY + direction));
                //Portal
                if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].type == ChessPieceType.Portal)
                {
                    for (int x = 0; x < tileCountX; x++)
                    {
                        for (int y = 0; y < tileCountY; y++)
                        {
                            if (x != currentX - 1 || y != currentY + direction)
                            {
                                if (board[x, y]?.type == ChessPieceType.Portal)
                                {
                                    if (y != 0 && y != tileCountY - 1)
                                    {
                                        if (x != 0)
                                        {
                                            if (board[x - 1, y + direction] != null && board[x - 1, y + direction].team != team && board[x - 1, y + direction].type != ChessPieceType.Portal)
                                            {
                                                r.Add(new Vector2Int(x - 1, y + direction));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        return r;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves, int tileCountX, int tileCountY)
    {
        int direction = (team == 0) ? 1 : -1;
        //if ((team == 0 && currentY == 6) || (team == 1 && currentY == 1))
        //    return SpecialMove.Promotion;
        //foreach(var move in availableMoves)
        //{
        //    if ((team == 0 && move.y == 7) || (team == 1 && move.y == 0))
        //        return SpecialMove.Promotion;
        //}
        // En Passant
        if (moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if (board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn) // if last move was a pawn
            {
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2) // if last move was 2 tiles
                {
                    if (board[lastMove[1].x, lastMove[1].y].team != team)
                    {
                        if (lastMove[1].y == currentY) //if both pawns are on the same y
                        {
                            if (lastMove[1].x == currentX - 1) //landed left
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                            if (lastMove[1].x == currentX + 1) //landed right
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }
                        if (currentY != 0 && currentY != tileCountY - 1)
                        {
                            if (currentX != tileCountX - 1)
                            {
                                if (board[currentX + 1, currentY + direction] != null)
                                {
                                    if (board[currentX + 1, currentY + direction]?.type == ChessPieceType.Portal) // up right
                                    {
                                        for (int x = 0; x < tileCountX; x++)
                                        {
                                            for (int y = 0; y < tileCountY; y++)
                                            {
                                                if (x != currentX + 1 || y != currentY + direction)
                                                {
                                                    if (board[x, y]?.type == ChessPieceType.Portal)
                                                    {
                                                        if (y != 0 && y != tileCountY - 1)
                                                        {
                                                            if (x != tileCountX - 1)
                                                            {
                                                                if (y == lastMove[1].y) //if portal and pawn are on the same y
                                                                {
                                                                    availableMoves.Add(new Vector2Int(x + 1,  y + direction));
                                                                    return SpecialMove.EnPassant;
                                                                }   
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (currentX != 0)
                            {
                                if (board[currentX - 1, currentY + direction])
                                {
                                    if (board[currentX - 1, currentY + direction]?.type == ChessPieceType.Portal) // up left
                                    {
                                        for (int x = 0; x < tileCountX; x++)
                                        {
                                            for (int y = 0; y < tileCountY; y++)
                                            {
                                                if (x != currentX - 1 || y != currentY + direction)
                                                {
                                                    if (board[x, y]?.type == ChessPieceType.Portal)
                                                    {
                                                        if (y != 0 && y != tileCountY - 1)
                                                        {
                                                            if (x != 0)
                                                            {
                                                                if (y == lastMove[1].y) //if portal and pawn are on the same y
                                                                {
                                                                    availableMoves.Add(new Vector2Int(currentY - 1, y + direction));
                                                                    return SpecialMove.EnPassant;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return SpecialMove.none;
    }
}
