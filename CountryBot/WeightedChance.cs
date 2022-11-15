using System;
using System.Collections.Generic;
using System.Linq;

namespace CountryBot
{
    public class WeightedChanceParam
    {
        public Action Func { get; }
        public double Ratio { get; }

        public WeightedChanceParam(Action func, double ratio)
        {
            Func = func;
            Ratio = ratio;
        }
    }
    public class WeightedChanceExecutor
    {
        public List<WeightedChanceParam> Parameters { get; }
        private Random r;

        public double RatioSum
        {
            get { return Parameters.Sum(p => p.Ratio); }
        }

        public WeightedChanceExecutor(List<WeightedChanceParam> parameters)
        {
            Parameters = parameters;
            r = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Execute()
        {
            double numericValue = r.NextDouble() * RatioSum;

            foreach (var parameter in Parameters)
            {
                numericValue -= parameter.Ratio;

                if (numericValue > 0)
                    continue;

                parameter.Func();
                return;
            }

        }
    }
}
