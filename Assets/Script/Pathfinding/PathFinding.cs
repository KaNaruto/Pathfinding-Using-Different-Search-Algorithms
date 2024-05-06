using System;
using UnityEngine;

namespace Script.Pathfinding
{
    public abstract class PathFinding : MonoBehaviour
    {
        public abstract void FindPath(PathRequest request,Action<PathResult> callback);
    }
}
