using UnityEngine;

public class NPCStateMachine : MonoBehaviour
{
    public enum State { Idle, Patrolling, Chasing, Attacking }
    public State currentState = State.Idle;

    public Transform player;
    public Transform[] patrolPoints;
    public float chaseRange = 5f;
    public float attackRange = 1.2f;
    public float speed = 2.5f;
    public float attackCooldown = 1.5f;
    private int currentPointIndex = 0;
    private float lastAttackTime = -10f;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Patrolling:
                Patrol();
                break;
            case State.Chasing:
                Chase();
                break;
            case State.Attacking:
                Attack();
                break;
        }
    }

    void Idle()
    {
        // NPC stays in idle state until player enters the chase range
        if (player != null && Vector2.Distance(transform.position, player.position) < chaseRange)
        {
            currentState = State.Chasing;
        }
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // NPC moves between patrol points
        Transform targetPoint = patrolPoints[currentPointIndex];
        if (targetPoint != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
            {
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            }
        }

        if (player != null && Vector2.Distance(transform.position, player.position) < chaseRange)
        {
            currentState = State.Chasing;
        }
    }

    void Chase()
    {
        if (player == null) return;

        // NPC chases the player when within range
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            currentState = State.Attacking;
        }
        else if (Vector2.Distance(transform.position, player.position) > chaseRange * 1.25f)
        {
            currentState = (patrolPoints != null && patrolPoints.Length > 0) ? State.Patrolling : State.Idle;
        }
    }

    void Attack()
    {
        // NPC attacks the player
        if (Time.time > lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log("NPC attacks the player!");
        }

        if (player != null && Vector2.Distance(transform.position, player.position) > attackRange)
        {
            currentState = State.Chasing;
        }
    }
}
