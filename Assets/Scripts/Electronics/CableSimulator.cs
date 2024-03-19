using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;

public class CableSimulator : MonoBehaviour
{

    //WATCH THIS!!!
    /*https://www.youtube.com/watch?v=aVwxzDHniEw&ab_channel=FreyaHolm%C3%A9r */

    private List<Vector3> originalPoints_ = new List<Vector3>();
    private List<Vector3> evenPoints_ = new List<Vector3>();
    private List<Vector3> bezierPoints_ = new List<Vector3>();
    private List<Vector3> prevPoints_ = new List<Vector3>();

    float offset = 0.5f;

    [SerializeField]
    GameObject type_;

    Mesh mesh; 


    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        GenerateConnectedWire(new Vector3(-3.6f, 6, -5.1f), new Vector3(-7.81f, 3, 6.04f));
        GenerateMesh(evenPoints_, evenPoints_.Count, 10);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < evenPoints_.Count; i++)
        {
            bool locked = false; 
            if (!locked)
            {
                Vector3 curr = evenPoints_[i];
                evenPoints_[i] = evenPoints_[i] + (evenPoints_[i] - prevPoints_[i]) * 0.25f + Vector3.down * 9.8f * Time.deltaTime * Time.deltaTime;
                prevPoints_[i] = curr;



            }
        }
    }

    private void GenerateMesh(List<Vector3> points, int length, int pointsPerCircle)
    {
        //take one point
        List<int> triangles = new List<int>();
        List<Vector3> vecList = new List<Vector3>();

        for (int i = 0; i < length; i++)
        {
            //grab a point
            //make other points out of it
            Vector3 centerVec = points[i];
            Vector3 normVec = Vector3.Normalize(centerVec);
            Vector3 crossVec = Vector3.Cross(centerVec, normVec);
           

            for (int j = 0; j < pointsPerCircle; j++)
            {
                float angle = (float)(j / pointsPerCircle) * Mathf.Rad2Deg;

                float posX = Mathf.Cos(angle) + 1.0f;
                float posY = Mathf.Sin(angle) + 1.0f;

                Vector3 newPoint = new Vector3();
                newPoint = (normVec * posX) + (crossVec * posY) + centerVec;
                vecList.Add(newPoint);

                triangles.Add(j);
                triangles.Add(j + 1);
                triangles.Add(j + pointsPerCircle);

                triangles.Add(j);
                triangles.Add(j + pointsPerCircle);
                triangles.Add(j + pointsPerCircle + 1);
            }
        }
        mesh.SetVertices(vecList);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
    }

    private void GenerateConnectedWire(Vector3 startPos, Vector3 endPos)
    {
        GameObject agent = new GameObject();
        agent.transform.position = startPos;
        agent.AddComponent<NavMeshAgent>();
        NavMeshPath path = new NavMeshPath();
        float totalDistance = 0;

        if (agent.GetComponent<NavMeshAgent>().CalculatePath(endPos, path))
        {
            for (int i = 0; i < path.corners.Length; i++)
            {
                if (i != path.corners.Length - 1)
                {
                    if (Vector3.Distance(path.corners[i], path.corners[i + 1]) < 0.5f)
                    {
                        continue;
                    }
                    totalDistance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }
                originalPoints_.Add(path.corners[i]);
            }
        }
        else
        {
            Debug.Log("Somethings gone wrong with the cables!\n");
        }

        float distanceBetween = totalDistance / originalPoints_.Count;
        Debug.Log(distanceBetween);

        int totalAmount = (int)(totalDistance / offset);
        float distanceTravelled = 0.0f;
        evenPoints_.Add(originalPoints_[0]);

        int tracker = 0;

        for (int i = 0; i < totalAmount + Mathf.Max(offset,1); i++)
        {
            distanceTravelled += offset;

            while (distanceTravelled >= Vector3.Distance(originalPoints_[tracker], originalPoints_[tracker + 1]))
            {
                distanceTravelled -= Vector3.Distance(originalPoints_[tracker], originalPoints_[tracker + 1]);
                tracker++;
            }

            Vector3 direction = (originalPoints_[tracker + 1] - originalPoints_[tracker]).normalized;
            Vector3 newPoint = originalPoints_[tracker] + direction * distanceTravelled;
            evenPoints_.Add(newPoint);
        }
        evenPoints_.Add(originalPoints_[originalPoints_.Count - 1]);

        Destroy(agent);
        Debug.Log(totalDistance);
    }

    private void ApplyBezierCurve(List<Vector3> vecList)
    {
        for (int i = 0; i < vecList.Count; i++)
        {
            bezierPoints_.Add(EvaluateQuadraticCurve(vecList[i], vecList[i + 1], vecList[i + 2], Time.deltaTime));
        }
    }

    public Vector2 EvaluateQuadraticCurve(Vector3 vecA, Vector3 vecB, Vector3 vecC, float t)
    {
        float tt = t * t;
        float u = 1f - t;
        float uu = u * u;

        Vector3 newVec = new Vector3();
        newVec += 2f * u * t * vecB;
        newVec += tt * vecC;

        return newVec;

        //Vector3 p0 = Vector3.Lerp(vecA, vecB, t);
        //Vector3 p1 = Vector3.Lerp(vecB, vecC, t);

        //return Vector3.Lerp(p0, p1, t);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < originalPoints_.Count; i++)
        {
            Gizmos.DrawSphere(originalPoints_[i], 0.1f);
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < evenPoints_.Count; i++)
        {
            Gizmos.DrawSphere(evenPoints_[i], 0.1f);
        }

        Gizmos.color = Color.black;
        for (int i = 0; i < evenPoints_.Count; i++)
        {
            Gizmos.DrawSphere(bezierPoints_[i], 0.1f);
        }
    }
}
