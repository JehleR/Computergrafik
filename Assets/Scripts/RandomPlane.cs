using System;
using UnityEngine;

public class RandomPlane : MonoBehaviour
{
    // resolution of the created mesh
    public int numberDivisions = 128;
    // size of the plane
    private float sizeOfGerneratedPlane = 20;
    // Height Difference between the generated Triangles
    public float randomHeightDiff = 10;
    // Offset for all vertices
    public int offsetHeight = 0;
    // How much Height difference per scroll
    public float manipulationSpeed = 5;

    // It´s a feature for creating something like a vulcan (only works on hills)
    public bool vulcanization = false;
    // Size of the Vulcan cone
    public float vulcanRadius = 5;

    // Mesh Collider for detection 
    private MeshCollider meshCollider;

    // Line Renderer for Laser
    private LineRenderer laser;

    // clicked vertex
    Vector3? clickedVertex = null;
    // offset for the Map to be on the Table
    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        // mesh Collider for the Ray detection
        meshCollider = gameObject.GetComponent<MeshCollider>();
        // creating the plane
        createRandomPlane();
        // map offset
        offset = transform.position;

        //create Line Renderer for laser
        laser = gameObject.AddComponent<LineRenderer>();
        // init with empty positions
        Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        // assign values to line renderer
        laser.SetPositions(initLaserPositions);
        // set laser size
        laser.startWidth = 0.3f;
        laser.endWidth = 0.3f;
        // color red
        laser.material.color = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        // when click only once
        if(Input.GetMouseButtonDown(0))
        {

            // get the clicked vertex
            clickedVertex = GetClickedVertex(Input.mousePosition);
        }
        // while Button clicked && Clicked on mesh
        if(Input.GetMouseButton(0) && clickedVertex != null)
        {
            // detect direction of scrolling direction || positive while up || negative while down
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            // only move if scrolled
            if(scrollWheel > 0f || scrollWheel < 0f)
            {
                // Move Vertices in choosen direction
                MovePlane(clickedVertex.Value - offset, scrollWheel * manipulationSpeed);
            }
        }
        // when release Button
        if(Input.GetMouseButtonUp(0))
        {
            // apply new mesh to the meshCollider
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            meshCollider.sharedMesh = mesh;

            // remove laser line
            laser.enabled = false;
        }
    }

    // detect the vertice cklicked on
    Vector3 GetClickedVertex(Vector3 mousePosition)
    {
        // create a ray from the center of the main camera
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        Vector3 clickedVertex = Vector3.zero;

        // send out a ray 
        if(meshCollider.Raycast(ray, out hit, Mathf.Infinity))
        {

            //Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow, 2.0f);
            // if hit get vertice
            clickedVertex = hit.point;

            // start position for laser ray
            laser.SetPosition(0, ray.origin);
            // end position for laser ray
            laser.SetPosition(1, clickedVertex);
            // draw line
            laser.enabled = true;
        }

        // return clicked vertice or empty
        return clickedVertex;
    }

    // move the vertices
    void MovePlane(Vector3 centroid, float speed)
    {
        // get mesh
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        // calculate variance depending on Height of the clicked vertice
        var variance = 1 + centroid.y / 5;
        var movementSpeed = speed * variance;

        // standard Gauss manipulation
        if(!vulcanization)
        {
            for(var i = 0; i < vertices.Length; i++)
            {
                // get vertex to move from the mesh 
                var vertex = vertices[i];
                // calculate Height to move the vertice based on Gauss | multivariate normal distribution
                var modifier = (float) ((1 / (2 * Math.PI * Math.Pow(variance, 2)))
                    * Math.Exp(-(1.0f / 2)
                    * ((Math.Pow((vertex.x - centroid.x), 2) / Math.Pow(variance, 2)) 
                    + (Math.Pow((vertex.z - centroid.z), 2) / Math.Pow(variance, 2)))
                    )
                    );
                // calculate new Height for the vertice
                vertex.y += modifier * movementSpeed;
                // vertices under 0.001 set to zero to avoid a big radius
                setHeightToZero(ref vertex, 0.001f);
                // replace the old mesh vertice with the new
                vertices[i] = vertex;
            }
        }
        // Vulcan manipulation
        else
        {
            for(var i = 0; i < vertices.Length; i++)
            {
                // get vertex to move from the mesh 
                var vertex = vertices[i];
                // calculate the distance from the actual mesh to the center mesh
                var distance = Math.Sqrt(Math.Pow(centroid.x - vertex.x, 2) + Math.Pow(centroid.z - vertex.z, 2));
                // calculate Height to move the vertice based on Gauss | multivariate normal distribution
                var modifier = (float) ((1 / (2 * Math.PI * Math.Pow(variance, 2)))
                    * Math.Exp(-(1.0f / 2)
                    * ((Math.Pow((vertex.x - centroid.x), 2) / Math.Pow(variance, 2)) 
                    + (Math.Pow((vertex.z - centroid.z), 2) / Math.Pow(variance, 2)))
                    )
                    );
                // check if vertice is inside the vulcan cone
                if(distance <= variance)
                {
                    // if inside --> reduce height by value --> negative speed
                    // ignore teh mousewheel direction
                    movementSpeed = -Mathf.Abs(movementSpeed);
                }
                else
                {
                    // if outside --> add height --> positive value
                    // ignore teh mousewheel direction
                    movementSpeed = Mathf.Abs(movementSpeed);
                }
                vertex.y += modifier * movementSpeed * variance;
                // vertices under 0.001 set to zero to avoid a big radius
                setHeightToZero(ref vertex, 0.001f);
                // replace the old mesh vertice with the new
                vertices[i] = vertex;
            }
        }
        // apply new vertices to the mesh
        mesh.vertices = vertices;
        // recalculate mesh
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }


    // create the Plane 
    void createRandomPlane()
    {
        // calculate the number of necessary vertices
        var verticeCounter = (numberDivisions + 1) * (numberDivisions + 1);
        // create an array for all vertices
        var vertices = new Vector3[verticeCounter];
        // create an uv array for the texture for the mesh
        var uv = new Vector2[verticeCounter];
        // create an array for all triangles or the Diamonds | 6 triangles per Diamond
        var trianglesForPlane = new int[numberDivisions * numberDivisions * 6];

        // claculate half size of the Plane
        float halfSize = sizeOfGerneratedPlane * 0.5f;
        // calculate the size of each vertice
        float divisionSize = sizeOfGerneratedPlane / numberDivisions;

        // create new mesh
        var mesh = new Mesh();
        // for a higher resolution
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        // get mesh filter
        GetComponent<MeshFilter>().mesh = mesh;

        // index for triangle position
        int triOffset = 0;

        // for each row
        for(int i = 0; i <= numberDivisions; i++)
        {
            // for each column
            for(int j = 0; j <= numberDivisions; j++)
            {
                // create a vertice at position
                vertices[i * (numberDivisions + 1) + j] = new Vector3(-halfSize + j * divisionSize, 0.0f, halfSize - i * divisionSize);
                // create uv at position
                uv[i * (numberDivisions + 1) + j] = new Vector2((float) i / numberDivisions, (float) j / numberDivisions);

                // set points for the Diamonds | increases index by 6 because of corners
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

        // set random values to the corner vertices
        CornerVertices(ref vertices);

        // start the diamond square
        int iterations = (int) Mathf.Log(numberDivisions, 2);
        // number of squares
        int numSquares = 1;
        int squareSize = numberDivisions;
        float tmpMaximumGenerateHeight = randomHeightDiff;
        // do a diamond square
        for(int i = 0; i < iterations; i++)
        {
            int row = 0;
            for(int j = 0; j < numSquares; j++)
            {
                int col = 0;
                for(int k = 0; k < numSquares; k++)
                {
                    // calculate new center and border values
                    DiamondSquareAlgorithm(
                        ref vertices, 
                        row, 
                        col, 
                        squareSize, 
                        tmpMaximumGenerateHeight
                    );

                    col += squareSize;

                }
                row += squareSize;
            }
            // new number of squares
            numSquares *= 2;
            // new square size
            squareSize /= 2;
            // new Height Difference possible
            tmpMaximumGenerateHeight *= 0.5f;
        }

        // set negative height back to zero
        for(var i = 0; i < vertices.Length; i++)
        {
            // check each vertice
            setHeightToZero(ref vertices[i]);
        }
        // apply vertices to the mesh
        mesh.vertices = vertices;
        // apply uv as texture array to the mesh
        mesh.uv = uv;
        // apply the triangles to the mesh
        mesh.triangles = trianglesForPlane;

        // recakuculate
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        // add new mesh to the mesh collider
        meshCollider.sharedMesh = mesh;
    }

    void CornerVertices(ref Vector3[] vertices)
    {
        vertices[0].y = UnityEngine.Random.Range(-randomHeightDiff, randomHeightDiff) 
            + offsetHeight;
        vertices[numberDivisions].y = UnityEngine.Random.Range(-randomHeightDiff, 
            randomHeightDiff) + offsetHeight;
        vertices[vertices.Length - 1].y = UnityEngine.Random.Range(-randomHeightDiff, 
            randomHeightDiff) + offsetHeight;
        vertices[vertices.Length - 1 - numberDivisions].y = 
            UnityEngine.Random.Range(-randomHeightDiff, randomHeightDiff) + offsetHeight;
    }


    // Diamond square algorithm
    void DiamondSquareAlgorithm(ref Vector3[] vertices, int row, int col, int size, float offset)
    {
        // calculate actual half size
        int halfSquareSize = (int) (size * 0.5f);

        // calculate index values
        int pointLeftTop = row * (numberDivisions + 1) + col;
        int pointLeftBottom = (row + size) * (numberDivisions + 1) + col;

        // calculate middle index
        int middle = (row + halfSquareSize) * (numberDivisions + 1) + col + halfSquareSize;

        // calculate center value
        vertices[middle].y = (vertices[pointLeftTop].y + vertices[pointLeftTop + size].y 
            + vertices[pointLeftBottom].y + vertices[pointLeftBottom + size].y) * 0.25f 
            + UnityEngine.Random.Range(-offset, offset);

        // calculate new squares | up center | middle left | middle right | down center 
        // --> 4 new squares out of one
        vertices[pointLeftTop + halfSquareSize].y = (vertices[pointLeftTop].y 
            + vertices[pointLeftTop + size].y + vertices[middle].y) / 3 
            + UnityEngine.Random.Range(-offset, offset);
        vertices[middle - halfSquareSize].y = (vertices[pointLeftTop].y 
            + vertices[pointLeftBottom].y + vertices[middle].y) / 3 
            + UnityEngine.Random.Range(-offset, offset);
        vertices[middle + halfSquareSize].y = (vertices[pointLeftTop + size].y 
            + vertices[pointLeftBottom + size].y + vertices[middle].y) / 3 
            + UnityEngine.Random.Range(-offset, offset);
        vertices[pointLeftBottom + halfSquareSize].y = (vertices[pointLeftBottom].y 
            + vertices[pointLeftBottom + size].y + vertices[middle].y) / 3 
            + UnityEngine.Random.Range(-offset, offset);


    }

    // set values unter the maxHeight to zero --> water
    void setHeightToZero(ref Vector3 vertex, float maxHeight = 0)
    {
        if(vertex.y < maxHeight)
        {
            vertex.y = 0;
        }
    }
}