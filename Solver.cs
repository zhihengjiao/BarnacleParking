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
        public readonly Metric METRIC_FACTORY;
        public readonly SolverResult SOLVER_RESULT_FACTORY;
        public List<SolverResult> resultRepository;
        public List<Solver> solvers;
        public Solver() { }
        public Solver(Metric metric, SolverResult solverResult)
        {
            this.resultRepository = new List<SolverResult>();
            this.METRIC_FACTORY = metric;
            this.SOLVER_RESULT_FACTORY = solverResult;
        }

        public List<SolverResult> Solve()
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
