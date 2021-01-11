# Utility AI
Simple utility system for designing AI agents with a visual node-like tool.
Aim of the UAI is to give game designers and AI designers more control when creating AI, especially during the tweaking phase.
This system is only for the decision making part of the AI.

## How to install
1. In Unity, open Package Manager.
2. Click the little '+' icon in the top left and then "Add package from git URL..."
3. Copy and paste "https://github.com/worntunic/uai-package" into the text box and click "Add"

##How to use
### Necessary files
For your agent you'll need to create 2 scriptable objects:

1. In the Project window, click on the context menu option "Create/UAI/Context". This will open up inspector window with your agent's context file, where you can define which properties does agent use to make decisions (string keys) and which actions can agent take (also string keys).
2. In the Project window, click on the context menu option "Create/UAI/New Utility AI". This creates a UAIGraphData file and will open up inspector window of your agent's Utility AI System. For "Context" choose or drag your previously created Context file. Everything below shouldn't be edited from here, but yeah it's v0.1 whatyougonnadoaboutit. Click on the top-most button "Load Graph Editor". This will open the Utility AI Editor window where you can create your agent's Utility system.

### Using Utility AI Editor

####Creating nodes
Right clicking the graph editor will pop up the context menu, which gives you options to add Scorers, QualiScorers and Qualifiers.
- Scorer node is the basic input of the UAI. When added, you can edit its 2 options:
    1. key - Agent's property that will be used as an input 
    2. uFunction - Transformation function for the input. X axis is input value, Y axis is output value.
- QualiScorer node is the summation node. It accepts Scorer and QualiScorer nodes as input, and assigns a weight for each input. The (relative) higher the weight, the more influence that input node has on the output of the QualiScorer. Properties:
    1. Type - How is the output value determined?
    2. Threshold - Used by some summation types, specific for each.
- Qualifier node - functionally the same as the QualiScorer node, but it has property actionName to determine which action does it refer to, and it doesn't have output connections, because it is implied that it has to be attached to Selector node.

Selector node isn't shown in the UAI editor because every UAI system must have one and only one. Selector options (selection type) can be chosen from the top of the editor window, from the toolbar. 

#####Handy editor tools
- You can select multiple nodes.
- You can move nodes by dragging them
- You can delete nodes and connections by selecting them, then pressing delete or through the context menu
- You can zoom in and out with the scroll wheel
- By pressing tab/shift+tab editor window is centered on the next/previous qualifier node

###Connecting it to your agent
To connect the system to your agent, you'll need to inherit from abstract class `UAI.AI.Context`. Here you should override the method `UpdateContext()`. This method should update all the input values of your agent by calling the method `UpdateValue(string key, float value)`. Note: Values should be in 0.0 - 1.0 range. If you'd like to use UAI debugging, you should set the aiGuid property of the Context. Example:

```
public class BunnyContext : Context
{
    public Bunny bunny;
    public BunnyContext(Bunny bunny, string aiGuid)
    {
        this.bunny = bunny;
        this.aiGuid = aiGuid;
    }
    public override void UpdateContext()
    {
        UpdateValue($"Thirst", bunny.stats.ThirstPercent);
        UpdateValue($"Hunger", bunny.stats.HungerPercent);
        UpdateValue($"Fatigue", bunny.stats.FatiguePercent);
    }
}
```

Each agent has their own unique context. However, all of the agents of the same type can use the same UAI Decision making system. To do this, create a new gameobject and attach a "DecisionMaking" component. Add your UAIGraphData file to this component's property. To initialize the system for every agent, you should create their context, and then call the `Init(Context context)` of the `DecisionMaking`. Example:

```
public DecisionMaking bunnyDecider;
private BunnyContext bunnyContext;

private void Start() 
{
    bunnyContext = new BunnyContext(this, aiGuid);
    bunnyDecider.Init(bunnyContext);
}
```

Whenever you want agent to make a decision, you should call `Decide(Context context)` method of the `DecisionMaking`. You'll probably want to update context before that, too. Example:

```
bunnyContext.UpdateContext();
string newActionName = bunnyDecider.Decide(bunnyContext);
```

## How does it work

###Simply:
For each agent you'll define some needs, properties or states as inputs and you'll define how agent perceives each action's utility based on some of those inputs. Comparing the utility values for each action determines the action that the agent should take.

###In-depthly:
AI agent's UAI system has needs as input and utility values for each action as the output. The Utility system is a graph like set where all of the inputs go from Scorer nodes, to QualiScorer or Qualifier nodes, which represent a single action.

1. Scorer node - It represents a single input into the system. Input value is gathered from the context based on the key (string) property of this Scorer. That value is that parsed through uFunction (AnimationCurve) to get the output value of the Scorer. It provides float value as an output (0.0 - 1.0).
    
2. QualiScorer node - Used to gather and sum up multiple Scorer or QualiScorer nodes. It provides float value (0.0 - 1.0) as an output. Input - Scorer and QualiScorer nodes that are participating in the summation. Each input also has a weight, so that certain inputs can be more or less influential on the output. The summation type of QualiScorer can be changed, and currently following are supported:
    - SumOfChildren - Output is arithmetic sum of the weighted input values.
    - AllOrNothing - Output is calculated the same as SumOfChildren, but only if the unweighted value of each input node is greater or equal than the threshold value of this QualiScorer. Otherwise, the output is zero.
    - Fixed - Output is a constant equal to the threshold value.
    - SumIfAboveThreshold - Output is calculated the same as SumOfChildren, but input nodes that have value lesser than the threshold value are ignored.
    - InvertedSumOfChildren - Output is calculated as a opposite of SumOfChildren, meaning (1 - SumOfChildren value)

3. Qualifier node - Used as a node to select an action. It is functionally the same as QualiScorer node, except it has a property actionName (string) to indicate action that it represents.

4. Selector node - This is the only unique node in each utility system. It can be viewed as a root if the system is viewed as a tree. All qualifier nodes must be inputs to this node. Whenever decision making should be calculated, it should be called through this node. The action that is chosen when all qualifiers calculate their value is based on the Selection type, which can be:
    - Best - Action of the qualifier with highest value is chosen.
    - Random from Best N - Action is randomly chosen from the N qualifiers with highest values. N is a property of the selector.
    - Weighted random from Best N - Same as Random from Best N, but qualifiers are weighted with their values, so better scoring qualifiers have higher chances of being picked
    - Random from Top% - Action is randomly chosen between all qualifiers whose values are in the margin of highest scoring qualifier's value minus p%. p% is a property of the selector.
    - Weighted random from Top% - Same as Random from Top%, but qualifiers are weighted with their values, so better scoring qualifiers have higher chances of being picked
    - Random - True random. Any action has equal chance of being picked. Is it AI at this point?