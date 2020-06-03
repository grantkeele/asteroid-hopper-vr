using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Node{

        public Node(Vector3 position, Vector3 normal){
            Position = position;
            Normal = normal;
        }

        public Vector3 Position{ get; }
        public Vector3 Normal{ get; }
    } 

public class StructuralPart : MonoBehaviour
{
    public enum PartType{
        PLATE_RECT,
        PLATE_TRI_RIGHT,
        PLATE_TRI_ISOCELES,
        WINDOW_RECT,
        WINDOW_TRI_RIGHT,
        WINDOW_TRI_ISOCELES,
        SUPPORT_GIRDER,
        SUPPORT_DOORFRAME
    };

    
    PartType[] PART_TYPES_PLATES = { PartType.PLATE_RECT, PartType.PLATE_TRI_RIGHT, PartType.PLATE_TRI_ISOCELES };
    PartType[] PART_TYPES_WINDOWS = { PartType.WINDOW_RECT, PartType.WINDOW_TRI_RIGHT, PartType.WINDOW_TRI_ISOCELES };
    PartType[] PART_TYPES_SUPPORTS = { PartType.SUPPORT_GIRDER, PartType.SUPPORT_DOORFRAME };

    


    // Input parameters and data 
    [SerializeField] private PartType partType;
    [SerializeField] private Mesh inputMesh;
    [SerializeField] private Vector3 inputMeshRotation;
    [SerializeField] private Vector3[] nodePositions;
    [SerializeField] private Vector3[] nodeNormals;

   
    [SerializeField] private float partLength; // z dimension
    [SerializeField] private float partWidth; // x dimension
    [SerializeField] private float partThickness; // y dimension

    
    private Node[] inputNodes;
    public Node[] localNodes;
    public Node[] worldNodes;

    public Material mainMaterial;
    public Material selectedMaterial;
    
    private MeshFilter meshFilter;
    private MeshCollider collider;

    private Mesh partMesh;

    void OnDrawGizmos()
    {
        try{
            Node[] drawNodes = worldNodes;
        
            for (int i = 0; i < drawNodes.Length; i++){
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(drawNodes[i].Position, 0.1f);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(drawNodes[i].Position, drawNodes[i].Position + drawNodes[i].Normal);
            }
        }
        catch{}
    }


    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        collider = GetComponent<MeshCollider>();

        // Fill nodes array with input positions and normals
        inputNodes = new Node[nodePositions.Length];

        for (int n = 0; n < nodePositions.Length; n++){
            inputNodes[n] = new Node(nodePositions[n], nodeNormals[n]);
        }
   
        partMesh = ScaleMeshByParams(inputMesh, partType);
        meshFilter.mesh = partMesh;
    }


    void Update()
    {   
        partMesh = ScaleMeshByParams(inputMesh, partType);

        meshFilter.mesh = partMesh;
        collider.sharedMesh = partMesh;

        localNodes = ScaleNodesByParams(inputNodes, partType);
        worldNodes = TransformNodes(localNodes, this.transform);
    }


    private Mesh ScaleMeshByParams(Mesh inputMesh, PartType part){
        Mesh outputMesh = new Mesh();

        Vector3[] inputVerts = inputMesh.vertices;
        Vector3[] outputVerts = new Vector3[inputVerts.Length];

        // Plates and windows
        if (IsPartTypeInArray(part, PART_TYPES_PLATES) || IsPartTypeInArray(part, PART_TYPES_WINDOWS)){

            for (int i = 0; i < inputVerts.Length; i++){
                Vector3 rotatedInput = Quaternion.Euler(inputMeshRotation) * inputVerts[i];
                Vector3 thisVert = new Vector3();

                thisVert.x = rotatedInput.x * partWidth;
                thisVert.y = rotatedInput.y * partThickness;
                thisVert.z = rotatedInput.z * partLength;

                outputVerts[i] = thisVert;
            }
        }

        // Support structures
        else if (IsPartTypeInArray(part, PART_TYPES_SUPPORTS)){

            for (int i = 0; i < inputVerts.Length; i++){    
                //Vector3 thisVert = new Vector3();

                //thisVert.x = inputVerts[v].x * partLength;
                //thisVert.y = inputVerts[v].y * partThickness;
                //thisVert.z = inputVerts[v].z * partWidth;

                //outputVerts[i] = thisVert;
            }
        }

        outputMesh.vertices = outputVerts;
        outputMesh.triangles = inputMesh.triangles;
        outputMesh.RecalculateNormals();

        Vector2[] uvIn = inputMesh.uv;
        Vector2[] uvOut = new Vector2[uvIn.Length];
        for (int i = 0; i < uvIn.Length; i++){
            //uvOut[i] = uvIn[i] * Mathf.Sqrt(partLength * partWidth);
            uvOut[i] = new Vector2(uvIn[i].x * partWidth, uvIn[i].y * partLength);
            //uvOut[i] = uvIn[i];
        }   

        outputMesh.uv = uvOut;

        return outputMesh;
    }


    private Node[] ScaleNodesByParams(Node[] inputNodes, PartType part){

        Node[] outputNodes = new Node[inputNodes.Length];

        // Plates and windows
        if (IsPartTypeInArray(part, PART_TYPES_PLATES) || IsPartTypeInArray(part, PART_TYPES_WINDOWS)){

            for (int i = 0; i < inputNodes.Length; i++){
                
                Vector3 newPos = inputNodes[i].Position;

                newPos.x *= partWidth;
                newPos.y *= partThickness;
                newPos.z *= partLength;

                Node thisNode = new Node(newPos, inputNodes[i].Normal);
                outputNodes[i] = thisNode;
            }
        }

        // Support structures
        else if (IsPartTypeInArray(part, PART_TYPES_SUPPORTS)){

            for (int i = 0; i < inputNodes.Length; i++){
                //foo
            }
        }


        return outputNodes;
    }


    private Node[] TransformNodes(Node[] inputNodes, Transform transform){

        Node[] outputNodes = new Node[inputNodes.Length];

        for (int i = 0; i < inputNodes.Length; i++){
                
            Vector3 newPos = inputNodes[i].Position;
            Vector3 newNormal = inputNodes[i].Normal;

            newNormal = transform.rotation * newNormal;

            newPos = transform.rotation * newPos;
            newPos += transform.position;


            Node thisNode = new Node(newPos, newNormal);
            outputNodes[i] = thisNode;
        }

        return outputNodes;
    }



    private bool IsPartTypeInArray(PartType part, PartType[] array){
        for (int i = 0; i < array.Length; i++){
            if (array[i] == part){
                return true;
            }
        }

        return false;
    }
}
