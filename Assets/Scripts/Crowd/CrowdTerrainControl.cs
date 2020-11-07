using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdTerrainControl : MonoBehaviour
{
    public float terrainSize;
    // Start is called before the first frame update
    void Start()
    {
        this.transform.localScale = new Vector3(terrainSize, terrainSize, terrainSize);
    }
}
