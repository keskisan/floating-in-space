
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class RocketIndicator : UdonSharpBehaviour
{
    [SerializeField]
    float laserWidth = 0.05f;

    Mesh mesh;

    Vector3[] newVertices;
    int VerticeCounter;
    int[] newTriangles;
    int TriangleCounter;

    private void Start()
    {
        mesh = gameObject.GetComponent<MeshFilter>().mesh;

        MeshSetup();
    }

    private void MeshSetup()
    {
        newVertices = new Vector3[4];
        newTriangles = new int[6];
        VerticeCounter = 0;
        TriangleCounter = 0;
    }

    public void DrawLine(Vector3 startPos, Vector3 endPos)
    {
        MeshSetup();
        AddQuad(
                       startPos + transform.up * laserWidth,
                       startPos - transform.up * laserWidth,
                       endPos + transform.up * laserWidth,
                       endPos - transform.up * laserWidth);
        DrawMesh();
    }



    void AddQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        newVertices[VerticeCounter] = p1; //1
        VerticeCounter++;
        newVertices[VerticeCounter] = p2; //2
        VerticeCounter++;
        newVertices[VerticeCounter] = p3; //3
        VerticeCounter++;
        newVertices[VerticeCounter] = p4; //4
        VerticeCounter++;

        newTriangles[TriangleCounter] = VerticeCounter - 4; //1
        TriangleCounter++;
        newTriangles[TriangleCounter] = VerticeCounter - 3; //2
        TriangleCounter++;
        newTriangles[TriangleCounter] = VerticeCounter - 2; //3
        TriangleCounter++;
        newTriangles[TriangleCounter] = VerticeCounter - 2; //3 
        TriangleCounter++;
        newTriangles[TriangleCounter] = VerticeCounter - 1; //4
        TriangleCounter++;
        newTriangles[TriangleCounter] = VerticeCounter - 4; //1
        TriangleCounter++;
    }

    void DrawMesh()
    {
        if (VerticeCounter == 0) return;
        mesh.Clear();
        Vector3[] tmpVertices = new Vector3[VerticeCounter];
        Array.Copy(newVertices, 0, tmpVertices, 0, VerticeCounter);
        int[] tmpTriangles = new int[TriangleCounter];
        Array.Copy(newTriangles, 0, tmpTriangles, 0, TriangleCounter);
        mesh.vertices = tmpVertices;
        mesh.triangles = tmpTriangles;
    }
}
