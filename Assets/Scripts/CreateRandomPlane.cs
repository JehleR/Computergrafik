using System;
using UnityEngine;

public class CreateRandomPlane : MonoBehaviour
{
    public int numberDivisions = 128;
    private float sizeOfGerneratedPlane = 20;
    public float triangleHeightDiff = 10;
    public int offsetHeight = 0;
    public float manipulationSpeed = 5;
    public bool vulcanization = false;
    public float vulcanRadius = 5;

    private MeshCollider meshCollider;
    
    Vector3? clickedVertex = null;
    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        meshCollider = gameObject.GetComponent<MeshCollider>();
        createRandomPlane();
        offset = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            clickedVertex = GetClickedVertex(Input.mousePosition);
        }
        if(Input.GetMouseButton(0) && clickedVertex != null)
        {
            //MovePlane(clickedVertex - offset, 1);
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if(scrollWheel > 0f || scrollWheel < 0f)
            {
                MovePlane(clickedVertex.Value - offset, scrollWheel * manipulationSpeed);
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            meshCollider.sharedMesh = mesh;
        }
    }

    void MovePlane(Vector3 centroid, float speed)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        //Vector3[] normals = mesh.normals;
        var variance = 1 + centroid.y / 5;

        if(!vulcanization)
        {
            for(var i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                var modifier = (float) ((1 / (2 * Math.PI * Math.Pow(variance, 2)))
                    * Math.Exp(-(1.0f / 2)
                    * ((Math.Pow((vertex.x - centroid.x), 2) / Math.Pow(variance, 2)) + (Math.Pow((vertex.z - centroid.z), 2) / Math.Pow(variance, 2)))
                    )
                    );
                vertex.y += modifier * speed;
                vertex.y = vertex.y < 0 ? 0 : vertex.y;
                //vertex += normals[i] * Mathf.Sin(Time.time);
                setHeightToZero(ref vertex, 0.001f);
                vertices[i] = vertex;
            }
        }
        else
        {
            for(var i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                var distance = Math.Sqrt(Math.Pow(centroid.x - vertex.x, 2) + Math.Pow(centroid.z - vertex.z, 2));
                var modifier = (float) ((1 / (2 * Math.PI * Math.Pow(variance, 2)))
                    * Math.Exp(-(1.0f / 2)
                    * ((Math.Pow((vertex.x - centroid.x), 2) / Math.Pow(variance, 2)) + (Math.Pow((vertex.z - centroid.z), 2) / Math.Pow(variance, 2)))
                    )
                    );
                if(distance <= variance)
                {
                    vertex.y += modifier * -speed * variance;
                }
                else
                {
                    vertex.y += modifier * speed * variance;
                }
                vertex.y = vertex.y < 0 ? 0 : vertex.y;
                setHeightToZero(ref vertex, 0.001f);
                vertices[i] = vertex;
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    Vector3 GetClickedVertex(Vector3 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        Vector3 clickedVertex = Vector3.zero;
        if(meshCollider.Raycast(ray, out hit, Mathf.Infinity))
        {
            clickedVertex = hit.point;
        }
        return clickedVertex;
    }

    void createRandomPlane()
    {
        var verticeCounter = (numberDivisions + 1) * (numberDivisions + 1);
        var vertices = new Vector3[verticeCounter];
        var uv = new Vector2[verticeCounter];
        var trianglesForPlane = new int[numberDivisions * numberDivisions * 6];

        float halfSize = sizeOfGerneratedPlane * 0.5f;
        float divisionSize = sizeOfGerneratedPlane / numberDivisions;

        var mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        int triOffset = 0;

        for(int i = 0; i <= numberDivisions; i++)
        {
            for(int j = 0; j <= numberDivisions; j++)
            {
                vertices[i * (numberDivisions + 1) + j] = new Vector3(-halfSize + j * divisionSize, 0.0f, halfSize - i * divisionSize);
                uv[i * (numberDivisions + 1) + j] = new Vector2((float) i / numberDivisions, (float) j / numberDivisions);

                if(i < numberDivisions && j < numberDivisions)
                {
                    int pointTopLeft = i * (numberDivisions + 1) + j;
                    int pointBottomLeft = (i + 1) * (numberDivisions + 1) + j;

                    trianglesForPlane[triOffset] = pointTopLeft;
                    trianglesForPlane[triOffset + 1] = pointTopLeft + 1;
                    trianglesForPlane[triOffset + 2] = pointBottomLeft + 1;

                    trianglesForPlane[triOffset + 3] = pointTopLeft;
                    trianglesForPlane[triOffset + 4] = pointBottomLeft + 1;
                    trianglesForPlane[triOffset + 5] = pointBottomLeft;

                    triOffset += 6;
                }
            }
        }

        vertices[0].y = UnityEngine.Random.Range(-triangleHeightDiff, triangleHeightDiff) + offsetHeight;
        vertices[numberDivisions].y = UnityEngine.Random.Range(-triangleHeightDiff, triangleHeightDiff) + offsetHeight;
        vertices[vertices.Length - 1].y = UnityEngine.Random.Range(-triangleHeightDiff, triangleHeightDiff) + offsetHeight;
        vertices[vertices.Length - 1 - numberDivisions].y = UnityEngine.Random.Range(-triangleHeightDiff, triangleHeightDiff) + offsetHeight;

        int iterations = (int) Mathf.Log(numberDivisions, 2);
        int numSquares = 1;
        int squareSize = numberDivisions;
        float tmpMaximumGenerateHeight = triangleHeightDiff;
        for(int i = 0; i < iterations; i++)
        {
            int row = 0;
            for(int j = 0; j < numSquares; j++)
            {
                int col = 0;
                for(int k = 0; k < numSquares; k++)
                {
                    DiamondSquareAlgorithm(ref vertices, row, col, squareSize, tmpMaximumGenerateHeight);
                    col += squareSize;

                }
                row += squareSize;
            }
            numSquares *= 2;
            squareSize /= 2;
            tmpMaximumGenerateHeight *= 0.5f;
        }

        // set negative height back to zero
        for(var i = 0; i < vertices.Length; i++)
        {
            setHeightToZero(ref vertices[i]);
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = trianglesForPlane;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    void DiamondSquareAlgorithm(ref Vector3[] vertices, int row, int col, int size, float offset)
    {
        int halfSquareSize = (int) (size * 0.5f);
        int pointLeftTop = row * (numberDivisions + 1) + col;
        int pointLeftBottom = (row + size) * (numberDivisions + 1) + col;

        int middle = (row + halfSquareSize) * (numberDivisions + 1) + col + halfSquareSize;
        vertices[middle].y = (vertices[pointLeftTop].y + vertices[pointLeftTop + size].y + vertices[pointLeftBottom].y + vertices[pointLeftBottom + size].y) * 0.25f + UnityEngine.Random.Range(-offset, offset);

        vertices[pointLeftTop + halfSquareSize].y = (vertices[pointLeftTop].y + vertices[pointLeftTop + size].y + vertices[middle].y) / 3 + UnityEngine.Random.Range(-offset, offset);
        vertices[middle - halfSquareSize].y = (vertices[pointLeftTop].y + vertices[pointLeftBottom].y + vertices[middle].y) / 3 + UnityEngine.Random.Range(-offset, offset);
        vertices[middle + halfSquareSize].y = (vertices[pointLeftTop + size].y + vertices[pointLeftBottom + size].y + vertices[middle].y) / 3 + UnityEngine.Random.Range(-offset, offset);
        vertices[pointLeftBottom + halfSquareSize].y = (vertices[pointLeftBottom].y + vertices[pointLeftBottom + size].y + vertices[middle].y) / 3 + UnityEngine.Random.Range(-offset, offset);


    }
    void setHeightToZero(ref Vector3 vertex, float maxHeight = 0)
    {
        if(vertex.y < maxHeight)
        {
            vertex.y = 0;
        }
    }
}