using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PRMNodes{
    public Vector3 position;
    public HashSet<int> neighbors;
    public PRMNodes(){
        position = new Vector3();
        neighbors = new HashSet<int>();
    }
}

public class NodesControl : MonoBehaviour
{
    // Start is called before the first frame update
    public int numMapNode= 100;
    public int numAgentNode = 10;
    public List<GameObject> agentList;
    private int numGoalNode, totalNode, startIdxOfAgent, startIdxOfGoal;
    private PRMNodes[] myNodes;
    private GameObject obstacles, templateAgent, selectedObject = null;
    System.Random rand = new System.Random();
    
    private void sampleNodes(int numNodeToUpdate){
        // sample nodes from 0 to numNodeToUpdate
        Utils.Rectangle tmpRec = new Utils.Rectangle();
        Utils.Circle tmpCircle = new Utils.Circle();
        float colliderHalfX = templateAgent.GetComponent<BoxCollider>().size.x / 2;
        float colliderHalfZ = templateAgent.GetComponent<BoxCollider>().size.z / 2;
        float agentRadius = Mathf.Sqrt(colliderHalfX * colliderHalfX + colliderHalfZ * colliderHalfZ);
        tmpCircle.radius = agentRadius;
        for(int i = 0; i < numNodeToUpdate; i++){
            bool collideObstacle = true;
            while(collideObstacle){
                Vector3 tmpPos = new Vector3(Random.Range(-ConstValues.kPRMSize * 5, ConstValues.kPRMSize * 5), 0, 
                Random.Range(-ConstValues.kPRMSize * 5, ConstValues.kPRMSize * 5));
                for(int j = 0; j < obstacles.transform.childCount; j++){
                    var curObstacle = obstacles.transform.GetChild(j);
                    tmpRec.origin = curObstacle.position;
                    tmpRec.sideLength = new Vector2(curObstacle.localScale.x, curObstacle.localScale.z);
                    tmpCircle.origin = tmpPos;
                    if(Utils.CollisionCheck.circleAABB2D(tmpRec, tmpCircle)){
                        break;
                    }
                    else if(j == obstacles.transform.childCount - 1){
                        myNodes[i].position = tmpPos;
                        collideObstacle = false;
                    }
                }
            }
        }
    }

    private void connectNeighbors(){
        Utils.Rectangle tmpRec = new Utils.Rectangle();
        Utils.Ray2D ray = new Utils.Ray2D();
        //Though box collider here but still consider it as circle, hard to check collisions when rectange rotates
        float colliderHalfX = templateAgent.GetComponent<BoxCollider>().size.x / 2;
        float colliderHalfZ = templateAgent.GetComponent<BoxCollider>().size.z / 2;
        float agentRadius = Mathf.Sqrt(colliderHalfX * colliderHalfX + colliderHalfZ * colliderHalfZ);
        for(int i = 0; i < totalNode; i++){
            for(int j = i + 1; j < totalNode; j++){
                ray.updateRay(myNodes[i].position, myNodes[j].position);
                for(int k = 0; k < obstacles.transform.childCount; k++){
                    var curObstacle = obstacles.transform.GetChild(k);
                    tmpRec.origin = curObstacle.position;
                    tmpRec.sideLength = new Vector2(curObstacle.localScale.x, curObstacle.localScale.z);
                    if(Utils.CollisionCheck.rayAABB2D(tmpRec, ray, agentRadius)){
                        break;
                    }
                    else if(k == obstacles.transform.childCount - 1){
                        myNodes[i].neighbors.Add(j);
                        myNodes[j].neighbors.Add(i);
                    }
                }
            }
        }
    }
    void generateAgents(){
        AgentControl.setStaticParams(myNodes, numMapNode, numAgentNode);
        int randModelIdx;
        GameObject instancedModel;
        // the map starts from 0-100, agent from 101-150, agent goal from 151-200
        for (int i = startIdxOfAgent; i < startIdxOfAgent + numAgentNode; i++)
        {
            // first agent is always ency
            // the rest can be anything specified within the agent list
            randModelIdx = rand.Next(agentList.Count);
            if(i == startIdxOfAgent){
                randModelIdx = 0;
            }
            instancedModel = Instantiate(agentList[randModelIdx], myNodes[i].position, Quaternion.identity);
            instancedModel.transform.name = "agent" + (i - startIdxOfAgent);
            instancedModel.transform.GetComponent<AgentControl>().setAgentIdx(i, i + this.numAgentNode);
            instancedModel.transform.SetParent(this.transform);
        }
        // set all template model inactivate, cuz template agent are in PRM node list
        for(int i = 0; i < agentList.Count; i++){
            agentList[i].SetActive(false);
        }
    }
    private void updateNodes(int numNodeToUpdate){
        for(int i = 0; i < myNodes.Length; i++){
            myNodes[i].neighbors.Clear();
        }
        // make sure the agent model position is consistant with myNode
        for(int i = 0; i < this.transform.childCount; i++){
            this.transform.GetChild(i).GetComponent<AgentControl>().sychronizePosition();
        }
        sampleNodes(numNodeToUpdate);
        connectNeighbors();
    }
    private void Awake() {
        numGoalNode = numAgentNode;
        totalNode = numMapNode + numAgentNode + numGoalNode;
        startIdxOfAgent = numMapNode;
        startIdxOfGoal = numMapNode + numAgentNode;
    }
    void Start()
    {
        myNodes = new PRMNodes[totalNode];
        obstacles = GameObject.Find("Obstacles");
        templateAgent = agentList[0];

        for(int i = 0; i < myNodes.Length; i++){
            myNodes[i] = new PRMNodes();
        }
        updateNodes(totalNode);
        generateAgents();
    }
    private void Update() {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                selectedObject = hit.collider.gameObject;
                Debug.Log(selectedObject.name);
            }
        }
        if(Input.GetKeyDown(KeyCode.R) || Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Alpha2) || 
        Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Q)){
            if(selectedObject)
                updateNodes(numMapNode);
        }
    }

    private void FixedUpdate() {
        if(Input.GetKey(KeyCode.Alpha1)){
            if(selectedObject){
                selectedObject.transform.position += new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
            }
        }
        if(Input.GetKey(KeyCode.Alpha2)){
            if(selectedObject){
                selectedObject.transform.position -= new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
            }
        }
        if(Input.GetKey(KeyCode.Q)){
            if(selectedObject){
                selectedObject.transform.position -= new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);
            }
        }
        if(Input.GetKey(KeyCode.E)){
            if(selectedObject){
                selectedObject.transform.position += new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);
            }
        }
    }

    private void OnRenderObject() {
        // for(int i = 0; i < numMapNode; i++){
        //     foreach(int k in myNodes[i].neighbors){
        //         if(k < numMapNode){
        //             Utils.RenderUtils.renderLines(myNodes[i].position, myNodes[k].position, new Color(1, 0, 0));
        //         }
        //     }
        // }
    }
}
