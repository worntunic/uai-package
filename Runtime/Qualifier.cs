using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UAI.AI
{
    public class Qualifier : QualiScorer
    {
        public string actionName;

        public Qualifier()
        {

        }

        public Qualifier(QualiType type) : base(type)
        {

        }
    }
}