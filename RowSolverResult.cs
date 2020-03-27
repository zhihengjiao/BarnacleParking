using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle
{
    public class RowSolverResult : SolverResult
    {
        // public List<RowNode> result;
        // public Metric metric;
        public double totalWidth;

        public RowSolverResult() : this(new List<RowNode>())
        { }
        public RowSolverResult(List<RowNode> result) : base(result)
        {
            this.totalWidth = 0;
        }

        public void Add(RowNode rowNode)
        {
            result.Add(rowNode);
            totalWidth += rowNode.GetWidth();
        }

        public override void Add(object rowNode)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }


    }

}
