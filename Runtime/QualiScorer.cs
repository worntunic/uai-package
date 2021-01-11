using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UAI.AI
{
    public struct WeightedScorer
    {
        public IScorable scorer;
        private float _weight;
        public float Weight
        {
            get { return _weight; } 
            /*set { _weight = Mathf.Clamp01(value); }*/
            set { _weight = value; }
        }
        public WeightedScorer(IScorable scorer, float weight)
        {
            this.scorer = scorer;
            _weight = 1;
            Weight = weight;
        }

        public float Evaluate(Context context)
        {
            return Weight * scorer.Evaluate(context);
        }
    }

    public class QualiScorer : IScorable
    {
        public enum QualiType
        {
            SumOfChildren, AllOrNothing, Fixed, SumIfAboveThreshold, InvertedSumOfChildren
        }
        public string guid;
        public QualiType type;
        private Func<Context, float> evalMethod;
        public float threshold;
        List<WeightedScorer> weightedScorers;
        public static event Action<string, string, float> OnEvaluation;

        public QualiScorer()
        {
            weightedScorers = new List<WeightedScorer>();
            type = QualiType.SumOfChildren;
            SelectEvalMethod();
        }

        public QualiScorer(QualiType type)
        {
            weightedScorers = new List<WeightedScorer>();
            this.type = type;
            SelectEvalMethod();
        }

        public void AddScorer(IScorable scorer, float weight)
        {
            weightedScorers.Add(new WeightedScorer(scorer, weight));
        }
        public void RemoveScorer(IScorable scorer)
        {
            weightedScorers.RemoveAll(ws => ws.scorer == scorer);
        }
        public float Evaluate(Context context)
        {
            float value = evalMethod(context);
            OnEvaluation?.Invoke(context.aiGuid, guid, value);
            return value;
        }

        private void SelectEvalMethod()
        {
            switch (type)
            {
                case QualiType.AllOrNothing:
                    evalMethod = EvalAllOrNothing;
                    break;
                case QualiType.Fixed:
                    evalMethod = EvalFixed;
                    break;
                case QualiType.SumOfChildren:
                    evalMethod = EvalSumOfChildren;
                    break;
                case QualiType.SumIfAboveThreshold:
                    evalMethod = EvalSumIfAbove;
                    break;
                case QualiType.InvertedSumOfChildren:
                    evalMethod = EvalInvertedSum;
                    break;
            }
        }

        public float EvalAllOrNothing(Context context)
        {
            float sum = 0;
            float weightSum = 0;
            for (int i = 0; i < weightedScorers.Count; i++)
            {
                float value = weightedScorers[i].Evaluate(context);
                if (value < threshold) { return 0f; }
                sum += value;
                weightSum += weightedScorers[i].Weight;
            }

            sum /= weightSum;
            return sum;
        }

        public float EvalFixed(Context context)
        {
            return threshold;
        }

        public float EvalSumOfChildren(Context context)
        {
            float sum = 0;
            float factorSum = 0;
            for (int i = 0; i < weightedScorers.Count; i++)
            {
                float curValue = weightedScorers[i].Evaluate(context);
                sum += curValue;
                factorSum += weightedScorers[i].Weight;
            }

            sum /= factorSum;
            return sum;
        }

        public float EvalSumIfAbove(Context context)
        {
            float sum = 0;
            float factorSum = 0;
            for (int i = 0; i < weightedScorers.Count; i++)
            {
                float curValue = weightedScorers[i].Evaluate(context);
                if (curValue > threshold)
                {
                    sum += curValue;
                    factorSum += weightedScorers[i].Weight;
                }
            }

            sum /= factorSum;
            return sum;
        }
        public float EvalInvertedSum(Context context)
        {
            return 1 - EvalSumOfChildren(context);
        }
    }
}
