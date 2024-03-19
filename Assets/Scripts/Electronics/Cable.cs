using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cable : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    [Range(1, 1000)]
    int length = 1;

    [SerializeField]
    float offset_;

    [SerializeField]
    GameObject partPrefab, parentObj;

    GameObject male_;
    GameObject female_;

    [SerializeField]
    bool reset, spawn, snapFirst, snapLast;

    private bool isPowered_;

    // Update is called once per frame

    private void Start()
    {
        SpawnCable();
    }
    void Update()
    {
        if (reset)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("CableSegment"))
            {
                Destroy(obj);
            }
            reset = false;
        }

        if (snapLast)
        {
            parentObj.transform.Find(parentObj.transform.childCount.ToString()).GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
        }

        if (spawn)
        {
            SpawnCable();
            spawn = false; 
        }
    }

    public void SpawnCable()
    {
        int count = (int)(length / offset_);

        for (int i = 0; i < count; i++)
        {
            GameObject temp;
            temp = Instantiate(partPrefab, new Vector3(transform.position.x, transform.position.y + offset_ * (i + 1), transform.position.z), 
                Quaternion.identity, parentObj.transform);
            temp.transform.eulerAngles = new Vector3(180, 0, 0);

            temp.name = parentObj.transform.childCount.ToString();
            if (i == 0)
            {
                Destroy(temp.GetComponent<CharacterJoint>());
                if (snapFirst)
                {
                    temp.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
                }
            }
            else
            {
                temp.GetComponent<CharacterJoint>().connectedBody = parentObj.transform.Find((parentObj.transform.childCount - 1).ToString()).GetComponent<Rigidbody>();
            }
        }

        if (snapLast)
        {
            parentObj.transform.Find(parentObj.transform.childCount.ToString()).GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;

        }
    }
}
