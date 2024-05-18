using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MapData
{
    public int xSize;
    public int ySize;
    public int seed;
    public float obstacleRate;

    public MapData(int xSize, int ySize, int seed, float obstacleRate)
    {
        this.xSize = xSize;
        this.ySize = ySize;
        this.seed = seed;
        this.obstacleRate = obstacleRate;
    }
}

[Serializable]
public struct UnitData
{
    [Serializable]
    public struct UnitInfo
    {
        public Vector3 position;
    }

    public List<UnitInfo> units;
    public Vector3 targetPosition;

    public UnitData(List<UnitInfo> units, Vector3 targetPosition)
    {
        this.units = units;
        this.targetPosition = targetPosition;
    }
}