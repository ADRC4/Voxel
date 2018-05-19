﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class Drawing : MonoBehaviour
{
    [SerializeField]
    Mesh _box;
    [SerializeField]
    Mesh _cylinder;
    [SerializeField]
    Material _opaque;
    [SerializeField]
    Material _transparent;

    static Drawing _instance;
    static Gradient _gradient = new Gradient();


    void Awake()
    {
        _instance = this;

        var gck = new GradientColorKey[2];
        var gak = new GradientAlphaKey[0];
        gck[0].color = Color.green;
        gck[0].time = 0.0F;
        gck[1].color = Color.red;
        gck[1].time = 1.0F;
        _gradient.SetKeys(gck, gak);

        var texture = new Texture2D(256, 1)
        {
            wrapMode = TextureWrapMode.Clamp,
        };

        for (int i = 0; i < 256; i++)
        {
            float t = i / 255f;
            var color = _gradient.Evaluate(t);
            texture.SetPixel(i, 0, color);
        }

        texture.Apply();

        _opaque.mainTexture = texture;
        _transparent.mainTexture = texture;
    }

    public static void DrawCube(Vector3 center, float size)
    {
        var matrix = Matrix4x4.TRS(
                center,
                Quaternion.identity,
                Vector3.one * (size * 0.999f)
                );

        Graphics.DrawMesh(_instance._box, matrix, _instance._transparent, 0);
    }

    public static void DrawBar(Vector3 start, Vector3 end, float radius, float t)
    {
        var color = _gradient.Evaluate(t);
        var properties = new MaterialPropertyBlock();
        properties.SetColor("_Color", color);
        properties.SetTexture("_MainTex", null);

        var vector = end - start;

        var matrix = Matrix4x4.TRS(
                        start + vector * 0.5f,
                        Quaternion.LookRotation(vector) * Quaternion.Euler(90, 0, 0),
                        new Vector3(radius, vector.magnitude * 0.5f, radius)
                        );

        Graphics.DrawMesh(_instance._cylinder, matrix, _instance._opaque, 0, null, 0, properties);
    }

    public static void DrawMesh(bool isTransparent, params Mesh[] mesh)
    {
        var material = isTransparent ? _instance._transparent : _instance._opaque;

        foreach (var m in mesh)
            Graphics.DrawMesh(m, Matrix4x4.identity, material, 0);
    }

    public static Mesh MakeTwistedBox(Vector3[] corners, float t, Mesh mesh = null)
    {
        Vector3 center = Vector3.zero;
        foreach (var corner in corners)
            center += corner;

        center /= corners.Length;

        corners = corners
            .Select(c => c + (center - c).normalized * 0.05f)
            .ToArray();

        var f = new[]
        {
            0,1,3,2,
            4,6,7,5,
            6,4,0,2,
            4,5,1,0,
            5,7,3,1,
            7,6,2,3
        };

        var v = f.Select(i => corners[i]).ToArray();

        if (mesh == null)
        {
            mesh = new Mesh()
            {
                vertices = v,
                uv = Enumerable.Repeat(new Vector2(t, 0), v.Length).ToArray()
            };

            var faces = Enumerable.Range(0, 24).ToArray();
            mesh.SetIndices(faces, MeshTopology.Quads, 0);
        }
        else
        {
            mesh.vertices = v;
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}