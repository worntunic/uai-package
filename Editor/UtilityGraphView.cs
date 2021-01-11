using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace UAI.AI.Edit {
    public class UtilityGraphView : GraphView
    {
        private UAIGraphData graphData;
        private SerializedObject graphDataSerialized;
        public Dictionary<string, BaseNode> allNodes;
        private List<QualifierNode> qualifierNodes = new List<QualifierNode>();
        int currentQualifier = 0;

        public UtilityGraphView(UAIGraphData graphData)
        {
            this.graphData = graphData;
            this.graphDataSerialized = new SerializedObject(graphData);
            styleSheets.Add(Resources.Load<StyleSheet>("UAIGraphStylesheet"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            /*MiniMap miniMap = new MiniMap();
            contentContainer.Add(miniMap);*/

            var grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);

            GenerateNodes();
            graphViewChanged = OnGraphViewChanged;

            RegisterCallback<KeyDownEvent>(OnTabEvent);
        }
        private void OnTabEvent(KeyDownEvent ev)
        {
            if (qualifierNodes.Count > 0 && ev.keyCode == KeyCode.Tab)
            {
                if (ev.shiftKey)
                {
                    currentQualifier = (currentQualifier - 1) < 0 ? qualifierNodes.Count - 1 : currentQualifier - 1;
                } else
                {
                    currentQualifier = (currentQualifier + 1) >= qualifierNodes.Count ? 0 : currentQualifier + 1;
                }
                qualifierNodes[currentQualifier].Select(this, false);
                FrameSelection();
                //UpdateViewTransform(qualifierNodes[currentQualifier].GetPosition().position, transform.scale);
            }
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            
            if (evt.target is GraphView || evt.target is Node)
            {
                
                Vector3 pos = evt.mousePosition / scale;
                pos -= viewTransform.position / scale;
                evt.menu.AppendAction("Add Scorer", (e) => { CreateNewScorerNode(pos) ; });
                evt.menu.AppendAction("Add QualiScorer", (e) => { CreateNewQualiScorerNode(pos); });
                evt.menu.AppendAction("Add Qualifier", (e) => { CreateNewQualifierNode(pos); });

            }
        }
        private void GenerateNodes()
        {
            allNodes = new Dictionary<string, BaseNode>();
            for (int i = 0; i < graphData.scorers.Count; i++)
            {
                CreateScorerNode(i);
            }
            for (int i = 0; i < graphData.qualiScorers.Count; i++)
            {
                CreateQualiScorerNode(i);

            }
            for (int i = 0; i < graphData.qualifiers.Count; i++)
            {
                CreateQualifierNode(i);
            }
            //foreach (Quali)
        }

        public void CreateNewScorerNode(Vector2 position)
        {
            ScorerData scData = new ScorerData();
            scData.position = position;
            scData.guid = Guid.NewGuid().ToString();
            scData.key = graphData.context.propertyNames[0];
            graphData.scorers.Add(scData);
            SaveGraphData();
            CreateScorerNode(graphData.scorers.Count - 1);
        }
        public ScorerNode CreateScorerNode(int index)
        {
            ScorerData scData = graphData.scorers[index];
            SerializedProperty scorerArr = graphDataSerialized.FindProperty("scorers");
            SerializedProperty serScData = scorerArr.GetArrayElementAtIndex(index);
            ScorerNode node = new ScorerNode(scData, serScData, graphData.context.propertyNames);
            this.AddElement(node);
            this.allNodes.Add(node.guid, node);
            return node;
        }
        public void CreateNewQualiScorerNode(Vector2 position)
        {
            QualiScorerData qsData = new QualiScorerData();
            qsData.position = position;
            qsData.guid = Guid.NewGuid().ToString();
            graphData.qualiScorers.Add(qsData);
            SaveGraphData();
            int qsIndex = graphData.qualiScorers.Count - 1;
            QualiScorerNode qsNode = CreateQualiScorerNode(qsIndex);
            AddNewQualiScorerPort(qsNode, qsIndex);
        }
        public QualiScorerNode CreateQualiScorerNode(int index)
        {
            QualiScorerData qsData = graphData.qualiScorers[index];
            SerializedProperty qsArr = graphDataSerialized.FindProperty("qualiScorers");
            SerializedProperty serData = qsArr.GetArrayElementAtIndex(index);
            QualiScorerNode node = new QualiScorerNode(qsData, serData);
            this.AddElement(node);
            for (int i = 0; i < qsData.inLinks.Count; i++)
            {
                AddExistingQualiScorerPort(node, index, i);
            }
            this.allNodes.Add(node.guid, node);
            return node;
        }
        public void CreateNewQualifierNode(Vector2 position)
        {
            QualifierData qData = new QualifierData();
            qData.position = position;
            qData.guid = Guid.NewGuid().ToString();
            qData.actionName = graphData.context.actionNames[0];
            graphData.qualifiers.Add(qData);
            SaveGraphData();
            int qIndex = graphData.qualifiers.Count - 1;
            QualifierNode qNode = CreateQualifierNode(qIndex);
            AddNewQualifierPort(qNode, qIndex);
        }
        public QualifierNode CreateQualifierNode(int index)
        {
            QualifierData qData = graphData.qualifiers[index];
            SerializedProperty qArr = graphDataSerialized.FindProperty("qualifiers");
            SerializedProperty serData = qArr.GetArrayElementAtIndex(index);
            QualifierNode node = new QualifierNode(qData, graphData.context.actionNames, serData);
            this.AddElement(node);
            for (int i = 0; i < qData.inLinks.Count; i++)
            {
                AddExistingQualifierPort(node, index, i);
            }
            this.allNodes.Add(node.guid, node);
            qualifierNodes.Add(node);
            return node;
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            System.Type[] allowedPortTypes;
            if (startPort.direction == Direction.Input)
            {
                allowedPortTypes = ((BaseNode)startPort.node).AllowedOutPorts;
            } else
            {
                allowedPortTypes = ((BaseNode)startPort.node).AllowedInPorts;
            }
            ports.ForEach((port) => {
                if (startPort.node != port.node && startPort.direction != port.direction)
                {
                    for (int i = 0; i < allowedPortTypes.Length; i++)
                    {
                        if (port.node.GetType() == allowedPortTypes[i])
                        {
                            compatiblePorts.Add(port);
                            break;
                        }
                    }
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        public void SaveGraphData()
        {
            EditorUtility.SetDirty(graphData);
            graphDataSerialized.Update();
            //AssetDatabase.SaveAssets();
            
        }
        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null)
            {
                foreach (GraphElement el in change.elementsToRemove)
                {
                    if (el is BaseNode)
                    {
                        BaseNode bNode = (BaseNode)el;
                        int index = GetNodeIndexByGuid(bNode.guid);
                        if (bNode is ScorerNode)
                        {
                            graphData.scorers.RemoveAt(index);
                        } else if (bNode is QualiScorerNode)
                        {
                            graphData.qualiScorers.RemoveAt(index);
                        } else if (bNode is QualifierNode)
                        {
                            graphData.qualifiers.RemoveAt(index);
                        }
                        SaveGraphData();
                    }
                    if (el is Edge)
                    {
                        Edge edge = (Edge)el;
                        DeleteEdge(edge);
                    }
                }
            }
            if (change.movedElements != null)
            {
                foreach (GraphElement el in change.movedElements)
                {
                    if (el is BaseNode)
                    {
                        BaseNode node = (BaseNode)el;
                        node.SavePosition();
                    }
                }
            }

            if (change.edgesToCreate != null)
            {
                foreach (Edge edge in change.edgesToCreate)
                {
                    Port input = edge.input;
                    if (input.node is QualiScorerNode)
                    {
                        var qsNode = (QualiScorerNode)input.node;
                        BaseNode outputNode = (BaseNode)edge.output.node;
                        int portIndex = input.parent.IndexOf(input);
                        int qsIndex = GetQualiScorerIndex(qsNode.guid);
                        graphData.qualiScorers[qsIndex].inLinks[portIndex].otherNodeID = outputNode.guid;

                        IEnumerable<VisualElement> possiblePorts = qsNode.inputContainer.Children();
                        int countOfConnected = 0;
                        int countTotal = 0;
                        foreach(VisualElement el in possiblePorts)
                        {
                            if (el is Port)
                            {
                                Port port = (Port)el;
                                countTotal++;
                                if (port.connected)
                                {
                                    countOfConnected++;
                                }
                            }
                        }
                        if (countOfConnected == countTotal - 1)
                        {
                            AddNewQualiScorerPort(qsNode, qsIndex);
                        }
                    } else if (input.node is QualifierNode)
                    {
                        var qNode = (QualifierNode)input.node;
                        BaseNode outputNode = (BaseNode)edge.output.node;
                        int portIndex = input.parent.IndexOf(input);
                        int qIndex = GetQualifierIndex(qNode.guid);
                        graphData.qualifiers[qIndex].inLinks[portIndex].otherNodeID = outputNode.guid;

                        IEnumerable<VisualElement> possiblePorts = qNode.inputContainer.Children();
                        int countOfConnected = 0;
                        int countTotal = 0;
                        foreach (VisualElement el in possiblePorts)
                        {
                            if (el is Port)
                            {
                                Port port = (Port)el;
                                countTotal++;
                                if (port.connected)
                                {
                                    countOfConnected++;
                                }
                            }
                        }
                        if (countOfConnected == countTotal - 1)
                        {
                            AddNewQualifierPort(qNode, qIndex);
                        }
                    }
                }
            }

            graphDataSerialized.ApplyModifiedProperties();
            SaveGraphData();
            return change;
        }
        private void DeleteEdge(Edge edge)
        {
            if (edge.input.node is QualiScorerNode)
            {
                QualiScorerNode qsNode = ((QualiScorerNode)edge.input.node);
                NodeData nData = GetDataByGuid(qsNode.guid);
                QualiScorerData qData = (QualiScorerData)nData;
                int portIndex = qsNode.inputContainer.IndexOf(edge.input);
                qData.inLinks[portIndex].otherNodeID = "";
                SaveGraphData();
            } else if (edge.input.node is QualifierNode)
            {
                QualifierNode qNode = ((QualifierNode)edge.input.node);
                NodeData nData = GetDataByGuid(qNode.guid);
                QualifierData qData = (QualifierData)nData;
                int portIndex = qNode.inputContainer.IndexOf(edge.input);
                qData.inLinks[portIndex].otherNodeID = "";
                SaveGraphData();
            }
        }
        private int GetQualiScorerIndex(string guid)
        {
            return graphData.qualiScorers.FindIndex(qs => qs.guid == guid);
        }
        private int GetQualifierIndex(string guid)
        {
            return graphData.qualifiers.FindIndex(qs => qs.guid == guid);
        }
        private void AddNewQualiScorerPort(QualiScorerNode qsNode, int qsIndex)
        {
            NodeWeightedLink nwLink = new NodeWeightedLink();
            nwLink.otherNodeID = "";
            nwLink.weight = 0.5f;
            graphData.qualiScorers[qsIndex].inLinks.Add(nwLink);
            int linkIndex = graphData.qualiScorers[qsIndex].inLinks.Count - 1;
            SaveGraphData();
            SerializedProperty serNWL = graphDataSerialized
                                                .FindProperty("qualiScorers")
                                                .GetArrayElementAtIndex(qsIndex)
                                                .FindPropertyRelative("inLinks")
                                                .GetArrayElementAtIndex(linkIndex)
                                                .FindPropertyRelative("weight");
            qsNode.AddInputPort(nwLink, serNWL);
        }
        private void AddNewQualifierPort(QualifierNode qNode, int qIndex)
        {
            NodeWeightedLink nwLink = new NodeWeightedLink();
            nwLink.otherNodeID = "";
            nwLink.weight = 0.5f;
            graphData.qualifiers[qIndex].inLinks.Add(nwLink);
            int linkIndex = graphData.qualifiers[qIndex].inLinks.Count - 1;
            SaveGraphData();
            SerializedProperty serNWL = graphDataSerialized
                                                .FindProperty("qualifiers")
                                                .GetArrayElementAtIndex(qIndex)
                                                .FindPropertyRelative("inLinks")
                                                .GetArrayElementAtIndex(linkIndex)
                                                .FindPropertyRelative("weight");
            qNode.AddInputPort(nwLink, serNWL);
        }
        private void AddExistingQualiScorerPort(QualiScorerNode qsNode, int qsIndex, int nwIndex)
        {
            NodeWeightedLink nwLink = graphData.qualiScorers[qsIndex].inLinks[nwIndex];
            SaveGraphData();
            SerializedProperty serNWL = graphDataSerialized
                                                .FindProperty("qualiScorers")
                                                .GetArrayElementAtIndex(qsIndex)
                                                .FindPropertyRelative("inLinks")
                                                .GetArrayElementAtIndex(nwIndex)
                                                .FindPropertyRelative("weight");
            Port port = qsNode.AddInputPort(nwLink, serNWL);
            AddExistingPort(nwLink, port, qsNode);
        }
        private void AddExistingQualifierPort(QualifierNode qNode, int qIndex, int nwIndex)
        {
            NodeWeightedLink nwLink = graphData.qualifiers[qIndex].inLinks[nwIndex];
            SaveGraphData();
            SerializedProperty serNWL = graphDataSerialized
                                                .FindProperty("qualifiers")
                                                .GetArrayElementAtIndex(qIndex)
                                                .FindPropertyRelative("inLinks")
                                                .GetArrayElementAtIndex(nwIndex)
                                                .FindPropertyRelative("weight");
            Port port = qNode.AddInputPort(nwLink, serNWL);
            AddExistingPort(nwLink, port, qNode);
        }
        private void AddExistingPort(NodeWeightedLink nwLink, Port port, BaseNode inNode)
        {
            if (!String.IsNullOrEmpty(nwLink.otherNodeID))
            {
                BaseNode node = NodeByGuid(nwLink.otherNodeID);
                Port outPort = null;
                if (node is ScorerNode)
                {
                    outPort = ((ScorerNode)node).outPort;
                }
                else if (node is QualiScorerNode)
                {
                    outPort = ((QualiScorerNode)node).outPort;
                }
                if (outPort != null)
                {
                    Edge edge = new Edge()
                    {
                        output = outPort,
                        input = port
                    };
                    port.Connect(edge);
                    outPort.Connect(edge);
                    Add(edge);
                }
            }
            inNode.Refresh();
        }
        private NodeData GetDataByGuid(string guid)
        {
            NodeData node;
            if ((node = (NodeData)graphData.scorers.FirstOrDefault( scData => scData.guid == guid)) != null)
            {
                return node;
            } else if ((node = (NodeData)graphData.qualiScorers.FirstOrDefault(qsData => qsData.guid == guid)) != null)
            {
                return node;
            } else if ((node =(NodeData)graphData.qualifiers.FirstOrDefault(qData => qData.guid == guid)) != null)
            {
                return node;
            }
            return null;
        }
        private int GetNodeIndexByGuid(string guid)
        {
            int index;
            if ((index = graphData.scorers.FindIndex(scData => scData.guid == guid)) != -1)
            {
                return index;
            }
            else if ((index = graphData.qualiScorers.FindIndex(scData => scData.guid == guid)) != -1)
            {
                return index;
            }
            else if ((index = graphData.qualifiers.FindIndex(scData => scData.guid == guid)) != -1)
            {
                return index;
            }
            return -1;
        }
        private BaseNode NodeByGuid(string guid)
        {
            BaseNode retNode = null;
            nodes.ForEach((node) =>
            {
                BaseNode bNode = (BaseNode)node;
                if (bNode.guid == guid)
                {
                    retNode = bNode;
                }
                
            });
            return retNode;
        }
    }
}