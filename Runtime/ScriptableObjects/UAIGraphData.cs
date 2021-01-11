using System.Collections;
using System.Collections.Generic;
using UAI.AI.SO;
using UnityEngine;
using static UAI.AI.QualiScorer;

namespace UAI.AI
{
    [CreateAssetMenu(fileName = "UAI", menuName = "UAI/New Utility AI", order = 2)]
    public class UAIGraphData : ScriptableObject
    {
        public ContextSO context;
        public List<ScorerData> scorers;
        public List<QualiScorerData> qualiScorers;
        public List<QualifierData> qualifiers;
        public SelectorData selectorData;
        public string monitorAIGuid;
    }
    public enum NodeType { Scorer, Qualiscorer, Qualifier }
    [System.Serializable]
    public class NodeWeightedLink
    {
        public string otherNodeID;
        public float weight;
    }
    [System.Serializable]
    public class NodeData
    {
        public string guid;
        public Vector2 position;
        public NodeType nodeType;
    }
    [System.Serializable]
    public class ScorerData : NodeData
    {
        public string key;
        public AnimationCurve uFunction;
        public Scorer GetScorer()
        {
            Scorer scorer = new Scorer();
            scorer.guid = guid;
            scorer.key = key;
            scorer.curve = uFunction;
            return scorer;
        }
    }
    [System.Serializable]
    public class QualiScorerData : NodeData
    {
        public QualiType qualiType;
        public float threshold;
        public List<NodeWeightedLink> inLinks = new List<NodeWeightedLink>();
        public QualiScorer GetQualiScorer()
        {
            QualiScorer qScorer = new QualiScorer();
            qScorer.guid = guid;
            qScorer.type = qualiType;
            qScorer.threshold = threshold;
            return qScorer;
        }
    }
    [System.Serializable]
    public class QualifierData : QualiScorerData
    {
        public string actionName;
        public Qualifier GetQualifier()
        {
            Qualifier qualifier = new Qualifier();
            qualifier.guid = guid;
            qualifier.type = qualiType;
            qualifier.threshold = threshold;
            qualifier.actionName = actionName;
            return qualifier;
        }
    }
    [System.Serializable]
    public class SelectorData
    {
        public Selector.SelectorType selectorType;
        public float bestPercent;
        public int bestN;
        public Selector GetSelector()
        {
            Selector selector = new Selector();
            selector.type = selectorType;
            selector.BestN = bestN;
            selector.BestPercent = bestPercent;
            return selector;
        }
    }
}

