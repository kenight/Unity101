using System;
using UnityEditor;
using UnityEngine;

public class A8Clock : MonoBehaviour
{
    public bool useSmoothSecond;

    void OnDrawGizmos()
    {
        Gizmos.matrix = Handles.matrix = transform.localToWorldMatrix;

        // 画表盘
        Handles.DrawWireDisc(default, Vector3.forward, 1);

        // 画分针, 共 60 次
        for (int i = 0; i < 60; i++)
        {
            var dir = ValueToDir(i, 60);
            DrawTick(dir, 0.05f, 1);
        }

        // 画时针, 共 12 次
        for (int i = 0; i < 12; i++)
        {
            var dir = ValueToDir(i, 12);
            DrawTick(dir, 0.08f, 3);
        }

        // 获取当前时间
        DateTime time = DateTime.Now;
        // 获取秒钟
        float seconds = time.Second;
        // 是否平滑时间, 否则是一格一格的效果
        if (useSmoothSecond)
        {
            // Millisecond 值从 0 到 1000, 除以1000, 得到 0 到 1 之间的小数
            // 相当于 normalize
            seconds += time.Millisecond / 1000f;
        }

        // 根据当前时间实时绘制秒、分、小时指针
        DrawHand(ValueToDir(seconds, 60), 0.9f, 1, Color.green);
        DrawHand(ValueToDir(time.Minute, 60), 0.7f, 3, Color.white);
        DrawHand(ValueToDir(time.Hour, 12), 0.5f, 6, Color.white);
    }

    // 将时间数值转换到角度,再将角度转换到方向
    Vector2 ValueToDir(float value, float max)
    {
        var percent = value / max;
        // percent * 2PI 转换到弧度, 偏移 0.25 使从 12 点方向开始, 取负改变方向
        var rad = (0.25f - percent) * 2 * Mathf.PI;
        // 将弧度转换为向量
        var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        return dir;
    }

    void DrawTick(Vector2 dir, float length, float thickness)
    {
        Handles.DrawLine(dir, dir * (1 - length), thickness);
    }

    void DrawHand(Vector2 dir, float length, float thickness, Color color)
    {
        using (new Handles.DrawingScope(color))
        {
            Handles.DrawLine(default, dir * length, thickness);
        }
    }
}