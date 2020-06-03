using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingToolManager : MonoBehaviour
{
    public enum BuildingTool{
        SelectPart,
        SelectNode,
        InputPartParameters,
        InputPartTransform,
        MovePartFree,
        RotateAroundAxis,
        SnapToNodes
    }



    [Header("Part Prefabs")]
    [SerializeField] private GameObject[] structuralPartPrefabs;

    [Header("Runtime Objects")]
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    
    [SerializeField] private GameObject partsContainer;

    [Header("Editor Constants")]
    [SerializeField] private Vector3 selectionOffset;
    [SerializeField] private float selectionRadius;
    [SerializeField] private float nodeRadius;

    [Header("Prefabs")]
    [SerializeField] private GameObject selectionSpherePrefab;
    [SerializeField] private GameObject nodeDisplayPrefab;

    [Header("Materials")]
    //[SerializeField] private Material partSelectedMat;
    [SerializeField] private Material nodeSelectedMat;
    [SerializeField] private Material nodeUnselectedMat;


    private GameObject mainHand;
    private GameObject selectionSphere;
    private GameObject[] allParts;
    private GameObject selectedPart;
    private Node selectedNode;

    private bool toolButtonDown;
    private Vector3 partDist;


    [HideInInspector] public BuildingTool currentTool;

    

    void Start()
    {
        mainHand = rightHand;
        currentTool = BuildingTool.SelectPart;

        selectionSphere = Instantiate(selectionSpherePrefab, mainHand.transform);
        selectionSphere.transform.localScale = new Vector3(selectionRadius*2, selectionRadius*2, selectionRadius*2);
        selectionSphere.transform.position = selectionOffset;
    }


    void Update()
    {
        // Update list of parts in ship
        allParts = GetChildren(partsContainer);


        if(OVRInput.GetDown(OVRInput.Button.Three)){
            if (currentTool == BuildingTool.MovePartFree)
                currentTool = BuildingTool.SelectPart;
            else{
                currentTool = BuildingTool.MovePartFree;
            }
            Debug.Log(currentTool);
        }


        // All possible tools
        switch (currentTool){
                
            case (BuildingTool.MovePartFree):

                if (selectedPart != null){

                    if(OVRInput.GetDown(OVRInput.Button.One)){
                        partDist = selectedPart.transform.position - selectionSphere.transform.position;
                    }

                    if(OVRInput.Get(OVRInput.Button.One)){
                        selectedPart.transform.position = selectionSphere.transform.position + partDist;
                    }
                }
                
                break;
            
            case (BuildingTool.SelectPart):

                if(OVRInput.GetDown(OVRInput.Button.One)){

                    foreach (GameObject part in allParts){
                    part.GetComponent<MeshRenderer>().material = part.GetComponent<StructuralPart>().mainMaterial;
                    }

                    if (IsPartInRange(allParts, selectionSphere.transform.position)){
                        selectedPart = SelectClosestPart(allParts, selectionSphere.transform.position);
                        
                        selectedPart.GetComponent<MeshRenderer>().material = selectedPart.GetComponent<StructuralPart>().selectedMaterial;
                        Debug.Log("Selected part " + selectedPart.name);
                    }
                    else{
                        selectedPart = null;
                    }
            
                }
                
                break;

            case (BuildingTool.SelectNode):

                if(OVRInput.GetDown(OVRInput.Button.One)){
                    if (selectedPart != null){
                        if (IsNodeInRange(selectedPart, selectionSphere.transform.position)){
                            selectedNode = SelectClosestNode(selectedPart, selectionSphere.transform.position);
                            
                            //selectedNode.GetComponent<MeshRenderer>().material = selectedPart.GetComponent<StructuralPart>().selectedMaterial;
                            Debug.Log("Selected node " + selectedNode.Position);
                        }
                        else{
                            //selectedNode = null;
                        }
                    }
                }
                

                break;
    
        }
        
    
        
    }



    void SpawnStructuralPart(GameObject partPrefab, Vector3 pos){
        Instantiate(partPrefab, pos, Quaternion.identity, partsContainer.transform);
    }


    bool IsPartInRange(GameObject[] allParts, Vector3 handPos){

        for (int i = 0; i < allParts.Length; i++){
            Collider collider = allParts[i].GetComponent<Collider>();

            Vector3 closestPoint = collider.ClosestPoint(handPos);
            float dist = (closestPoint - handPos).magnitude;

            if (dist < selectionRadius){
                return true;
            }
        }

        return false;
    }

    GameObject SelectClosestPart(GameObject[] allParts, Vector3 handPos){

        float minDist = selectionRadius;
        GameObject closestPart = new GameObject();

        for (int i = 0; i < allParts.Length; i++){
            Collider collider = allParts[i].GetComponent<Collider>();

            Vector3 closestPoint = collider.ClosestPoint(handPos);
            float dist = (closestPoint - handPos).magnitude;

            if (dist < minDist){
                minDist = dist;
                closestPart = allParts[i];
            }
        }

        return closestPart;
    }


    bool IsNodeInRange(GameObject selectedPart, Vector3 handPos){
        StructuralPart partScript = selectedPart.GetComponent<StructuralPart>();
        Node[] nodes = partScript.worldNodes;

        float inclusionRadius = nodeRadius + selectionRadius;

        for (int i = 0; i < nodes.Length; i++){
            // Return first node inside of selection radius
            Vector3 nodePos = nodes[i].Position;

            float dist = (nodePos - handPos).magnitude;

            if (dist < inclusionRadius){
                return true;
            }
        }
        return false;
    }

    Node SelectClosestNode(GameObject selectedPart, Vector3 handPos){
        StructuralPart partScript = selectedPart.GetComponent<StructuralPart>();
        Node[] nodes = partScript.worldNodes;

        float inclusionRadius = nodeRadius + selectionRadius;
        float minDist = inclusionRadius;
        Node closestNode = new Node();

        // Calculate closest node in the array
        for (int i = 0; i < nodes.Length; i++){
            
            Vector3 nodePos = nodes[i].Position;

            float dist = (nodePos - handPos).magnitude;

            if (dist < minDist){
                minDist = dist;
                closestNode = nodes[i];
            }
        }

        return closestNode;
    }

    GameObject[] GetChildren(GameObject parent){
        GameObject[] children = new GameObject[parent.transform.childCount];
        int i = 0;

        foreach (Transform childTransform in parent.transform){
            GameObject child = childTransform.gameObject;
            children[i] = child;
            i++;
        }
        
        return children;
    }

}
