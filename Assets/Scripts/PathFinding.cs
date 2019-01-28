using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QuickGraph;

public class PathFinding : MonoBehaviour
{
    // UI
    [SerializeField]
    GUISkin _skin;

    bool _toggleVoids = true;
    bool _toggleTransparency = false;
    string _voxelSize = "0.8";
    Rect _windowRect = new Rect(20, 20, 150, 160);

    // grid
    Grid3d _grid = null;
    GameObject _voids;
    Mesh[] _meshes;

    private void Start()
    {
        _voids = GameObject.Find("Voids");
    }

    void OnGUI()
    {
        GUI.skin = _skin;
        _windowRect = GUI.Window(0, _windowRect, WindowFunction, string.Empty);
    }

    void WindowFunction(int windowID)
    {
        int i = 1;
        int s = 25;

        _voxelSize = GUI.TextField(new Rect(s, s * i++, 100, 20), _voxelSize);

        if (GUI.Button(new Rect(s, s * i++, 100, 20), "Generate"))
            MakeGrid();

        if (_toggleVoids != GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleVoids, "Show voids"))
        {
            _toggleVoids = !_toggleVoids;

            foreach (var r in _voids.GetComponentsInChildren<Renderer>())
                r.enabled = _toggleVoids;
        }

        _toggleTransparency = GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleTransparency, "Transparent");
    }

    void Update()
    {
        if (_grid == null) return;

        //foreach (var face in _grid.GetFaces())
        //    Debug.DrawRay(face.Center, face.Normal * 0.2f, Color.white);

        Drawing.DrawMesh(_toggleTransparency, _meshes);
    }

    void MakeGrid()
    {
        // create grid with voids
        var colliders = _voids
                      .GetComponentsInChildren<MeshCollider>()
                      .ToArray();

        var voxelSize = float.Parse(_voxelSize);
        _grid = Grid3d.MakeGridWithVoids(colliders, voxelSize);

        // select edges of boundary faces
        var edges = _grid.GetEdges().Where(e => e.ClimbableFaces.Length == 2);

        // create graph from edges
        var graphEdges = edges.Select(e => new TaggedEdge<Face, Edge>(e.ClimbableFaces[0], e.ClimbableFaces[1], e));
        var graph = graphEdges.ToUndirectedGraph<Face, TaggedEdge<Face, Edge>>();

        // start face for shortest path
        var start = _grid.GetFaces().Where(f => f.IsClimbable).Skip(10).First();

        // calculate shortest path from start face to all boundary faces
        var shortest = QuickGraph.Algorithms.AlgorithmExtensions.ShortestPathsDijkstra(graph, e => 1.0, start);

        // select an end face to draw one specific path
        var end = _grid.GetFaces().Where(f => f.IsClimbable).Skip(200).First();

        IEnumerable<TaggedEdge<Face, Edge>> endPath;
        shortest(end, out endPath);

        // unsorted distinct faces of the path from start to end faces
        var endPathFaces = new HashSet<Face>(endPath.SelectMany(e => new[] { e.Source, e.Target }));

        // create a mesh face for every outer face colored based on the path length (except a solid yellow path to end face)
        var faceMeshes = new List<CombineInstance>();

        foreach (var face in _grid.GetFaces().Where(f => f.IsClimbable))
        {
            float t = 1;

            IEnumerable<TaggedEdge<Face, Edge>> path;
            if (shortest(face, out path))
            {
                t = path.Count() * 0.04f;
                t = Mathf.Clamp01(t);
            }

            Mesh faceMesh;

            // paint face yellow if its part of the start-end path, gradient color for other faces
            if (endPathFaces.Contains(face))
            {
                faceMesh = Drawing.MakeFace(face.Center, face.Direction, _grid.VoxelSize, 0, 1);
            }
            else
            {
                faceMesh = Drawing.MakeFace(face.Center, face.Direction, _grid.VoxelSize, t);
            }

            faceMeshes.Add(new CombineInstance() { mesh = faceMesh });
        }

        var mesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        mesh.CombineMeshes(faceMeshes.ToArray(), true, false, false);

        Mesh pathMesh = new Mesh();

        // draw a polyline for the start-end path
        {
            IEnumerable<TaggedEdge<Face, Edge>> path;
            if (shortest(end, out path))
            {
                float offset = 0.1f;
                var vertices = new List<Vector3>();

                var current = start;
                vertices.Add(current.Center + current.Normal * offset);

                foreach (var edge in path)
                {
                    vertices.Add(edge.Tag.Center + edge.Tag.Normal * offset);
                    current = edge.GetOtherVertex(current);
                    vertices.Add(current.Center + current.Normal * offset);
                }

                pathMesh.SetVertices(vertices);
                pathMesh.subMeshCount = 2;
                pathMesh.SetIndices(Enumerable.Range(0, vertices.Count).ToArray(), MeshTopology.LineStrip, 1);
            }
        }

        _meshes = new[] { mesh, pathMesh };
    }
}
