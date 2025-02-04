using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class DataQLearning : MonoBehaviour
{

    private string path = Directory.GetCurrentDirectory() + "/Assets/Data/";
    public string textFileName = "QLearning.txt";
    private GraphGenerator graphGeneratorInstance;
    public Dictionary<(Node, int), float> Q;
    private Graph graph;
    public bool isGenerated = false;
    public bool isSaved = true;
    public bool hasToReset = false;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        Q = new Dictionary<(Node, int), float>();
        path = path + textFileName;
        print(path);
        graphGeneratorInstance = GameObject.Find("Sols").GetComponent<GraphGenerator>();
        yield return new WaitUntil(() => graphGeneratorInstance.isGenerated);
        graph = graphGeneratorInstance.graph;
        if (hasToReset)
        {
            generateQ();
        }
        else
        {
            Q = loadQ(path);
        }
        print("Le dictionnaire Q est pr�t.");
        isGenerated = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Set each element of Q to value = 0
    void setQToZero()
    {
        foreach (Node node in graph.nodes.Values)
        {
            Q.Add((node, 0), 0f);
            Q.Add((node, 1), 0f);
            Q.Add((node, 2), 0f);
            Q.Add((node, 3), 0f);
            Q.Add((node, 4), 0f);
            Q.Add((node, 5), 0f);
            Q.Add((node, 6), 0f);
            Q.Add((node, 7), 0f);
        }
    }

    // Save in file each element in Q, as following model : x,y,action,value
    public void saveQ(string path, Dictionary<(Node, int), float> Q)
    {
        File.WriteAllLines(path, Q.Select(kvp => string.Format("{0}|{1}|{2}|{3}", kvp.Key.Item1.pos.Item1, kvp.Key.Item1.pos.Item2, kvp.Key.Item2, kvp.Value)));
        isSaved = true;
    }

    // Rebuild Q with the given txt file (Every line is a element, and each line is like : x,y,action,value)
    Dictionary<(Node, int), float> loadQ(string path)
    {
        Dictionary<(Node, int), float> Q = new Dictionary<(Node, int), float>();
        string[] lines = File.ReadAllLines(path);
        int x, y, action;
        float value;
        Node node;

        foreach (string element in lines)
        {
            var firstLetter = element[0];
            if (firstLetter != '<' && firstLetter != '>' && firstLetter != '=')
            {
                string[] splittedLines = element.Split('|');
                x = int.Parse(splittedLines[0]);
                y = int.Parse(splittedLines[1]);
                action = int.Parse(splittedLines[2]);
                value = float.Parse(splittedLines[3]);
                if (!graph.nodes.ContainsKey((x, y)))
                {
                    node = graph.nodes[(x, y)];
                    Q.Add((node, action), value);
                }
            }
        }

        return Q;
    }

    public void save()
    {
        saveQ(path, Q);
    }


    void generateQ()
    {
        setQToZero();
        save();
    }
}
