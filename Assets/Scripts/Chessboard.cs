using System.Collections.Generic;
using System.IO;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

public enum SpecialMove
{
    none = 0,
    EnPassant,
    Castling,
    Promotion
}

public class Chessboard : MonoBehaviour
{
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    [Header("Art Stuff")] [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.3f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float deathHeight = 0.4f;
    [SerializeField] private float dragOffset = 1.5f;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private Transform rematchIndicator;
    [SerializeField] private Button rematchButton;

    [Header("Prefabs && Materials")] [SerializeField]
    private GameObject[] whitePrefabs;

    [SerializeField] private GameObject[] blackPrefabs;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private Vector3 bounds;

    // LOGIC
    private ChessPiece[,] chessPieces;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private ChessPiece currentlyDragging;
    private int currentTeam = -1;
    private readonly List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private readonly List<ChessPiece> deadWhites = new List<ChessPiece>();
    private readonly string fileName = Directory.GetCurrentDirectory() + "\\log.txt";
    private bool isWhiteTurn;
    private bool localGame = true;
    private int logNumber = 0;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();

    //  multiplayer logic
    private int playerCount = -1;
    private readonly bool[] playerRematch = new bool[2];
    private SpecialMove specialMove;
    private GameObject[,] tiles;

    private void Start()
    {
        isWhiteTurn = true;

        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        CreateGameLog();
        PositionAllPieces();
        RegisterEvents();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        var ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            // Get the indexes of the tile i've hit
            var hitPosition = LookupTileIndex(info.transform.gameObject);

            // If we're hovering a tile after not hovering any tiles
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we were already hovering a tile, change the previous one
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = ContainsValidMove(ref availableMoves, currentHover)
                    ? LayerMask.NameToLayer("Highlight")
                    : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //if click
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("click click");
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    Debug.Log("Clicked a piece");
                    Debug.Log($"Current Team = {currentTeam} : isWhiteTurn {isWhiteTurn}");
                    Debug.Log("touched your team on your turn");
                    // is it our turn?
                    if (chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn && currentTeam == 0 ||
                        chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn && currentTeam == 1)
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];

                        //get a list of where to go and highlight tiles
                        availableMoves =
                            currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                        Debug.Log(availableMoves.Count);
                        //Get a list of special moves
                        specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList,
                            ref availableMoves, TILE_COUNT_X, TILE_COUNT_Y);
                        Debug.Log(specialMove);
                        PreventCheck();

                        HighlightTiles();
                    }
                }
            }

            //if release click
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                var previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                if (ContainsValidMove(ref availableMoves, new Vector2Int(hitPosition.x, hitPosition.y)))
                {
                    MoveTo(previousPosition.x, previousPosition.y, hitPosition.x, hitPosition.y);
                    var moved = chessPieces[hitPosition.x, hitPosition.y];

                    CheckPromotion(moved);
                    // Net implementation
                    var mm = new NetMakeMove();
                    mm.originalX = previousPosition.x;
                    mm.originalY = previousPosition.y;
                    mm.destinationX = hitPosition.x;
                    mm.destinationY = hitPosition.y;
                    mm.teamId = currentTeam;
                    Client.Instance.SendToServer(mm);
                }
                else
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    currentlyDragging = null;
                    RemoveHighlightTiles();
                }
            }
        }

        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = ContainsValidMove(ref availableMoves, currentHover)
                    ? LayerMask.NameToLayer("Highlight")
                    : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }

        //if dragging piece
        if (currentlyDragging)
        {
            var horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            var distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
    }

    //Generate Board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3(tileCountX / 2 * tileSize, 0, tileCountX / 2 * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (var x = 0; x < tileCountX; x++)
        for (var y = 0; y < tileCountY; y++)
            tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        var tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        var mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        var vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = {0, 1, 2, 1, 3, 2};

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    //Spawn Pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        byte whiteTeam = 0, blackTeam = 1;

        //White team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[4, 3] = SpawnSinglePiece(ChessPieceType.Portal, whiteTeam);
        for (var i = 0; i < TILE_COUNT_X; i++) chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);

        ////Black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[3, 4] = SpawnSinglePiece(ChessPieceType.Portal, blackTeam);

        for (var i = 0; i < TILE_COUNT_X; i++) chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, byte team)
    {
        if (team == 0)
        {
            var cp = Instantiate(whitePrefabs[(int) type - 1], transform).GetComponent<ChessPiece>();

            cp.type = type;
            cp.team = team;
            return cp;
        }

        if (team == 1)
        {
            var cp = Instantiate(blackPrefabs[(int) type - 1], transform).GetComponent<ChessPiece>();

            cp.type = type;
            cp.team = team;
            return cp;
        }

        return null;
    }

    //Positioning
    private void PositionAllPieces()
    {
        for (var x = 0; x < TILE_COUNT_X; x++)
        for (var y = 0; y < TILE_COUNT_Y; y++)
            if (chessPieces[x, y] != null)
                PositionSinglePiece(x, y, true);
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    // Highlighting Tiles
    private void HighlightTiles()
    {
        for (var i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }

    private void RemoveHighlightTiles()
    {
        for (var i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        availableMoves.Clear();
    }

    //Checkmate
    private void Checkmate(byte team)
    {
        DisplayVictory(team);
    }

    private void DisplayVictory(byte winningTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }

    public void OnRematchButton()
    {
        if (localGame)
        {
            var wrm = new NetRematch();
            wrm.teamId = 0;
            wrm.wantRematch = 1;
            Client.Instance.SendToServer(wrm);

            var brm = new NetRematch();
            brm.teamId = 1;
            brm.wantRematch = 1;
            Client.Instance.SendToServer(brm);
        }
        else
        {
            var rm = new NetRematch();
            rm.teamId = currentTeam;
            rm.wantRematch = 1;
            Client.Instance.SendToServer(rm);
        }
    }

    public void GameReset()
    {
        // UI

        rematchButton.interactable = true;

        rematchIndicator.transform.GetChild(0).gameObject.SetActive(false);
        rematchIndicator.transform.GetChild(1).gameObject.SetActive(false);

        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        //Fields Reset
        currentlyDragging = null;
        availableMoves.Clear();
        moveList.Clear();
        playerRematch[0] = playerRematch[1] = false;
        if (localGame)
            currentTeam = 0;
        //clean up

        for (var x = 0; x < TILE_COUNT_X; x++)
        for (var y = 0; y < TILE_COUNT_Y; y++)
        {
            if (chessPieces[x, y] != null)
                Destroy(chessPieces[x, y].gameObject);

            chessPieces[x, y] = null;
        }

        for (var i = 0; i < deadWhites.Count; i++)
            Destroy(deadWhites[i].gameObject);
        for (var i = 0; i < deadBlacks.Count; i++)
            Destroy(deadBlacks[i].gameObject);

        deadWhites.Clear();
        deadBlacks.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
        CreateGameLog();
    }

    public void OnMenuButton()
    {
        var rm = new NetRematch();
        rm.teamId = currentTeam;
        rm.wantRematch = 0;
        Client.Instance.SendToServer(rm);

        GameReset();
        GameUI.Instance.OnLeaveFromGameMenu();

        Invoke("ShutdownRelay", 1.0f);

        //Reset some values
        playerCount = -1;
        currentTeam = -1;
    }
    //SpecialMoves

    private void ProcessSpecialMove()
    {
        if (specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            var myPawn = chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPostion = moveList[moveList.Count - 2];
            var enemyPawn = chessPieces[targetPawnPostion[1].x, targetPawnPostion[1].y];

            if (myPawn.currentX == enemyPawn.currentX)
                if (myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1)
                {
                    if (enemyPawn.team == 0)
                    {
                        deadWhites.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffset + deathHeight, -1 * tileSize)
                                              - bounds
                                              + new Vector3(tileSize / 2, 0, tileSize / 2)
                                              + Vector3.forward * deathSpacing * deadWhites.Count);
                    }
                    else
                    {
                        deadBlacks.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(new Vector3(-1 * tileSize, yOffset + deathHeight, 8 * tileSize)
                                              - bounds
                                              + new Vector3(tileSize / 2, 0, tileSize / 2)
                                              + Vector3.back * deathSpacing * deadBlacks.Count);
                    }

                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
        }

        if (specialMove == SpecialMove.Promotion)
        {
            var lastMove = moveList[moveList.Count - 1];
            var targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if (targetPawn.type == ChessPieceType.Pawn)
            {
                if (targetPawn.team == 0 && lastMove[1].y == 7)
                {
                    var newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }

                if (targetPawn.team == 1 && lastMove[1].y == 0)
                {
                    var newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }

        if (specialMove == SpecialMove.Castling)
        {
            var lastMove = moveList[moveList.Count - 1];

            //left rook
            if (lastMove[1].x == 2)
            {
                if (lastMove[1].y == 0) //white side
                {
                    var rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7) //black side
                {
                    var rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }
            //right rook
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0) //white side
                {
                    var rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7) //black side
                {
                    var rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }

    private void CheckPromotion(ChessPiece piece)
    {
        if (piece.type == ChessPieceType.Pawn)
            if (piece.team == 0 && piece.currentY == 7 || piece.team == 1 && piece.currentY == 0)
            {
                var newQueen = SpawnSinglePiece(ChessPieceType.Queen, piece.team);
                newQueen.transform.position = chessPieces[piece.currentX, piece.currentY].transform.position;
                Destroy(chessPieces[piece.currentX, piece.currentY].gameObject);
                chessPieces[piece.currentX, piece.currentY] = newQueen;
                PositionSinglePiece(piece.currentX, piece.currentY);
            }
    }

    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (var x = 0; x < TILE_COUNT_X; x++)
        for (var y = 0; y < TILE_COUNT_Y; y++)
            if (chessPieces[x, y] != null)
                if (chessPieces[x, y].type == ChessPieceType.King)
                    if (chessPieces[x, y].team == currentlyDragging.team)
                        targetKing = chessPieces[x, y];

        //delete moves that put us in check
        SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves, targetKing);
    }

    private void SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        // save current values, to reset after function
        var actualX = cp.currentX;
        var actualY = cp.currentY;

        var movesToRemove = new List<Vector2Int>();

        //going through all the moves, simulate them
        for (var i = 0; i < moves.Count; i++)
        {
            var simX = moves[i].x;
            var simY = moves[i].y;

            var kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
            // Did we simulate the kings move
            if (cp.type == ChessPieceType.King)
                kingPositionThisSim = new Vector2Int(simX, simY);

            //Copy the [,] and not a reference
            var simulation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
            var simAttackingPieces = new List<ChessPiece>();

            for (var x = 0; x < TILE_COUNT_X; x++)
            for (var y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                {
                    simulation[x, y] = chessPieces[x, y];
                    if (simulation[x, y].team != cp.team)
                        simAttackingPieces.Add(simulation[x, y]);
                }

            //Simulate that move
            simulation[actualX, actualY] = null;
            cp.currentX = simX;
            cp.currentY = simY;
            simulation[simX, simY] = cp;

            //Did on of the piece got taken down during our simulation

            var deadPiece = simAttackingPieces.Find(c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null)
                simAttackingPieces.Remove(deadPiece);

            // Get all the simulated attacking pieces move
            var simMoves = new List<Vector2Int>();
            for (var a = 0; a < simAttackingPieces.Count; a++)
            {
                var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);
                for (var b = 0; b < pieceMoves.Count; b++)
                    simMoves.Add(pieceMoves[b]);
            }

            // is the king in trouble? if so, remove move
            if (ContainsValidMove(ref simMoves, kingPositionThisSim)) movesToRemove.Add(moves[i]);

            // Restore the actual CP data
            cp.currentX = actualX;
            cp.currentY = actualY;
        }


        // remove from the current available move list
        for (var i = 0; i < movesToRemove.Count; i++) moves.Remove(movesToRemove[i]);
    }

    private bool CheckForCheckmate()
    {
        var lastMove = moveList[moveList.Count - 1];
        var targetTeam = chessPieces[lastMove[1].x, lastMove[1].y].team == 0 ? 1 : 0;

        var attackingPiece = new List<ChessPiece>();
        var defendingPiece = new List<ChessPiece>();
        ChessPiece targetKing = null;
        for (var x = 0; x < TILE_COUNT_X; x++)
        for (var y = 0; y < TILE_COUNT_Y; y++)
            if (chessPieces[x, y] != null)
            {
                if (chessPieces[x, y].team == targetTeam)
                {
                    defendingPiece.Add(chessPieces[x, y]);
                    if (chessPieces[x, y].type == ChessPieceType.King)
                        targetKing = chessPieces[x, y];
                }
                else
                {
                    attackingPiece.Add(chessPieces[x, y]);
                }
            }

        // is the king attacked right now
        var currentAvailableMoves = new List<Vector2Int>();
        for (var i = 0; i < attackingPiece.Count; i++)
        {
            var pieceMoves = attackingPiece[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            for (var b = 0; b < pieceMoves.Count; b++)
                currentAvailableMoves.Add(pieceMoves[b]);
        }

        //Are we in check right now?
        if (ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
            // King is under attack, can we move something to help him?
            for (var i = 0; i < defendingPiece.Count; i++)
            {
                var defendingMoves =
                    defendingPiece[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                SimulateMoveForSinglePiece(defendingPiece[i], ref defendingMoves, targetKing);

                if (defendingMoves.Count != 0)
                    return false;
            }

            return true; //Checkmate
        }

        var defMoves = false;

        for (var i = 0; i < defendingPiece.Count; i++)
        {
            var defendingMoves = defendingPiece[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            SimulateMoveForSinglePiece(defendingPiece[i], ref defendingMoves, targetKing);
            if (defendingMoves.Count != 0)
            {
                defMoves = true;
                break;
            }
        }

        if (!defMoves)
            return true;

        return false;
    }

    //Operations
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (var x = 0; x < TILE_COUNT_X; x++)
        for (var y = 0; y < TILE_COUNT_Y; y++)
            if (tiles[x, y] == hitInfo)
                return new Vector2Int(x, y);

        return -Vector2Int.one; //-1 -1 INVALID
    }

    private void MoveTo(int originalX, int originalY, int x, int y)
    {
        var cp = chessPieces[originalX, originalY];
        var previousPosition = new Vector2Int(originalX, originalY);

        // is there another piece there
        if (chessPieces[x, y] != null)
        {
            var ocp = chessPieces[x, y];

            if (cp.team == ocp.team)
                return;

            //if its enemy
            if (ocp.team == 0)
            {
                if (ocp.type == ChessPieceType.King)
                    Checkmate(1);

                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3(8 * tileSize, yOffset + deathHeight, -1 * tileSize)
                                - bounds
                                + new Vector3(tileSize / 2, 0, tileSize / 2)
                                + Vector3.forward * deathSpacing * deadWhites.Count);
            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                    Checkmate(0);

                deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3(-1 * tileSize, yOffset + deathHeight, 8 * tileSize)
                                - bounds
                                + new Vector3(tileSize / 2, 0, tileSize / 2)
                                + Vector3.back * deathSpacing * deadBlacks.Count);
            }
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        isWhiteTurn = !isWhiteTurn;
        if (localGame)
            currentTeam = currentTeam == 0 ? 1 : 0;
        moveList.Add(new[] {previousPosition, new Vector2Int(x, y)});

        ProcessSpecialMove();

        if (currentlyDragging)
            currentlyDragging = null;
        RemoveHighlightTiles();

        if (CheckForCheckmate())
            Checkmate(cp.team);
    }

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (var i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;
        return false;
    }


    private void CreateGameLog()
    {
        // var foundName = false;
        // while (!foundName)
        // {
        //     if(logNumber > 100)
        //     {
        //         foundName = true;
        //     }
        //     if (File.Exists(fileName))
        //     {
        //         if (logNumber == 0)
        //             fileName = fileName.Replace("log", $"log({logNumber + 1})");
        //         else
        //             fileName = fileName.Replace(logNumber.ToString(), (logNumber + 1).ToString());
        //         logNumber++;
        //     }
        //     else
        //     {
        //         File.Create(fileName);
        //         foundName = true;
        //     }
        // }
    }

    private void AddToLog(string logMessage)
    {
        using (var w = File.AppendText(fileName))
        {
            w.WriteLine(logMessage);
        }
    }

    #region

    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_REMATCH += OnRematchServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_REMATCH += OnRematchClient;


        GameUI.Instance.SetLocalGame += OnSetLocalGame;
    }

    private void UnRegisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_REMATCH -= OnRematchServer;

        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_REMATCH -= OnRematchClient;

        GameUI.Instance.SetLocalGame -= OnSetLocalGame;
    }

    //server
    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        //Cleint has connected, assign a team and return the message back to him
        var nw = msg as NetWelcome;

        //Assign a team
        nw.AssignedTeam = ++playerCount;

        //Return back to the client
        Server.Instance.SendToClient(cnn, nw);

        //if full, start game
        if (playerCount == 1) Server.Instance.Broadcast(new NetStartGame());
    }

    private void OnMakeMoveServer(NetMessage msg, NetworkConnection cnn)
    {
        var mm = msg as NetMakeMove;
        // Receive, and just broadcast it back
        Server.Instance.Broadcast(mm);
    }

    private void OnRematchServer(NetMessage msg, NetworkConnection cnn)
    {
        Server.Instance.Broadcast(msg);
    }

    //client
    private void OnWelcomeClient(NetMessage msg)
    {
        // Recieve the connection message
        var nw = msg as NetWelcome;

        //Assign the team
        currentTeam = nw.AssignedTeam;

        Debug.Log($"My asssigned team is {nw.AssignedTeam}");

        if (localGame && currentTeam == 0)
            Server.Instance.Broadcast(new NetStartGame());
    }

    private void OnStartGameClient(NetMessage msg)
    {
        GameUI.Instance.ChangeCamera(currentTeam == 0 ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
    }

    private void OnMakeMoveClient(NetMessage msg)
    {
        var mm = msg as NetMakeMove;

        Debug.Log($"MM : {mm.teamId} :  {mm.originalX} {mm.originalY} -> {mm.destinationX} {mm.destinationY}");

        if (mm.teamId != currentTeam)
        {
            AddToLog(
                $"MM : {mm.teamId} : {chessPieces[mm.originalX, mm.originalY].type.ToString()[0]} {mm.originalX} {mm.originalY} -> {mm.destinationX} {mm.destinationY}");
            var target = chessPieces[mm.originalX, mm.originalY];

            availableMoves = target.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            specialMove = target.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves, TILE_COUNT_X,
                TILE_COUNT_Y);

            MoveTo(mm.originalX, mm.originalY, mm.destinationX, mm.destinationY);
            CheckPromotion(target);
        }
        else
        {
            AddToLog(
                $"MM : {mm.teamId} : {chessPieces[mm.destinationX, mm.destinationY].type.ToString()[0]} {mm.originalX} {mm.originalY} -> {mm.destinationX} {mm.destinationY}");
        }
    }

    private void OnRematchClient(NetMessage msg)
    {
        //Recieve the connection message
        var rm = msg as NetRematch;

        //set bool for rematch
        playerRematch[rm.teamId] = rm.wantRematch == 1;

        //activate the piece of ui
        if (rm.teamId != currentTeam)
        {
            rematchIndicator.transform.GetChild(rm.wantRematch == 1 ? 0 : 1).gameObject.SetActive(true);
            if (rm.wantRematch != 1) rematchButton.interactable = false;
        }

        //if both want rematch
        if (playerRematch[0] && playerRematch[1])
            GameReset();
    }

    //
    private void ShutdownInRelay()
    {
        Client.Instance.Shutdown();
        Server.Instance.Shutdown();
    }

    private void OnSetLocalGame(bool v)
    {
        playerCount = -1;
        currentTeam = -1;
        localGame = v;
    }

    #endregion
}