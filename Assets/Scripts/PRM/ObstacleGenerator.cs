using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleGenerator : MonoBehaviour
{
    private Mesh[] cylinderObjects;
    public int obstacleNum = 100;
    private GameObject obstacleCube = null;
    // Start is called before the first frame update
    void generateObstacles(){
        obstacleCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        for(int i = 0; i < obstacleNum; i++){
            float obstacleScalingXZ = Random.Range(10, 20);
            Vector3 obstacleScaling = new Vector3(obstacleScalingXZ, Random.Range(20, 50), obstacleScalingXZ);
            Vector3 obstaclePosition = new Vector3(Random.Range(-ConstValues.kPRMSize * 5, ConstValues.kPRMSize * 5), obstacleScaling.y / 2, Random.Range(-ConstValues.kPRMSize * 5, ConstValues.kPRMSize * 5));
            GameObject instancedMesh = Instantiate(obstacleCube, obstaclePosition, Quaternion.identity);
            instancedMesh.transform.localScale = obstacleScaling;
            instancedMesh.transform.name = "building" + i;
            instancedMesh.transform.SetParent(this.transform);
        }
        obstacleCube.SetActive(false);
    }
    void Awake()
    {
        generateObstacles();
        // generateNodes();
    }
}
