using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UAI.AI.SO
{
    [CreateAssetMenu(fileName = "Context", menuName = "UAI/Context", order = 1)]
    public class ContextSO : ScriptableObject
    {
        public List<string> propertyNames = new List<string>();
        public List<string> actionNames = new List<string>();
    }
}

