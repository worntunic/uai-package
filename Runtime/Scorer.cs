using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UAI.AI
{
    public interface IScorable
    {
        float Evaluate(Context context);
    }
    public class Scorer : IScorable
    {
        public string guid;
        public AnimationCurve curve;
        public string key;
        public static event System.Action<string, string, float> OnEvaluation;

        public float Evaluate(Context context)
        {
            float value = curve.Evaluate(context.GetValue(key));
            OnEvaluation?.Invoke(context.aiGuid, guid, value);
            return value;
        }
    }
    
}

