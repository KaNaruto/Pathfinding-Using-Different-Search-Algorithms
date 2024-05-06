using Script.Utility;
using UnityEngine;

namespace Script.Pathfinding
{
    public class Node : IHeapItem<Node>
    {
        public enum ComparisonMode
        {
            FCost,
            HCost,
            GCost
        }
    
        public readonly bool Walkable;
        public Vector3 WorldPosition;

        public Node Parent;
        public int GCost; // Distance to current position
        public int HCost; // Heuristic cost (Distance to target position)

        private int FCost => GCost + HCost; // Total cost

        public int MovementPenalty;

        public readonly int GridX;
        public readonly int GridY;

        public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY, int movementPenalty)
        {
            Walkable = walkable;
            WorldPosition = worldPosition;
            GridX = gridX;
            GridY = gridY;
            MovementPenalty = movementPenalty;
        }

        public static ComparisonMode comparisonMode = ComparisonMode.FCost;

        public int CompareTo(Node other)
        {
            switch (comparisonMode)
            {
                case ComparisonMode.FCost:
                    return CompareFCost(other);
                case ComparisonMode.HCost:
                    return CompareHCost(other);
                case ComparisonMode.GCost:
                    return CompareGCost(other);
                default:
                    return 0;
            }
        }

        private int CompareFCost(Node other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
            {
                compare = HCost.CompareTo(other.HCost);
            }
            return -compare;
        }

        private int CompareHCost(Node other)
        {
            return -HCost.CompareTo(other.HCost);
        }

        private int CompareGCost(Node other)
        {
            return -GCost.CompareTo(other.GCost);
        }


        public int HeapIndex { get; set; }
    }
}