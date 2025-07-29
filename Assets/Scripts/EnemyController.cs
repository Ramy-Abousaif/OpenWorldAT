using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;

    public float attackRange = 3.0f;
    public float viewAngle = 45.0f;
    public float rayDistance = 5.0f;
    public float stoppingDistance = 5.0f;   
    public float speed = 2.0f;
    public float aggroRange = 5.0f;
    public int health = 3;

    private Vector3 testPosition;
    private Vector3 _destination;
    private Quaternion _desiredRotation;
    private Vector3 _direction;
    private GameObject player;
    public GameObject eye;
    private Rigidbody rb;
    private enemyState _currentState;
    private GenerateMap map;

    public CameraShake cameraShake;

    public CapsuleCollider boxC;

    private void Start()
    {
        testPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        map = GameObject.Find("Map").GetComponent<GenerateMap>();
        cameraShake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        switch (_currentState)
        {
            case enemyState.Wander:
                {
                    if (NeedsDestination())
                    {
                        GetDestination();
                    }
                    Debug.DrawRay(testPosition, Vector3.down * 10000000000000);
                    Debug.DrawRay(testPosition, Vector3.up * 10000000000000);
                    transform.rotation = Quaternion.Slerp(transform.rotation, _desiredRotation, Time.deltaTime * 10.0f);
                    transform.Translate((Vector3.forward * Time.deltaTime * speed));

                    var rayColor = IsPathBlocked() ? Color.red : Color.green;
                    Debug.DrawRay(eye.transform.position, _direction * rayDistance, rayColor);
                    RaycastHit hit;
                    while (IsPathBlocked() || (!(Physics.Raycast(testPosition, Vector3.down * 10000000000000, out hit, 10000000000000) ||
                        Physics.Raycast(testPosition, -Vector3.down * 10000000000000, out hit, 10000000000000))))
                    {
                        GetDestination();
                    }

                    var targetToAggro = CheckForAggro();
                    if (targetToAggro != null)
                    {
                        if(!(player.GetComponent<PlayerMovement>().health <= 0))
                        {
                            _currentState = enemyState.Chase;
                        }
                    }
                    break;
                }
            case enemyState.Chase:
                {
                    transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));

                    if (Vector3.Distance(new Vector3(transform.position.x, transform.position.y + boxC.center.y, transform.position.z), player.transform.position) < attackRange)
                    {
                        _currentState = enemyState.Attack;
                    }
                    else
                    {
                        transform.Translate((Vector3.forward * Time.deltaTime * speed));
                    }
                    break;
                }
            case enemyState.Attack:
                {
                    StartCoroutine(Attack());

                    _currentState = enemyState.Wander;
                    break;
                }
        }
        if (Vector3.Distance(transform.position, player.transform.position) > map.distanceToActive - 20)
            Destroy(gameObject);
    }

    IEnumerator Attack()
    {
        StartCoroutine(cameraShake.Shake(0.15f, 0.4f));
        player.GetComponent<PlayerMovement>().health -= 1;
        rb.AddForce(Vector3.Normalize(new Vector3(transform.position.x, transform.position.y + boxC.center.y, transform.position.z) - player.transform.position) * 1000.0f);
        player.GetComponent<Rigidbody>().velocity = new Vector3(Vector3.Normalize(player.transform.position - new Vector3(transform.position.x, transform.position.y + boxC.center.y, transform.position.z)).x * 0.5f, 0.1f, Vector3.Normalize(player.transform.position - new Vector3(transform.position.x, transform.position.y + boxC.center.y, transform.position.z)).z * 0.5f);
        yield return null;
    }

    private bool IsPathBlocked()
    {
        Ray ray = new Ray(eye.transform.position, _direction);
        var hitSomething = Physics.RaycastAll(ray, rayDistance, _layerMask);
        return hitSomething.Any();
    }

    private void GetDestination()
    {
        testPosition = (transform.position + (transform.forward * 4.0f)) +
        new Vector3(Random.Range(-4.5f, 4.5f), 0.0f, Random.Range(-4.5f, 4.5f));
        _destination = new Vector3(testPosition.x, transform.position.y, testPosition.z);
        _direction = Vector3.Normalize(_destination - transform.position);
        _direction = new Vector3(_direction.x, 0.0f, _direction.z);
        _desiredRotation = Quaternion.LookRotation(_direction);
    }

    private bool NeedsDestination()
    {
        if (_destination == Vector3.zero)
        {
            return true;
        }
        var distance = Vector3.Distance(transform.position, _destination);
        if (distance <= stoppingDistance)
        {
            return true;
        }

        return false;
    }

    Quaternion startingAngle = Quaternion.AngleAxis(120, Vector3.up);
    Quaternion stepAngle = Quaternion.AngleAxis(5, Vector3.up);

    private Transform CheckForAggro()
    {
        if(Vector3.Distance(new Vector3(transform.position.x, transform.position.y + boxC.center.y, transform.position.z), player.transform.position) <= aggroRange)
        {
            if(Vector3.Angle(transform.forward, (player.transform.position - new Vector3(transform.position.x, transform.position.y + boxC.center.y, transform.position.z))) <= viewAngle)
            {
                return player.transform;
            }
        }
        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Weapon"))
        {
            rb.AddForce(Vector3.Normalize(new Vector3(transform.position.x, transform.position.y + boxC.center.y, transform.position.z) - player.transform.position) * 800.0f);
            StartCoroutine(Hit());
        }
    }

    IEnumerator Hit()
    {
        StartCoroutine(cameraShake.Shake(0.15f, 0.4f));
        _currentState = enemyState.Chase;
        yield return new WaitForSeconds(0.2f);
        if (health == 1)
        {
            Destroy(gameObject);
            Instantiate(Resources.Load("Prefabs/Smoke"), transform.position + boxC.center, Quaternion.identity);
            if (player.transform.GetComponent<Status>().quest.isActive)
            {
                player.transform.GetComponent<Status>().quest.goal.CurrentAmt++;
            }
        }
        else
        {
            health -= 1;
            Instantiate(Resources.Load("Prefabs/Blood"), transform.position + boxC.center, Quaternion.identity);
        }
    }
}

public enum enemyState
{
    Wander,
    Chase,
    Attack
}
