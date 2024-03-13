using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : MonoBehaviour
{
  //Navigation params
  public NavMeshAgent agent;
  public Transform player;
  public LayerMask isGround, isPlayer;

  //Patrolling params
  public Vector3 goal;
  public bool isGoalSet;
  public float goalRange;

  //Offensive params
  public float attackCooldown;
  public bool isReady;

  //State params
  public float sightRange, attackRange;
  public bool playerSightRange, playerAttackRange;

  private void Awake()
  {
    agent = GetComponent<NavMeshAgent>();
    //Set target to nearest player if multiple

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
    if (!player)
    {
      Patrolling();
    }
    //Check if player is within sight or attack range
    playerSightRange = Physics.CheckSphere(transform.position, sightRange, isPlayer);
    playerAttackRange = Physics.CheckSphere(transform.position, attackRange, isPlayer);

    //Simple state machine based on available actions
    Patrolling();

  }

  //State Functions
  private void Patrolling()
  {
    if (!isGoalSet)
      SearchWalkPoint();
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
}
