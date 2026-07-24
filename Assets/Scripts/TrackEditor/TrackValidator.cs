using System.Collections.Generic;
using UnityEngine;

namespace DrawAndRace.TrackEditor
{
    public struct TrackValidationResult
    {
        public bool IsValid;
        public string ErrorMessage;

        public static TrackValidationResult Success() => new TrackValidationResult { IsValid = true, ErrorMessage = string.Empty };
        public static TrackValidationResult Failure(string message) => new TrackValidationResult { IsValid = false, ErrorMessage = message };
    }

    /// <summary>
    /// Validates track drawings for closed-loop connectivity, absence of self-intersections (V1 rule),
    /// and applies Ramer-Douglas-Peucker curve simplification.
    /// </summary>
    public static class TrackValidator
    {
        public const int MaxPointCount = 500;

        /// <summary>
        /// Runs complete validation pipeline on a sequence of 3D world points.
        /// </summary>
        public static TrackValidationResult ValidateTrack(List<Vector3> points, float maxLoopGapMeters = 5.0f)
        {
            if (points == null || points.Count < 10)
            {
                return TrackValidationResult.Failure("Track is too short. Please draw a longer closed loop.");
            }

            // 1. Closed Loop Verification
            float startEndDistance = Vector3.Distance(points[0], points[points.Count - 1]);
            if (startEndDistance > maxLoopGapMeters)
            {
                return TrackValidationResult.Failure($"Track loop is not closed. Connect the end of your drawing to the start point (gap: {startEndDistance:F1}m).");
            }

            // 2. Self-Intersection Check (V1 Requirement)
            if (HasSelfIntersection(points))
            {
                return TrackValidationResult.Failure("Track self-intersects. Figure-eight tracks are not supported in V1.");
            }

            return TrackValidationResult.Success();
        }

        /// <summary>
        /// Detects if any non-adjacent line segments cross each other on the 2D ground plane (XZ).
        /// </summary>
        public static bool HasSelfIntersection(List<Vector3> points)
        {
            int count = points.Count;
            for (int i = 0; i < count - 1; i++)
            {
                Vector2 a1 = new Vector2(points[i].x, points[i].z);
                Vector2 a2 = new Vector2(points[i + 1].x, points[i + 1].z);

                for (int j = i + 2; j < count - 1; j++);
                // Skip adjacent segment connection at the loop endpoint
                for (int j = i + 2; j < count - 1; j++)
                {
                    if (i == 0 && j == count - 2) continue; // Start/End segment connection

                    Vector2 b1 = new Vector2(points[j].x, points[j].z);
                    Vector2 b2 = new Vector2(points[j + 1].x, points[j + 1].z);

                    if (SegmentsIntersect(a1, a2, b1, b2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool SegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float d1 = Direction(p3, p4, p1);
            float d2 = Direction(p3, p4, p2);
            float d3 = Direction(p1, p2, p3);
            float d4 = Direction(p1, p2, p4);

            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            {
                return true;
            }

            return false;
        }

        private static float Direction(Vector2 pi, Vector2 pj, Vector2 pk)
        {
            return (pk.x - pi.x) * (pj.y - pi.y) - (pj.x - pi.x) * (pk.y - pi.y);
        }

        /// <summary>
        /// Simplifies a 3D point cloud using Ramer-Douglas-Peucker algorithm and caps total points to MaxPointCount.
        /// </summary>
        public static List<Vector3> DownsamplePoints(List<Vector3> points, float tolerance = 0.5f)
        {
            if (points == null || points.Count <= 2) return points;

            List<Vector3> simplified = RamerDouglasPeucker(points, tolerance);

            // Force cap if still exceeding limit
            if (simplified.Count > MaxPointCount)
            {
                float step = (float)simplified.Count / MaxPointCount;
                List<Vector3> capped = new List<Vector3>(MaxPointCount);
                for (int i = 0; i < MaxPointCount; i++)
                {
                    int index = Mathf.Clamp(Mathf.FloorToInt(i * step), 0, simplified.Count - 1);
                    capped.Add(simplified[index]);
                }
                return capped;
            }

            return simplified;
        }

        private static List<Vector3> RamerDouglasPeucker(List<Vector3> points, float epsilon)
        {
            float maxDistance = 0f;
            int index = 0;
            int end = points.Count - 1;

            for (int i = 1; i < end; i++)
            {
                float distance = PerpendicularDistance(points[i], points[0], points[end]);
                if (distance > maxDistance)
                {
                    index = i;
                    maxDistance = distance;
                }
            }

            if (maxDistance > epsilon)
            {
                List<Vector3> recResults1 = RamerDouglasPeucker(points.GetRange(0, index + 1), epsilon);
                List<Vector3> recResults2 = RamerDouglasPeucker(points.GetRange(index, end - index + 1), epsilon);

                recResults1.RemoveAt(recResults1.Count - 1);
                recResults1.AddRange(recResults2);
                return recResults1;
            }
            else
            {
                return new List<Vector3> { points[0], points[end] };
            }
        }

        private static float PerpendicularDistance(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineDir = (lineEnd - lineStart).normalized;
            Vector3 pointDir = point - lineStart;
            Vector3 projected = Vector3.Project(pointDir, lineDir);
            return Vector3.Distance(pointDir, projected);
        }
    }
}
