using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle
{
    public abstract class SolverResult
    {
        public List<RowNode> result;
        public Metric metric;

        public SolverResult() : this(new List<RowNode>())
        { }
        public SolverResult(List<RowNode> result)
        {
            this.result = result;
        }

        public abstract void Add(Object rowNode);

        public SolverResult CreateNew()
        {
            return Copy.DeepClone<SolverResult>(this);
        }
    }
}
