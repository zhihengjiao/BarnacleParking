using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Barnacle
{
    [Serializable]
    public abstract class RowNode
    {
        public RowNode next;
        public RowNode prev;
        public IMeta metaItem;
        public Line referenceLine;
        public int baseLineID;
        public Zone zone;
        public static String name;

        public Line highLine;
        public Line lowLine;
        public Line middleLine;



        public RowNode(int baseLineID, Line referenceLine, IMeta metaItem, Zone zone)
        {
            this.baseLineID = baseLineID;
            this.referenceLine = referenceLine;
            this.metaItem = metaItem;
            this.zone = zone;
            name = "RowNode";
            next = null;
            prev = null;

            highLine = zone.OffsetInZone(referenceLine, baseLineID, metaItem.GetWidth());
            lowLine = referenceLine;
            middleLine = midLine(lowLine, highLine);
        }

        public static Line midLine(Line a, Line b)
        {
            /*
            Point3d start = new Point3d(
                (a.FromX + b.FromX) / 2,
                (a.FromY + b.FromY) / 2,
                (a.FromZ + b.FromZ) / 2);
            Point3d end = new Point3d(
                (a.ToX + b.ToX) / 2,
                (a.ToY + b.ToY) / 2,
                (a.ToZ + b.ToZ) / 2);
             */
            if (a.Length > b.Length)
            {
                return midLine(b, a);
            }
            Point3d midFrom = a.PointAt(0.5);
            Point3d midTo = b.ClosestPoint(midFrom, true);
            Vector3d dir = new Vector3d(
              (midTo.X - midFrom.X) / 2,
              (midTo.Y - midFrom.Y) / 2,
              (midTo.Z - midFrom.Z) / 2);
            Line mid = new Line(a.From, a.To);
            mid.Transform(Transform.Translation(dir));

            return mid;
        }

        public double GetWidth()
        {
            return metaItem.GetWidth();
        }

        public int GetBaseLineID()
        {
            return baseLineID;
        }

        public Line GetReferenceLine()
        {
            return referenceLine;
        }

        public double GetLineLength()
        {
            return referenceLine.Length;
        }

        public abstract void AddConnection();

        public abstract bool IsConnectedToRoadRow();

        public abstract List<GeometryBase> Draw();
    }


    [Serializable]
    class CarStallRow : RowNode
    {
        int requiredConnection;
        public CarStallRow(int baseLineID, Line referenceLine, CarStallMeta metaItem, Zone zone) :
            base(baseLineID, referenceLine, metaItem, zone)
        {
            requiredConnection = metaItem.RequiredConnection();
            name = "CarStallRow";
        }

        public override void AddConnection()
        {
            requiredConnection -= 1;
        }

        public override List<GeometryBase> Draw()
        {
            List<GeometryBase> list = new List<GeometryBase>();
            CarStallMeta c = (CarStallMeta)metaItem;
            MessageBox.Show(c.ToString());
            double[] divideParam = middleLine.ToNurbsCurve().DivideByLength(c.GetWidth(), false);
            foreach (double p in divideParam)
            {
                Plane plane = new Plane(middleLine.PointAt(p), Vector3d.ZAxis);
                plane.Rotate(c.GetDegree(), Vector3d.ZAxis);

                list.AddRange(c.Draw(plane, zone.offsetDirection[baseLineID]));
            }
            return list;
        }

        public override bool IsConnectedToRoadRow()
        {
            return requiredConnection <= 0;
        }



        public override string ToString()
        {
            return "CarStallRow" + this.metaItem.ToString();
        }
    }


    [Serializable]
    class RoadRow : RowNode
    {

        public RoadRow(int baseLineID, Line referenceLine, RoadMeta metaItem, Zone zone) :
            base(baseLineID, referenceLine, metaItem, zone)
        {
            name = "RoadRow";
        }

        public override void AddConnection()
        {
            
        }

        public override List<GeometryBase> Draw()
        {
            List<GeometryBase> list = new List<GeometryBase>();
            list.Add(highLine.ToNurbsCurve());
            list.Add(lowLine.ToNurbsCurve());
            return list;
        }

        public override bool IsConnectedToRoadRow()
        {
            return true;
        }

        public override string ToString()
        {
            return "RoadRow";
        }
    }


}
