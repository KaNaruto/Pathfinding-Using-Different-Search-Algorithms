using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Script.Pathfinding.Algorithms
{
    public class DepthFirst : PathFinding
    {
        private Grid _grid;
        private Stack<Node> _openNodes;
        private HashSet<Node> _visitedNodes;
        private readonly object _syncLock = new object();

        private void Awake()
        {
            _grid = GetComponent<Grid>();

            _openNodes = new Stack<Node>();
            _visitedNodes = new HashSet<Node>();
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
                    _openNodes.Clear();
                    _visitedNodes.Clear();
                    _openNodes.Push(startNode);

                    while (_openNodes.Count > 0)
                    {
                        Node currentNode = _openNodes.Pop();
                        _visitedNodes.Add(currentNode);

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
                            if (!neighbourNode.Walkable || _visitedNodes.Contains(neighbourNode))
                                continue;

                            neighbourNode.Parent = currentNode;
                            _openNodes.Push(neighbourNode);
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
