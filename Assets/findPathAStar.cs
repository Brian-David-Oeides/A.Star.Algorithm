using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.IO.Compression; // a library?

//create a new class for nodes and markers
public class Path_Marker
{
    public MapLocation location; //Maplocation? location?
    public float G;
    public float H;
    public float F; //fitness marker of neighbor determines path
    public GameObject marker;
    public Path_Marker parent; 
    public Path_Marker(MapLocation l, float g, float h, float f, GameObject marker, Path_Marker p)
    {
        location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }
    //need to compare path markers so need to create an override for the equals method
    public override bool Equals(object obj) // what is each term in syntax? Equals? override?
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType())) // GetType?
        {
        return false;
        }
        else
        {
            return location.Equals(((Path_Marker)obj).location);
        }
    }
    public override int GetHashCode()
    {
        return 0;
    }
}
public class findPathAStar : MonoBehaviour
{
    // running the algorithm than visualizing it on the screen
    public Maze maze; //Maze? 
    public Material closedMaterial; // material? change the colors of nodes as processed
    public Material openMaterial; // change the colors of nodes as processed
    List<Path_Marker> open = new List<Path_Marker>(); // creates a list of markers that open
    List<Path_Marker> closed = new List<Path_Marker>(); // creates a list of markers that close
    public GameObject start; // bring in game objects
    public GameObject end; // 
    public GameObject pathP; // bring in path point
    Path_Marker goalNode;
    Path_Marker startNode;
    Path_Marker lastPos;
    bool done = false;

    void RemoveAllMarkers() // destroys the closed markers
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in markers)
            Destroy(m);
    }

    void BeginSearch() //will be called repeatedly to Remove markers that are done
    {
        done = false;
        RemoveAllMarkers();

        List<MapLocation> locations = new List<MapLocation>(); 
        for (int z = 1; z < maze.depth - 1; z++)
            for (int x = 1; x < maze.width - 1; x++)
            {
                if (maze.map[x,z] != 1)
                    locations.Add(new MapLocation(x,z));
            }
        locations.Shuffle();
        //at the same time we create the path marker we instantiate the prefabs

        Vector3 startLocation = new Vector3(locations[0].x * maze.scale, 0,locations[0].z * maze.scale);
        startNode = new Path_Marker(new MapLocation(locations[0].x,locations[0].z),0,0,0, Instantiate(start, startLocation, Quaternion.identity),null);

        Vector3 goalLocation = new Vector3(locations[1].x * maze.scale,0,locations[1].z* maze.scale);
        goalNode = new Path_Marker(new MapLocation(locations[1].x,locations[1].z),0,0,0, Instantiate(start, goalLocation, Quaternion.identity),null);

        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;
        //pick 2 positions within the maze at start and end
    }

    void Search(Path_Marker thisNode)
    {
        //if (thisNode == null) return;
        if (thisNode.Equals(goalNode)) {done = true; return;} // goal has been found

        //start looping through neighbors/map directions
        foreach(MapLocation dir in maze.directions) 
        {
            MapLocation neighbor = dir + thisNode.location;
            if (maze.map[neighbor.x,neighbor.z] == 1) continue;
            if (neighbor.x < 1 || neighbor.x >= maze.width || neighbor.z < 1 || neighbor.z >= maze.width) continue;
            if (IsClosed(neighbor)) continue;

            float G = Vector2.Distance(thisNode.location.ToVector(), neighbor.ToVector()) + thisNode.G;
            float H = Vector2.Distance(neighbor.ToVector(), goalNode.location.ToVector()) + thisNode.G;
            float F = G + H;

            GameObject pathBlock = Instantiate(pathP, new Vector3(neighbor.x * maze.scale, 0, neighbor.z * maze.scale), Quaternion.identity);

            TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
            values[0].text = "G: " + G.ToString("0.00");
            values[0].text = "H: " + H.ToString("0.00");
            values[0].text = "F: " + F.ToString("0.00");

            if(!UpDateMarker(neighbor,G,H,F, thisNode))
                open.Add(new Path_Marker(neighbor,G,H,F, pathBlock, thisNode));
        }
        //the node with the lowest F value using the Linq library
        open = open.OrderBy(p => p.F).ThenBy(n => n.H).ToList<Path_Marker>();
        Path_Marker pm = (Path_Marker) open.ElementAt(0);
        closed.Add(pm);

        open.RemoveAt(0);
        pm.marker.GetComponent<Renderer>().material = closedMaterial;

        lastPos = pm;

    }
    bool UpDateMarker(MapLocation pos, float g, float h, float f, Path_Marker prt)
    {
        foreach (Path_Marker p in open)
        {
            if (p.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;    
                p.parent = prt;
                return true;

            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker) //custom method that checks if a neighbor is in the closed list
    {
        foreach (Path_Marker p in closed)
        {
            if (p.location.Equals(marker)) return true;
        }
        return false;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    void GetPath()
    {
        RemoveAllMarkers();
        Path_Marker begin = lastPos;

        while (!startNode.Equals(begin) && begin != null)
        {
            Instantiate(pathP, new Vector3(begin.location.x * maze.scale, 0, begin.location.z * maze.scale),
            Quaternion.identity);
            begin = begin.parent;
        }
        Instantiate(pathP, new Vector3(startNode.location.x * maze.scale,0, startNode.location.z * maze.scale),
        Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))BeginSearch();
        if(Input.GetKeyDown(KeyCode.C) && !done) Search(lastPos);
        if(Input.GetKeyDown(KeyCode.M))GetPath();
    }
}
