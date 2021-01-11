using System;
using System.Collections;
using System.Collections.Generic;
using UAI.AI.SO;
using UnityEngine;

namespace UAI.AI
{
    public abstract class Context
    {
        //public List<string> keys = new List<string>();
        public string aiGuid;
        private Dictionary<string, float> values;

        public void Init(ContextSO contextSO)
        {
            values = new Dictionary<string, float>();
            for (int i = 0; i < contextSO.propertyNames.Count; i++)
            {
                values.Add(contextSO.propertyNames[i], 0);
            }
        }

        public void UpdateValue(string key, float newValue)
        {
            values[key] = newValue;
        }
        public float GetValue(string key)
        {
            return values[key];
        }

        public abstract void UpdateContext();
    }
}

