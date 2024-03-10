using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchCardPuzzle : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject[] array_;
    public GameObject type_; 

    void Start()
    {
        array_ = new GameObject[16];
        int i = 0;

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                array_[i] = Instantiate(type_, new Vector3(x, y, 1.0f), Quaternion.identity);
                i++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
