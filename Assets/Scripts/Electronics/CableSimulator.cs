using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;

public class CableSimulator : MonoBehaviour
{

    //WATCH THIS!!!
    /*https://www.youtube.com/watch?v=aVwxzDHniEw&ab_channel=FreyaHolm%C3%A9r */
    class Point
    {
        public Vector3 position;
        public Vector3 prevPosition;
        public bool locked_;
    }

    private Point[] points_;
    private List<Vector3> vecList_ = new List<Vector3>();
    int offset = 5;

    //[SerializeField]
    //GameObject type_;


    void Start()
    {
        GenerateConnectedWire(new Vector3(0, 0, 0), new Vector3(-7, 12, 0));
    }

    // Update is called once per frame
    void Update()
    {
        //foreach (Point p in points_)
        //{
        //    if (!p.locked_)
        //    {
        //        Vector3 positionBeforeUpdate = p.position;
        //        p.position += p.position - p.prevPosition;
        //        Vector3 newPos = new Vector3(0, 9.81f * Time.deltaTime * Time.deltaTime, 0);
        //        p.position += newPos;
        //        p.prevPosition = positionBeforeUpdate;

                

        //    }
        //}
    }

    private void GenerateMesh(Point[] points, int length, int pointsPerCircle)
    {
        ////take one point
        //List<int> triangles = new List<int>();

        //for (int i = 0; i < length; i++)
        //{
        //    //grab a point
        //    //make other points out of it
        //    float angle = Mathf.Rad2Deg;

        //    float posX = Mathf.Cos(angle);
        //    float posY = Mathf.Sin(angle);

        //    for (int j = 0; j < pointsPerCircle; j++)
        //    {
        //        triangles.Add(j);
        //        triangles.Add(j + 1);
        //        triangles.Add(j + pointsPerCircle);

        //        triangles.Add(j);
        //        triangles.Add(j + pointsPerCircle);
        //        triangles.Add(j + pointsPerCircle + 1);
        //    }



        //}

    }

    private void GenerateConnectedWire(Vector3 startPos, Vector3 endPos)
    {
        GameObject agent = new GameObject();
        agent.transform.position = startPos;
        agent.AddComponent<NavMeshAgent>();
        NavMeshPath path = new NavMeshPath();
        if (agent.GetComponent<NavMeshAgent>().CalculatePath(endPos, path))
        {
            for (int i = 0; i < path.corners.Length; i++)
            {
                GameObject temp = new GameObject("TestPlace");
                
                temp.transform.position = path.corners[i];
            }
        }
        else
        {
            Debug.Log("Somethings gone wrong with the cables!\n");
        }
        //create a navmesh agent
        //grab the shortest path
        //create evenly offset points from startpos to endpos, following the path
        // apply bezier curves to the points
        //boom
    }
}
