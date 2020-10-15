using UnityEngine;
[System.Serializable]
public struct Wall
{
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 scale;
    public Transform transform;
    public FourWayDirection fourWayDirection;

    public Wall(Vector3 position, Vector3 scale, Transform transform, FourWayDirection fourWayDirection)
    {
        this.position = position;
        this.scale = scale;
        this.transform = transform;
        this.fourWayDirection =fourWayDirection;
    }

}