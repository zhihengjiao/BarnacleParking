using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Barnacle
{
    [Serializable]
    public abstract class SolverResult : IComparable
    {
        public List<RowNode> result;
        public Metric metric;

        public SolverResult() : this(new List<RowNode>())
        {
            
        }
        public SolverResult(List<RowNode> result)
        {
            this.result = result;
        }

        public void Add(RowNode rowNode)
        {
            result.Add(rowNode);
        }

        public int CompareTo(object obj)
        {
            SolverResult other = (SolverResult)obj;
            return this.metric.CompareTo(other.metric);
        }

        public SolverResult CreateNew()
        {
            return Copy.DeepClone<SolverResult>(this);
        }

        public abstract List<GeometryBase> Draw();
    }
}
