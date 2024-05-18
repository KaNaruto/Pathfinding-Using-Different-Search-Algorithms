using System;
using System.Collections.Generic;
using UnityEngine;
using Grid = Script.Pathfinding.Grid;
using Random = System.Random;

namespace Script.Map
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] Map[] maps;
        [SerializeField] int mapIndex;

        [SerializeField] Transform tilePrefab;
        [SerializeField] private Transform obstacle;
        [SerializeField] Transform mapFloor;
    


        [SerializeField] float tileSize = 1;
        [SerializeField] Vector2 maxMapSize;


        [Range(0, 1)] [SerializeField] float outlineRate;

        private Map _currentMap;

        private List<Coordinate> _tileCoordinates;
        private Queue<Coordinate> _shuffledTileCoordinates;

        private Transform[,] _tileMap;
        private Queue<Coordinate> _shuffledOpenTileCoordinates;

        public Transform target;

        private void Start()
        {
            MapData? mapData = SaveSystem.LoadMapData();
            if (mapData != null)
            {
                MapData data = mapData.Value;
                SetProperties(data.xSize,data.ySize,data.seed,data.obstacleRate);
            }
        }

        public void SetProperties(int x, int y, int seed, float obstacleRate)
        {
            maps[mapIndex].mapSize.x = x;
            maps[mapIndex].mapSize.y = y;
            maps[mapIndex].seed = seed;
            maps[mapIndex].obstacleRate = obstacleRate;
            
            SaveSystem.SaveMapData(new MapData(x, y, seed, obstacleRate));
            GenerateMap();
            FindObjectOfType<Grid>().GridProperties(x, y);
        }

        public void GenerateMap()
        {
            _currentMap = maps[mapIndex];

            _tileMap = new Transform[_currentMap.mapSize.x, _currentMap.mapSize.y];
            Random random = new Random(_currentMap.seed);


            // Destroy old objects
            _tileCoordinates = new List<Coordinate>();
            string holderName = "Generated Map";
            if (transform.Find(holderName))
                DestroyImmediate(transform.Find(holderName).gameObject);

            Transform mapHolder = new GameObject(holderName).transform;
            mapHolder.parent = transform;


            // Tiles
            for (int x = 0; x < _currentMap.mapSize.x; x++)
            {
                for (int y = 0; y < _currentMap.mapSize.y; y++)
                {
                    Vector3 tilePosition = CoordinatesToPosition(x, y);
                    Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));
                    newTile.localScale = Vector3.one * ((1 - outlineRate) * tileSize);
                    newTile.parent = mapHolder;
                    _tileMap[x, y] = newTile;


                    // Coordinate
                    _tileCoordinates.Add(new Coordinate(x, y));
                }
            }

            // Obstacles
            _shuffledTileCoordinates =
                new Queue<Coordinate>(Utility.Utility.ShuffleArray(_tileCoordinates.ToArray(), _currentMap.seed));

            bool[,] obstacleMap = new bool[_currentMap.mapSize.x, _currentMap.mapSize.y];
            int obstacleCount = (int)((_currentMap.mapSize.x * _currentMap.mapSize.y) * _currentMap.obstacleRate);
            List<Coordinate> allOpenTileCoordinates = new List<Coordinate>(_tileCoordinates);
            int currentObstacleCount = 0;
            for (int i = 0; i < obstacleCount; i++)
            {
                Coordinate randomCoordinate = GetRandomCoordinates();
                obstacleMap[randomCoordinate.x, randomCoordinate.y] = true;
                currentObstacleCount++;

                if (!IsThereAnyUnit(randomCoordinate) && MapIsFullAccessible(obstacleMap, currentObstacleCount))
                {
                    float obstacleHeight = Mathf.Lerp(_currentMap.minObstacleHeight, _currentMap.maxObstacleHeight,
                        (float)random.NextDouble());

                    Vector3 obstaclePosition = CoordinatesToPosition(randomCoordinate.x, randomCoordinate.y);
                    Transform newObstacle = Instantiate(obstacle, obstaclePosition + Vector3.up * obstacleHeight / 2,
                        Quaternion.identity);
                    newObstacle.parent = mapHolder;
                    newObstacle.localScale = new Vector3((1 - outlineRate) * tileSize, obstacleHeight,
                        ((1 - outlineRate) * tileSize));


                    Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                    Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                    float colorPercent = randomCoordinate.y / (float)_currentMap.mapSize.y;
                    obstacleMaterial.color =
                        Color.Lerp(_currentMap.foregroundColor, _currentMap.backgroundColor, colorPercent);
                    obstacleRenderer.sharedMaterial = obstacleMaterial;

                    // Remove the tile that has obstacle
                    allOpenTileCoordinates.Remove(randomCoordinate);
                }
                else
                {
                    obstacleMap[randomCoordinate.x, randomCoordinate.y] = false;
                    currentObstacleCount--;
                }
            }

            _shuffledOpenTileCoordinates =
                new Queue<Coordinate>(Utility.Utility.ShuffleArray(allOpenTileCoordinates.ToArray(), _currentMap.seed));

            mapFloor.localScale = new Vector3(_currentMap.mapSize.x * tileSize, _currentMap.mapSize.y * tileSize);
            
            FindObjectOfType<UnitManager>().FindNewPath();
        }

        bool IsThereAnyUnit(Coordinate coordinate)
        {
            // Check if the coordinate is the map center
            if (coordinate == _currentMap.MapCentre)
                return true;

            // Convert the coordinate to world position
            Vector3 worldPosition = CoordinatesToPosition(coordinate.x, coordinate.y);

            // Check against all units
            Unit[] units = FindObjectsOfType<Unit>();
            foreach (Unit unit in units)
            {
                if (unit != null)
                {
                    Vector3 unitPosition = unit.transform.position;
                    if (Vector3.Distance(unitPosition, worldPosition) < tileSize)
                        return true;
                }
            }

            // Check against the target
            Vector3 targetPosition = target.transform.position;
            if (Vector3.Distance(targetPosition, worldPosition) < tileSize)
                return true;

            // If no unit or target is at the coordinate, return false
            return false;
        }

        // Flood fill algorithm
        bool MapIsFullAccessible(bool[,] obstacleMap, int currentObstacleCount)
        {
            bool[][] mapFlags = new bool[obstacleMap.GetLength(0)][];
            for (int index = 0; index < obstacleMap.GetLength(0); index++)
            {
                mapFlags[index] = new bool[obstacleMap.GetLength(1)];
            }

            Queue<Coordinate> queue = new Queue<Coordinate>();
            queue.Enqueue(_currentMap.MapCentre);
            mapFlags[_currentMap.MapCentre.x][_currentMap.MapCentre.y] = true;

            int accessibleTileCount = 1;
            while (queue.Count > 0)
            {
                Coordinate tile = queue.Dequeue();

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        int neighbourX = tile.x + x;
                        int neighbourY = tile.y + y;
                        if (x == 0 ^ y == 0)
                        {
                            if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 &&
                                neighbourY < obstacleMap.GetLength(1))
                            {
                                if (!mapFlags[neighbourX][neighbourY] && !obstacleMap[neighbourX, neighbourY])
                                {
                                    mapFlags[neighbourX][neighbourY] = true;
                                    queue.Enqueue(new Coordinate(neighbourX, neighbourY));
                                    accessibleTileCount++;
                                }
                            }
                        }
                    }
                }
            }

            int targetAccessibleTileCount = _currentMap.mapSize.x * _currentMap.mapSize.y - currentObstacleCount;

            return targetAccessibleTileCount == accessibleTileCount;
        }

        Vector3 CoordinatesToPosition(int x, int y)
        {
            return new Vector3(-_currentMap.mapSize.x / 2f + .5f + x, 0, -_currentMap.mapSize.y / 2f + .5f + y) * tileSize;
        }


        public Transform GetTileFromPosition(Vector3 position)
        {
            int x = Mathf.RoundToInt(position.x / tileSize + (_currentMap.mapSize.x - 1) / 2f);
            int y = Mathf.RoundToInt(position.z / tileSize + (_currentMap.mapSize.y - 1) / 2f);

            x = Mathf.Clamp(x, 0, _tileMap.GetLength(0) - 1);
            y = Mathf.Clamp(y, 0, _tileMap.GetLength(1) - 1);
            return _tileMap[x, y];
        }

        private Coordinate GetRandomCoordinates()
        {
            Coordinate coordinates = _shuffledTileCoordinates.Dequeue();
            _shuffledTileCoordinates.Enqueue(coordinates);

            return coordinates;
        }

        public Transform GetRandomOpenTile()
        {
            Coordinate coordinates = _shuffledOpenTileCoordinates.Dequeue();
            _shuffledOpenTileCoordinates.Enqueue(coordinates);
            return _tileMap[coordinates.x, coordinates.y];
        }


        [Serializable]
        public struct Coordinate
        {
            public int x;
            public int y;

            public Coordinate(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public static bool operator ==(Coordinate c1, Coordinate c2)
            {
                return c1.x == c2.x && c1.y == c2.y;
            }

            public static bool operator !=(Coordinate c1, Coordinate c2)
            {
                return !(c1 == c2);
            }

            private bool Equals(Coordinate other)
            {
                return x == other.x && y == other.y;
            }

            public override bool Equals(object obj)
            {
                return obj is Coordinate other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(x, y);
            }
        }

        [Serializable]
        public class Map
        {
            public Coordinate mapSize;
            [Range(0, 1)] public float obstacleRate;
            public int seed;
            public float minObstacleHeight;
            public float maxObstacleHeight;
            public Color foregroundColor;
            public Color backgroundColor;

            public Coordinate MapCentre => new(mapSize.x / 2, mapSize.y / 2);
        }
    }
}