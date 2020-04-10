using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Barnacle
{

    public interface IMeta
    {
        double GetWidth();
    }

    [Serializable]
    public class CarStallMeta : IMeta
    {
        double degree;
        bool isDoubleRow;
        double width = 2.7;
        double length = 5.3;
        int requiredConnection = 1;
        int stallCount = 1;

        public static CarStallMeta NINETY_DEGREE = new CarStallMeta(Math.PI, false);
        public static CarStallMeta NINETY_DEGREE_DOUBLE = new CarStallMeta(Math.PI, true);
        public static CarStallMeta SIXTY_DEGREE = new CarStallMeta(Math.PI * 2 / 3, false);
        public static CarStallMeta SIXTY_DEGREE_DOUBLE = new CarStallMeta(Math.PI * 2 / 3, true);
        public static CarStallMeta FOUTYFIVE_DEGREE = new CarStallMeta(Math.PI / 2, false);
        public static CarStallMeta FOUTYFIVE_DEGREE_DOUBLE = new CarStallMeta(Math.PI / 2, true);
        public static CarStallMeta THIRTY_DEGREE = new CarStallMeta(Math.PI / 3, false);
        public static CarStallMeta THIRTY_DEGREE_DOUBLE = new CarStallMeta(Math.PI / 3, true);
        public static CarStallMeta ZERO_DEGREE = new CarStallMeta(0, false);
        public static CarStallMeta ZERO_DEGREE_DOUBLE = new CarStallMeta(0, true);

        public static CarStallMeta[] META_LIST = new CarStallMeta[] { 
             NINETY_DEGREE_DOUBLE, NINETY_DEGREE, SIXTY_DEGREE, SIXTY_DEGREE_DOUBLE, FOUTYFIVE_DEGREE,
        FOUTYFIVE_DEGREE_DOUBLE,  THIRTY_DEGREE, THIRTY_DEGREE_DOUBLE, ZERO_DEGREE, ZERO_DEGREE_DOUBLE};

        public CarStallMeta(double degree, bool isDoubleRow)
        {
            this.degree = degree;
            this.isDoubleRow = isDoubleRow;
            if (isDoubleRow)
            {
                requiredConnection = 2;
                stallCount = 2;
                double tempW = width;
                double tempL = length;
                if (degree == Math.PI)
                {
                    width = length;
                }
                else
                {
                    width = Math.Cos(degree) * tempW + Math.Sin(degree) * tempL;
                    length = Math.Cos(degree) * tempW;
                }
            }
        }
        public double GetWidth()
        {
            return width;
        }
        
        public double GetLength()
        {
            return length;
        }

        public double GetDegree()
        {
            return degree;
        }

        public List<GeometryBase> Draw(Plane plane, Vector3d vec)
        {
            List<GeometryBase> list = new List<GeometryBase>();
            Rectangle3d rec = new Rectangle3d(
                plane,
                ZERO_DEGREE.GetWidth(),
                ZERO_DEGREE.GetLength());
            Vector3d move = new Vector3d(rec.Center.X - plane.Origin.X ,rec.Center.Y - plane.Origin.Y, rec.Center.Z - plane.Origin.Z);
            move.Reverse();
            rec.Transform(Transform.Translation(move));
            if (!isDoubleRow)
            {
                list.Add(rec.ToNurbsCurve());
            }
            else
            {
                NurbsCurve stallUp = rec.ToNurbsCurve();
                NurbsCurve stallDown = rec.ToNurbsCurve();
                Vector3d vecUp = new Vector3d();
                Vector3d vecDown;
                double dist = width / 2;

                vecUp.Unitize();
                vecUp = new Vector3d(vecUp.X * dist, vecUp.Y*dist, vecUp.Z*dist);
                stallUp.Transform(Transform.Translation(vecUp));
                list.Add(stallUp);

                vecDown = new Vector3d(vecUp);
                vecDown.Reverse();
                stallDown.Transform(Transform.Translation(vecDown));
                list.Add(stallDown);
            }
            return list;
            

        }
        public string ToString()
        {
            return "Car" + ((degree / Math.PI) * 90);
        }

        public int RequiredConnection()
        {
            return requiredConnection;
        }
    }

    [Serializable]
    public class RoadMeta : IMeta
    {
        static double width = 7;

        public static RoadMeta NORMAL_ROAD = new RoadMeta();

        public RoadMeta()
        {

        }
        public double GetWidth()
        {
            return width;
        }
    }

}
