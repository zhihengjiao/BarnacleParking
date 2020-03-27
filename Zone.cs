using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle
{
    public class Zone
    {
        public Point[] vertices;
        public Line[] edges;
        public Surface surface;
        public Point center;
        public Vector[] offsetDirection;
        public double[] maxOffsetLength;
        public Polygon boundary;

        public Zone(Point[] vertices, Line[] edges, Surface surface)
        {
            this.vertices = vertices;
            this.edges = edges;
            this.surface = surface;
            this.boundary = Polygon.ByPoints(vertices);
            center = Polygon.ByPoints(vertices).Center();
            maxOffsetLength = GetMaxOffsetLength();
            offsetDirection = new Vector[edges.Length];
            AppendEdgeDirection();

        }

        public Line OffsetInZone(Line referenceLine, int baseLineID, double dist)
        {

            Line newLine = (Line)referenceLine.Translate(offsetDirection[baseLineID], dist);
            Plane plane = Plane.ByOriginNormal(newLine.PointAtParameter(0.5), Vector.ByCoordinates(0, 0, 1));
            Point[] pointList = (Point[])newLine.Scale(plane, 50, 50, 1).Intersect(boundary);
            if (pointList.Length == 2)
            {
                return Line.ByStartPointEndPoint(pointList[0], pointList[1]);
            }
            else { return null; }
        }

        double[] GetMaxOffsetLength()
        {
            double[] res = new double[edges.Length];
            for (int i = 0; i < res.Length; i++)
            {
                double maxLength = 0;
                foreach (Point point in vertices)
                {
                    maxLength = Math.Max(maxLength, point.DistanceTo(edges[i]));
                }
                res[i] = maxLength;
            }
            return res;
        }

        void AppendEdgeDirection()
        {
            for (int i = 0; i < edges.Length; i++)
            {
                Vector curveVector = Vector.ByTwoPoints(edges[i].StartPoint, edges[i].EndPoint);
                Point mid = edges[i].PointAtParameter(0.5);
                Vector cen = Vector.ByTwoPoints(mid, center);
                Vector vec1 = curveVector.Cross(Vector.ByCoordinates(0, 0, 1));
                Vector vec2 = curveVector.Cross(Vector.ByCoordinates(0, 0, -1));
                Vector offset = null;
                if (vec1.Dot(cen) > 0)
                {
                    offset = vec1;
                }
                else if (vec2.Dot(cen) > 0)
                {
                    offset = vec2;
                }
                offsetDirection[i] = offset;
            }
        }
    }
}
