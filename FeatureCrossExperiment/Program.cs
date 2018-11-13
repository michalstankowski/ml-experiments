using Accord.Math;
using Accord.Math.Optimization.Losses;
using Deedle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FeatureCrossExperiment
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Loading data....");
            var path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\california_housing.csv"));
            var housing = Frame.ReadCsv(path, separators: ",");
            housing = housing.Where(kv => ((decimal)kv.Value["median_house_value"]) < 500000);

            // shuffle row indices
            var rnd = new Random();
            var indices = Enumerable.Range(0, housing.Rows.KeyCount).OrderBy(v => rnd.NextDouble());

            // shuffle the frame using the indices
            housing = housing.IndexRowsWith(indices).SortRowsByKey();

            // convert the house value range to thousands
            housing["median_house_value"] /= 1000;

            // create the rooms_per_person feature
            housing.AddColumn("rooms_per_person",
               (housing["total_rooms"] / housing["population"]).Select(v => v.Value <= 4.0 ? v.Value : 4.0));



            // create training, validation, and test frames
            var training = housing.Rows[Enumerable.Range(0, 12000)];
            var validation = housing.Rows[Enumerable.Range(12000, 2500)];
            var test = housing.Rows[Enumerable.Range(14500, 2500)];

            //Setup of learner matrix
            var columns = new List<string>();

            columns.Add("median_income");
            columns.Add("rooms_per_person");

            var learnerMatrix = new LearnerMatrix(training, columns);

            //Verification of results
            DisplayTrainingResults("Training data", learnerMatrix, training);
            DisplayTrainingResults("Validation data", learnerMatrix, validation);
            DisplayTrainingResults("Test data", learnerMatrix, test);

            Console.ReadLine();
        }

        private static void DisplayTrainingResults(string name, LearnerMatrix learnerMatrix,
            Frame<int, string> testData)
        {
            Console.WriteLine($"{name}:");

            var labels = testData["median_house_value"].Values.ToArray();
            var predictions = learnerMatrix.Transform(testData);

            var rmse = Math.Sqrt(new SquareLoss(labels).Loss(predictions));
            var mae = new AbsoluteLoss(labels.ToJagged()).Loss(predictions.ToJagged()) / labels.Length;

            var range = Math.Abs(labels.Max() - labels.Min());
            Console.WriteLine($"Label range: {range}");
            Console.WriteLine($"RMSE:        {rmse} {rmse / range * 100:0.00}%");
            Console.WriteLine($"MAE:        {mae} {mae / range * 100:0.00}%");
            Console.WriteLine();
        }
    }
}
