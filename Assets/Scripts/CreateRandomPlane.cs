using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRandomPlane : MonoBehaviour
{
    public int numberDivisions = 128;
    public float sizeOfGerneratedPlane = 30;
    public float triangleHeightDiff = 10;
    public int offsetMap = 0;
    int scrollScale = 100;
    int scrollWidth;
    double variance = 10;

    public MeshCollider meshCollider ;

    // https://docs.unity3d.com/ScriptReference/Mesh.html 
    //necessary for mesh
    Vector3[] vertices;
    int verticeCounter;
    Vector2[] uv;
    int[] trianglesForPlane;
    Mesh mesh;
    int index;

    // Start is called before the first frame update
    void Start()
    {
        meshCollider = gameObject.GetComponent<MeshCollider>();
        createRandomPlane();
    }

    // Update is called once per frame
    void Update()
    {
        scrollWidth = Convert.ToInt32(variance*0.3f);
        if (Input.GetMouseButtonDown(0))
        {
            index = getClickedVertex(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            //Debug.Log("Triggered: " + scrollWheel);
            if (scrollWheel > 0f || scrollWheel < 0f)
            {
                //scrollWheel = Input.GetAxis("Mouse ScrollWheel");
                //Debug.Log(Input.mousePosition);
                //Debug.Log("Erhöhung: " + scrollWheel);
                updateHeight(scrollWheel, index);

            }

        }
    }

    int getClickedVertex(Vector3 position)
    {
        //Debug.Log("Postiton: "+position);
        //Debug.Log("Camera: "+Camera.main);
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        Vector3 clickpos, lastClick;
        int indexActiveVert = 0;
        //Debug.Log("Look if hit");
        //Debug.Log(meshCollider.Raycast(ray, out hit, Mathf.Infinity));
        if (meshCollider.Raycast(ray, out hit, Mathf.Infinity))
        {
            //Debug.Log("hit mesh");
            clickpos = hit.point;
            //Debug.Log(clickpos);
            lastClick = clickpos;
            Vector3 nearestVertex = Vector3.zero;
            Vector3 activeVert = Vector3.zero;
            int index = 0;
            float minDistanceSqr = Mathf.Infinity;

            foreach (Vector3 vertex in mesh.vertices)
            {
                Vector3 diff = lastClick - vertex;
                float distSqr = diff.sqrMagnitude;
                if (distSqr < minDistanceSqr)
                {
                    indexActiveVert = index;
                    minDistanceSqr = distSqr;
                    nearestVertex = vertex;
                }
                index++;
            }
            activeVert = nearestVertex;
        }
        return indexActiveVert;

    }

    void updateHeight(float scrollWheel, int position)
    {
        
        scrollWheel *= scrollScale;
        //über die position wird noch diskutiert
        moveVertices(scrollWheel, position);
        updateMesh();
    }

    void moveVertices(float delta, int index)
    {
        Vector3[] tmpVerts = vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (i == index)
            {
                for (int k = 1; k < scrollWidth; k++)
                {
                    //Debug.Log(tmpVerts[i].y); //Debug.Log(delta);if ((tmpVerts[i].y += delta) < 0){tmpVerts[i].y = 0;}
                    //x direction to the right
                    for (int j = i; j < i + scrollWidth; j++)
                    {
                        if (delta < 0)
                        {
                            //Funktion aus wikipedia --> erste Formel oben
                            // https://en.wikipedia.org/wiki/Normal_distribution
                            //tmpVerts[j].y -= Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow(0, 2) / (2 * Math.Pow(variance, 2))))));
                            //tmpVerts[j +k* numberDivisions].y -= Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow(0, 2) / (2 * Math.Pow(variance, 2))))));
                            //tmpVerts[j - k * numberDivisions].y -= Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow(0, 2) / (2 * Math.Pow(variance, 2))))));
                            //https://de.wikipedia.org/wiki/Mehrdimensionale_Normalverteilung
                            //--> Dichte der zweidimensionalen Normalverteilung
                            tmpVerts[j].y -= Math.Abs(delta)*((float)((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2)*(Math.Pow(tmpVerts[j].x, 2)-2*tmpVerts[j].x*tmpVerts[j].z+Math.Pow(tmpVerts[j].z,2)))));
                            tmpVerts[j - k * numberDivisions].y -= Math.Abs(delta)*((float)((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2)*((Math.Pow(tmpVerts[j - k * numberDivisions].x, 2) / Math.Pow(variance, 2)) + (Math.Pow(tmpVerts[j - k * numberDivisions].z, 2) / Math.Pow(variance, 2))))));
                            tmpVerts[j + k * numberDivisions].y -= Math.Abs(delta)*((float)((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2)*((Math.Pow(tmpVerts[j + k * numberDivisions].x, 2) / Math.Pow(variance, 2)) + (Math.Pow(tmpVerts[j + k * numberDivisions].z, 2) / Math.Pow(variance, 2))))));

                        }
                        else
                        {
                            //tmpVerts[j].y += Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow(0, 2) / (2 * Math.Pow(variance, 2))))));
                            //tmpVerts[j +k* numberDivisions].y += Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow(0, 2) / (2 * Math.Pow(variance, 2))))));
                            //tmpVerts[j - k * numberDivisions].y += Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow(0, 2) / (2 * Math.Pow(variance, 2))))));
                            tmpVerts[j].y += Math.Abs(delta)*((float)((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2)*(Math.Pow(tmpVerts[j].x, 2) - 2 * tmpVerts[j].x * tmpVerts[j].z + Math.Pow(tmpVerts[j].z, 2)))));
                            tmpVerts[j - k * numberDivisions-k].y += Math.Abs(delta)*((float) ((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2)*((Math.Pow(tmpVerts[j - k * numberDivisions].x, 2) / Math.Pow(variance, 2)) + (Math.Pow(tmpVerts[j - k * numberDivisions].z, 2) / Math.Pow(variance, 2))))));
                            tmpVerts[j + k * numberDivisions+k].y += Math.Abs(delta)*((float) ((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2)*((Math.Pow(tmpVerts[j + k * numberDivisions].x, 2) / Math.Pow(variance, 2)) + (Math.Pow(tmpVerts[j + k * numberDivisions].z, 2) / Math.Pow(variance, 2))))));

                        }
                    }
                    //x direction to the left
                    for (int j = i; j > i - scrollWidth; j--)
                    {
                        if (delta < 0)
                        {
                            //tmpVerts[j].y -= Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow(0, 2) / (2 * Math.Pow(variance, 2))))));
                            //tmpVerts[j -k* numberDivisions].y -= Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow(0, 2) / (2 * Math.Pow(variance, 2))))));
                            //tmpVerts[j + k * numberDivisions].y -= Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow(0, 2) / (2 * Math.Pow(variance, 2))))));
                            tmpVerts[j].y -= Math.Abs(delta) * ((float)((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2) * (Math.Pow(tmpVerts[j].x, 2) / Math.Pow(variance, 2)) + (Math.Pow(tmpVerts[j].z, 2) / Math.Pow(variance, 2)))));
                            tmpVerts[j - k * numberDivisions].y -= Math.Abs(delta)*((float) ((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2) * ((Math.Pow(tmpVerts[j - k * numberDivisions].x, 2) / Math.Pow(variance, 2)) + (Math.Pow(tmpVerts[j - k * numberDivisions].z, 2) / Math.Pow(variance, 2))))));
                            tmpVerts[j + k * numberDivisions].y -= Math.Abs(delta)*((float) ((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2) * ((Math.Pow(tmpVerts[j + k * numberDivisions].x, 2) / Math.Pow(variance, 2)) + (Math.Pow(tmpVerts[j + k * numberDivisions].z, 2) / Math.Pow(variance, 2))))));

                        }
                        else
                        {
                            //tmpVerts[j].y += Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow((tmpVerts[j].x), 2) / (2 * Math.Pow(variance, 2))))));
                            //tmpVerts[j - k*numberDivisions].y += Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow(0, 2) / (2 * Math.Pow(variance, 2))))));
                            //tmpVerts[j + k * numberDivisions].y += Math.Abs(delta) * ((float)(1 / (Math.Sqrt(2 * Math.PI * Math.Pow(variance, 2))) * Math.Pow(Math.E, -(Math.Pow(0, 2) / (2 * Math.Pow(variance, 2))))));
                            tmpVerts[j].y += Math.Abs(delta) * ((float)((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2) * (Math.Pow(tmpVerts[j].x, 2) - 2 * tmpVerts[j].x * tmpVerts[j].z + Math.Pow(tmpVerts[j].z, 2)))));
                            tmpVerts[j - k * numberDivisions - k].y += Math.Abs(delta)*((float) ((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2) * ((Math.Pow(tmpVerts[j - k * numberDivisions].x, 2) / Math.Pow(variance, 2)) + (Math.Pow(tmpVerts[j - k * numberDivisions].z, 2) / Math.Pow(variance, 2))))));
                            tmpVerts[j + k * numberDivisions + k].y += Math.Abs(delta)*((float) ((1 / (2 * Math.PI * variance * variance * Math.Sqrt(1))) * Math.Exp(-(1 / 2) * ((Math.Pow(tmpVerts[j + k * numberDivisions].x, 2) / Math.Pow(variance, 2)) + (Math.Pow(tmpVerts[j + k * numberDivisions].z, 2) / Math.Pow(variance, 2))))));

                        }
                    }
                }
            }
        }
        vertices = tmpVerts;
    }


    void createRandomPlane()
    {
        verticeCounter = (numberDivisions + 1) * (numberDivisions + 1);
        vertices = new Vector3[verticeCounter];
        uv = new Vector2[verticeCounter];
        trianglesForPlane = new int[numberDivisions * numberDivisions * 6];

        float halfSize = sizeOfGerneratedPlane * 0.5f;
        float divisionSize = sizeOfGerneratedPlane / numberDivisions;

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        int triOffset = 0;

        for (int i = 0; i <= numberDivisions; i++)
        {
            for (int j = 0; j <= numberDivisions; j++)
            {
                vertices[i * (numberDivisions + 1) + j] = new Vector3(-halfSize + j * divisionSize, 0.0f, halfSize - i * divisionSize);
                uv[i * (numberDivisions + 1) + j] = new Vector2((float)i / numberDivisions, (float)j / numberDivisions);

                if (i < numberDivisions && j < numberDivisions)
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

        vertices[0].y = UnityEngine.Random.Range(-triangleHeightDiff, triangleHeightDiff) + offsetMap;
        vertices[numberDivisions].y = UnityEngine.Random.Range(-triangleHeightDiff, triangleHeightDiff) + offsetMap;
        vertices[vertices.Length - 1].y = UnityEngine.Random.Range(-triangleHeightDiff, triangleHeightDiff) + offsetMap;
        vertices[vertices.Length - 1 - numberDivisions].y = UnityEngine.Random.Range(-triangleHeightDiff, triangleHeightDiff) + offsetMap;

        int iterations = (int)Mathf.Log(numberDivisions, 2);
        int numSquares = 1;
        int squareSize = numberDivisions;
        float tmpMaximumGenerateHeight = triangleHeightDiff;
        for (int i = 0; i < iterations; i++)
        {
            int row = 0;
            for (int j = 0; j < numSquares; j++)
            {
                int col = 0;
                for (int k = 0; k < numSquares; k++)
                {
                    DiamondSquareAlgorithm(row, col, squareSize, tmpMaximumGenerateHeight);
                    col += squareSize;

                }
                row += squareSize;
            }
            numSquares *= 2;
            squareSize /= 2;
            tmpMaximumGenerateHeight *= 0.5f;

        }
        updateMesh();
    }

    void updateMesh()
    {
        removeNegativeValues();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = trianglesForPlane;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    void DiamondSquareAlgorithm(int row, int col, int size, float offset)
    {
        int halfSquareSize = (int)(size * 0.5f);
        int pointLeftTop = row * (numberDivisions + 1) + col;
        int pointLeftBottom = (row + size) * (numberDivisions + 1) + col;
        
        int middle = (int)(row + halfSquareSize) * (numberDivisions + 1) + (int)(col + halfSquareSize);
        vertices[middle].y = (vertices[pointLeftTop].y + vertices[pointLeftTop + size].y + vertices[pointLeftBottom].y + vertices[pointLeftBottom + size].y) * 0.25f + UnityEngine.Random.Range(-offset, offset);

        vertices[pointLeftTop + halfSquareSize].y = (vertices[pointLeftTop].y + vertices[pointLeftTop + size].y + vertices[middle].y) / 3 + UnityEngine.Random.Range(-offset, offset);
        vertices[middle - halfSquareSize].y = (vertices[pointLeftTop].y + vertices[pointLeftBottom].y + vertices[middle].y) / 3 + UnityEngine.Random.Range(-offset, offset);
        vertices[middle + halfSquareSize].y = (vertices[pointLeftTop + size].y + vertices[pointLeftBottom + size].y + vertices[middle].y) / 3 + UnityEngine.Random.Range(-offset, offset);
        vertices[pointLeftBottom + halfSquareSize].y = (vertices[pointLeftBottom].y + vertices[pointLeftBottom + size].y + vertices[middle].y) / 3 + UnityEngine.Random.Range(-offset, offset);


    }
    void removeNegativeValues()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y < 0)
            {
                vertices[i].y = 0;
            }
        }
    }


   
}