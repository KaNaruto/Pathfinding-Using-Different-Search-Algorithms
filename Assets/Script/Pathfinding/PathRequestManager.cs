using System;
using System.Collections.Generic;
using System.Threading;
using Script.Pathfinding.Algorithms;
using UnityEngine;

namespace Script.Pathfinding
{
    public class PathRequestManager : MonoBehaviour
    {
        private AStar _aStar;
        private DepthFirst _depthFirst;
        private BreadthFirst _breadthFirst;
        private Dijkstra _dijkstra;
        private GreedyBestFit _greedyBestFit;
        public PathFinding algorithm;


        public static PathRequestManager Instance;
        private readonly Queue<PathResult> _results = new Queue<PathResult>();

        private void Awake()
        {
            Instance = this;
            algorithm = GetComponent<AStar>();
        }
    
        private void Update()
        {
            lock (_results)
            {
                if (_results.Count > 0)
                {
                    int itemsInQueue = _results.Count;
                    lock (_results)
                    {
                        for (int i = 0; i < itemsInQueue; i++)
                        {
                            PathResult result = _results.Dequeue();
                            result.Callback(result.Path, result.Success);
                        }
                    }
                }
            }
        }

        public void SetAlgorithm(int value)
        {
            switch (value)
            {
                case 0:
                    algorithm = GetComponent<AStar>();
                    break;
                case 1:
                    algorithm = GetComponent<BreadthFirst>();
                    break;
                case 2:
                    algorithm = GetComponent<DepthFirst>();
                    break;
                case 3:
                    algorithm = GetComponent<GreedyBestFit>();
                    break;
                case 4:
                    algorithm = GetComponent<Dijkstra>();
                    break;
            }
        }
        public static void RequestPath(PathRequest request)
        {
            ThreadStart threadStart = delegate
            {
                Instance.algorithm.FindPath(request, Instance.FinishedProcessingPath);
            };
            threadStart.Invoke();
        }


        void FinishedProcessingPath(PathResult result)
        {
            lock (_results)
            {
                _results.Enqueue(result);
            }
        }
    }

    public struct PathResult
    {
        public readonly Vector3[] Path;
        public readonly bool Success;
        public readonly Action<Vector3[], bool> Callback;

        public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
        {
            this.Path = path;
            this.Success = success;
            this.Callback = callback;
        }
    }

    public struct PathRequest
    {
        public Vector3 PathStart;
        public Vector3 PathEnd;
        public readonly Action<Vector3[], bool> Callback;

        public PathRequest(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
        {
            PathStart = pathStart;
            PathEnd = pathEnd;
            Callback = callback;
        }
    }
}