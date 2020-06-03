using UnityEngine;

public struct VoxelPoint
{
    public Vector3Int localPosition;
    public float density;
    public int materialID;

    public VoxelPoint(Vector3Int localPosition, float density, int materialID)
    {
        this.localPosition = localPosition;
        this.density = density;
        this.materialID = materialID;
    }
}