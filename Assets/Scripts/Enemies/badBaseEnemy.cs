using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class badBaseEnemy : MonoBehaviour
{
  //Navigation params
  public NavMeshAgent agent;
  public Transform player;
  public LayerMask isGround, isPlayer;

  //Hiding params
  public Vector3 safeSpot;
  public bool hideSpotSet;
  public bool scared;
  private float hidingDur;

  //Patrolling params
  public Vector3 goal;
  public bool isGoalSet;
  public float goalRange;
  LayerMask ignoreLayers;
  //Offensive params
  public float attackCooldown;
  public bool isReady;

  //State params
  public float sightRange, attackRange, hideRange;
  public bool playerSightRange, playerAttackRange, playerHideRange;

  private void Awake()
  {
    agent = GetComponent<NavMeshAgent>();
    //Set target to nearest player if multiple
    ignoreLayers = LayerMask.GetMask("isEnemy");

  }

  void SetNearestPlayer()
  {
    Vector3 enemyPos = transform.position;
    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
    if (players.Length > 1)
    {
      Transform nearestBuffer = players[0].transform;
      for (int i = 0; i < players.Length; ++i)
      {
        if (Vector3.Distance(enemyPos, players[i].transform.position) < Vector3.Distance(enemyPos, nearestBuffer.position))
        {
          nearestBuffer = players[i].transform;
        }
      }
      player = nearestBuffer;
    }
    else
    {
      player = GameObject.FindGameObjectWithTag("Player").transform;
    }
  }
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    SetNearestPlayer();
    //if (!player)
    //{
    //  Patrolling();
    //}
    //Check if player is within sight or attack range
    playerSightRange = Physics.CheckSphere(transform.position, sightRange, isPlayer);
    playerAttackRange = Physics.CheckSphere(transform.position, attackRange, isPlayer);
    playerHideRange = Physics.CheckSphere(transform.position, hideRange, isPlayer);
    if (Input.GetKey(KeyCode.P))
    {
      if (!scared)
      {

        Debug.Log("Marcus is now scared\n");
        scared = true;
        hideSpotSet = false;
      }
      else
      {
        scared = false;
        Debug.Log("Marcus is now brave\n");
      }
    }
    //Simple state machine based on available actions
    if (playerSightRange && !scared)
    {
      Debug.Log("Marcus is on the hunt\n");
      Chasing();
    }
    else
    {
      if (scared)
        Hiding();
      else
        Patrolling();
    }

  }

  //State Functions
  private void Hiding()
  {
    if (!hideSpotSet)
    {
      safeSpot = SearchHidePoint();
      hideSpotSet = true;
    }
    else
    {
      agent.SetDestination(safeSpot);
    }
    if (Vector3.Magnitude(transform.position - safeSpot) <= 1.0f)
    {
      if (IsVisibleToPlayer(safeSpot, player))
      {
        hideSpotSet = false;
      }
    }
    hidingDur += Time.deltaTime;
    if (hidingDur >= 30.0f)
    {
      hidingDur = 0.0f;
      Debug.Log("Marcus is no longer scared \n");
      scared = false;
    }
  }
  private void Patrolling()
  {
    if (!isGoalSet)
    {
      Debug.Log("Marcus is patrolling, searching for a new destination \n");
      SearchWalkPoint();
    }
    else
    {
      agent.SetDestination(goal);
    }
    if (Vector3.Magnitude(transform.position - goal) <= 2.0f)
      isGoalSet = false;
  }
  private void Chasing()
  {
    agent.SetDestination(player.position);
    if (Vector3.Magnitude(transform.position - player.position) <= 2.0f)
    {

    }
  }
  private void Attacking()
  {

  }
  //Patrol helper to get a new destination
  private void SearchWalkPoint()
  {
    float randomZ = Random.Range(-goalRange, goalRange);
    float randomX = Random.Range(-goalRange, goalRange);

    Vector3 randomPoint = transform.position + new Vector3(randomX, 0, randomZ);
    NavMeshHit hit; // This will store information about the NavMesh query

    // Try to find the nearest point on the NavMesh to this random point
    // Also, make sure to pass the distance to check and the NavMesh layer mask (assuming 'isGround' is your layer mask)
    if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.GetAreaFromName("isGround")))
    {
      goal = hit.position; // If successful, use the hit position as the new goal
      isGoalSet = true;
    }
    else
    {
      isGoalSet = false; // If there's no valid NavMesh point close to the random point, the goal is not set
    }
  }
  private Vector3 SearchHidePoint()
  {
    if (!hideSpotSet)
    {

      Vector3 best;
      List<Vector3> potentialPoints = GetHiddenPoints(transform.position, goalRange);

      // Filter out points that are visible to the player
      List<Vector3> hiddenPoints = new List<Vector3>();
      NavMeshHit hiddenHit;
      foreach (Vector3 point in potentialPoints)
      {
        if (!IsVisibleToPlayer(point, player) && NavMesh.SamplePosition(point, out hiddenHit, 2f, NavMesh.GetAreaFromName("isGround")))
        {
          hiddenPoints.Add(hiddenHit.position);
        }
      }
      best = hiddenPoints[0];
      for (int i = 0; i < hiddenPoints.Count; ++i)
      {
        if ((Vector3.Distance(transform.position, hiddenPoints[i]) < 5.0f) || playerHideRange)
        {
          hiddenPoints.RemoveAt(i);
          --i;
          //if (Vector3.Distance(transform.position, point) < Vector3.Distance(transform.position, closestHide) && Vector3.Distance(point, player.position) > 4.0f)
          //{
          //  closestHide = point;
          //}
        }
        else
        {
          Debug.DrawRay(player.transform.position, hiddenPoints[i] - player.transform.position, Color.cyan, 10.0f);

        }
      }
      best = hiddenPoints[Random.Range(0, hiddenPoints.Count)];
      hideSpotSet = true;
      Debug.DrawRay(player.transform.position, best - player.transform.position, Color.red, 10.0f);
      return hiddenPoints[Random.Range(0, hiddenPoints.Count)];
    }
    return safeSpot;
  }
  private bool IsVisibleToPlayer(Vector3 loc, Transform playerTrans)
  {
    // raycast from the player camera towards the point
    if (Physics.Raycast(playerTrans.position, loc - playerTrans.position, out RaycastHit hit, Mathf.Infinity, ~ignoreLayers))
    {

      // Check if the hit point is the location,
      return Vector3.Distance(hit.point, loc) < 0.05f;
    }
    return false;
  }
  List<Vector3> GetHiddenPoints(Vector3 center, float radius)
  {
    HashSet<Vector3> uniquePoints = new HashSet<Vector3>();
    List<Vector3> tempPoints = new List<Vector3>();

    // Sample the center position               e
    if (NavMesh.SamplePosition(center, out NavMeshHit hit, radius, NavMesh.AllAreas))
    {
      uniquePoints.Add(hit.position);
    }

    // Sample additional points around the center
    for (int i = 0; i < 8; i++)
    {
      Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * radius;
      if (NavMesh.SamplePosition(randomPoint, out hit, radius, NavMesh.AllAreas))
      {
        uniquePoints.Add(hit.position);
      }
    }

    // Create a temporary list to store points for path calculation
    tempPoints.AddRange(uniquePoints);

    // Calculate path between sampled points
    foreach (Vector3 pointA in tempPoints)
    {
      foreach (Vector3 pointB in tempPoints)
      {
        if (pointA != pointB)
        {
          NavMeshPath path = new NavMeshPath();
          NavMesh.CalculatePath(pointA, pointB, NavMesh.AllAreas, path);
          foreach (Vector3 corner in path.corners)
          {
            uniquePoints.Add(corner); // Add path corners to the HashSet
          }
        }
      }
    }

    return new List<Vector3>(uniquePoints);
  }
}
