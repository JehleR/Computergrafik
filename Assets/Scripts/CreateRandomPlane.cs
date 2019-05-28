using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRandomPlane : MonoBehaviour
{
    public int numberDivisions = 128;
    public float sizeOfGerneratedPlane = 30;
    public float maximumGenerateHeight = 10;
    public int offset = 0;

    // https://docs.unity3d.com/ScriptReference/Mesh.html 
    //necessary for mesh
    Vector3[] vertices;
    int verticeCounter;
    Vector2[] uv;
    int[] trianglesForPlane;
    Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        createRandomPlane();
    }

    // Update is called once per frame
    void createRandomPlane()
    {
        verticeCounter = (numberDivisions + 1) * (numberDivisions + 1);
        vertices = new Vector3[verticeCounter];
        uv = new Vector2[verticeCounter];
        trianglesForPlane = new int[numberDivisions * numberDivisions * 6];

        float halfSize = sizeOfGerneratedPlane * 0.5f;
        float divisionSize = sizeOfGerneratedPlane / numberDivisions;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        int triOffset = 0;

        for (int i = 0;i<= numberDivisions; i++)
        {
            for (int j = 0; j <= numberDivisions; j++)
            {
                vertices[i * (numberDivisions + 1) + j] = new Vector3(-halfSize+j*divisionSize, 0.0f,halfSize-i*divisionSize);
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

        vertices[0].y = Random.Range(-maximumGenerateHeight, maximumGenerateHeight)+offset;
        vertices[numberDivisions].y = Random.Range(-maximumGenerateHeight, maximumGenerateHeight)+offset;
        vertices[vertices.Length - 1].y = Random.Range(-maximumGenerateHeight, maximumGenerateHeight)+offset;
        vertices[vertices.Length - 1 - numberDivisions].y = Random.Range(-maximumGenerateHeight, maximumGenerateHeight)+offset;

        int iterations = (int)Mathf.Log(numberDivisions, 2);
        int numSquares = 1;
        int squareSize = numberDivisions;
        for (int i = 0; i < iterations; i++)
        {
            int row = 0;
            for (int j = 0; j < numSquares; j++)
            {
                int col = 0;
                for (int k = 0; k < numSquares; k++)
                {
                    DiamondSquareAlgorithm(row, col, squareSize, maximumGenerateHeight);
                    col += squareSize;

                }
                row += squareSize;
            }
            numSquares *= 2;
            squareSize /= 2;
            maximumGenerateHeight *= 0.5f;

        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = trianglesForPlane;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    void DiamondSquareAlgorithm(int row, int col, int size , float offset)
    {
        int halfSquareSize = (int)(size * 0.5f);
        int pointLeftTop = row * (numberDivisions + 1) + col;
        int pointLeftBottom = (row + size) * (numberDivisions + 1) + col;
        //verursacht keine negativen Werte aber auch keine 0 Werte
        //pointLeftTop = removeNegativeValues(pointLeftTop);
        //pointLeftBottom = removeNegativeValues(pointLeftBottom);

        int middle = (int)(row + halfSquareSize) * (numberDivisions + 1) + (int)(col + halfSquareSize);
        vertices[middle].y = (vertices[pointLeftTop].y + vertices[pointLeftTop + size].y+vertices[pointLeftBottom].y+vertices[pointLeftBottom+size].y)*0.25f+Random.Range(-offset,offset);

        vertices[pointLeftTop + halfSquareSize].y = (vertices[pointLeftTop].y + vertices[pointLeftTop + size].y + vertices[middle].y)/3+Random.Range(-offset,offset);
        vertices[middle-halfSquareSize].y = (vertices[pointLeftTop].y + vertices[pointLeftBottom].y + vertices[middle].y)/3 + Random.Range(-offset, offset);
        vertices[middle + halfSquareSize].y = (vertices[pointLeftTop+size].y + vertices[pointLeftBottom+size].y + vertices[middle].y)/3 + Random.Range(-offset, offset);
        vertices[pointLeftBottom + halfSquareSize].y = (vertices[pointLeftBottom].y + vertices[pointLeftBottom + size].y + vertices[middle].y) / 3 + Random.Range(-offset, offset);

        
    }
    int removeNegativeValues(int value)
    {
        if(value < 0)
        {
            value = 0;
        }
        return value;
    }
}
