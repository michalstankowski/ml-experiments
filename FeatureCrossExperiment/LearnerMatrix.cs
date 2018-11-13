using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Deedle;
using ml_csharp_lesson5;

namespace FeatureCrossExperiment
{
    public class LearnerMatrix
    {
        private readonly Bin[] _latitudeBins;
        private readonly Bin[] _longitudeBins;
        private readonly CoordinateLearner[,] _learners;
        private readonly CoordinateLearner _generalLearner;
        private readonly List<string> _columns;

        public LearnerMatrix(Frame<int, string> training, List<string> columns)
        {
            _latitudeBins = Bin.Bins(32, 43).ToArray();
            _longitudeBins = Bin.Bins(-125, -114).ToArray();

            _learners = new CoordinateLearner[_latitudeBins.Length, _longitudeBins.Length];
            _columns = columns;

            for (var i = 0; i < _latitudeBins.Length; i++)
                for (var j = 0; j < _longitudeBins.Length; j++)
                    _learners[i, j] = new CoordinateLearner(training, columns, _latitudeBins[i], _longitudeBins[j]);

            _generalLearner = new CoordinateLearner(training, columns, null, null);
        }

        public double[] Transform(Frame<int, string> data)
        {
            var dataArray = data.Columns[_columns].ToArray2D<double>().ToJagged();
            var coordinatesArray = data.Columns[new [] { "latitude", "longitude"}].ToArray2D<double>().ToJagged();
            var results = new double[dataArray.Length];

            for (var row = 0; row < dataArray.Length; row++)
            {
                var coordinates = coordinatesArray[row];
                var dataRow = dataArray[row];

                var learner = GetLearner(coordinates[0], coordinates[1]);

                results[row] = learner.GetResult(dataRow);
            }

            return results;
        }

        private CoordinateLearner GetLearner(double latitude, double longitude)
        {
            int latitudeBinIndex = -1;

            for(var i = 0; i < _latitudeBins.Length; i++)
                if (_latitudeBins[i].IsInBin(latitude))
                {
                    latitudeBinIndex = i;
                    break;
                }

            if (latitudeBinIndex >= 0)
            {
                var longitudeIndex = -1;

                for(var j = 0; j < _latitudeBins.Length; j++)
                    if (_longitudeBins[j].IsInBin(longitude))
                    {
                        longitudeIndex = j;
                        break;
                    }

                if (longitudeIndex >= 0)
                {
                    var learner = _learners[latitudeBinIndex, longitudeIndex];

                    if (learner.HasData)
                        return learner;
                }
                    
            }

            return _generalLearner;
        }
    }
}
