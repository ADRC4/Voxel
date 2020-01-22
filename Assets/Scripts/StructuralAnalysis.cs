using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BriefFiniteElementNet;
using BriefFiniteElementNet.Elements;
using DenseGrid;

public class StructuralAnalysis : MonoBehaviour
{
    // UI
    [SerializeField]
    GUISkin _skin = null;

    bool _toggleVoids = true;
    bool _toggleTransparency = false;
    float _displacement = 10f;
    float _tempDisplacement = 10f;
    string _voxelSize = "0.8";
    Rect _windowRect = new Rect(20, 20, 150, 160);

    // grid
    Grid3d _grid = null;
    GameObject _voids;
    Mesh[] _meshes;

    void Start()
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
            ToggleVoids();
        }

        _toggleTransparency = GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleTransparency, "Transparent");

        if (_grid != null)
            _tempDisplacement = GUI.HorizontalSlider(new Rect(s, s * i++, 100, 20), _tempDisplacement, 0, 500);
    }

    void Update()
    {
        if (_grid == null) return;

        if (_displacement != _tempDisplacement && Time.frameCount % 10 == 0)
        {
            _displacement = _tempDisplacement;
            MakeVoxelMesh();
        }

        Drawing.DrawMesh(_toggleTransparency, _meshes);
    }

    void ToggleVoids()
    {
        _toggleVoids = !_toggleVoids;

        foreach (var r in _voids.GetComponentsInChildren<Renderer>())
            r.enabled = _toggleVoids;
    }

    void MakeGrid()
    {
        var colliders = _voids
                      .GetComponentsInChildren<MeshCollider>()
                      .ToArray();

        var voxelSize = float.Parse(_voxelSize);

        _grid = Grid3d.MakeGridWithVoids(colliders, voxelSize);
        Analysis();
        MakeVoxelMesh();
        ToggleVoids();
    }

    void Analysis()
    {
        // analysis model
        var model = new Model();

        var corners = _grid.GetCorners()
            .Where(c => c.GetConnectedVoxels().Any(v => v.IsActive))
            .Select(c => new FeaCorner(c))
            .ToList();

        var nodes = corners.Select(c => c.Node).ToArray();

        var elements = _grid.GetVoxels()
             .Where(b => b.IsActive)
             .SelectMany(v => MakeTetrahedra(v))
             .ToArray();

        model.Nodes.Add(nodes);
        model.Elements.Add(elements);

        model.Solve();

        // analysis results
        foreach (var corner in corners)
        {
            var d = corner.Node
           .GetNodalDisplacement(LoadCase.DefaultLoadCase)
           .Displacements;

            corner.Displacement = new Vector3((float)d.X, (float)d.Z, (float)d.Y);
            var length = corner.Displacement.magnitude;

            foreach (var voxel in corner.GetConnectedVoxels())
                voxel.Value += length;
        }

        var activeVoxels = _grid.GetVoxels().Where(v => v.IsActive);

        foreach (var voxel in activeVoxels)
            voxel.Value /= voxel.GetCorners().Count();

        var min = activeVoxels.Min(v => v.Value);
        var max = activeVoxels.Max(v => v.Value);

        foreach (var voxel in activeVoxels)
            voxel.Value = Mathf.InverseLerp(min, max, voxel.Value);
    }

    public void MakeVoxelMesh()
    {
        _meshes = _grid.GetVoxels()
            .Where(v => v.IsActive)
            .Select(v =>
            {
                var corners = v.GetCorners()
                        .Select(c => c.Position + ((c as FeaCorner).Displacement * _displacement))
                        .ToArray();

                return Drawing.MakeTwistedBox(corners, v.Value, null);
            }).ToArray();
    }

    public IEnumerable<Tetrahedral> MakeTetrahedra(Voxel voxel)
    {
        var c = voxel.GetCorners().ToArray();

        var t = new[,]
        {
           { c[0], c[1], c[2], c[4]},
           { c[3], c[1], c[2], c[7]},
           { c[1], c[2], c[4], c[7]},
           { c[4], c[5], c[7], c[1]},
           { c[4], c[7], c[6], c[2]}
        };

        for (int i = 0; i < 5; i++)
        {
            var tetra = new Tetrahedral() { E = 210e9, Nu = 0.33 };

            for (int j = 0; j < 4; j++)
                tetra.Nodes[j] = (t[i, j] as FeaCorner).Node;

            yield return tetra;
        }
    }
}

class FeaCorner : Corner
{
    public Node Node;
    public Vector3 Displacement;

    public FeaCorner(Corner corner) : base(corner)
    {
        Node = new Node
        {
            Location = new Point(corner.Position.x, corner.Position.z, corner.Position.y),
            Constraints = corner.Index.y == 0 ? Constraints.Fixed : Constraints.RotationFixed
        };

        Node.Loads.Add(new NodalLoad(new Force(0, 0, -2000000, 0, 0, 0)));
    }
}