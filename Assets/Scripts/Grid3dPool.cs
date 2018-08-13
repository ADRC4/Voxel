using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Grid3dPool
{
    private ConcurrentBag<Grid3d> _objects;
    Grid3d _grid;

    public Grid3dPool(Grid3d grid)
    {
        _objects = new ConcurrentBag<Grid3d>();
        _grid = grid;
    }

    public Grid3d GetObject()
    {
        Grid3d item;
        if (_objects.TryTake(out item)) return item;
        return _grid.Clone();
    }

    public void PutObject(Grid3d item)
    {
        _objects.Add(item);
    }
}