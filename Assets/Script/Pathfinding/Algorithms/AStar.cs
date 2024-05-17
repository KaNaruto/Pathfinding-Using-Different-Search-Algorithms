using System;
using System.Collections.Generic;
using System.Diagnostics;
using Script.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Script.Pathfinding.Algorithms
{
    public class AStar : PathFinding
    {
        private Grid _grid;
        private Heap<Node> _openList;
        private HashSet<Node> _visitedList;
        private readonly object _syncLock = new object();
    
        private void Awake()
        {
            _grid = GetComponent<Grid>();
       
            _openList = new Heap<Node>(_grid.MaxSize);
            _visitedList = new HashSet<Node>();
        }

        public override void FindPath(PathRequest request, Action<PathResult> callback)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool pathSuccess = false;
            Node startNode = _grid.GetNodeFromWorldPosition(request.PathStart);
            Node targetNode = _grid.GetNodeFromWorldPosition(request.PathEnd);

            lock (_syncLock)
            {
                if (startNode.Walkable && targetNode.Walkable)
                {
                    _openList.Clear();
                    _visitedList.Clear();
                    Node.comparisonMode = Node.ComparisonMode.FCost;
                    _openList.Add(startNode);

                    while (_openList.Count > 0)
                    {
                        Node currentNode = _openList.RemoveFirst();
                        _visitedList.Add(currentNode);

                        if (currentNode == targetNode)
                        {
                            sw.Stop();
                            Debug.Log("Elapsed time= " + sw.ElapsedMilliseconds + " ms");
                            pathSuccess = true;
                            break;
                        }

                        List<Node> neighbours = _grid.GetNeighbours(currentNode);
                        foreach (Node neighbourNode in neighbours)
                        {
                            if (!neighbourNode.Walkable || _visitedList.Contains(neighbourNode))
                                continue;

                            int newMovementCostToNeighbour =
                                currentNode.GCost + PathManager.GetDistance(currentNode, neighbourNode) + neighbourNode.MovementPenalty;
                            if (newMovementCostToNeighbour < neighbourNode.GCost || !_openList.Contains(neighbourNode))
                            {
                                neighbourNode.GCost = newMovementCostToNeighbour;
                                neighbourNode.HCost = PathManager.GetDistance(neighbourNode, targetNode);
                                neighbourNode.Parent = currentNode;
                                if (!_openList.Contains(neighbourNode))
                                    _openList.Add(neighbourNode);
                                else
                                    _openList.UpdateItem(neighbourNode);
                            }
                        }
                    }
                }
            }

            Vector3[] waypoints = pathSuccess ? PathManager.RetracePath(startNode, targetNode) : Array.Empty<Vector3>();
            pathSuccess = waypoints.Length > 0;
            callback(new PathResult(waypoints, pathSuccess, request.Callback));
        }
    }
}
