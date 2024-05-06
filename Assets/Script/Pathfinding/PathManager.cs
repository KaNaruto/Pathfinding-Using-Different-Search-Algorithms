using System;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Pathfinding
{
    public class PathManager 
    {
        public static Vector3[] RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            Vector3[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);

            return waypoints;
        }

        static Vector3[] SimplifyPath(List<Node> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            Vector2 directionOld = Vector2.zero;

            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(path[i - 1].GridX - path[i].GridX, path[i - 1].GridY - path[i].GridY);
                if (directionNew != directionOld)
                {
                    waypoints.Add(path[i].WorldPosition);
                }

                directionOld = directionNew;
            }

            return waypoints.ToArray();
        }
    
        public static int GetDistance(Node startNode, Node targetNode)
        {
            int distanceX = Mathf.Abs(startNode.GridX - targetNode.GridX);
            int distanceY = Mathf.Abs(startNode.GridY - targetNode.GridY);

            if (distanceX > distanceY)
                return 14 * distanceY + 10 * (distanceX - distanceY);
            else
                return 14 * distanceX + 10 * (distanceY - distanceX);
        }
    }
}
