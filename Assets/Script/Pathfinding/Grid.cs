using System.Collections.Generic;
using UnityEngine;

namespace Script.Pathfinding
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] bool displayGridGizmos;
        Node[,] _grid;
        [SerializeField] Vector2 gridWorldSize;
        [SerializeField] LayerMask unwalkableMask;

        [SerializeField] float nodeRadius;
        private float _nodeDiameter;
        private int _gridSizeX, _gridSizeY;
        [SerializeField] private int obstacleProximityPenalty = 10;

        public int MaxSize => _maxSize;

        private int _maxSize;

        private void Awake()
        {
            // Compute how many grid will be
            _nodeDiameter = nodeRadius * 2;
            _gridSizeX = Mathf.RoundToInt(gridWorldSize.x / _nodeDiameter);
            _gridSizeY = Mathf.RoundToInt(gridWorldSize.y / _nodeDiameter);
            _maxSize = _gridSizeX * _gridSizeY;
            CreateGrid();
        }

        void CreateGrid()
        {
            _grid = new Node[_gridSizeX, _gridSizeY];

            Vector3 wordBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 -
                                     Vector3.forward * gridWorldSize.y / 2;
            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Vector3 worldPoint = wordBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius) +
                                         Vector3.forward * (y * _nodeDiameter + nodeRadius);
                    bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);

                    int movementPenalty = 0;

                    if (!walkable)
                        movementPenalty = obstacleProximityPenalty;


                    _grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
                }
            }
            BlurPenaltyMap(3);
        }


        private void OnDrawGizmos()
        {
            if (displayGridGizmos && _grid != null)
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
                foreach (Node x in _grid)
                {
                    Gizmos.color = (x.Walkable) ? Color.green : Color.red;
                    Gizmos.DrawCube(x.WorldPosition, Vector3.one * (_nodeDiameter - .05f));
                }
            }
        }

        public Node GetNodeFromWorldPosition(Vector3 worldPosition)
        {
            Vector3 wordBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 -
                                     Vector3.forward * gridWorldSize.y / 2;
            int x = Mathf.RoundToInt((worldPosition.x + nodeRadius - wordBottomLeft.x) / _nodeDiameter) - 1;
            int y = Mathf.RoundToInt((worldPosition.z + nodeRadius - wordBottomLeft.z) / _nodeDiameter) - 1;

            // Ensure x and y are within the grid bounds
            x = Mathf.Clamp(x, 0, _gridSizeX - 1);
            y = Mathf.Clamp(y, 0, _gridSizeY - 1);


            return _grid[x, y];
        }

        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.GridX + x;
                    int checkY = node.GridY + y;

                    if ((checkX >= 0 && checkX < _gridSizeX) && (checkY >= 0 && checkY < _gridSizeY))
                        neighbours.Add(_grid[checkX, checkY]);
                }
            }

            return neighbours;
        }

        void BlurPenaltyMap(int blurSize)
        {
            int kernelSize = blurSize * 2 + 1;
            int kernelExtents = (kernelSize - 1) / 2;

            int[,] penaltiesHorizontalPass = new int[_gridSizeX, _gridSizeY];
            int[,] penaltiesVerticalPass = new int[_gridSizeX, _gridSizeY];

            for (int y = 0; y < _gridSizeY; y++)
            {
                for (int x = -kernelExtents; x <= kernelExtents; x++)
                {
                    int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                    penaltiesHorizontalPass[0, y] += _grid[sampleX, y].MovementPenalty;
                }

                for (int x = 1; x < _gridSizeX; x++)
                {
                    int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, _gridSizeX);
                    int addIndex = Mathf.Clamp(x + kernelExtents, 0, _gridSizeX - 1);

                    penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] -
                        _grid[removeIndex, y].MovementPenalty + _grid[addIndex, y].MovementPenalty;
                }
            }

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = -kernelExtents; y <= kernelExtents; y++)
                {
                    int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                    penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
                }

                int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
                _grid[x, 0].MovementPenalty = blurredPenalty;

                for (int y = 1; y < _gridSizeY; y++)
                {
                    int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, _gridSizeY);
                    int addIndex = Mathf.Clamp(y + kernelExtents, 0, _gridSizeY - 1);

                    penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] -
                        penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                    blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                    _grid[x, y].MovementPenalty = blurredPenalty;
                }
            }
        }
    }
}