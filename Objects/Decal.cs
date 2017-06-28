using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Decal : MonoBehaviour
{
    #region Values
    /// <summary>
    /// Determines if the game is currently allowing for decals to be cast.
    /// </summary>
    public static bool AllowDecals;

    /// <summary>
    /// Defines the scale of the decal.
    /// </summary>
    public float Scale;

    /// <summary>
    /// Defines the range of the decals lifespan.
    /// </summary>
    public Vector2 LifeSpanRange;
    // Defines the life span of the decal
    private float _lifeSpan;


    /// <summary>
    /// Defines the distance in-which the decal will be offset from the intersecting surface.
    /// </summary>
    [Space(10)]
    public float SurfaceOffset = 0.01f;
    /// <summary>
    /// Defines the max angle that the decal will project onto the intersecting surface.
    /// </summary>
    public float MaxAngle = 60;
    

    /// <summary>
    /// Defines the color of the decal.
    /// </summary>
    [Space(10)]
    public Color Color;

    public enum UVDivisionType
    {
        X1,
        X4
    }
    /// <summary>
    /// Determines how the texture used is to be spliced when generating UVs.
    /// </summary>
    [Space(10)]
    public UVDivisionType UVDivision;


    /// <summary>
    /// Determines the opacity over the lifetime of the decals fade.
    /// </summary>
    /// 
    /// <summary>
    /// Defines the decals base material.
    /// </summary>
    [Space(5)]
    public Material Material;
    // Defines the instantiated material of the renderer.
    private Material _materialInstance;
    /// <summary>
    /// Defines the string of the materials color property.
    /// </summary>
    public string MaterialColorProperty;
    /// <summary>
    /// Defines the property name of the decals material that is to be set after determining surface color.
    /// </summary>
    public string MaterialLightmapColorProperty;
    /// <summary>
    /// Defines the property name of the decals material fade start time.
    /// </summary>
    public string MaterialFadeStartProperty;
    /// <summary>
    /// Defines the property name of the decals material fade end time.
    /// </summary>
    public string MaterialFadeEndProperty;


    // Defines the mesh filter component of the game object.
    private MeshFilter _meshFilter;
    // Defines the mesh renderer component of the game object.
    private MeshRenderer _meshRenderer;


    // Contain each mesh aspect while building
    private List<Vector3> _bufferVertices = new List<Vector3>();
    private List<Vector3> _bufferNormals = new List<Vector3>();
    private List<Vector2> _bufferTexCoords = new List<Vector2>();
    private List<int> _bufferIndices = new List<int>(); 
    #endregion

    #region Unity Functions
    private void OnDestroy()
    {
        Destroy(_materialInstance);
    }

    private void OnDrawGizmosSelected()
    {
        // Show the decals size, position and direction
        Matrix4x4 matrixBackup = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = matrixBackup;

        Gizmos.color = Color.cyan;
        float offsetExtent = this.transform.lossyScale.z / 2;
        Gizmos.DrawLine(this.transform.position - (offsetExtent * this.transform.forward), this.transform.position);

        Gizmos.DrawWireSphere(this.transform.position, 0.1f);
    }
    #endregion

    #region Functions
    private void Initialize(Color lightmapColor)
    {
        this.transform.localScale = Vector3.one * Scale;
        // Randomly rotate the decal before projection
        this.transform.eulerAngles += new Vector3(180, 0, Random.Range(0, 360));

        // Define the life span of the decal
        _lifeSpan = Random.Range(LifeSpanRange.x, LifeSpanRange.y);

        // Define the mesh renderer and its defaults
        _meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // Define the material and its defaults
        _materialInstance = new Material(Material);
        _meshRenderer.material = _materialInstance;
        _materialInstance.SetColor(MaterialColorProperty, Color);
        _materialInstance.SetFloat(MaterialFadeStartProperty, Time.timeSinceLevelLoad);
        _materialInstance.SetFloat(MaterialFadeEndProperty, Time.timeSinceLevelLoad + _lifeSpan);
        _materialInstance.SetColor(MaterialLightmapColorProperty, lightmapColor);

        _meshFilter = this.gameObject.AddComponent<MeshFilter>();

        Destroy(this.gameObject, _lifeSpan);

    }

    public void Cast(GameObject affectedObject, Color lightmapColor)
    {
        Mesh mesh = affectedObject.GetComponent<MeshFilter>().sharedMesh;

        if (!AllowDecals || mesh == null || affectedObject.layer != Globals.STRUCTURE_LAYER)
        {
            Destroy(this.gameObject);
            return;
        }

        Initialize(lightmapColor);
        
        GetMeshData(mesh, affectedObject.transform.localToWorldMatrix);
        CompositeMesh();
    }
    

    private void CompositeMesh()
    {
        if (_bufferIndices.Count == 0)
            return;

        Mesh mesh = new Mesh();
        mesh.name = this.gameObject.name;

        mesh.vertices = _bufferVertices.ToArray();
        mesh.normals = _bufferNormals.ToArray();
        mesh.uv = _bufferTexCoords.ToArray();
        mesh.uv2 = _bufferTexCoords.ToArray();
        mesh.triangles = _bufferIndices.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        _bufferVertices.Clear();
        _bufferNormals.Clear();
        _bufferTexCoords.Clear();
        _bufferIndices.Clear();

        _meshFilter.mesh = mesh;
    }

    private void GetMeshData(Mesh mesh, Matrix4x4 localToWorldMatrix)
    {
        // Define clipping planes
        Plane right = new Plane(Vector3.right, Vector3.right / 2f);
        Plane left = new Plane(-Vector3.right, -Vector3.right / 2f);
        Plane top = new Plane(Vector3.up, Vector3.up / 2f);
        Plane bottom = new Plane(-Vector3.up, -Vector3.up / 2f);
        Plane front = new Plane(Vector3.forward, Vector3.forward / 2f);
        Plane back = new Plane(-Vector3.forward, -Vector3.forward / 2f);

        // Define current mesh vert count
        int startVertexCount = _bufferVertices.Count;

        // Define the surfaces vertices and triangles
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Define vertex matrix
        Matrix4x4 matrix = this.transform.worldToLocalMatrix * localToWorldMatrix;


        Development.Out(this, string.Format("{0} Triangles,  {1} Vertices", triangles.Length, vertices.Length), Development.MessagePriority.Low);

        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Define indices
            int indice0 = triangles[i];
            int indice1 = triangles[i + 1];
            int indice2 = triangles[i + 2];

            // Define and offset verts to world space
            Vector3 vertice0 = matrix.MultiplyPoint(vertices[indice0]);
            Vector3 vertice1 = matrix.MultiplyPoint(vertices[indice1]);
            Vector3 vertice2 = matrix.MultiplyPoint(vertices[indice2]);

            // Define triangle normal
            Vector3 side1 = vertice1 - vertice0;
            Vector3 side2 = vertice2 - vertice0;
            Vector3 normal = Vector3.Cross(side1, side2).normalized;


            if (Vector3.Angle(-Vector3.forward, normal) >= MaxAngle)
                continue;


            List<Vector3> poly = new List<Vector3> { vertice0, vertice1, vertice2 };

            ClipPolygon(ref poly, right);
            if (poly.Count == 0)
                continue;
            ClipPolygon(ref poly, left);
            if (poly.Count == 0)
                continue;

            ClipPolygon(ref poly, top);
            if (poly.Count == 0)
                continue;
            ClipPolygon(ref poly, bottom);
            if (poly.Count == 0)
                continue;

            ClipPolygon(ref poly, front);
            if (poly.Count == 0)
                continue;
            ClipPolygon(ref poly, back);
            if (poly.Count == 0)
                continue;

            AddPolygon(poly, normal);
        }

        PostProcessMesh(startVertexCount);
    }
    
    private void AddPolygon(List<Vector3> vertices, Vector3 normal)
    {
        int indice0 = AddVertex(vertices[0], normal);
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            int indice1 = AddVertex(vertices[i], normal);
            int indice2 = AddVertex(vertices[i + 1], normal);

            _bufferIndices.Add(indice0);
            _bufferIndices.Add(indice1);
            _bufferIndices.Add(indice2);
        }
    }
    private void ClipPolygon(ref List<Vector3> vertices, Plane plane)
    {
        bool[] positive = new bool[9];
        int positiveCount = 0;

        for (int i = 0; i < vertices.Count; i++)
        {
            positive[i] = !plane.GetSide(vertices[i]);
            if (positive[i])
                positiveCount++;
        }

        if (positiveCount == 0)
        {
            vertices = new List<Vector3>();
            return;
        }
        if (positiveCount == vertices.Count)
            return;

        List<Vector3> tempVertices = new List<Vector3>();

        for (int i = 0; i < vertices.Count; i++)
        {
            int next = i + 1;
            next %= vertices.Count;

            if (positive[i])
                tempVertices.Add(vertices[i]);

            if (positive[i] != positive[next])
            {
                Vector3 v1 = vertices[next];
                Vector3 v2 = vertices[i];

                Vector3 v = LineCast(plane, v1, v2);
                tempVertices.Add(v);
            }
        }
        vertices = tempVertices;
    }

    private int AddVertex(Vector3 vertex, Vector3 normal)
    {
        int index = FindVertex(vertex);
        if (index == -1)
        {
            _bufferVertices.Add(vertex);
            _bufferNormals.Add(normal);
            index = _bufferVertices.Count - 1;
        }
        else
        {
            Vector3 t = _bufferNormals[index] + normal;
            _bufferNormals[index] = t.normalized;
        }
        return index;
    }
    private int FindVertex(Vector3 vertex)
    {
        for (int i = 0; i < _bufferVertices.Count; i++)
            if (Vector3.Distance(_bufferVertices[i], vertex) < 0.01f)
                return i;
        return -1;
    }

    private Vector3 LineCast(Plane plane, Vector3 a, Vector3 b)
    {
        float distance;
        Ray ray = new Ray(a, b - a);
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    private void PostProcessMesh(int start)
    {
        Vector2 UvOffset = new Vector2(Random.Range(0, 2), Random.Range(0, 2));

        for (int i = start; i < _bufferVertices.Count; i++)
        {
            OffsetVertice(i);
            MapUV(i, UvOffset, UVDivision);
        }
    }
    private void OffsetVertice(int verticeIndex)
    {
        Vector3 normal = _bufferNormals[verticeIndex];
        _bufferVertices[verticeIndex] += normal * SurfaceOffset;
    }
    private void MapUV(int verticeIndex, Vector2 Offset, UVDivisionType uVDivisionType)
    {
        if (uVDivisionType == UVDivisionType.X1)
        {
            Vector2 uv = new Vector2(_bufferVertices[verticeIndex].x + 0.5f, _bufferVertices[verticeIndex].y + 0.5f);
            _bufferTexCoords.Add(uv);
        }
        else if (uVDivisionType == UVDivisionType.X4)
        {
            Vector2 uv = new Vector2(_bufferVertices[verticeIndex].x + 0.5f, _bufferVertices[verticeIndex].y + 0.5f);
            uv.x = Mathf.Lerp(0.5f, Offset.x, uv.x);
            uv.y = Mathf.Lerp(0.5f, Offset.y, uv.y);
            _bufferTexCoords.Add(uv);
        }
    }
    #endregion
}