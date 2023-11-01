using UnityEngine;
/// <summary>
/// 游戏特定计算公式
/// </summary>
public static class Formula
{
    /// <summary>
    /// 计算向量与指向正右向量形成的角度（360）
    /// </summary>
    /// <param Name="dir"></param>
    /// <returns></returns>
    public static float CalculateAngle(Vector2 dir)
    {
        float angle = Vector2.Angle(Vector2.right, dir);
        if (dir.y < 0)
        {
            angle *= -1;
            angle += 360;
        }
        return angle;
    }
}
