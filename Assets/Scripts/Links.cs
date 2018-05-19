using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BriefFiniteElementNet;
using BriefFiniteElementNet.Elements;
using Constraint = BriefFiniteElementNet.Constraints;
using System;

public class Link
{
    public Voxel Start;
    public Voxel End;
    // public FrameElement2Node Frame;

    public bool IsActive => Start.IsActive && End.IsActive;

    public Link(Voxel start, Voxel end)
    {
        Start = start;
        End = end;
        start.Links.Add(this);
        end.Links.Add(this);

        //Frame = new FrameElement2Node(start, end)
        //{
        //    Iy = 0.02,
        //    Iz = 0.02,
        //    A = 0.01,
        //    J = 0.05,
        //    E = 210e9,
        //    G = 70e9,
        //    ConsiderShearDeformation = false,
        //};
    }
}
