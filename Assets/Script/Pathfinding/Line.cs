using UnityEngine;

namespace Script.Pathfinding
{
    public readonly struct Line
    {
        private const float VerticalLineGradient = 1e6f;

        private readonly Vector2 _pointOnLine1;
        private readonly Vector2 _pointOnline2;

        private readonly bool _approachSide;

        public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
        {
            float gradient;
            float gradientPerpendicular;
            float dx = pointOnLine.x - pointPerpendicularToLine.x;
            float dy = pointOnLine.y - pointPerpendicularToLine.y;

            if (dx == 0)
                gradientPerpendicular = VerticalLineGradient;
            else
                gradientPerpendicular = dy / dx;

            if (gradientPerpendicular == 0)
                gradient = VerticalLineGradient;
            else
                gradient = -1 / gradientPerpendicular;

            _pointOnLine1 = pointOnLine;
            _pointOnline2 = pointOnLine + new Vector2(1, gradient);

            _approachSide = false;
            _approachSide = GetSide(pointPerpendicularToLine);
        }

        bool GetSide(Vector2 point)
        {
            return (point.x - _pointOnLine1.x) * (_pointOnline2.y - _pointOnLine1.y) >
                   (point.y - _pointOnLine1.y) * (_pointOnline2.x - _pointOnLine1.x);
        }

        public bool HasCrossedLine(Vector2 p)
        {
            return GetSide(p) != _approachSide;
        }
    }
}