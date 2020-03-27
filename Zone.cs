using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace Barnacle
{

    [Serializable]
    public class Zone
    {
        public Point3d[] vertices;
        public Line[] edges;
        public Surface surface;
        public Point3d center;
        public Vector3d[] offsetDirection;
        public double[] maxOffsetLength;
        public Polyline boundary;

        public Zone(Point3d[] vertices, Line[] edges)
        {
            this.vertices = vertices;
            this.edges = edges;
            this.boundary = new Polyline(vertices);
            center = this.boundary.CenterPoint();
            maxOffsetLength = GetMaxOffsetLength();
            offsetDirection = new Vector3d[edges.Length];
            AppendEdgeDirection();

        }

        public Zone(Point3d[] vertices)
        {
            Line[] lines = new Line[4];
            lines[0] = new Line(vertices[0], vertices[1]);
            lines[1] = new Line(vertices[1], vertices[2]);
            lines[2] = new Line(vertices[2], vertices[3]);
            lines[3] = new Line(vertices[3], vertices[0]);

            this.vertices = vertices;
            this.edges = lines;
            this.boundary = new Polyline(vertices);
            center = this.boundary.CenterPoint();
            maxOffsetLength = GetMaxOffsetLength();
            offsetDirection = new Vector3d[edges.Length];
            AppendEdgeDirection();
        }

        public Line OffsetInZone(Line referenceLine, int baseLineID, double dist)
        {

            Line newLine = referenceLine;
            newLine.Transform(Transform.Translation(Vector3d.Multiply(dist, offsetDirection[baseLineID])));
            Plane plane = new Plane(newLine.PointAt(0.5), new Vector3d(0, 0, 1));
            newLine.Transform(Transform.Scale(plane, 50, 50, 1));

            Interval intersectInterval;
            Point3d[] pointList = new Point3d[2];
            Intersection.LineBox(newLine, new BoundingBox(vertices), 0.001, out intersectInterval);
            pointList[0] = newLine.PointAt(intersectInterval.T0);
            pointList[1] = newLine.PointAt(intersectInterval.T1);
            if (pointList.Length == 2)
            {
                return new Line(pointList[0], pointList[1]);
            }
            else { throw new ArgumentException("No Intersection"); }
        }

        double[] GetMaxOffsetLength()
        {
            double[] res = new double[edges.Length];
            for (int i = 0; i < res.Length; i++)
            {
                double maxLength = 0;
                foreach (Point3d point in vertices)
                {
                    maxLength = Math.Max(maxLength, edges[i].DistanceTo(point, false));
                }
                res[i] = maxLength;
            }
            return res;
        }

        void AppendEdgeDirection()
        {
            for (int i = 0; i < edges.Length; i++)
            {
                Vector3d curveVector = new Vector3d(edges[i].To - edges[i].From);
                Point3d mid = edges[i].PointAt(0.5);
                Vector3d cen = new Vector3d(center - mid);
                Vector3d vec1 = Vector3d.CrossProduct(curveVector, new Vector3d(0, 0, 1));
                Vector3d vec2 = Vector3d.CrossProduct(curveVector, new Vector3d(0, 0, -1));
                Vector3d offset;
                if (Vector3d.Multiply(vec1, cen) > 0)
                {
                    offset = vec1;
                }
                else
                {
                   // if (Vector3d.Multiply(vec2, cen) > 0)
                   offset = vec2;
                }
                offset.Unitize();
                offsetDirection[i] = offset;
            }
        }
    }
}
