using UnityEngine;

namespace Script.Pathfinding
{
    public class Path
    {
        public readonly Vector3[] LookPoints;
        public readonly Line[] TurnBoundaries;
        public readonly int FinishLineIndex;

        public Path(Vector3[] waypoints, Vector3 startPos, float turnDistance)
        {
            LookPoints = waypoints;
            TurnBoundaries = new Line[LookPoints.Length];
            FinishLineIndex = TurnBoundaries.Length - 1;

            Vector2 previousPoint = Vector3ToVector2(startPos);
            for (int i = 0; i < LookPoints.Length; i++)
            {
                Vector2 currentPoint = Vector3ToVector2(LookPoints[i]);
                Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
                Vector2 turnBoundaryPoint =
                    (i == FinishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDistance;
                TurnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDistance);
                previousPoint = turnBoundaryPoint;
            }
        }


        Vector2 Vector3ToVector2(Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }

        public void DrawWithGizmos()
        {
            Gizmos.color = Color.grey;
            foreach (Vector3 points in LookPoints)
            {
                Gizmos.DrawCube(points+Vector3.up,new Vector3(0.25f,0,0.25f));
            }

            for (int i = 0; i < LookPoints.Length-1;i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(LookPoints[i]+Vector3.up,LookPoints[i+1]+Vector3.up);
            }
        }
    }
}