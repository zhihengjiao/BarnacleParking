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
    public class RowSolver : Solver
    {
        public SortedSet<RowSolverResult> resultRepository;
        public Zone zone;
        List<List<IMeta>> permutation = new List<List<IMeta>>();
        public String log = "";
        public RowSolver(Metric metric, RowSolverResult RowSolverResult) : base(metric, RowSolverResult)
        {
            resultRepository = new SortedSet<RowSolverResult>();
        }

        public RowSolver WithZone(Zone newZone)
        {
            this.zone = newZone;
            return this;
        }

        

        /*
        public void Solve()
        {
            // for each ref edge
            for (int i = 0; i < zone.edges.Length; i++)
            {
                List<List<IMeta>> permutation = new List<List<IMeta>>();
                // permute
                // init
                Line refLine = zone.edges[i];
                double offsetBound = zone.maxOffsetLength[i];
                
                List<Line> rows = new List<Line>();
                List<double> totalWidth = new List<double>();
                double curOffset = 0;
                bool shouldBeCarRow = true;
                List<IMeta> curPermutation = new List<IMeta>();
                while (offsetBound < curOffset + CarStallMeta.ZERO_DEGREE.GetWidth())
                {
                    if (shouldBeCarRow)
                    {
                        for (int c = 0; c < CarStallMeta.META_LIST.Length; c++)
                        {
                            CarStallMeta firstRow = CarStallMeta.META_LIST[c];
                            if (firstRow.GetWidth() > offsetBound)
                            {
                                break;
                            }
                            else
                            {
                                // getting the middle line
                                Line highLine = zone.OffsetInZone(refLine, i, firstRow.GetWidth());
                                Line lowLine = refLine;
                                Line middleLine = midLine(lowLine, highLine);
                                // add to list
                                rows.Add(middleLine);
                                curPermutation.Add(firstRow);
                                permutation.Add(firstRow);
                            }
                        }
                }
                   
                }
                // more rows
                
                for (int c = 0; c < CarStallMeta.META_LIST.Length; c++)
                {

                }
            }
        }

        public void DFS(int baseLineID, List<IMeta> curPermutation, bool shouldBeCarRow, double curOffset)
        {
            // base case
            if (zone.maxOffsetLength[baseLineID] > curOffset)
            {
                curPermutation.RemoveAt(curPermutation.Count - 1);
                permutation.Add(curPermutation);
            }
            // recursive
            if (shouldBeCarRow)
            {
                for (int c = 0; c < CarStallMeta.META_LIST.Length; c++)
                {
                    CarStallMeta firstRow = CarStallMeta.META_LIST[c];
                    if (firstRow.GetWidth() > zone.maxOffsetLength[baseLineID])
                    {
                        break;
                    }
                    else
                    {
                        // getting the middle line
                        Line highLine = zone.OffsetInZone(refLine, i, firstRow.GetWidth());
                        Line lowLine = refLine;
                        Line middleLine = midLine(lowLine, highLine);
                        // add to list
                        rows.Add(middleLine);
                        curPermutation.Add(firstRow);
                        permutation.Add(firstRow);
                    }
                }

            }
        */
        
        public new SortedSet<RowSolverResult> Solve()
        {
            for (int i = 0; i < zone.edges.Length; i++)
            {
                DFS(i);
            }
            return resultRepository;
        }
        

        void DFS(int baseLineID)
        {
            //foreach (CarStallMeta carMeta in CarStallMeta.META_LIST)
            
                CarStallRow c = new CarStallRow(baseLineID,
                zone.edges[baseLineID],
                CarStallMeta.NINETY_DEGREE, zone);
                // RoadRow r = new RoadRow(baseLineID,  zone.edges[baseLineID], RoadMeta.NORMAL_ROAD);

                // create new result and metric
                RowSolverResult branchC = (RowSolverResult)this.SOLVER_RESULT_FACTORY.CreateNew();
                // RowSolverResult branchR = (RowSolverResult) this.SOLVER_RESULT_FACTORY.CreateNew();
                Metric mC = this.METRIC_FACTORY.CreateNew();
                // Metric mR = this.METRIC_FACTORY.CreateNew();
                SetMetricAndResult(mC, branchC);
                // SetMetricAndResult(mR, branchR);


                RowNode startC = GrowNode(null, c, branchC);
                // RowNode startR = GrowNode(null, r, branchR);
                //  RowNode startC = c;
                // RowNode startR = r;
                branchC.Add(startC);
                //  branchR.Add(startR);

                Grow(startC, branchC, baseLineID);
                // Grow(startR, branchR, baseLineID);

            
        }


        RowNode GrowNode(RowNode node, RowNode newNode, RowSolverResult branch)
        {
            Console.WriteLine("GrowNode\n");
            // node is Road && newNode is CarStall
            // For adding connection to newNode
            if (node != null)
            {
                if (node is RoadRow && newNode is CarStallRow)
                {
                    newNode = (CarStallRow)newNode;
                    // newNode.AddConnection();
                }
            }
            newNode.prev = node;

            // add new Node
            branch.Add(newNode);

            return newNode;
        }

        bool IsLegalAddition(RowNode node)
        {
            if (node is CarStallRow)
            {
                if (!node.prev.IsConnectedToRoadRow())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        void Grow(RowNode node, RowSolverResult branch, int baseLineID)
        {
            // reach end.. base case
            if (branch.totalWidth > zone.maxOffsetLength[baseLineID])
            {
                branch.StepBack();
                resultRepository.Add(branch);
                return;
            }

            // recursive
            // add RoadRow
            if (node != null && node is CarStallRow)
            {
                RoadRow r = new RoadRow(
                            baseLineID,
                            zone.OffsetInZone(node.referenceLine, baseLineID, node.GetWidth()),
                            RoadMeta.NORMAL_ROAD,
                            zone
                            );

                RowSolverResult newBranch =branch.Clone();
                RowNode newNode = GrowNode(node, r, newBranch);
                Grow(newNode, newBranch, baseLineID);
            }
            // add CarRow
            else
            {
                foreach (CarStallMeta carMeta in CarStallMeta.META_LIST)
                {
                    CarStallRow c = new CarStallRow(
                                baseLineID,
                                zone.OffsetInZone(node.referenceLine, baseLineID, node.GetWidth()),
                                carMeta,
                                zone
                                );
                    // Grow a Car Branch
                    RowSolverResult carBranch = branch.Clone();
                    RowNode carNode = GrowNode(node, c, carBranch);
                    Grow(carNode, carBranch, baseLineID);

                }
            }
            









            /* OLD IMPLEMENTATION
            foreach (CarStallMeta carMeta in CarStallMeta.META_LIST)
            {

                // If exceed the maxWidth(base case)
                if (branch.totalWidth + carMeta.GetWidth() >= zone.maxOffsetLength[baseLineID])
                {
                    resultRepository.Add(branch);
                    continue;
                    if (!IsLegalAddition(node))
                    {
                        continue;
                    }
                    else
                    {
                        resultRepository.Add(branch);
                        
                        if (resultRepository.Count >= BEST_RESULT_NUMBER)
                        {
                            resultRepository.Remove(resultRepository.Min);
                        }
                        
                        continue;
                    }
                }

                // recursive case
                // if node.referenceLine is None:
                // self.resultRepository.append(branch)
                // return
                

                // if this node is CarStall
                if (node is CarStallRow)
                {
                    // not connected to Road
                    if (!node.IsConnectedToRoadRow())
                    {
                        RoadRow r = new RoadRow(
                            baseLineID,
                            zone.OffsetInZone(node.referenceLine, baseLineID, node.GetWidth()),
                            RoadMeta.NORMAL_ROAD
                            );

                        RowSolverResult newBranch = Copy.DeepClone(branch);
                        RowNode newNode = GrowNode(node, r, newBranch);
                        Grow(newNode, newBranch, baseLineID);
                    }
                    // if connected to Road
                    else
                    {
                        CarStallRow c = new CarStallRow(
                                baseLineID,
                                zone.OffsetInZone(node.referenceLine, baseLineID, node.GetWidth()),
                                carMeta
                                );

                        RoadRow r = new RoadRow(
                                baseLineID,
                                zone.OffsetInZone(node.referenceLine, baseLineID, node.GetWidth()),
                                RoadMeta.NORMAL_ROAD
                                );

                        // Grow a Car Branch
                        RowSolverResult carBranch = Copy.DeepClone(branch);
                        RowNode carNode = GrowNode(node, c, carBranch);
                        Grow(carNode, carBranch, baseLineID);

                        // Grow a Road Branch
                        RowSolverResult roadBranch = Copy.DeepClone(branch);
                        RowNode roadNode = GrowNode(node, c, roadBranch);
                        Grow(roadNode, roadBranch, baseLineID);
                    }
                }
                // if this node is a Road
                else if (node is RoadRow)
                {
                    CarStallRow c = new CarStallRow(
                                baseLineID,
                                zone.OffsetInZone(node.referenceLine, baseLineID, node.GetWidth()),
                                carMeta
                                );
                    RowSolverResult newBranch = Copy.DeepClone(branch);
                    RowNode newNode = GrowNode(node, c, newBranch);
                    Grow(newNode, newBranch, baseLineID);

                }
            }
            */
        }

        public RowSolverResult GetBest(int i)
        {
            if (i >= resultRepository.Count())
            {
                return null;
            }
            
            foreach (RowSolverResult cur in resultRepository)
            {
                double curValue = cur.CalculateTotalStall();
            }
            // double max = resultRepository.ElementAt(i).CalculateTotalStall();
            RowSolverResult res = resultRepository.Max;
            writeLog(res.endNode);
            return res;
        }

        public void writeLog(RowNode node)
        {
            RowNode cur = node;
            while (cur != null)
            {
                log += cur.ToString();
                log += ", ";
                cur = cur.prev;
            }
        }


    }

}
