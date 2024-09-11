using Godot;
using System;
using System.Collections.Generic;

public struct MarchingSquaresResult
{
    public Vector3[] Vertices;
    public Vector3[] Normals;
    public int[] Triangles;
}

public class MarchingSquares
{
    private float iso;
    private Dictionary<int, Vector3> indexToVertex = new();
    private Dictionary<Vector3, int> vertexToIndex = new();
    private List<int> triangles = new();

    private int[][] triTable = new int[16][]
    {
        new int[] { },
        new int[] { 6, 2, 1 },
        new int[] { 1, 3, 7 },
        new int[] { 6, 2, 3, 6, 3, 7 },
        new int[] { 0, 5, 3 },
        new int[] { 0, 5, 3, 6, 2, 1 },
        new int[] { 1, 0, 5, 1, 5, 7 },
        new int[] { 6, 2, 7, 2, 0, 7, 0, 5, 7 },
        new int[] { 2, 4, 0 },
        new int[] { 6, 4, 0, 6, 0, 1 },
        new int[] { 2, 4, 0, 1, 3, 7 },
        new int[] { 6, 4, 0, 6, 0, 3, 6, 3, 7 },
        new int[] { 2, 4, 5, 2, 5, 3 },
        new int[] { 6, 4, 1, 1, 4, 3, 4, 5, 3 },
        new int[] { 2, 4, 5, 2, 5, 1, 1, 5, 7 },
        new int[] { 6, 4, 5, 6, 5, 7 }
    };

    public MarchingSquares(float iso = 0.5f)
    {
        this.iso = iso;
    }

    private Vector2 Interpolate(Vector2 p1, Vector2 p2, float v1, float v2)
    {
        if (Math.Abs(iso - v1) < 0.00001) return (p1);
        if (Math.Abs(iso - v2) < 0.00001) return (p2);
        if (Math.Abs(v1 - v2) < 0.00001) return (p1);
        float mu = (iso - v1) / (v2 - v1);
        return new Vector2(p1.X + mu * (p2.X - p1.X), p1.Y + mu * (p2.Y - p1.Y));
    }

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        triangles.Add(InsertVertex(v1));
        triangles.Add(InsertVertex(v2));
        triangles.Add(InsertVertex(v3));
    }

    private int InsertVertex(Vector3 v)
    {
        if (vertexToIndex.TryGetValue(v, out var index))
        {
            return index;
        }

        index = indexToVertex.Count;
        indexToVertex.Add(index, v);
        vertexToIndex.Add(v, index);
        return index;
    }

    private int GetIndex(int x, int y, int size)
    {
        return y * size + x;
    }

    public MarchingSquaresResult BuildMesh(float[] data, int size, float scale)
    {
        var localVertices = new Vector2[8];
        for (int y = 0; y < size - 1; y++)
        {
            for (int x = 0; x < size - 1; x++)
            {
                float bl = data[GetIndex(x, y, size)];
                float br = data[GetIndex(x + 1, y, size)];
                float tr = data[GetIndex(x + 1, y + 1, size)];
                float tl = data[GetIndex(x, y + 1, size)];

                int index = 0;
                if (bl > iso) index |= 1;
                if (br > iso) index |= 2;
                if (tr > iso) index |= 4;
                if (tl > iso) index |= 8;

                localVertices[0] = Interpolate(new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), tl, tr); // Top
                localVertices[1] = Interpolate(new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f), bl, br); // Bottom
                localVertices[2] = Interpolate(new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, -0.5f), tl, bl); // Left
                localVertices[3] = Interpolate(new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), tr, br); // Right

                localVertices[4] = new Vector2(-0.5f, 0.5f); // Top left
                localVertices[5] = new Vector2(0.5f, 0.5f); // Top right
                localVertices[6] = new Vector2(-0.5f, -0.5f); // Bottom left
                localVertices[7] = new Vector2(0.5f, -0.5f); // Bottom right

                for (int i = 0; i < localVertices.Length; i++)
                {
                    localVertices[i] += new Vector2(0.5f, 0.5f);
                }

                var localTriangles = triTable[index];
                for (int i = 0; i < localTriangles.Length; i += 3)
                {
                    var a = localVertices[localTriangles[i]];
                    var b = localVertices[localTriangles[i + 1]];
                    var c = localVertices[localTriangles[i + 2]];
                    AddTriangle(new Vector3(x + a.X, y + a.Y, 0), new Vector3(x + b.X, y + b.Y, 0),
                        new Vector3(x + c.X, y + c.Y, 0));
                }
            }
        }

        var res = new MarchingSquaresResult();
        res.Vertices = new Vector3[indexToVertex.Count];
        res.Normals = new Vector3[indexToVertex.Count];
        res.Triangles = triangles.ToArray();

        for (int i = 0; i < indexToVertex.Count; i++)
        {
            res.Vertices[i] = indexToVertex[i] * scale;
            res.Normals[i] = new Vector3(0, 0, 1);
        }

        return res;
    }
}