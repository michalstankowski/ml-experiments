using System;
using System.Collections.Generic;
using System.Linq;

namespace ml_csharp_lesson5
{
    public class Bin
    {
        public int Min { get; }
        public int Max { get; }

        public Bin(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public bool IsInBin(double value)
        {
            return value >= Min && value < Max;
        }

        public bool IsInBin(decimal value)
        {
            return value >= Min && value < Max;
        }

        public static IEnumerable<Bin> Bins(int start, int end)
        {
            return Enumerable.Range(start, end - start + 1)
                .Select(v => new Bin(v, v + 1));
        }

        public static int BinNumber(int start, decimal value)
        {
            return (int)Math.Floor(value) - start;
        }
    }
}
