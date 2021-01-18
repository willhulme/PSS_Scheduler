using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.Sat;

namespace ConsoleApp4
{
    public class Program
    {
        static void Main()
        {
            // Data from SQL.
            int[,] kits = {
            { 900, 800, 750, 700 }, { 350, 850, 550, 650 }, { 1250, 950, 900, 950 }, { 450, 1100, 950, 1150 }, { 500, 1000, 900, 1000 },
        };
            int numWorkers = kits.GetLength(0);
            int numJobs = kits.GetLength(1);

            // Model.
            CpModel cpModel = new CpModel();

            // Variables.
            IntVar[,] x = new IntVar[numWorkers, numJobs];
            // Variables in a 1-dim array.
            IntVar[] xFlat = new IntVar[numWorkers * numJobs];
            int[] costsFlat = new int[numWorkers * numJobs];
            for (int i = 0; i < numWorkers; ++i)
            {
                for (int j = 0; j < numJobs; ++j)
                {
                    x[i, j] = cpModel.NewIntVar(0, 1, $"worker_{i}_task_{j}");
                    int k = i * numJobs + j;
                    xFlat[k] = x[i, j];
                    costsFlat[k] = kits[i, j];
                }
            }

            // The Constraints:
            // Each worker is assigned to at most one task.
            for (int i = 0; i < numWorkers; ++i)
            {
                IntVar[] vars = new IntVar[numJobs];
                for (int j = 0; j < numJobs; ++j)
                {
                    vars[j] = x[i, j];
                }
                cpModel.Add(LinearExpr.Sum(vars) <= 1);
            }

            // Each task is assigned to exactly one worker.
            for (int j = 0; j < numJobs; ++j)
            {
                IntVar[] vars = new IntVar[numWorkers];
                for (int i = 0; i < numWorkers; ++i)
                {
                    vars[i] = x[i, j];
                }
                cpModel.Add(LinearExpr.Sum(vars) == 1);
            }

            // Objective
            cpModel.Minimize(LinearExpr.ScalProd(xFlat, costsFlat));

            // Solve
            CpSolver cpSolver = new CpSolver();
            CpSolverStatus status = cpSolver.Solve(cpModel);
            Console.WriteLine($"Solve status: {status}");

            // Print solution.
            // Check that the problem has a feasible solution.
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                Console.WriteLine($"Total cost: {cpSolver.ObjectiveValue}\n");
                for (int i = 0; i < numWorkers; ++i)
                {
                    for (int j = 0; j < numJobs; ++j)
                    {
                        if (cpSolver.Value(x[i, j]) > 0.5)
                        {
                            Console.WriteLine($"Worker {i} assigned to kit {j}. Cost: {kits[i, j]}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No solution could be found.");
            }

            Console.WriteLine("Statistics");
            Console.WriteLine($"  conflicts : {cpSolver.NumConflicts()}");
            Console.WriteLine($"  branches  : {cpSolver.NumBranches()}");
            Console.WriteLine($"  wall time : {cpSolver.WallTime()}s");
        }
    }
}
