using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdAgentControl : MonoBehaviour
{
    public static CrowdNodes[] crowds;
    private static int numAgent;
    private int agentID;
    private float agentRadius;
    const int kRadiusCohesion = 10, kRadiusAlignment = 10, kRadiusSeperation = 5;
	const float kMaxSpeed = 15;
	const float kMaxCohesionForce = 1, kMaxSeparationForce = 100, kMaxAlignmentForce = 1, kMaxToCenterForce = 1;
    const float kMinParticleToParticleDistance = 0.05f;
	const float kCohesionFactor = 0.5f, kAlignmentFactor = 0.5f, kSeperationFactor = 2.5f, kToCenterFactor = 0.001f;
    Animator animator;
    
    public void setAgentID(int inID){
        agentID = inID;
    }
    public static void setStaticParams(CrowdNodes[] inCrowds, int inNumAgent){
        crowds = inCrowds;
        numAgent = inNumAgent;
    }
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        animator.Play("HumanoidWalk");

        if(crowds[agentID].velocity.magnitude != 0){
            this.transform.rotation = Quaternion.LookRotation(crowds[agentID].velocity.normalized, Vector3.up);
        }
        float colliderX = this.GetComponent<BoxCollider>().size.x / 2;
        float colliderZ = this.GetComponent<BoxCollider>().size.z / 2;
        agentRadius = Mathf.Sqrt(colliderX * colliderX + colliderZ * colliderZ);
    }
    void updateBoids(){
        Vector3 cohesionPosition = new Vector3(0,0,0), alignmentVelocity = new Vector3(0,0,0), separationForce = new Vector3(0,0,0);
        float distance, cohesionCount = 0, alignmentCount = 0, separationCount = 0;
        for(int j = 0; j < crowds.Length; j++){
            if(j == agentID){
                continue;
            }
            distance = Vector3.Distance(crowds[agentID].position, crowds[j].position);
            if (distance < kRadiusCohesion) {
                cohesionPosition += crowds[j].position;
                cohesionCount++;
            }
            if (distance < kRadiusAlignment) {
                alignmentVelocity += crowds[agentID].velocity;
                alignmentCount++;
            }
            if (distance < kRadiusSeperation) {
                if (distance > 0) {
                    distance -= agentRadius;
                    if (distance < kMinParticleToParticleDistance)
                        distance = kMinParticleToParticleDistance;
                    separationForce += Vector3.Normalize(crowds[agentID].position - crowds[j].position) / distance;
                    separationCount++;
                }
            }

        }
        if (cohesionCount > 0) {
            cohesionPosition /= cohesionCount;
            var attractionForce = (cohesionPosition - crowds[agentID].position);
            attractionForce /= (kRadiusCohesion * 2);
            attractionForce *= kCohesionFactor;
            attractionForce = Vector3.ClampMagnitude(attractionForce, kMaxCohesionForce);
            crowds[agentID].acceleration += attractionForce ;
        }
        if (alignmentCount > 0) {
            alignmentVelocity /= alignmentCount * kMaxSpeed;
            var alignmentForce = alignmentVelocity * kAlignmentFactor;
            alignmentForce = Vector3.ClampMagnitude(alignmentForce, kMaxAlignmentForce);
            crowds[agentID].acceleration += alignmentForce;
        }
        if (separationCount > 0) {
            separationForce /= separationCount;
            separationForce *= kSeperationFactor;
            separationForce = Vector3.ClampMagnitude(separationForce, kMaxSeparationForce);
            crowds[agentID].acceleration += separationForce;
        }
        Vector3 toCenterForce = crowds[agentID].center - crowds[agentID].position;
        if (toCenterForce.magnitude > 0) {
            crowds[agentID].acceleration += toCenterForce * kToCenterFactor;
        }
        crowds[agentID].velocity += crowds[agentID].acceleration * Time.fixedDeltaTime;
        crowds[agentID].velocity = Vector3.ClampMagnitude(crowds[agentID].velocity, kMaxSpeed);
        var deltaPosition = crowds[agentID].velocity * Time.fixedDeltaTime;
        crowds[agentID].acceleration = new Vector3(0, 0, 0);
        if (deltaPosition.magnitude != 0) {
            this.transform.rotation = Quaternion.LookRotation(deltaPosition.normalized, Vector3.up);
        }
        crowds[agentID].position += deltaPosition;
        this.transform.position += deltaPosition;
    }
    private void FixedUpdate() {
        updateBoids();
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
