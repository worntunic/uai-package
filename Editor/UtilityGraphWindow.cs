using System.Collections;
using System.Collections.Generic;
using UAI.Edit.Extensions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UAI.AI.Edit
{
    public class UtilityGraphWindow : GraphViewEditorWindow
    {
        private UtilityGraphView graphView;
        private UAIGraphData graphData;
        IntegerField bestNField;
        LimitedFloatField bestPercentField;
        TextField aiGuidField;

        public static void OpenEditorWindow(UAIGraphData graphData)
        {
            UtilityGraphWindow window = GetWindow<UtilityGraphWindow>();
            window.graphData = graphData;
            window.OnEnable();
            window.titleContent = new GUIContent("Utility AI Editor");

        }
        private void GenerateGraphView()
        {
            graphView = new UtilityGraphView(graphData)
            {
                name = "Utility AI Graph"
            };
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            toolbar.styleSheets.Add(Resources.Load<StyleSheet>("UAIGraphWindowStylesheet"));

            /*var createScorerBtn = new Button(OnCreateScorerClicked);
            createScorerBtn.text = "Create Scorer";
            toolbar.Add(createScorerBtn);

            var createQualiScorerBtn = new Button(OnCreateQualiScorerClicked);
            createQualiScorerBtn.text = "Create QualiScorer";
            toolbar.Add(createQualiScorerBtn);

            var createQualifierBtn = new Button(OnCreateQualifierClicked);
            createQualifierBtn.text = "Create Qualifier";
            toolbar.Add(createQualifierBtn);*/



            SerializedObject graphSer = new SerializedObject(graphData);

            aiGuidField = new TextField("Monitor AI:");
            aiGuidField.BindProperty(graphSer.FindProperty("monitorAIGuid"));
            toolbar.Add(aiGuidField);

            EnumField selectorTypeField = new EnumField("Selector Type", graphData.selectorData.selectorType);
            selectorTypeField.BindProperty(graphSer.FindProperty("selectorData").FindPropertyRelative("selectorType"));
            toolbar.Add(selectorTypeField);


            selectorTypeField.RegisterValueChangedCallback(x => {
                Selector.SelectorType newValue = (Selector.SelectorType)x.newValue;
                SetSelectorFieldsVisibility(newValue);
            });
            bestNField = new IntegerField("N", graphData.selectorData.bestN);
            bestNField.BindProperty(graphSer.FindProperty("selectorData").FindPropertyRelative("bestN"));
            toolbar.Add(bestNField);
            bestPercentField = new LimitedFloatField("Top %", 0, 1);
            bestPercentField.value = graphData.selectorData.bestPercent;
            bestPercentField.BindProperty(graphSer.FindProperty("selectorData").FindPropertyRelative("bestPercent"));
            toolbar.Add(bestPercentField);
            SetSelectorFieldsVisibility(graphData.selectorData.selectorType);

            rootVisualElement.Add(toolbar);
        }
        private void SetSelectorFieldsVisibility(Selector.SelectorType type)
        {
            if (type == Selector.SelectorType.RandomFromBestN || type == Selector.SelectorType.WeightedRandomFromBestN)
            {
                bestNField.visible = true;
                bestPercentField.visible = false;
            }
            else if (type == Selector.SelectorType.RandomFromTopPercent || type == Selector.SelectorType.WeightedRandomFromTopPercent)
            {
                bestNField.visible = false;
                bestPercentField.visible = true;
            }
            else
            {
                bestNField.visible = false;
                bestPercentField.visible = false;
            }
        }
        //Events
        private void OnCreateScorerClicked()
        {
            graphView.CreateNewScorerNode(Vector2.zero);
        }
        private void OnCreateQualiScorerClicked()
        {
            graphView.CreateNewQualiScorerNode(Vector2.zero);
        }
        private void OnCreateQualifierClicked()
        {
            graphView.CreateNewQualifierNode(Vector2.zero);
        }
        private void OnEnable()
        {
            if (graphData)
            {
                GenerateGraphView();

                GenerateToolbar();
                SubscribeToEvents();
            }
            
        }
        private void OnDisable()
        {
            if (graphView != null)
            {
                graphView.SaveGraphData();
                rootVisualElement.Remove(graphView);
                UnsubscribeToEvents();
            }

        }
        public void SubscribeToEvents()
        {
            Scorer.OnEvaluation += OnEvaluation;
            QualiScorer.OnEvaluation += OnEvaluation;
        }
        public void UnsubscribeToEvents()
        {
            Scorer.OnEvaluation -= OnEvaluation;
            QualiScorer.OnEvaluation -= OnEvaluation;
        }
        private void OnEvaluation(string aiGuid, string nGuid, float value)
        {
            if (graphView != null && aiGuid == aiGuidField.value && graphView.allNodes.ContainsKey(nGuid))
            {
                graphView.allNodes[nGuid].SetMonitorValue(value);
            }
        }
    }
}
