using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Decal : MonoBehaviour
{
    #region Values
    public enum TextureDivisionType
    {
        X1,
        X4
    }
    /// <summary>
    /// Determines how the texture used is to be spliced when generating UVs. (NOTE: Section is currently randomized)
    /// </summary>
    public TextureDivisionType TextureDivisions;

    [Space(10)]
    public LayerMask IgnoredLayers;

    /// <summary>
    /// Defines the distance in-which the decal will be offset from the intersecting surface.
    /// </summary>
    public float SurfaceOffset = 0.01f;
    public float MaxAngle = 90;

    [Space(10)]
    public Vector2 LifeSpanRange;
    private float _lifeSpan;
    private float _lifeProgress;


    /// <summary>
    /// Defines the decals base material.
    /// </summary>
    [Space(10)]
    public Material Material;
    // Defines the instantiated material of the renderer.
    private Material _material;

    public Color Color;
    public string MaterialColorProperty;

    /// <summary>
    /// Defines the property name of the decals material that is to be set after determining surface color.
    /// </summary>
    public string MaterialLightmapColorProperty;
    /// <summary>
    /// Defines the property name of the decals material that is to be set to fade.
    /// </summary>
    [Space(5)]
    public string MaterialFadeProperty;
    /// <summary>
    /// Determines the opacity over the lifetime of the decals fade.
    /// </summary>
    public AnimationCurve FadeFalloff = AnimationCurve.Linear(0, 1, 1, 1);


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
    
    private void Update()
    {
        UpdateFade();
    }

    private void OnDestroy()
    {
        Destroy(_material);
    }
    #endregion

    #region Functions

    public void Cast(GameObject affectedObject, Color lightmapColor)
    {
        if (affectedObject == null || affectedObject.layer != Globals.STRUCTURE_LAYER)
        {
            Destroy(this.gameObject);
            return;
        }


        _lifeSpan = Random.Range(LifeSpanRange.x, LifeSpanRange.y);
        
        _meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        _material = new Material(Material);
        _material.SetColor(MaterialColorProperty, Color);
        _material.SetColor(MaterialLightmapColorProperty, lightmapColor);
        _meshRenderer.material = _material;

        _meshFilter = this.gameObject.AddComponent<MeshFilter>();

        this.transform.eulerAngles += new Vector3(180, 0, Random.Range(0, 360));

        BuildMesh(affectedObject);
    }

    private void UpdateFade()
    {
        if (MaterialFadeProperty == string.Empty)
            return;


        _lifeProgress = Mathf.Min(_lifeProgress + Time.deltaTime, _lifeSpan);

        float fade = FadeFalloff.Evaluate(_lifeProgress / _lifeSpan);

        _material.SetFloat(MaterialFadeProperty, fade);

        if (_lifeProgress == _lifeSpan)
            Destroy(this.gameObject);
    }

    private void BuildMesh(GameObject affectedObject)
    {
        Mesh mesh = affectedObject.GetComponent<MeshFilter>().sharedMesh;

        if (mesh == null)
            return;

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
        Matrix4x4 matrix = this.transform.worldToLocalMatrix * affectedObject.transform.localToWorldMatrix;


        Development.Out(this, string.Format("{0} Triangles,  {1} Vertices", triangles.Length, vertices.Length), Development.MessagePriority.Low);

        for (int triangle = 0; triangle < triangles.Length; triangle += 3)
        {
            // Define indices
            int indice0 = triangles[triangle];
            int indice1 = triangles[triangle + 1];
            int indice2 = triangles[triangle + 2];

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

        GenerateUVs(startVertexCount);

        Offset();
        CompositeMesh();
    }
    private void CompositeMesh()
    {
        if (_bufferIndices.Count == 0)
            return;

        Mesh mesh = new Mesh();

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
        
        mesh.name = this.gameObject.name;
        _meshFilter.mesh = mesh;

        if (MaterialFadeProperty == string.Empty)
            Destroy(this.gameObject, _lifeSpan);
    }

    private void Offset()
    {
        for (int vertice = 0; vertice < _bufferVertices.Count; vertice++)
        {
            Vector3 normal = _bufferNormals[vertice];
            _bufferVertices[vertice] += normal * SurfaceOffset;
        }
    }

    private void GenerateUVs(int start)
    {
        if (TextureDivisions == TextureDivisionType.X1)
        {
            for (int vertice = start; vertice < _bufferVertices.Count; vertice++)
            {
                Vector2 uv = new Vector2(_bufferVertices[vertice].x + 0.5f, _bufferVertices[vertice].y + 0.5f);
                _bufferTexCoords.Add(uv);
            }
        }
        else
        {
            float horizontalOffset = Random.Range(0, 2);
            float verticalOffset = Random.Range(0, 2);

            for (int vertice = start; vertice < _bufferVertices.Count; vertice++)
            {
                Vector2 uv = new Vector2(_bufferVertices[vertice].x + 0.5f, _bufferVertices[vertice].y + 0.5f);
                uv.x = Mathf.Lerp(0.5f, horizontalOffset, uv.x);
                uv.y = Mathf.Lerp(0.5f, verticalOffset, uv.y);
                _bufferTexCoords.Add(uv);
            }
        }
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
        for (int vertice = 0; vertice < _bufferVertices.Count; vertice++)
            if (Vector3.Distance(_bufferVertices[vertice], vertex) < 0.01f)
                return vertice;
        return -1;
    }

    private void AddPolygon(List<Vector3> vertices, Vector3 normal)
    {
        int indice0 = AddVertex(vertices[0], normal);
        for (int vertice = 1; vertice < vertices.Count - 1; vertice++)
        {
            int indice1 = AddVertex(vertices[vertice], normal);
            int indice2 = AddVertex(vertices[vertice + 1], normal);

            _bufferIndices.Add(indice0);
            _bufferIndices.Add(indice1);
            _bufferIndices.Add(indice2);
        }
    }
    private void ClipPolygon(ref List<Vector3> vertices, Plane plane)
    {
        bool[] positive = new bool[9];
        int positiveCount = 0;

        for (int vertice = 0; vertice < vertices.Count; vertice++)
        {
            positive[vertice] = !plane.GetSide(vertices[vertice]);
            if (positive[vertice])
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

        for (int vertice = 0; vertice < vertices.Count; vertice++)
        {
            int next = vertice + 1;
            next %= vertices.Count;

            if (positive[vertice])
                tempVertices.Add(vertices[vertice]);

            if (positive[vertice] != positive[next])
            {
                Vector3 v1 = vertices[next];
                Vector3 v2 = vertices[vertice];

                Vector3 v = LineCast(plane, v1, v2);
                tempVertices.Add(v);
            }
        }
        vertices = tempVertices;
    }

    private Vector3 LineCast(Plane plane, Vector3 a, Vector3 b)
    {
        float distance;
        Ray ray = new Ray(a, b - a);
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    } 
    #endregion
}