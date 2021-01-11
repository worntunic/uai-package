using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UAI.Edit.Extensions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UAI.AI.Edit
{
    public class BaseNode : Node
    {
        private readonly Vector2 defaultNodeSize = new Vector2(200, 200);
        public string guid;
        protected System.Type[] _allowedInPorts;
        protected System.Type[] _allowedOutPorts;
        public System.Type[] AllowedInPorts { get { return _allowedInPorts; } }
        public System.Type[] AllowedOutPorts { get { return _allowedOutPorts; } }
        protected SerializedProperty serData;
        private Label monitorLabel;
        private VisualElement monitorProgress;

        private void Initialize(SerializedProperty serData)
        {
            this.serData = serData;
            this.guid = serData.FindPropertyRelative("guid").stringValue;
            Vector2 position = serData.FindPropertyRelative("position").vector2Value;
            styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            AddToClassList("node");
            Rect positionRect = new Rect(position, defaultNodeSize);
            this.SetPosition(positionRect);
            GenerateMonitorElements();
            SetAllowedPorts();
        }
        private void GenerateMonitorElements()
        {

            VisualElement monitorContainer = new VisualElement();
            monitorContainer.AddToClassList("monitorContainer");

            monitorLabel = new Label("");
            monitorLabel.AddToClassList("monitorLabel");
            monitorContainer.Add(monitorLabel);

            VisualElement monitorProgressContainer = new VisualElement();
            monitorProgressContainer.AddToClassList("monitorProgressContainer");
            monitorContainer.Add(monitorProgressContainer);

            monitorProgress = new VisualElement();
            monitorProgress.AddToClassList("monitorProgress");
            titleContainer.Add(monitorProgress);

            titleContainer.Add(monitorContainer);
        }
        public BaseNode(SerializedProperty serData)
        {
            Initialize(serData);
        }

        protected Port AddPort(string name, Orientation orientation, Direction direction, Port.Capacity capacity, System.Type type = null)
        {
            var port = this.InstantiatePort(orientation, direction, capacity, type);
            if (!String.IsNullOrEmpty(name))
            {
                port.portName = name;
            }
            if (direction == Direction.Input)
            {
                this.inputContainer.Add(port);
            } else
            {
                this.outputContainer.Add(port);
            }
            //Refresh();
            return port;
        }
        public void Refresh()
        {
            RefreshExpandedState();
            RefreshPorts();
        }
        protected virtual void SetAllowedPorts()
        {
            _allowedInPorts = new System.Type[0];
            _allowedOutPorts = new System.Type[0];
        }
        public void SavePosition()
        {
            serData.FindPropertyRelative("position").vector2Value = GetPosition().position;
        }
        public void SetMonitorValue(float monitorValue)
        {
            monitorLabel.text = monitorValue.ToString("n3");
            monitorProgress.style.width = new StyleLength(new Length(monitorValue * 100, LengthUnit.Percent));
            monitorProgress.style.backgroundColor = Color.Lerp(Color.red, Color.green, monitorValue);
        }
    }
    public class ScorerNode : BaseNode
    {
        public Port outPort;
        public ScorerNode(ScorerData scData, SerializedProperty serScData, List<string> keys) : base(serScData) {
            title = "Scorer";
            DrawFields(scData, keys);
            outPort = AddPort("Output", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(QualiScorerNode));
            Refresh();
        }
        private void DrawFields(ScorerData scData, List<string> keys)
        {
            CurveField curveField = new CurveField("uFunction");
            int index = keys.IndexOf(scData.key);
            PopupField<string> popupField = new PopupField<string>("key", keys, index);
            /*UnityExtensions.DebugLogEnumerable(popupField.GetClasses());
            UnityExtensions.DebugLogEnumerable(curveField.GetClasses());*/
            curveField.BindProperty(serData.FindPropertyRelative("uFunction"));
            popupField.BindProperty(serData.FindPropertyRelative("key"));
            contentContainer.Add(curveField);
            contentContainer.Add(popupField);
        }
        protected override void SetAllowedPorts()
        {
            _allowedInPorts = new System.Type[0];
            _allowedOutPorts = new System.Type[2] { typeof(QualiScorerNode), typeof(QualifierNode) };
        }
    }
    public class QualiScorerNode : BaseNode
    {
        public Port outPort;
        public QualiScorerNode(QualiScorerData qsData, SerializedProperty serData) : base(serData)
        {
            title = "QualiScorer";
            /*for(int i = 0; i < qsData.inLinks.Count; i++)
            {
                SerializedProperty linkProp = serData.FindPropertyRelative("inLinks").GetArrayElementAtIndex(i).FindPropertyRelative("weight");
                AddInputPort(qsData.inLinks[i], linkProp);
            }*/
            //AddInputPort(true);
            DrawFields(qsData);
            outPort = AddPort("Output", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            Refresh();
        }
        private void DrawFields(QualiScorerData qsData)
        {
            EnumField qualTypeField = new EnumField("type", Qualifier.QualiType.SumOfChildren);
            qualTypeField.BindProperty(serData.FindPropertyRelative("qualiType"));
            LimitedFloatField thresholdField = new LimitedFloatField("threshold", 0, 1);
            thresholdField.BindProperty(serData.FindPropertyRelative("threshold"));

            contentContainer.Add(qualTypeField);
            contentContainer.Add(thresholdField);
        }
        protected override void SetAllowedPorts()
        {
            _allowedInPorts = new System.Type[2] { typeof(ScorerNode), typeof(QualiScorerNode) };
            _allowedOutPorts = new System.Type[2] { typeof(QualiScorerNode), typeof(QualifierNode) };
        }
        public Port AddInputPort(NodeWeightedLink existingLink, SerializedProperty nodeLink)
        {
            Port port = AddPort("", Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            LimitedFloatField weightField = new LimitedFloatField(0, 1);
            weightField.BindProperty(nodeLink);
            port.contentContainer.Add(weightField);
            Refresh();
            return port;
        }
    }
    public class QualifierNode : BaseNode
    {
        public QualifierNode(QualifierData qData, List<string> actions, SerializedProperty serData) : base(serData)
        {
            title = "Qualifier";
            DrawFields(qData, actions);
            Refresh();
        }
        protected override void SetAllowedPorts()
        {
            _allowedOutPorts = new System.Type[2] { typeof(ScorerNode), typeof(QualiScorerNode) };
        }
        private void DrawFields(QualifierData qData, List<string> actions)
        {
            EnumField qualTypeField = new EnumField("type", QualiScorer.QualiType.SumOfChildren);
            qualTypeField.BindProperty(serData.FindPropertyRelative("qualiType"));
            LimitedFloatField thresholdField = new LimitedFloatField("threshold", 0, 1);
            thresholdField.BindProperty(serData.FindPropertyRelative("threshold"));
            int actionIndex = actions.IndexOf(qData.actionName);
            PopupField<string> actionNameField = new PopupField<string>("action", actions, actionIndex);
            actionNameField.BindProperty(serData.FindPropertyRelative("actionName"));

            contentContainer.Add(qualTypeField);
            contentContainer.Add(thresholdField);
            contentContainer.Add(actionNameField);
        }
        public Port AddInputPort(NodeWeightedLink existingLink, SerializedProperty nodeLink)
        {
            Port port = AddPort("", Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            LimitedFloatField weightField = new LimitedFloatField(0, 1);
            weightField.BindProperty(nodeLink);
            port.contentContainer.Add(weightField);
            Refresh();
            return port;
        }
    }

}
