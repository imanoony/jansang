using UnityEngine;

public static class Vector2Ex
{
    public static Vector2 SetX(this Vector2 v, float x)
    {
        return new Vector2(x, v.y);
    }
    public static Vector2 SetY(this Vector2 v, float y)
    {
        return new Vector2(v.x, y);
    }
    public static Vector2 AddX(this Vector2 v, float x)
    {
        return new Vector2(v.x + x, v.y);
    }
    public static Vector2 AddY(this Vector2 v, float y)
    {
        return new Vector2(v.x, v.y + y);
    }
}

public static class Vector3Ex
{
    public static Vector3 SetX(this Vector3 v, float x)
    {
        return new Vector3(x, v.y, v.z);
    }
    public static Vector3 SetY(this Vector3 v, float y)
    {
        return new Vector3(v.x, y, v.z);
    }
    public static Vector3 SetZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }
    public static Vector3 AddX(this Vector3 v, float x)
    {
        return new Vector3(v.x + x, v.y, v.z);
    }
    public static Vector3 AddY(this Vector3 v, float y)
    {
        return new Vector3(v.x, v.y + y, v.z);
    }
    public static Vector3 AddZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, v.z + z);
    }
}

public static class ColorEx
{
    public static Color MyGray => new(0.3522012f, 0.3522012f, 0.3522012f, 1f);
    public static Color MyRed => new(0.7610062f, 0.30871f, 0.30871f, 1f);
}