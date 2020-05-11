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
        double GetClearHeight();
        string Type();
        bool IsDouble();
    }

    [Serializable]
    public class CarStallMeta : IMeta
    {
        double degree;
        public bool isDoubleRow;
        double width = 2.7;
        double length = 5.3;
        double clearHeight = 2.7;
        double clearLength = 5.3;
        int requiredConnection = 1;
        int stallCount = 1;

        public static CarStallMeta NINETY_DEGREE = new CarStallMeta(Math.PI / 2, false);
        public static CarStallMeta NINETY_DEGREE_DOUBLE = new CarStallMeta(Math.PI / 2, true);
        public static CarStallMeta SIXTY_DEGREE = new CarStallMeta(Math.PI * 1 / 3, false);
        public static CarStallMeta SIXTY_DEGREE_DOUBLE = new CarStallMeta(Math.PI * 1 / 3, true);
        public static CarStallMeta FOUTYFIVE_DEGREE = new CarStallMeta(Math.PI / 4, false);
        public static CarStallMeta FOUTYFIVE_DEGREE_DOUBLE = new CarStallMeta(Math.PI / 4, true);
        public static CarStallMeta THIRTY_DEGREE = new CarStallMeta(Math.PI / 6, false);
        public static CarStallMeta THIRTY_DEGREE_DOUBLE = new CarStallMeta(Math.PI / 6, true);
        public static CarStallMeta ZERO_DEGREE = new CarStallMeta(0, false);
        public static CarStallMeta ZERO_DEGREE_DOUBLE = new CarStallMeta(0, true);

        public static CarStallMeta[] META_LIST = new CarStallMeta[] { 
             NINETY_DEGREE_DOUBLE, NINETY_DEGREE, SIXTY_DEGREE, SIXTY_DEGREE_DOUBLE, FOUTYFIVE_DEGREE,
        FOUTYFIVE_DEGREE_DOUBLE,  THIRTY_DEGREE, THIRTY_DEGREE_DOUBLE, ZERO_DEGREE, ZERO_DEGREE_DOUBLE};

        public static CarStallMeta[] SINGLE_META_LIST = new CarStallMeta[] {
             NINETY_DEGREE, SIXTY_DEGREE, FOUTYFIVE_DEGREE,
        THIRTY_DEGREE, ZERO_DEGREE};

        public CarStallMeta(double degree, bool isDoubleRow)
        {
            this.degree = degree;
            this.isDoubleRow = isDoubleRow;
            if (isDoubleRow)
            {
                requiredConnection = 2;
                stallCount = 2;
            }
            // double tempW = width; // 2.7
            // double tempL = length; // 5.3
            if (degree == Math.PI / 2)
            {
                if (isDoubleRow)
                {
                    requiredConnection = 2;
                    stallCount = 2;
                    clearHeight = length * 2;
                    clearLength = width;
                }
                else
                {
                    clearHeight = length;
                    clearLength = width;
                }
            }
            else if (degree == 0)
            {
                if (isDoubleRow)
                {
                    requiredConnection = 2;
                    stallCount = 2;
                    clearHeight = width * 2;
                    clearLength = length;
                }
                else
                {
                    // default
                }
            }
            else
            {
                // (0, 90) degreezxxzzx
                if (isDoubleRow)
                {
                    requiredConnection = 2;
                    stallCount = 2;
                    clearHeight = Math.Cos(degree) * width + Math.Sin(degree) * length * 2;
                    clearLength = width / Math.Sin(degree);
                }
                else
                {
                    clearLength = width / Math.Sin(degree);
                    // clearLength = Math.Cos(degree) * tempW + Math.Sin(degree) * tempL; //5.3
                    clearHeight = Math.Cos(degree) * width + Math.Sin(degree) * length;
                    // clearHeight = Math.Cos(degree) * tempW; //2.7
                }

            }
            
        }
        public double GetClearHeight()
        {
            return clearHeight;
        }

        public double GetClearLength()
        {
            return clearLength;
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
                ZERO_DEGREE.GetLength(),
                ZERO_DEGREE.GetWidth());
            Vector3d move = new Vector3d(rec.Center.X - plane.Origin.X ,rec.Center.Y - plane.Origin.Y, rec.Center.Z - plane.Origin.Z);
            move.Reverse();
            rec.Transform(Transform.Translation(move));
            if (!isDoubleRow)
            {
                list.Add(rec.ToNurbsCurve());
            }
            else
            {
                // 分裂
                NurbsCurve stallUp = rec.ToNurbsCurve();
                NurbsCurve stallDown = rec.ToNurbsCurve();
                Vector3d vecUp = new Vector3d(vec);

                // 平面几何余弦定理
                Vector3d a = new Vector3d(rec.Corner(3) - rec.Corner(0));
                Vector3d b = new Vector3d((rec.Corner(2) - rec.Corner(3)));
               
                if (degree == Math.PI / 2)
                {
                    vecUp = b / 2;
                } else if (degree == 0)
                {
                    vecUp = a / 2;
                } else {
                    b.Unitize();
                    b = b * (length - width / Math.Tan(degree)); // () is the b length
                    vecUp = new Vector3d(a + b) / 2;
                }
                Vector3d vecDown;

                /*
                // dist
                double dist;
                if (degree == 0)
                {
                    dist = width / 2;
                } else if (degree == Math.PI / 2)
                {
                    dist = length / 2;
                } else
                {
                    dist = length * Math.Sin(degree) / 2;
                }
                */
                // vecUp.Unitize();
                // vecUp = new Vector3d(vecUp.X * dist, vecUp.Y*dist, vecUp.Z*dist);
                stallUp.Transform(Transform.Translation(vecUp));
                list.Add(stallUp);

                vecDown = new Vector3d(vecUp);
                vecDown.Reverse();
                stallDown.Transform(Transform.Translation(vecDown));
                list.Add(stallDown);
                
            }
            return list;
            

        }

        public override string ToString()
        {
            return "Car" + ((degree / Math.PI) * 90);
        }

        public int RequiredConnection()
        {
            return requiredConnection;
        }

        public bool IsDouble()
        {
            return isDoubleRow ;
        }

        public string Type()
        {
            return "car";
        }
    }

    [Serializable]
    public class RoadMeta : IMeta
    {
        static double width = 7;
        static double clearHeight = width;

        public static RoadMeta NORMAL_ROAD = new RoadMeta();

        public RoadMeta()
        {

        }
        public double GetClearHeight()
        {
            return clearHeight;
        }

        public bool IsDouble()
        {
           return false;
        }

        public string Type()
        {
            return "road";
        }
    }

}
