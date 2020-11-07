using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PRMTerrainControl : MonoBehaviour
{
    private float terrainSize = ConstValues.kPRMSize;
    // Start is called before the first frame update
    void Start()
    {
        this.transform.localScale = new Vector3(terrainSize, terrainSize, terrainSize);
    }
}
