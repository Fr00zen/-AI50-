using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MiscUtil.IO;
using System.Linq;

public class Agent_ACO : MonoBehaviour
{
    public bool isGenerated = false;
    public float speed = 1f;
    public bool isGraphLoading = false;
    public Node startPoint;


    private Vector3 oldPos;

    private LoadGraph loadGraph;
    private GraphGenerator graphGenerator;
    private AgentManageur agentManageur;
    private AgentGestionnaire agentGestionnaire;

    [NonSerialized] private Node node;
    public Node destination;

    [NonSerialized] private List<Node> priorityQueue;
    public List<Node> pathToNode;

    private Dictionary<Node, int> nodeAssignated;
    private Dictionary<(Node, Node), List<Node>> shortestPathData;

    private Graph graph;

    //private bool isRandomDestination = false;
    private bool clear = false;

    private GameObject capsule;
    private Renderer capsuleRenderer;

    private Color patrollingColor1;
    private Color patrollingColor2;
    private Color patrollingColor3;

    public Manager_ACO manag;

    public bool startIsDOne = false;

    

    public IEnumerator Start()
    {
        yield return new WaitUntil(() => FindObjectOfType<AStar_ACO>().isGenerated);

        transform.position = startPoint.realPos;
        node = startPoint;

        // Liaison entre les �venements et leurs m�thodes
        //EventManager.current.onNodeTaggedVisited += OnNodeVisited;
        //EventManager.current.onUpdateNodeAssignation += OnUpdateNodeAssignation;
        //EventManager.current.onSendShortestPathData += OnSendShortestPathData;

        //Set position of agent
        //transform.position = startPoint.realPos;

        // R�cup�ration des instance d'autres scripts
        graphGenerator = FindObjectOfType<GraphGenerator>();
        //agentManageur = FindObjectOfType<AgentManageur>();
        //agentGestionnaire = FindObjectOfType<AgentGestionnaire>();
        capsuleRenderer = GetComponentInChildren<Renderer>();
        loadGraph = FindObjectOfType<LoadGraph>();

        // Setup des variables
        

        //priorityQueue = new List<Node>();
        pathToNode = new List<Node>();

        //nodeAssignated = new Dictionary<Node, int>();
        //shortestPathData = new Dictionary<(Node, Node), List<Node>>();

        patrollingColor1 = new Color(0.5f, 0.9f, 0.2f); // Green
        patrollingColor2 = new Color(1f, 0.2f, 0.5f); // Red
        patrollingColor3 = new Color(0.2f, 0.5f, 1f); // Blue


        //node = graph.nodes.Values.OrderBy(x => Vector3.Distance(transform.position, new Vector3(x.pos.Item1, 0, x.pos.Item2))).First();
        //node.WarnAgentVisit();

        oldPos = transform.position;
        //isGenerated = true;
        //FindObjectOfType<TimeManager>().delta = 1;

        manag = FindObjectOfType<Manager_ACO>();
        //pathToNode = manag.realTimeNodes;
        //print("---");
        //print("Select nodes :" + manag.realTimeNodes.Count);
        //print("---");
        foreach (var s in manag.realTimeNodes)
        {
            pathToNode.Add(s);
            
        }

        //foreach (var g in pathToNode) print("jk :" + g.realPos);
        //startPoint = pathToNode[0];
        
        //transform.position = startPoint.realPos;
        destination = null;
        //print("FIN DU START");
        //print("---");
        //print("pathnode count au start  :" + pathToNode.Count);
        //print("---");
        startIsDOne = true;
    }

    private void Update()
    {
        if(startIsDOne && node != null)
        {
            //print(destination.realPos + " dest");
            //print(transform.position + "pos reel");
            //print("destination" + destination);
            //int inde = 0;
            if (destination == null)
            {
                destination = pathToNode[0]; // donne ton prochain node
                
            }
            GoToDestination();
            //if (destination.realPos == transform.position) // Quand on arrive a destination
            //{
                //print("I DID IT BROOOOOOOOO !!!!!");
                //print("pathnode count au update :" + pathToNode.Count);
                //if (inde < pathToNode.Count)
                //{
                    //inde++;
                    //oldPos = transform.position;
                    //destination = pathToNode[inde];
                    //GoToDestination();

                //}

                //else
                //{
                    //inde = 0;
                    //oldPos = transform.position;
                    //destination = pathToNode[inde];
                    //GoToDestination();
                //}
            //}
        }
       
    }

    // M�thode permettant de faire avancer l'agent vers sa prochaine destination.
    private void GoToDestination()
    {
        speed = Mathf.Abs(speed);
        if (transform.position != oldPos)
        {
            throw new Exception("Erreur : la position a �t� modifi� par un autre script.");
        }
        float mouvement = speed * Time.deltaTime;
        GoToDestination(mouvement);
        oldPos = transform.position;
    }

    // M�thode permettant de faire d�placer l'agent vers sa prochaine destination.
    private void GoToDestination(float mouvement)
    {
        if (destination != null)
        {
            Vector3 moveToward = Vector3.MoveTowards(transform.position, destination.realPosFromagentHeights, mouvement);
            mouvement -= Vector3.Distance(transform.position, moveToward);
            transform.position = moveToward;
            if (Vector3.Distance(moveToward, destination.realPosFromagentHeights) < 0.01)
            {
                
                node.agentPresence = false;
                node = destination;
                node.WarnAgentVisit();
                pathToNode.Add(pathToNode[0]);
                pathToNode.RemoveAt(0);
                destination = null;
               
            }
        }
    }

    public void SetList(List<Node> n)
    {
        pathToNode.Clear();
        foreach (var y in n) pathToNode.Add(y);
    }
    // M�thode li� � l'�venement d'update de nodeAssignation. Clear la liste existante et r�cup�re les nodes pr�sent dans la nouvelle qui lui sont assign�s..
  
    // M�thode renvoyant le node sur lequel se trouve l'agent.
    public Node GetNode()
    {
        return node;
    }


}
