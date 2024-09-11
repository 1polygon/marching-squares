using Godot;

public partial class MarchingSquaresMesh : MeshInstance3D
{
    private Vector2 lastMousePosition;
    private float lastSculpt = Time.GetTicksMsec();
    private int size = 32;
    private float scale = 1.0f;
    private float[] data;

    public override void _Ready()
    {
        data = new float[size * size];

        Reset();
    }

    public override void _Process(double delta)
    {
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (Time.GetTicksMsec() - lastSculpt > 1f / 60f)
            {
                var pos = GetViewport().GetMousePosition();
                if (pos.DistanceTo(lastMousePosition) > 0)
                {
                    var camera = GetNode<Camera3D>("/root/Node3D/Camera3D");
                    Sculpt(camera.ProjectRayOrigin(pos), 0.5f);
                    lastSculpt = Time.GetTicksMsec();
                    lastMousePosition = pos;
                }
            }
        }
    }

    public void Rebuild()
    {
        var res = new MarchingSquares(0.5f).BuildMesh(data, size, scale);

        var surfaceArray = new Godot.Collections.Array();
        surfaceArray.Resize((int)Mesh.ArrayType.Max);
        surfaceArray[(int)Mesh.ArrayType.Vertex] = res.Vertices;
        surfaceArray[(int)Mesh.ArrayType.Normal] = res.Normals;
        surfaceArray[(int)Mesh.ArrayType.Index] = res.Triangles;

        var arrMesh = Mesh as ArrayMesh;
        if (arrMesh != null)
        {
            arrMesh.ClearSurfaces();
            arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
            Position = new Vector3(-(scale * size) / 2f, -(scale * size) / 2f, 0f);
        }
    }

    public void Reset()
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 1.0f;
        }

        Rebuild();
    }

    public void Sculpt(Vector3 position, float radius)
    {
        Vector3 localPosition =
            ToLocal(position) * (new Vector3(size, size, 1.0f) / new Vector3(scale * size, scale * size, 1.0f));
        for (int i = 0; i < data.Length; i++)
        {
            int x = i % size;
            int y = i / size;
            float dist = (new Vector2(x, y).DistanceTo(new Vector2(localPosition.X, localPosition.Y)) - radius);
            data[i] = Mathf.Min(dist, data[i]);
        }

        Rebuild();
    }

    public void OnResetButtonPressed()
    {
        Reset();
    }
}