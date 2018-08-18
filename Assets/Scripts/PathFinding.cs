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
        //   Drawing.DrawMesh(false, _grid.Mesh);
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
        var mesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        var meshes = new List<CombineInstance>();

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

            meshes.Add(new CombineInstance() { mesh = faceMesh });
        }

        mesh.CombineMeshes(meshes.ToArray(), true, false, false);
        GetComponent<MeshFilter>().mesh = mesh;

        // draw a polyline for the start-end path
        {
            IEnumerable<TaggedEdge<Face, Edge>> path;
            if (shortest(end, out path))
            {
                int vertexCount = mesh.vertexCount;
                var vertices = new List<Vector3>(mesh.vertices);

                var current = start;
                vertices.Add(current.Center);

                foreach (var edge in path)
                {
                    vertices.Add(edge.Tag.Center);
                    current = edge.GetOtherVertex(current);
                    vertices.Add(current.Center);
                }

                mesh.SetVertices(vertices);
                mesh.subMeshCount = 2;
                mesh.SetIndices(Enumerable.Range(vertexCount, vertices.Count - vertexCount).ToArray(), MeshTopology.LineStrip, 1);
            }
        }
    }

    void ToggleVoids()
    {
        _toggleVoids = !_toggleVoids;

        foreach (var r in _voids.GetComponentsInChildren<Renderer>())
            r.enabled = _toggleVoids;
    }
}
