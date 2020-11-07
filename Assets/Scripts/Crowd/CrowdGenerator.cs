using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdNodes{
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 center;
    public CrowdNodes(){
        position = new Vector3();
        center = new Vector3();
        velocity = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
        acceleration = new Vector3(0, 0, 0);
    }
}

public class CrowdGenerator : MonoBehaviour
{
    public int numAgent = 100;
    private CrowdNodes[] crowd;
    private float agentRadius, colliderX, colliderZ;
    private GameObject templateAgent;
    // Start is called before the first frame update
    private void sampleNodes(int start, int end, Vector3 center){
        Utils.Rectangle tmpRec = new Utils.Rectangle();
        Utils.Circle tmpCircle = new Utils.Circle();
        tmpCircle.radius = agentRadius;
        for(int i = start; i < end; i++){
            bool collideObstacle = true;
            while(collideObstacle){
                Vector3 tmpPos = new Vector3(Random.Range(-ConstValues.kCrowdRange, ConstValues.kCrowdRange), 0, 
                Random.Range(-ConstValues.kCrowdRange, ConstValues.kCrowdRange)) - center;
                for(int j = 0; j < i; j++){
                    tmpRec.origin = crowd[i].position;
                    tmpRec.sideLength = new Vector2(colliderX, colliderZ);
                    tmpCircle.origin = tmpPos;
                    if(Utils.CollisionCheck.circleAABB2D(tmpRec, tmpCircle)){
                        break;
                    }
                    else if(j == i - 1){
                        crowd[i].position = tmpPos;
                        crowd[i].center = center;
                        collideObstacle = false;
                    }
                }
                if(i == 0){
                    crowd[i].position = tmpPos;
                    crowd[i].center = center;
                    collideObstacle = false;
                }
            }
        }
    }

    void generateAgents(){
        CrowdAgentControl.setStaticParams(crowd, numAgent);
        for(int i = 0; i < crowd.Length; i++){
            GameObject instancedModel = Instantiate(templateAgent, crowd[i].position, Quaternion.identity);
            instancedModel.transform.name = "agent" + (i);
            instancedModel.transform.GetComponent<CrowdAgentControl>().setAgentID(i);
            instancedModel.transform.SetParent(this.transform);
        }
        templateAgent.SetActive(false);
    }
    private void Awake() {
        crowd = new CrowdNodes[numAgent];
        for(int i = 0; i < numAgent; i++){
            crowd[i] = new CrowdNodes();
        }
        templateAgent = GameObject.Find("TemplateAgent");
        colliderX = templateAgent.GetComponent<BoxCollider>().size.x;
        colliderZ = templateAgent.GetComponent<BoxCollider>().size.z;
        agentRadius = Mathf.Sqrt(colliderX * colliderX + colliderZ * colliderZ) / 2;
    }
    void Start()
    {
        // sampleNodes(0, numAgent, new Vector3(0, 0, 0));
        // sampleNodes(0, numAgent / 2, new Vector3(80, 0, 80));
        // sampleNodes(numAgent / 2, numAgent, new Vector3(-80, 0, -80));

        sampleNodes(0, numAgent / 4, new Vector3(80, 0, 80));
        sampleNodes(numAgent / 4, numAgent / 2, new Vector3(-80, 0, -80));
        sampleNodes(numAgent / 2, numAgent *3 / 4, new Vector3(80, 0, -80));
        sampleNodes(numAgent * 3 / 4, numAgent, new Vector3(-80, 0, 80));

        generateAgents();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
