using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentControl : MonoBehaviour
{
    // Start is called before the first frame update
    public static PRMNodes[] myNodes;
    private static int numMapNode, numAgentNode;
    private int agentIdx, goalIdx, agentId, nextNodeIdx;
    private HashSet<int> vistedNode;
    private List<int> routePath;
    private int[] backTrackParents;
    private float agentRadius;
    private Vector3 curDirection;
    public float agentVelocity = 0.1f;
    private GameObject agentParent;
    Animator animator;
    
    public static void setStaticParams(PRMNodes[] inNodes, int inNumMapNode, int inNumAgentNode){
        myNodes = inNodes;
        numMapNode = inNumMapNode;
        numAgentNode = inNumAgentNode;
    }
    public void setAgentIdx(int inAgentIdx, int inGoalIdx){
        agentIdx = inAgentIdx;
        goalIdx = inGoalIdx;
        agentId = agentIdx - numMapNode;
    }
    private void pathPlanning(){
        nextNodeIdx = 1;
        vistedNode.Clear();
        routePath.Clear();
        for(int i = 0; i < backTrackParents.Length; i++){
            backTrackParents[i] = -1;
        }
        AStar();
        backTrack();
    }
    private void AStar(){
        int curNode = agentIdx;
        int maxIter = 0;
        vistedNode.Add(agentIdx);
        while(curNode != goalIdx && maxIter < 100){
            int minNext = -1;
            float minDist = float.MaxValue;
            maxIter++;
            foreach(int k in myNodes[curNode].neighbors){
                if(!vistedNode.Contains(k)){
                    float heuristicDist = Vector3.Distance(myNodes[k].position, myNodes[goalIdx].position);
                    float toNextDist = Vector3.Distance(myNodes[k].position, myNodes[curNode].position);
                    if(toNextDist + heuristicDist < minDist){
                        minDist = toNextDist + heuristicDist;
                        minNext = k;
                    }
                }
            }
            if(minNext == -1){
                break;
            }
            vistedNode.Add(minNext);
            backTrackParents[minNext] = curNode;
            curNode = minNext;
        }
    }
    public void sychronizePosition(){
        myNodes[agentIdx].position = this.transform.position;
    }

    public Vector3 getVelocityVector(){
        return curDirection.normalized * agentVelocity;
    }

    private void agentCollisionCheck(){
        //float minCollisionTime = float.MaxValue; 
        for(int i = 0; i < agentParent.transform.childCount; i++){
            if(i == agentId){
                continue;
            }
            var anotherAgent = agentParent.transform.GetChild(i);
            var distVector = anotherAgent.position - this.transform.position;
            if(distVector.magnitude > agentRadius * 2 + ConstValues.kToTurnRange){
                continue;
            }
            var anotherAgentVelocity = anotherAgent.GetComponent<AgentControl>().getVelocityVector();
            var relavateVelocity = this.getVelocityVector() - anotherAgentVelocity;
            if(Vector3.Dot(relavateVelocity, distVector) <= 0){
                continue;
            }
            Debug.Log("collision!");
        }
    }
    private void agentMove(){
        if(nextNodeIdx == routePath.Count){
            animator.Play("HumanoidIdle");
            curDirection = new Vector3(0, 0, 0);
            return;
        }
        var nextNode = routePath[nextNodeIdx];
        if(nextNode == -1){
            animator.Play("HumanoidIdle");
            return;
        }
        var distToNextNode = Vector3.Distance(this.transform.position, myNodes[nextNode].position);
        if(distToNextNode < 0.1){
            nextNodeIdx++;
            return;
        }
        curDirection = (myNodes[nextNode].position - this.transform.position).normalized;
        this.transform.position += curDirection * agentVelocity;
        //agentCollisionCheck();
        if(distToNextNode < ConstValues.kToTurnRange){
            // smooth rotation here
            if(nextNodeIdx < routePath.Count - 1){
                var nextNextNode = routePath[nextNodeIdx + 1];
                var nextDir = myNodes[nextNextNode].position - myNodes[nextNode].position;
                var turnLength = Mathf.Min(nextDir.magnitude / ConstValues.kToTurnRange, 1) * (ConstValues.kToTurnRange - distToNextNode);
                nextDir = nextDir.normalized * turnLength;
                curDirection = (myNodes[nextNode].position + nextDir - this.transform.position).normalized;
            }
        }
        transform.rotation = Quaternion.LookRotation(curDirection, Vector3.up);
        animator.Play("HumanoidWalk");
    }
    //back track the path
    private void backTrack(){
        int curNode = backTrackParents[goalIdx];
        routePath.Insert(0, goalIdx);
        while(curNode != -1 && curNode != agentIdx){
            routePath.Insert(0, curNode);
            curNode = backTrackParents[curNode];
        }
        routePath.Insert(0, curNode);
    }

    private void Awake() {
        routePath = new List<int>();
        vistedNode = new HashSet<int>();
        curDirection = new Vector3();
    }
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        animator.Play("HumanoidIdle");
        float colliderHalfX = this.GetComponent<BoxCollider>().size.x / 2;
        float colliderHalfZ = this.GetComponent<BoxCollider>().size.z / 2;
        agentRadius = Mathf.Sqrt(colliderHalfX * colliderHalfX + colliderHalfZ * colliderHalfZ);

        if(myNodes==null || myNodes.Length <= 0){
            return;
        }
        backTrackParents = new int[myNodes.Length];
        pathPlanning();
        agentParent = GameObject.Find("Nodes");
    }
    private void OnRenderObject() {
        for(int i = 0; i < routePath.Count - 1; i++){
            if(routePath[i] == -1 || routePath[i + 1] == -1){
                break;
            }
            Utils.RenderUtils.renderLines(myNodes[routePath[i]].position, myNodes[routePath[i + 1]].position, 
            Utils.RenderUtils.IdxMapColor(agentId, numAgentNode));
        }
    }
    private void FixedUpdate() {
        agentMove();
    }
    private void Update() {
    if(Input.GetKeyDown(KeyCode.R) || Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Alpha2) || 
        Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Q)){
            pathPlanning();
        }
    }
}
