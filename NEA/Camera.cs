using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathsOperations;

internal class Camera
{
    private vec3 Pos;
    private vec3 Target;
    private float Radius;
    private float AngleX;
    private float AngleY;
    private vec3 Front;
    private vec3 Right;
    private vec3 Up;
    public float Speed;
    public float Sensitivity;
    public Camera(vec3 target, float angleX, float angleY, float radius, float speed, float sensitivity)
    {
        Target = new vec3(target);
        AngleX = angleX;
        AngleY = angleY;
        Radius = radius;
        Speed = speed;
        Sensitivity = sensitivity;
        UpdateVectors();
    }
    public void ChangeAngles(float xChange, float yChange)
    {
        AngleX -= xChange/200;
        AngleY -= yChange/200;
        if (AngleY > DegreesToRadians(89f)) AngleY = DegreesToRadians(89f);
        if (AngleY < DegreesToRadians(-89f)) AngleY = DegreesToRadians(-89f);

        UpdateVectors();
    }
    public void ChangeRadius(double x, double y)
    {
        if (y <= -1)
        {
            Radius *= 1.1f;
        }
        else
        {
            Radius /= 1.1f;
        }
        UpdateVectors();
    }
    public void ChangeTarget(vec3 target)
    {
        Target[0] = target[0];
        Target[1] = target[1];
        Target[2] = target[2];
        UpdateVectors();
    }
    public vec3 GetTarget()
    {
        return Target;
    }
    public mat4 GetViewMatrix()
    {
        mat4 result = new mat4(1f);
        for (int i = 0; i < 3; i++) result[i] = Right[i];
        for (int i = 0; i < 3; i++) result[i + 4] = Up[i];
        for (int i = 0; i < 3; i++) result[i + 8] = -Front[i];
        result = result * Translate(new mat4(1f), new vec3(-Pos[0], -Pos[1], -Pos[2]));
        return result;
    }
    private void UpdateVectors()
    {
        Pos = SphericalToXYZ(AngleX, AngleY, Radius) + Target;
        Front = Normalize(Target - Pos);
        Right = Normalize(Cross(Front, new vec3(0f, 1f, 0f)));
        Up = Normalize(Cross(Right, Front));
    }
}
