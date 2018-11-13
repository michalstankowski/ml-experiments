using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Statistics.Models.Regression.Linear;
using Deedle;

namespace ml_csharp_lesson5
{
    public class CoordinateLearner
    {
        private readonly MultipleLinearRegression _regression;
        private readonly bool _hasData;

        public CoordinateLearner(Frame<int, string> training, List<string> columns, Bin latitudeBin, Bin longitudeBin)
        {
            var filteredTraining = training;

            if(latitudeBin != null && longitudeBin != null)
                filteredTraining = training.Where(h => latitudeBin.IsInBin((decimal) h.Value["latitude"]) && longitudeBin.IsInBin((decimal) h.Value["longitude"]));

            var learner = new OrdinaryLeastSquares() { IsRobust = true };
            _hasData = filteredTraining.RowCount > 0;

            if (_hasData)
            {
                _regression = learner.Learn(
                    filteredTraining.Columns[columns].ToArray2D<double>().ToJagged(), // features
                    filteredTraining["median_house_value"].Values.ToArray());
            }
        }

        public bool HasData => _hasData;

        public double GetResult(double[] inputRow)
        {
            var prediction = _regression.Transform(inputRow);

            return prediction;
        }
    }
}
