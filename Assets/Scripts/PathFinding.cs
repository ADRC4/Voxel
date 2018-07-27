using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QuickGraph;

public class PathFinding : MonoBehaviour
{
    Grid3d _grid = null;
    GameObject _voids;
    bool _toggleVoids = true;
    bool _toggleTransparency = false;
    string _voxelSize = "0.8";

    [SerializeField]
    GUISkin _skin;

    private void Awake()
    {
        _voids = GameObject.Find("Voids");
    }

    private void Start()
    {
        MakeGrid();
        ToggleVoids();
    }

    void Update()
    {
        if (_grid == null) return;

        foreach (var face in _grid.Faces.Where(f => f.Voxels.Any(v => v != null && v.IsActive)))
        {
            if (face.Geometry == null)
                face.Geometry = Drawing.MakeFace(face.Center, face.Direction, _grid.VoxelSize, 1);

            Drawing.DrawMesh(false, face.Geometry);
        }
    }

    void OnGUI()
    {
        int i = 1;
        int s = 25;
        GUI.skin = _skin;

        _voxelSize = GUI.TextField(new Rect(s, s * i++, 100, 20), _voxelSize);

        if (GUI.Button(new Rect(s, s * i++, 100, 20), "Generate"))
        {
            MakeGrid();
        }

        if (_toggleVoids != GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleVoids, "Show voids"))
        {
            ToggleVoids();
        }

        _toggleTransparency = GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleTransparency, "Transparent");
    }

    void MakeGrid()
    {
        // create grid with voids
        var colliders = _voids
                      .GetComponentsInChildren<MeshCollider>()
                      .ToArray();

        var voxelSize = float.Parse(_voxelSize);
        _grid = new Grid3d(colliders, voxelSize);

        // select edges of boundary faces
        var edges = _grid.Edges.Where(e => e.ClimbableFaces.Length == 2);

        // create graph from edges
        var graphEdges = edges.Select(e => new Edge<Face>(e.ClimbableFaces[0], e.ClimbableFaces[1]));
        var graph = graphEdges.ToUndirectedGraph<Face, Edge<Face>>();

        // start face for shortest path
        var start = _grid.Faces.Where(f => f.IsClimbable).Skip(10).First();

        // calculate shortest path from start face to all boundary faces
        var shortest = QuickGraph.Algorithms.AlgorithmExtensions.ShortestPathsDijkstra(graph, e => 1.0, start);

        // create a mesh face for every outer face colored based on the path length
        foreach (var face in _grid.Faces.Where(f => f.IsClimbable))
        {
            float t = 1;

            if (face == start)
            {
                t = 1;
            }
            else
            {
                IEnumerable<Edge<Face>> path;
                if (shortest(face, out path))
                {
                    t = path.Count() * 0.06f;
                    t = Mathf.Clamp01(t);
                }
            }

            face.Geometry = Drawing.MakeFace(face.Center, face.Direction, _grid.VoxelSize, t);
        }
    }

    void ToggleVoids()
    {
        _toggleVoids = !_toggleVoids;

        foreach (var r in _voids.GetComponentsInChildren<Renderer>())
            r.enabled = _toggleVoids;
    }
}