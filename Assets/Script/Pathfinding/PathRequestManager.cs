using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
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
        private readonly ConcurrentQueue<PathResult> _results = new ConcurrentQueue<PathResult>();
        private readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

        private void Awake()
        {
            Instance = this;
            algorithm = GetComponent<AStar>();
        }

        private void Update()
        {
            while (_results.TryDequeue(out PathResult result))
            {
                result.Callback(result.Path, result.Success);
            }

            while (_actions.TryDequeue(out var action))
            {
                action.Invoke();
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
            Task.Run(() =>
            {
                Instance.EnqueueAction(() =>
                {
                    Instance.algorithm.FindPath(request, Instance.FinishedProcessingPath);
                });
            });
        }

        private void EnqueueAction(Action action)
        {
            _actions.Enqueue(action);
        }

        void FinishedProcessingPath(PathResult result)
        {
            _results.Enqueue(result);
        }
    }

    public struct PathResult
    {
        public readonly Vector3[] Path;
        public readonly bool Success;
        public readonly Action<Vector3[], bool> Callback;

        public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
        {
            Path = path;
            Success = success;
            Callback = callback;
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
