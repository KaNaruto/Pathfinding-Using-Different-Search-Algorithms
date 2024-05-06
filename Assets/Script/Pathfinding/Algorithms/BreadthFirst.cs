using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Script.Pathfinding.Algorithms
{
    public class BreadthFirst : PathFinding
    {
        private Grid _grid;
        private Queue<Node> _openNodes;
        private HashSet<Node> _visitedNodes;

        private void Awake()
        {
            _grid = GetComponent<Grid>();
            _openNodes = new Queue<Node>(_grid.MaxSize);
            _visitedNodes = new HashSet<Node>();
        }

        public override void FindPath(PathRequest request, Action<PathResult> callback)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool pathSuccess = false;
            Node startNode = _grid.GetNodeFromWorldPosition(request.PathStart);
            Node targetNode = _grid.GetNodeFromWorldPosition(request.PathEnd);

            if (startNode.Walkable && targetNode.Walkable)
            {
                _openNodes.Clear();
                _visitedNodes.Clear();
                _openNodes.Enqueue(startNode);

                while (_openNodes.Count > 0)
                {
                    Node currentNode = _openNodes.Dequeue();
                    _visitedNodes.Add(currentNode);

                    if (currentNode == targetNode)
                    {
                        sw.Stop();
                        Debug.Log("Path found: " + sw.ElapsedMilliseconds + " ms");
                        pathSuccess = true;
                        break;
                    }

                    foreach (Node neighbor in _grid.GetNeighbours(currentNode))
                    {
                        if (!neighbor.Walkable || _visitedNodes.Contains(neighbor))
                            continue;

                        if (!_openNodes.Contains(neighbor))
                        {
                            neighbor.Parent = currentNode;
                            _openNodes.Enqueue(neighbor);
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
