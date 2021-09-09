using System.Collections.Generic;
using System.IO;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

public enum SpecialMove
{
    None = 0,
    EnPassant,
    Castling,
    Promotion
}

public class Chessboard : MonoBehaviour
{
    private const int TileCountX = 8;
    private const int TileCountY = 8;
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
    private readonly List<ChessPiece> _deadBlacks = new List<ChessPiece>();
    private readonly List<ChessPiece> _deadWhites = new List<ChessPiece>();
    private readonly string _fileName = Directory.GetCurrentDirectory() + "\\log.txt";
    private readonly bool[] _playerRematch = new bool[2];
    private List<Vector2Int> _availableMoves = new List<Vector2Int>();
    private Vector3 _bounds;

    // LOGIC
    private ChessPiece[,] _chessPieces;
    private Camera _currentCamera;
    private Vector2Int _currentHover;
    private ChessPiece _currentlyDragging;
    private int _currentTeam = -1;
    private bool _isWhiteTurn;
    private bool _localGame = true;
    private List<Vector2Int[]> _moveList = new List<Vector2Int[]>();

    //  multiplayer logic
    private int _playerCount = -1;
    private SpecialMove _specialMove;
    private GameObject[,] _tiles;

    private void Start()
    {
        _isWhiteTurn = true;

        GenerateAllTiles(tileSize, TileCountX, TileCountY);
        SpawnAllPieces();
        CreateGameLog();
        AddNewGameToLog();
        PositionAllPieces();
        RegisterEvents();
    }

    private void Update()
    {
        if (!_currentCamera)
        {
            _currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        var ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            // Get the indexes of the tile i've hit
            var hitPosition = LookupTileIndex(info.transform.gameObject);

            // If we're hovering a tile after not hovering any tiles
            if (_currentHover == -Vector2Int.one)
            {
                _currentHover = hitPosition;
                _tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we were already hovering a tile, change the previous one
            if (_currentHover != hitPosition)
            {
                _tiles[_currentHover.x, _currentHover.y].layer = ContainsValidMove(ref _availableMoves, _currentHover)
                    ? LayerMask.NameToLayer("Highlight")
                    : LayerMask.NameToLayer("Tile");
                _currentHover = hitPosition;
                _tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //if click
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("click click");
                if (_chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    Debug.Log("Clicked a piece");
                    Debug.Log($"Current Team = {_currentTeam} : isWhiteTurn {_isWhiteTurn}");
                    Debug.Log("touched your team on your turn");
                    // is it our turn?
                    if (_chessPieces[hitPosition.x, hitPosition.y].team == 0 && _isWhiteTurn && _currentTeam == 0 ||
                        _chessPieces[hitPosition.x, hitPosition.y].team == 1 && !_isWhiteTurn && _currentTeam == 1)
                    {
                        _currentlyDragging = _chessPieces[hitPosition.x, hitPosition.y];

                        //get a list of where to go and highlight tiles
                        _availableMoves =
                            _currentlyDragging.GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
                        Debug.Log(_availableMoves.Count);
                        //Get a list of special moves
                        _specialMove = _currentlyDragging.GetSpecialMoves(ref _chessPieces, ref _moveList,
                            ref _availableMoves, TileCountX, TileCountY);
                        Debug.Log(_specialMove);
                        PreventCheck();

                        HighlightTiles();
                    }
                }
            }

            //if release click
            if (_currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                var previousPosition = new Vector2Int(_currentlyDragging.currentX, _currentlyDragging.currentY);

                if (ContainsValidMove(ref _availableMoves, new Vector2Int(hitPosition.x, hitPosition.y)))
                {
                    MoveTo(previousPosition.x, previousPosition.y, hitPosition.x, hitPosition.y);
                    var moved = _chessPieces[hitPosition.x, hitPosition.y];

                    CheckPromotion(moved);
                    // Net implementation
                    var mm = new NetMakeMove();
                    mm.OriginalX = previousPosition.x;
                    mm.OriginalY = previousPosition.y;
                    mm.DestinationX = hitPosition.x;
                    mm.DestinationY = hitPosition.y;
                    mm.TeamId = _currentTeam;
                    Client.Instance.SendToServer(mm);
                }
                else
                {
                    _currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    _currentlyDragging = null;
                    RemoveHighlightTiles();
                }
            }
        }

        else
        {
            if (_currentHover != -Vector2Int.one)
            {
                _tiles[_currentHover.x, _currentHover.y].layer = ContainsValidMove(ref _availableMoves, _currentHover)
                    ? LayerMask.NameToLayer("Highlight")
                    : LayerMask.NameToLayer("Tile");
                _currentHover = -Vector2Int.one;
            }

            if (_currentlyDragging && Input.GetMouseButtonUp(0))
            {
                _currentlyDragging.SetPosition(GetTileCenter(_currentlyDragging.currentX, _currentlyDragging.currentY));
                _currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }

        //if dragging piece
        if (_currentlyDragging)
        {
            var horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            var distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
                _currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
    }

    //Generate Board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        _bounds = new Vector3(tileCountX / 2 * tileSize, 0, tileCountX / 2 * tileSize) + boardCenter;

        _tiles = new GameObject[tileCountX, tileCountY];
        for (var x = 0; x < tileCountX; x++)
        for (var y = 0; y < tileCountY; y++)
            _tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        var tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        var mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        var vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - _bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - _bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - _bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - _bounds;

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
        _chessPieces = new ChessPiece[TileCountX, TileCountY];

        byte whiteTeam = 0, blackTeam = 1;

        //White team
        _chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        _chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        _chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        _chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        _chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        _chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        _chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        _chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        _chessPieces[4, 3] = SpawnSinglePiece(ChessPieceType.Portal, whiteTeam);
        for (var i = 0; i < TileCountX; i++) _chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);

        ////Black team
        _chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        _chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        _chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        _chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        _chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        _chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        _chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        _chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        _chessPieces[3, 4] = SpawnSinglePiece(ChessPieceType.Portal, blackTeam);

        for (var i = 0; i < TileCountX; i++) _chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
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
        for (var x = 0; x < TileCountX; x++)
        for (var y = 0; y < TileCountY; y++)
            if (_chessPieces[x, y] != null)
                PositionSinglePiece(x, y, true);
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        _chessPieces[x, y].currentX = x;
        _chessPieces[x, y].currentY = y;
        _chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - _bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    // Highlighting Tiles
    private void HighlightTiles()
    {
        for (var i = 0; i < _availableMoves.Count; i++)
            _tiles[_availableMoves[i].x, _availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }

    private void RemoveHighlightTiles()
    {
        for (var i = 0; i < _availableMoves.Count; i++)
            _tiles[_availableMoves[i].x, _availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        _availableMoves.Clear();
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
        if (_localGame)
        {
            var wrm = new NetRematch();
            wrm.TeamId = 0;
            wrm.WantRematch = 1;
            Client.Instance.SendToServer(wrm);

            var brm = new NetRematch();
            brm.TeamId = 1;
            brm.WantRematch = 1;
            Client.Instance.SendToServer(brm);
        }
        else
        {
            var rm = new NetRematch();
            rm.TeamId = _currentTeam;
            rm.WantRematch = 1;
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
        _currentlyDragging = null;
        _availableMoves.Clear();
        _moveList.Clear();
        _playerRematch[0] = _playerRematch[1] = false;
        if (_localGame)
            _currentTeam = 0;
        //clean up

        for (var x = 0; x < TileCountX; x++)
        for (var y = 0; y < TileCountY; y++)
        {
            if (_chessPieces[x, y] != null)
                Destroy(_chessPieces[x, y].gameObject);

            _chessPieces[x, y] = null;
        }

        for (var i = 0; i < _deadWhites.Count; i++)
            Destroy(_deadWhites[i].gameObject);
        for (var i = 0; i < _deadBlacks.Count; i++)
            Destroy(_deadBlacks[i].gameObject);

        _deadWhites.Clear();
        _deadBlacks.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        _isWhiteTurn = true;
        AddNewGameToLog();
    }

    public void OnMenuButton()
    {
        var rm = new NetRematch();
        rm.TeamId = _currentTeam;
        rm.WantRematch = 0;
        Client.Instance.SendToServer(rm);

        GameReset();
        GameUI.Instance.OnLeaveFromGameMenu();

        Invoke("ShutdownRelay", 1.0f);

        //Reset some values
        _playerCount = -1;
        _currentTeam = -1;
    }
    //SpecialMoves

    private void ProcessSpecialMove()
    {
        if (_specialMove == SpecialMove.EnPassant)
        {
            var newMove = _moveList[_moveList.Count - 1];
            var myPawn = _chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPostion = _moveList[_moveList.Count - 2];
            var enemyPawn = _chessPieces[targetPawnPostion[1].x, targetPawnPostion[1].y];

            if (myPawn.currentX == enemyPawn.currentX)
                if (myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1)
                {
                    if (enemyPawn.team == 0)
                    {
                        _deadWhites.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffset + deathHeight, -1 * tileSize)
                                              - _bounds
                                              + new Vector3(tileSize / 2, 0, tileSize / 2)
                                              + Vector3.forward * deathSpacing * _deadWhites.Count);
                    }
                    else
                    {
                        _deadBlacks.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(new Vector3(-1 * tileSize, yOffset + deathHeight, 8 * tileSize)
                                              - _bounds
                                              + new Vector3(tileSize / 2, 0, tileSize / 2)
                                              + Vector3.back * deathSpacing * _deadBlacks.Count);
                    }

                    _chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
        }

        if (_specialMove == SpecialMove.Promotion)
        {
            var lastMove = _moveList[_moveList.Count - 1];
            var targetPawn = _chessPieces[lastMove[1].x, lastMove[1].y];

            if (targetPawn.type == ChessPieceType.Pawn)
            {
                if (targetPawn.team == 0 && lastMove[1].y == 7)
                {
                    var newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = _chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(_chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    _chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }

                if (targetPawn.team == 1 && lastMove[1].y == 0)
                {
                    var newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = _chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(_chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    _chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }

        if (_specialMove == SpecialMove.Castling)
        {
            var lastMove = _moveList[_moveList.Count - 1];

            //left rook
            if (lastMove[1].x == 2)
            {
                if (lastMove[1].y == 0) //white side
                {
                    var rook = _chessPieces[0, 0];
                    _chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    _chessPieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7) //black side
                {
                    var rook = _chessPieces[0, 7];
                    _chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    _chessPieces[0, 7] = null;
                }
            }
            //right rook
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0) //white side
                {
                    var rook = _chessPieces[7, 0];
                    _chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    _chessPieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7) //black side
                {
                    var rook = _chessPieces[7, 7];
                    _chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    _chessPieces[7, 7] = null;
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
                newQueen.transform.position = _chessPieces[piece.currentX, piece.currentY].transform.position;
                Destroy(_chessPieces[piece.currentX, piece.currentY].gameObject);
                _chessPieces[piece.currentX, piece.currentY] = newQueen;
                PositionSinglePiece(piece.currentX, piece.currentY);
            }
    }

    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (var x = 0; x < TileCountX; x++)
        for (var y = 0; y < TileCountY; y++)
            if (_chessPieces[x, y] != null)
                if (_chessPieces[x, y].type == ChessPieceType.King)
                    if (_chessPieces[x, y].team == _currentlyDragging.team)
                        targetKing = _chessPieces[x, y];

        //delete moves that put us in check
        SimulateMoveForSinglePiece(_currentlyDragging, ref _availableMoves, targetKing);
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
            var simulation = new ChessPiece[TileCountX, TileCountY];
            var simAttackingPieces = new List<ChessPiece>();

            for (var x = 0; x < TileCountX; x++)
            for (var y = 0; y < TileCountY; y++)
                if (_chessPieces[x, y] != null)
                {
                    simulation[x, y] = _chessPieces[x, y];
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
                var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation, TileCountX, TileCountY);
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
        var lastMove = _moveList[_moveList.Count - 1];
        var targetTeam = _chessPieces[lastMove[1].x, lastMove[1].y].team == 0 ? 1 : 0;

        var attackingPiece = new List<ChessPiece>();
        var defendingPiece = new List<ChessPiece>();
        ChessPiece targetKing = null;
        for (var x = 0; x < TileCountX; x++)
        for (var y = 0; y < TileCountY; y++)
            if (_chessPieces[x, y] != null)
            {
                if (_chessPieces[x, y].team == targetTeam)
                {
                    defendingPiece.Add(_chessPieces[x, y]);
                    if (_chessPieces[x, y].type == ChessPieceType.King)
                        targetKing = _chessPieces[x, y];
                }
                else
                {
                    attackingPiece.Add(_chessPieces[x, y]);
                }
            }

        // is the king attacked right now
        var currentAvailableMoves = new List<Vector2Int>();
        for (var i = 0; i < attackingPiece.Count; i++)
        {
            var pieceMoves = attackingPiece[i].GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
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
                    defendingPiece[i].GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
                SimulateMoveForSinglePiece(defendingPiece[i], ref defendingMoves, targetKing);

                if (defendingMoves.Count != 0)
                    return false;
            }

            return true; //Checkmate
        }

        var defMoves = false;

        for (var i = 0; i < defendingPiece.Count; i++)
        {
            var defendingMoves = defendingPiece[i].GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
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
        for (var x = 0; x < TileCountX; x++)
        for (var y = 0; y < TileCountY; y++)
            if (_tiles[x, y] == hitInfo)
                return new Vector2Int(x, y);

        return -Vector2Int.one; //-1 -1 INVALID
    }

    private void MoveTo(int originalX, int originalY, int x, int y)
    {
        var cp = _chessPieces[originalX, originalY];
        var previousPosition = new Vector2Int(originalX, originalY);

        // is there another piece there
        if (_chessPieces[x, y] != null)
        {
            var ocp = _chessPieces[x, y];

            if (cp.team == ocp.team)
                return;

            //if its enemy
            if (ocp.team == 0)
            {
                if (ocp.type == ChessPieceType.King)
                    Checkmate(1);

                _deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3(8 * tileSize, yOffset + deathHeight, -1 * tileSize)
                                - _bounds
                                + new Vector3(tileSize / 2, 0, tileSize / 2)
                                + Vector3.forward * deathSpacing * _deadWhites.Count);
            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                    Checkmate(0);

                _deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(new Vector3(-1 * tileSize, yOffset + deathHeight, 8 * tileSize)
                                - _bounds
                                + new Vector3(tileSize / 2, 0, tileSize / 2)
                                + Vector3.back * deathSpacing * _deadBlacks.Count);
            }
        }

        _chessPieces[x, y] = cp;
        _chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        _isWhiteTurn = !_isWhiteTurn;
        if (_localGame)
            _currentTeam = _currentTeam == 0 ? 1 : 0;
        _moveList.Add(new[] {previousPosition, new Vector2Int(x, y)});

        ProcessSpecialMove();

        if (_currentlyDragging)
            _currentlyDragging = null;
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
        if (!File.Exists(_fileName)) File.Create(_fileName);
    }

    private void AddNewGameToLog()
    {
        using (var w = File.AppendText(_fileName))
        {
            w.WriteLine("");
            w.WriteLine("");
            w.WriteLine("---NEW GAME---");
            w.WriteLine("");
            w.WriteLine("");
        }
    }

    private void AddToLog(string logMessage)
    {
        using (var w = File.AppendText(_fileName))
        {
            w.WriteLine(logMessage);
        }
    }

    #region

    private void RegisterEvents()
    {
        NetUtility.SWelcome += OnWelcomeServer;
        NetUtility.SMakeMove += OnMakeMoveServer;
        NetUtility.SRematch += OnRematchServer;

        NetUtility.CWelcome += OnWelcomeClient;
        NetUtility.CStartGame += OnStartGameClient;
        NetUtility.CMakeMove += OnMakeMoveClient;
        NetUtility.CRematch += OnRematchClient;


        GameUI.Instance.SetLocalGame += OnSetLocalGame;
    }

    private void UnRegisterEvents()
    {
        NetUtility.SWelcome -= OnWelcomeServer;
        NetUtility.SMakeMove -= OnMakeMoveServer;
        NetUtility.SRematch -= OnRematchServer;

        NetUtility.CWelcome -= OnWelcomeClient;
        NetUtility.CStartGame -= OnStartGameClient;
        NetUtility.CMakeMove += OnMakeMoveClient;
        NetUtility.CRematch -= OnRematchClient;

        GameUI.Instance.SetLocalGame -= OnSetLocalGame;
    }

    //server
    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        //Cleint has connected, assign a team and return the message back to him
        var nw = msg as NetWelcome;

        //Assign a team
        nw.AssignedTeam = ++_playerCount;

        //Return back to the client
        Server.Instance.SendToClient(cnn, nw);

        //if full, start game
        if (_playerCount == 1) Server.Instance.Broadcast(new NetStartGame());
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
        _currentTeam = nw.AssignedTeam;

        Debug.Log($"My asssigned team is {nw.AssignedTeam}");

        if (_localGame && _currentTeam == 0)
            Server.Instance.Broadcast(new NetStartGame());
    }

    private void OnStartGameClient(NetMessage msg)
    {
        GameUI.Instance.ChangeCamera(_currentTeam == 0 ? CameraAngle.WhiteTeam : CameraAngle.BlackTeam);
    }

    private void OnMakeMoveClient(NetMessage msg)
    {
        var mm = msg as NetMakeMove;

        Debug.Log($"MM : {mm.TeamId} :  {mm.OriginalX} {mm.OriginalY} -> {mm.DestinationX} {mm.DestinationY}");

        if (mm.TeamId != _currentTeam)
        {
            AddToLog(
                $"MM : {mm.TeamId} : {_chessPieces[mm.OriginalX, mm.OriginalY].type.ToString()[0]} {mm.OriginalX} {mm.OriginalY} -> {mm.DestinationX} {mm.DestinationY}");
            var target = _chessPieces[mm.OriginalX, mm.OriginalY];

            _availableMoves = target.GetAvailableMoves(ref _chessPieces, TileCountX, TileCountY);
            _specialMove = target.GetSpecialMoves(ref _chessPieces, ref _moveList, ref _availableMoves, TileCountX,
                TileCountY);

            MoveTo(mm.OriginalX, mm.OriginalY, mm.DestinationX, mm.DestinationY);
            CheckPromotion(target);
        }
        else
        {
            AddToLog(
                $"MM : {mm.TeamId} : {_chessPieces[mm.DestinationX, mm.DestinationY].type.ToString()[0]} {mm.OriginalX} {mm.OriginalY} -> {mm.DestinationX} {mm.DestinationY}");
        }
    }

    private void OnRematchClient(NetMessage msg)
    {
        //Recieve the connection message
        var rm = msg as NetRematch;

        //set bool for rematch
        _playerRematch[rm.TeamId] = rm.WantRematch == 1;

        //activate the piece of ui
        if (rm.TeamId != _currentTeam)
        {
            rematchIndicator.transform.GetChild(rm.WantRematch == 1 ? 0 : 1).gameObject.SetActive(true);
            if (rm.WantRematch != 1) rematchButton.interactable = false;
        }

        //if both want rematch
        if (_playerRematch[0] && _playerRematch[1])
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
        _playerCount = -1;
        _currentTeam = -1;
        _localGame = v;
    }

    #endregion
}