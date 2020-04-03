using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle
{
    public interface ISolver
    {
        SolverResult Solve();
        void SortResultByMetrics();
    }


    [Serializable]
    public abstract class Solver
    {
        public Metric METRIC_FACTORY;
        public SolverResult SOLVER_RESULT_FACTORY;
        public SortedSet<RowSolverResult> resultRepository;
        public List<Solver> solvers;
        public static int BEST_RESULT_NUMBER = 5;
        public Solver() { }
        public Solver(Metric metric, SolverResult solverResult)
        {
            this.resultRepository = new SortedSet<RowSolverResult>();
            this.METRIC_FACTORY = metric;
            this.SOLVER_RESULT_FACTORY = solverResult;
        }

        public SortedSet<RowSolverResult> Solve()
        {
            /*
            foreach (Solver solver in solvers)
            {
                resultRepository.Add(solver.Solve());
            }
            */
            return resultRepository;
        }

        public static void SetMetricAndResult(Metric metric, SolverResult solverResult)
        {
            metric.solverResult = solverResult;
            solverResult.metric = metric;
        }


    }
}
