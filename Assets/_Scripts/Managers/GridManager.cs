using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityRandom = UnityEngine.Random;
using Random = System.Random;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    //[SerializeField] private int _width, _height;

    //[SerializeField] private Tile _grassTile, _mountainTile;

    //[SerializeField] private Transform _cam;

    private Dictionary<Vector2, Tile> _tiles;

    [SerializeField] private Vector2Int _size;
    [SerializeField] private Vector2 _gap;
    [SerializeField, Range(0, 0.8f)] private float _skipAmount = 0.1f;
    [SerializeField, Range(0, 1)] private float _mountainAmount = 0.3f;
    [SerializeField] private GridType _gridType;
    [SerializeField] private ScriptableGridConfig[] _configs;

    private bool _requiresGeneration = true;
    private Camera _cam;
    private Grid _grid;

    private Vector3 _cameraPositionTarget;
    private float _cameraSizeTarget;
    private Vector3 _moveVel;
    private float _cameraSizeVel;

    private Vector2 _currentGap;
    private Vector2 _gapVel;

    void Awake()
    {
        Instance = this;
        _grid = GetComponent<Grid>();
        _cam = Camera.main;
        _currentGap = _gap;
    }

    private void OnValidate() => _requiresGeneration = true;

    private void LateUpdate()
    {
        if (Vector2.Distance(_currentGap, _gap) > 0.01f)
        {
            _currentGap = Vector2.SmoothDamp(_currentGap, _gap, ref _gapVel, 0.1f);
            _requiresGeneration = true;
        }

        if (_requiresGeneration) GenerateGrid();

        _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, _cameraPositionTarget, ref _moveVel, 0.5f);
        _cam.orthographicSize = Mathf.SmoothDamp(_cam.orthographicSize, _cameraSizeTarget, ref _cameraSizeVel, 0.5f);
    }

    public void GenerateGrid()
    {
        var config = _configs.First(c => c.Type == _gridType);

        _grid.cellLayout = config.Layout;
        _grid.cellSize = config.CellSize;
        if (_grid.cellLayout != GridLayout.CellLayout.Hexagon) _grid.cellGap = _currentGap;
        _grid.cellSwizzle = config.GridSwizzle;

        var coordinates = new List<Vector3Int>();

        for (int x = 0; x < _size.x; x++)
        {
            for (int y = 0; y < _size.y; y++)
            {
                coordinates.Add(new Vector3Int(x, y));
            }
        }

        var bounds = new Bounds();
        var skipCount = Mathf.FloorToInt(coordinates.Count * _skipAmount);
        var mountainCount = Mathf.FloorToInt(coordinates.Count * _mountainAmount);
        var index = 0;
        var rand = new Random(420);

        _tiles = new Dictionary<Vector2, Tile>();

        foreach (var coordinate in coordinates.OrderBy(t => rand.Next()).Take(coordinates.Count - skipCount))
        {
            var isMountain = index++ < mountainCount;
            var prefab = isMountain ? config.MountainPrefab : config.GrassPrefab;
            var position = _grid.GetCellCenterWorld(coordinate);
            var spawned = Instantiate(prefab, position, Quaternion.identity, transform);
            spawned.name = $"Tile {coordinate.x} {coordinate.y}";
            spawned.Init(coordinate);
            _tiles[new Vector2(coordinate.x, coordinate.y)] = spawned;
            bounds.Encapsulate(position);
        }

        SetCamera(bounds);

        _requiresGeneration = false;

        /*
        _tiles = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var randomTile = UnityRandom.Range(0, 6) == 3 ? _mountainTile : _grassTile;
                var spawnedTile = Instantiate(randomTile, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";


                spawnedTile.Init(x, y);


                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }

        _cam.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
        */

        GameManager.Instance.ChangeState(GameState.SpawnHeroes);
    }

    private void SetCamera(Bounds bounds)
    {
        bounds.Expand(2);

        var vertical = bounds.size.y;
        var horizontal = bounds.size.x * _cam.pixelHeight / _cam.pixelWidth;

        _cameraPositionTarget = bounds.center + Vector3.back;
        _cameraSizeTarget = Mathf.Max(horizontal, vertical) * 0.5f;
    }

    public Tile GetHeroSpawnTile()
    {
        return _tiles.Where(t => t.Key.x < _size.x / 2 && t.Value.Walkable).OrderBy(t => UnityRandom.value).First().Value;
    }

    public Tile GetEnemySpawnTile()
    {
        return _tiles.Where(t => t.Key.x > _size.x / 2 && t.Value.Walkable).OrderBy(t => UnityRandom.value).First().Value;
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile)) return tile;
        return null;
    }
}

[CreateAssetMenu]
public class ScriptableGridConfig : ScriptableObject
{
    public GridType Type;
    [Space(10)]
    public GridLayout.CellLayout Layout;
    public Tile GrassPrefab, MountainPrefab;
    public Vector3 CellSize;
    public GridLayout.CellSwizzle GridSwizzle;
}

[Serializable]
public enum GridType
{
    Rectangle,
    Isometric,
    HexagonPointy,
    HexagonFlat
}