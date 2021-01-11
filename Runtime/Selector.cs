using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UAI.AI
{
    public struct ActionValue
    {
        public string action;
        public float value;

        public ActionValue(string action, float value)
        {
            this.action = action;
            this.value = value;
        }
        public override string ToString()
        {
            return $"({action}:{value}";
        }
    }
    public class Selector
    {
        public enum SelectorType
        {
            Best, RandomFromBestN, WeightedRandomFromBestN, RandomFromTopPercent, WeightedRandomFromTopPercent, TrueRandom
        }

        public List<Qualifier> qualifiers;
        public SelectorType type;
        public int _bestN;
        public int BestN { get { return _bestN; } set { _bestN = (value > 1) ? value : 1; } }
        private float _bestPercent;
        public float BestPercent { get { return _bestPercent; } set { _bestPercent = Mathf.Clamp01(value); } }
        private Func<Context, List<ActionValue>> evalMethod;

        public Selector()
        {
            qualifiers = new List<Qualifier>();
            type = SelectorType.Best;
            SelectEvalMethod();
        }

        public Selector(SelectorType type)
        {
            qualifiers = new List<Qualifier>();
            this.type = type;
            SelectEvalMethod();
        }


        public void AddQualifier(Qualifier qualifier)
        {
            qualifiers.Add(qualifier);
        }

        public void RemoveQualifier(Qualifier qualifier)
        {
            qualifiers.Remove(qualifier);
        }
        public List<ActionValue> Evaluate(Context context)
        {
            return evalMethod(context);
        }
        private void SelectEvalMethod()
        {
            switch (type)
            {
                case SelectorType.Best:
                {
                    evalMethod = EvalBest;
                    break;
                }
                case SelectorType.RandomFromBestN:
                {
                    evalMethod = EvalBestRandomFromN;
                    break;
                }
                case SelectorType.WeightedRandomFromBestN:
                {
                    evalMethod = EvalBestWeightedRandomFromN;
                    break;
                }
                case SelectorType.RandomFromTopPercent:
                {
                    evalMethod = EvalBestRandomTopPercent;
                    break;
                }
                case SelectorType.WeightedRandomFromTopPercent:
                {
                    evalMethod = EvalBestWeightedRandomTopPercent;
                    break;
                }
                case SelectorType.TrueRandom:
                {
                    evalMethod = EvalRandom;
                    break;
                }

            }
        }

        private List<ActionValue> EvalBest(Context context)
        {
            return GetOrderedActionValues(context);
        }
        private List<ActionValue> EvalBestRandomFromN(Context context)
        {
            int index = UnityEngine.Random.Range(0, BestN);
            List<ActionValue> actionValues = GetOrderedActionValues(context);
            ActionValue tmp = actionValues[index];
            actionValues[index] = actionValues[0];
            actionValues[0] = tmp;
            return actionValues;
        }
        private List<ActionValue> EvalBestWeightedRandomFromN(Context context)
        {
            List<ActionValue> actionValues = GetOrderedActionValues(context);
            float valueSum = actionValues.GetRange(0, BestN).Sum(av => av.value);
            float randomNumber = UnityEngine.Random.Range(0, valueSum);
            int index = 0;
            for (int i = 0; i < BestN; i++)
            {
                if (actionValues[i].value >= randomNumber)
                {
                    index = i;
                    break;
                }
            }
            ActionValue tmp = actionValues[index];
            actionValues[index] = actionValues[0];
            actionValues[0] = tmp;
            return actionValues;
        }
        private List<ActionValue> EvalBestRandomTopPercent(Context context)
        {
            List<ActionValue> actionValues = GetOrderedActionValues(context);
            int topCount = 1;
            float minVal = actionValues[0].value * (1 - BestPercent);
            for (int i = 1; i < actionValues.Count; i++)
            {
                if (actionValues[i].value >= minVal)
                {
                    topCount++;
                } else
                {
                    break;
                }
            }
            int index = UnityEngine.Random.Range(0, topCount);
            ActionValue tmp = actionValues[index];
            actionValues[index] = actionValues[0];
            actionValues[0] = tmp;
            return actionValues;
        }
        private List<ActionValue> EvalBestWeightedRandomTopPercent(Context context)
        {
            List<ActionValue> actionValues = GetOrderedActionValues(context);
            float minVal = actionValues[0].value * (1 - BestPercent);
            float valueSum = actionValues[0].value;
            for (int i = 1; i < actionValues.Count; i++)
            {
                if (actionValues[i].value >= minVal)
                {
                    valueSum += actionValues[i].value;
                }
                else
                {
                    break;
                }
            }
            float randomNumber = UnityEngine.Random.Range(0, valueSum);
            int index = 0;
            for (int i = 0; i < BestN; i++)
            {
                if (actionValues[i].value >= randomNumber)
                {
                    index = i;
                    break;
                }
            }
            ActionValue tmp = actionValues[index];
            actionValues[index] = actionValues[0];
            actionValues[0] = tmp;
            return actionValues;
        }

        private List<ActionValue> EvalRandom(Context context)
        {
            List<ActionValue> actionValues = GetOrderedActionValues(context);
            int index = UnityEngine.Random.Range(0, actionValues.Count);
            ActionValue tmp = actionValues[index];
            actionValues[index] = actionValues[0];
            actionValues[0] = tmp;
            return actionValues;
        }
        private List<ActionValue> ConvertToActionValues(List<(Qualifier, float)> qualValues)
        {
            List<ActionValue> retList = new List<ActionValue>();
            for (int i = 0; i < qualValues.Count; i++)
            {
                retList.Add(new ActionValue(qualValues[i].Item1.actionName, qualValues[i].Item2));
            }
            return retList;
        }
        private List<ActionValue> GetOrderedActionValues(Context context)
        {
            return ConvertToActionValues(GetOrdered(context));
        }
        private List<(Qualifier, float)> GetOrdered(Context context)
        {
            List<(Qualifier, float)> qts = new List<(Qualifier, float)>(qualifiers.Count);
            for (int i = 0; i < qualifiers.Count; i++)
            {
                qts.Add((qualifiers[i], qualifiers[i].Evaluate(context)));
            }
            qts.Sort(delegate ((Qualifier, float) x, (Qualifier, float) y) {
                return (y.Item2 - x.Item2).RoundOutToInt();
            });
            return qts;
        }


    }
}