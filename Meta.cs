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
        public static CarStallMeta SIXTY_DEGREE = new CarStallMeta(Math.PI * 2 / 3, false);
        public static CarStallMeta SIXTY_DEGREE_DOUBLE = new CarStallMeta(Math.PI * 2 / 3, true);

        public static CarStallMeta[] META_LIST = new CarStallMeta[] { NINETY_DEGREE, SIXTY_DEGREE, SIXTY_DEGREE_DOUBLE };
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
