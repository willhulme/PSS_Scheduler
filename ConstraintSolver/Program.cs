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
            // Data.
            int[,] costs = {
            { 90, 80, 75, 70 }, { 35, 85, 55, 65 }, { 125, 95, 90, 95 }, { 45, 110, 95, 115 }, { 50, 100, 90, 100 },
        };
            int numWorkers = costs.GetLength(0);
            int numTasks = costs.GetLength(1);

            // Model.
            CpModel model = new CpModel();

            // Variables.
            IntVar[,] x = new IntVar[numWorkers, numTasks];
            // Variables in a 1-dim array.
            IntVar[] xFlat = new IntVar[numWorkers * numTasks];
            int[] costsFlat = new int[numWorkers * numTasks];
            for (int i = 0; i < numWorkers; ++i)
            {
                for (int j = 0; j < numTasks; ++j)
                {
                    x[i, j] = model.NewIntVar(0, 1, $"worker_{i}_task_{j}");
                    int k = i * numTasks + j;
                    xFlat[k] = x[i, j];
                    costsFlat[k] = costs[i, j];
                }
            }

            // Constraints
            // Each worker is assigned to at most one task.
            for (int i = 0; i < numWorkers; ++i)
            {
                IntVar[] vars = new IntVar[numTasks];
                for (int j = 0; j < numTasks; ++j)
                {
                    vars[j] = x[i, j];
                }
                model.Add(LinearExpr.Sum(vars) <= 1);
            }

            // Each task is assigned to exactly one worker.
            for (int j = 0; j < numTasks; ++j)
            {
                IntVar[] vars = new IntVar[numWorkers];
                for (int i = 0; i < numWorkers; ++i)
                {
                    vars[i] = x[i, j];
                }
                model.Add(LinearExpr.Sum(vars) == 1);
            }

            // Objective
            model.Minimize(LinearExpr.ScalProd(xFlat, costsFlat));

            // Solve
            CpSolver solver = new CpSolver();
            CpSolverStatus status = solver.Solve(model);
            Console.WriteLine($"Solve status: {status}");

            // Print solution.
            // Check that the problem has a feasible solution.
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                Console.WriteLine($"Total cost: {solver.ObjectiveValue}\n");
                for (int i = 0; i < numWorkers; ++i)
                {
                    for (int j = 0; j < numTasks; ++j)
                    {
                        if (solver.Value(x[i, j]) > 0.5)
                        {
                            Console.WriteLine($"Worker {i} assigned to task {j}. Cost: {costs[i, j]}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No solution found.");
            }

            Console.WriteLine("Statistics");
            Console.WriteLine($"  - conflicts : {solver.NumConflicts()}");
            Console.WriteLine($"  - branches  : {solver.NumBranches()}");
            Console.WriteLine($"  - wall time : {solver.WallTime()}s");
        }
    }
}
