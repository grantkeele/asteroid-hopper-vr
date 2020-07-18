using OVRTouchSample;
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
    [SerializeField] private Vector3 selectorOffset;
    [SerializeField] private float selectorRadius;
    [SerializeField] private float nodeRadius;
    [SerializeField] private float gripThreshold = 0.5f;

    [Header("Prefabs")]
    [SerializeField] private GameObject selectorSpherePrefab;
    [SerializeField] private GameObject nodeDisplayPrefab;

    [Header("Materials")]
    //[SerializeField] private Material partSelectedMat;
    [SerializeField] private Material nodeSelectedMat;
    [SerializeField] private Material nodeUnselectedMat;


    private GameObject primaryHand;
    private GameObject selectorSphere;
    private GameObject[] allParts;
    private GameObject selectedPart;
    private Node selectedNode;

    private Vector3 handAnchorPos;
    private Vector3 playerAnchorPos;

    private bool toolButtonHeld = false;
    private bool gripHeld = false;

    private Vector3 partDist;
    private Vector3 handDist;


    [HideInInspector] public BuildingTool currentTool;

    

    void Start()
    {
        primaryHand = rightHand;
        currentTool = BuildingTool.SelectPart;

        selectorSphere = Instantiate(selectorSpherePrefab, primaryHand.transform);
        selectorSphere.transform.localScale = new Vector3(selectorRadius*2, selectorRadius*2, selectorRadius*2);
        selectorSphere.transform.position = selectorOffset;
    }


    void Update()
    {
        // Update list of all parts in this ship
        allParts = GetChildren(partsContainer);


        // switch tool if X (left hand, lower button) is pressed
        if(OVRInput.GetDown(OVRInput.Button.Three)){
            if (currentTool == BuildingTool.MovePartFree)
                currentTool = BuildingTool.SelectPart;
            else{
                currentTool = BuildingTool.MovePartFree;
            }
            Debug.Log(currentTool);

        }


        // Handle player movement if secondary grip button is held
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) >= gripThreshold){

            // Set the anchor position on the first frame the grip is held
            if (gripHeld == false) {
                handAnchorPos = primaryHand.transform.position;
                playerAnchorPos = transform.position;
                gripHeld = true;
            }

            Vector3 handOffset = handAnchorPos - primaryHand.transform.position;
            transform.position = playerAnchorPos + handOffset;
        }
        else {
            gripHeld = false;
        }


        if (OVRInput.Get(OVRInput.Button.One)) {
            selectedPart.transform.position = selectorSphere.transform.position + partDist;
        }


        // All possible tools
        switch (currentTool){
                
            case (BuildingTool.MovePartFree):

                if (selectedPart != null){

                    if(OVRInput.GetDown(OVRInput.Button.One)){
                        partDist = selectedPart.transform.position - selectorSphere.transform.position;
                    }

                    if(OVRInput.Get(OVRInput.Button.One)){
                        selectedPart.transform.position = selectorSphere.transform.position + partDist;
                    }
                }
                
                break;
            
            case (BuildingTool.SelectPart):

                if(OVRInput.GetDown(OVRInput.Button.One)){

                    foreach (GameObject part in allParts){
                    part.GetComponent<MeshRenderer>().material = part.GetComponent<StructuralPart>().mainMaterial;
                    }

                    if (IsPartInRange(allParts, selectorSphere.transform.position)){
                        selectedPart = SelectClosestPart(allParts, selectorSphere.transform.position);
                        
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
                        if (IsNodeInRange(selectedPart, selectorSphere.transform.position)){
                            selectedNode = SelectClosestNode(selectedPart, selectorSphere.transform.position);
                            
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

            if (dist < selectorRadius){
                return true;
            }
        }

        return false;
    }

    GameObject SelectClosestPart(GameObject[] allParts, Vector3 handPos){

        float minDist = selectorRadius;
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

        float inclusionRadius = nodeRadius + selectorRadius;

        for (int i = 0; i < nodes.Length; i++){
            // Return first node inside of selector radius
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

        float inclusionRadius = nodeRadius + selectorRadius;
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
