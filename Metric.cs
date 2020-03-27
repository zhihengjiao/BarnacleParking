using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle
{
    public abstract class Metric
    {
        protected double metricValue;
        public SolverResult solverResult;

        public Metric() : this(0)
        {

        }

        public Metric(double metricValue)
        {
            this.metricValue = metricValue;
        }

        public Metric CreateNew()
        {
            return Copy.DeepClone<Metric>(this);
        }

        public abstract int CompareTo(Object other);

        public bool SetValue(double value)
        {
            metricValue = value;
            return true;
        }

        public double value()
        {
            return this.metricValue;
        }

        public abstract void Calculate();

    }



    public class StallCountMetric : Metric
    {
        public new RowSolverResult solverResult;
        public StallCountMetric() : base() { }

        public override void Calculate()
        {
            this.metricValue = 0;
            foreach (RowNode node in solverResult.result)
            {
                if (node is CarStallRow)
                {
                    CarStallRow carNode = ((CarStallRow)node);
                    double temp = carNode.GetLineLength() / carNode.GetWidth();
                    this.metricValue += temp;
                }
            }
        }

        public override int CompareTo(object other)
        {
            if (!(other is StallCountMetric))
            {
                throw new ArgumentException();

            }
            double res = this.metricValue - ((StallCountMetric)other).metricValue;
            if (res == 0) return 0;
            else if (res > 0) return 1;
            else return -1;
        }


    }
}
