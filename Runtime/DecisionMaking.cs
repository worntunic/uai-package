using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UAI.AI
{
    public class DecisionMaking : MonoBehaviour
    {
        public UAIGraphData graphData;
        private Selector selector;
        private bool _initialized = false;
        public bool Initialized { get { return _initialized; } }

        public ActionValue DecideAndReturnValue(Context context)
        {
            List<ActionValue> actionValues = selector.Evaluate(context);
            return actionValues[0];
        }
        public string Decide(Context context)
        {
            /*System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();*/
            List<ActionValue> actionValues = selector.Evaluate(context);
            //UnityExtensions.DebugLogEnumerable(actionValues);
            //sw.Stop();
            //Debug.Log($"Decision time: {sw.ElapsedMilliseconds}ms");
            return actionValues[0].action;
        }
        public void Init(Context context)
        {
            /*System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();*/
            context.Init(graphData.context);
            if (_initialized)
            {
                return;
            }
            _initialized = true;


            selector = graphData.selectorData.GetSelector();

            Dictionary<string, Scorer> guidScorers = new Dictionary<string, Scorer>();
            for (int i = 0; i < graphData.scorers.Count; i++)
            {
                guidScorers.Add(graphData.scorers[i].guid, graphData.scorers[i].GetScorer());
            }
            Dictionary<string, QualiScorer> guidQualiScorers = new Dictionary<string, QualiScorer>();
            for (int i = 0; i < graphData.qualiScorers.Count; i++)
            {
                guidQualiScorers.Add(graphData.qualiScorers[i].guid, graphData.qualiScorers[i].GetQualiScorer());
            }
            Dictionary<string, Qualifier> guidQualifiers = new Dictionary<string, Qualifier>();
            for (int i = 0; i < graphData.qualifiers.Count; i++)
            {
                guidQualifiers.Add(graphData.qualifiers[i].guid, graphData.qualifiers[i].GetQualifier());
            }
            //Assign to qualifiers and to selector
            foreach (QualifierData qd in graphData.qualifiers)
            {
                Qualifier q = guidQualifiers[qd.guid];
                selector.AddQualifier(q);
                foreach(NodeWeightedLink nwl in qd.inLinks)
                {
                    if (guidScorers.ContainsKey(nwl.otherNodeID))
                    {
                        q.AddScorer(guidScorers[nwl.otherNodeID], nwl.weight);
                    } else if (guidQualiScorers.ContainsKey(nwl.otherNodeID))
                    {
                        q.AddScorer(guidQualiScorers[nwl.otherNodeID], nwl.weight);
                    }
                }
            }
            foreach (QualiScorerData qsd in graphData.qualiScorers)
            {
                QualiScorer qs = guidQualiScorers[qsd.guid];
                foreach (NodeWeightedLink nwl in qsd.inLinks)
                {
                    if (guidScorers.ContainsKey(nwl.otherNodeID))
                    {
                        qs.AddScorer(guidScorers[nwl.otherNodeID], nwl.weight);
                    }
                    else if (guidQualiScorers.ContainsKey(nwl.otherNodeID))
                    {
                        qs.AddScorer(guidQualiScorers[nwl.otherNodeID], nwl.weight);
                    }
                }
            }
            /*sw.Stop();
            Debug.Log($"DecisionInit {sw.ElapsedMilliseconds}ms");*/
        }

    }
}

