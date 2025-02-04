using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QLearningPatrolling : MonoBehaviour
{

    private GraphGenerator graphGeneratorInstance;
    private DataQLearning dataQLearningInstance;
    private Graph graph;
    private int nbState;
    private Dictionary<(Node,int) , float> Q;
    private float time;
    private List<int> listAction;
    private Node firstState;
    private bool ready = false;
    private System.Random random;
    private Vector3 newPosition;
    private int saveIterator = 0;
    private Vector3 diff;
    private float rewardValue = 1;

    public int nbAction = 8;
    public Component agent;
    public float gamma = 0.9f;
    public float epsilonRange = 0.3f;
    public float vitesse = 4.0f;
    public int range = 2;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        random = new System.Random();
        newPosition = new Vector3();
        diff = new Vector3();
        graphGeneratorInstance = GameObject.Find("Sols").GetComponent<GraphGenerator>();
        dataQLearningInstance = GameObject.Find("Sols").GetComponent<DataQLearning>();
        yield return new WaitUntil(() => graphGeneratorInstance.isGenerated);
        yield return new WaitUntil(() => dataQLearningInstance.isGenerated);
        print("Ca y est on peut commencer � apprendre !");
        Q = dataQLearningInstance.Q;
        listAction = new List<int> { 0,1,2,3,4,5,6,7 };
        graph = graphGeneratorInstance.graph;
        //nbState = graph.nodes.Count;
        //setQToZero();
        //setTimeSinceLastVisit(graph);
        firstState = graph.nodes[((int)agent.transform.position.x, (int)agent.transform.position.z)];
        firstState.timeSinceLastVisit = 0;
        newPosition = agent.transform.position;
        ready = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (ready == true)
        {
            // Permet d'avoir un d�sordre dans les actions (0,1,2,3 => 1,2,3,0 => 2,3,0,1 => 3,0,1,2 => ...)
            listAction.Add(listAction[0]);
            listAction.RemoveAt(0);          
            
            // D�placement vers la nouvelle position
            if (isPositionReach(agent.transform.position, newPosition))
            {
                // Run d'une it�ration du Q learning
                firstState = runQLearning(firstState, gamma, epsilonRange, listAction, graph);
                saveIterator++;
                // Calcul de la nouvelle position vers laquelle se diriger pour l'agent
                newPosition.Set(firstState.pos.Item1, agent.transform.position.y, firstState.pos.Item2);
                diff = newPosition - agent.transform.position;
            }

            // Mouvement de l'agent
            
            Vector3 diffTest = newPosition - agent.transform.position;
            agent.transform.Translate(diff * Time.deltaTime * vitesse);

            // Toutes les 200 it�rations, on save Q dans via le script DataQLearning dans le fichier text d�di� � cel�.
            if (saveIterator > 200)
            {
                print("Save Q");
                saveIterator = 0;

                // On met en pause le script en question le temps que la sauvegarde se fasse
                //Time.timeScale = 0;
                dataQLearningInstance.isSaved = false;
                dataQLearningInstance.save();
                StartCoroutine(pauseUntilSaveIsDone());

            }
        }
    }

    IEnumerator pauseUntilSaveIsDone()
    {
        yield return new WaitUntil(() => dataQLearningInstance.isSaved);
    }

    bool isPositionReach(Vector3 agentPosition, Vector3 positionToReach)
    {
        Vector3 diff = agentPosition - positionToReach;
        if (diff.x <= 0.2 && diff.x >= -0.2 && diff.z <= 0.2 && diff.z >= -0.2)
        {
            return true;
        }
        return false;
    }
    
    // The reward function : check if the nextState has the best value in "timeSinceLastVisit". If yes, reward = rewardValue, if no, reward  = -10
    float getReward(Node state, Node nextState, Graph graph)
    {
        List<Node> neighbours = new List<Node>();
        foreach (Edge edge in state.neighs)
        {
            neighbours.Add(edge.to);
        }

        float lastVisitedValueWithBestAction = nextState.timeSinceLastVisit;
        foreach(Node neighbour in neighbours)
        {
            if (lastVisitedValueWithBestAction < neighbour.timeSinceLastVisit)
            {
                return -2;
            }
        }
        return rewardValue;

    }

    int getBestAction_2(Node state, float epsilonRange)
    {
        print("New bestAction");
        // Random choice with epsilon
        if (random.NextDouble() < epsilonRange)
        {
            return listAction[0];
        }

        // If not random, check the best action to do with the timeSinceLastVisit value
        // Get all neighbours of actual state, check which one has the best timeSincelastUpdate value, and the action that lead to this state for nextState
        List<Node> neighbours = new List<Node>();
        neighbours = getAllNeighboursByRange(state, range);

        Node neighbourSelected = neighbours[0];
        foreach (Node neighbour in neighbours)
        {
            if (neighbour.timeSinceLastVisit > neighbourSelected.timeSinceLastVisit)
            {
                neighbourSelected = neighbour;
            }
        }
        return getActionWithTwoState(state, neighbourSelected);
    }

    List<Node> getAllNeighboursByRange(Node state, int range)
    {
        List<Node> neighbours = new List<Node>();
        List<Node> neighboursSub = new List<Node>();

        // Get all first neighbours of the state
        foreach (Edge edge in state.neighs)
        {
            neighbours.Add(edge.to);
        }

        // Get all neighbours that are not already in the list (every time you do a for iteration, it will add neighbours for next range)
        for (int i = 1 ; i <= range ; i++)
        {
            print("Iteration : " + i);
            foreach (Node node in neighbours)
            {
                foreach (Edge edge in node.neighs)
                {
                    if (!neighbours.Contains(edge.to) && !edge.to.Equals(state))
                    {
                        neighboursSub.Add(edge.to);
                    }
                }
            }
            neighbours.AddRange(neighboursSub);
        }
        return neighbours;
    }

    int getActionWithTwoState(Node state, Node neighbour)
    {
        print("state 1 : (" + state.pos.Item1 + "," + state.pos.Item2 + ") and neigh (" + neighbour.pos.Item1 + "," + neighbour.pos.Item2 + ")");
        bool isRealNeighbour = false;
        bool setup = false;
        bool isBreak = false;
        List<Node> potentialNeighbours = new List<Node>();
        List<Node> potentialNeighboursSub = new List<Node>();

        foreach (Edge edge in neighbour.neighs)
        {
            if (edge.to.Equals(state))
            {
                isRealNeighbour = true;
            }
        }

        while (!isRealNeighbour)
        {
            if (!setup)
            {
                
                foreach (Edge edge in neighbour.neighs)
                {
                    potentialNeighbours.Add(edge.to);
                    print("Adding (" + edge.to.pos.Item1 + "," + edge.to.pos.Item2 + ") to potentialNeighbours.");
                }
                setup = true;
                print("SetUpDone");
            }

            foreach(Node node in potentialNeighbours)
            {
                if (isBreak)
                {
                    break;
                }
                foreach(Edge edge in node.neighs)
                {
                    if (edge.to.Equals(state))
                    {
                        neighbour = node;
                        isRealNeighbour = true;
                        print("IsReal == true");
                        isBreak = true;
                        break;
                        
                    }
                    else
                    {
                        if (!potentialNeighbours.Contains(edge.to))
                        {
                            print("Adding (" + edge.to.pos.Item1 + "," + edge.to.pos.Item2 + ") to potentialNeighbours.");
                            potentialNeighboursSub.Add(edge.to);
                        }
                    }
                }
            }
            potentialNeighbours.AddRange(potentialNeighboursSub);
            print("Tour de while");
        }

        print("Check finito, on a r�cup notre voisin le plus proche");
        print("State : (" + state.pos.Item1 + "," + state.pos.Item2 + ") ; neighbour : (" + neighbour.pos.Item1 + "," + neighbour.pos.Item2 + ")");

        // UP
        if (state.pos.Item2 == neighbour.pos.Item2 - 1 && state.pos.Item1 == neighbour.pos.Item1)
        {
            return 0;
        }
        // Down
        if (state.pos.Item2 == neighbour.pos.Item2 + 1 && state.pos.Item1 == neighbour.pos.Item1)
        {

            return 1;
        }
        // Left
        if (state.pos.Item1 == neighbour.pos.Item1 + 1 && state.pos.Item2 == neighbour.pos.Item2)
        {

            return 2;
        }
        // Right
        if (state.pos.Item1 == neighbour.pos.Item1 - 1 && state.pos.Item2 == neighbour.pos.Item2)
        {

            return 3;
        }
        // TOP LEFT
        if (state.pos.Item1 == neighbour.pos.Item1 + 1 && state.pos.Item2 == neighbour.pos.Item2 - 1)
        {
            return 4;
        }
        // TOP RIGHT
        if (state.pos.Item1 == neighbour.pos.Item1 - 1 && state.pos.Item2 == neighbour.pos.Item2 - 1)
        {
            return 4;
        }
        // BOT LEFT
        if (state.pos.Item1 == neighbour.pos.Item1 + 1 && state.pos.Item2 == neighbour.pos.Item2 + 1)
        {
            return 4;
        }
        // BOT RIGHT
        if (state.pos.Item1 == neighbour.pos.Item1 - 1 && state.pos.Item2 == neighbour.pos.Item2 + 1)
        {
            return 4;
        }

        print("Aucune best action trouv�");
        return -1;
    }

    // Method that return the next node after doing this action in actual node
    // Actions are corresponding to : 0 = UP, 1 = DOWN, 2 = LEFT, 3 = RIGHT
    Node getNextState(Node state, int action, Graph graph)
    {
        int x = state.pos.Item1;
        int z = state.pos.Item2;

        if (action == 0)
        {
            z = z + 1;
        }
        else if (action == 1)
        {
            z = z - 1;
        }
        else if (action == 2)
        {
            x = x - 1;
        }
        else if (action == 3)
        {
            x = x + 1;
        }
        else if (action == 4)
        {
            x = x + 1;
            z = z - 1;
        }
        else if (action == 5)
        {
            x = x + 1;
            z = z + 1;
        }
        else if (action == 6)
        {
            x = x - 1;
            z = z - 1;
        }
        else if (action == 7)
        {
            x = x - 1;
            z = z + 1;
        }

        if (graph.nodes.ContainsKey((x, z)))
        {
            return graph.nodes[(x, z)];
        }
        return state;
 
    }

    Node runQLearning(Node state,float gamma, float epsilonRange, List<int> listAction, Graph graph)
    {
        // getBestAction(state,epsilonRange) give best action due to Q, getBestActio_2 give best action due to the best neighbour timeSinceLastVisit value
        int bestAction = getBestAction_2(state, epsilonRange);
        Node nextState = getNextState(state, bestAction, graph);
        float reward = getReward(state, nextState, graph);
        print("Action : " + bestAction + " ; nextState : (" + nextState.pos.Item1 + "," + nextState.pos.Item2 + ")");
        Q[(state, bestAction)] = (1 - gamma) * Q[(state, bestAction)] + gamma * (reward + gamma * getMaxNextState(state,listAction));
        state.timeSinceLastVisit = 0;
        return nextState;
    }

    // Return the best action due to Q values
    int getBestAction(Node state, float epsilonRange)
    {
        float bestActionValue = -99999;
        int bestAction = -3;

        foreach (int action in listAction)
        {
            if (random.NextDouble() < epsilonRange)
            {
                return action;
            }
            if (bestActionValue < Q[(state, action)])
            {
                bestAction = action;
            }
        }
        return bestAction;
    }

    // Return the max value in Q for a given state and all his possible action
    float getMaxNextState(Node state, List<int> listAction)
    {
        List<float> listValue = new List<float>();

        foreach (int i in listAction)
        {
            listValue.Add(Q[(state, i)]);
        }

        return listValue.Max();
    }
}
