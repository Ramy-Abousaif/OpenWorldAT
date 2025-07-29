using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    public Rigidbody rb;
    public LayerMask whatIsEnemies;

    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity;

    public int damage;
    public float explosionRange;
    public float explosionForce;
    public float playerKnockBackMulti;

    public int maxCollisions;
    public float maxLifeTime;
    public bool destroyOnTouch = true;

    int collisions;
    public bool collisionEffect = false;
    PhysicMaterial physics_mat;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        if (collisions > maxCollisions) Explode();

        maxLifeTime -= Time.deltaTime;
        if (maxLifeTime <= 0) Explode();
    }

    private void Explode()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);

        for (int i = 0; i < enemies.Length; i++)
        {
            //Get component of enemy and call take damage

            if (enemies[i].GetComponent<Rigidbody>() != null)
            {
                if (!enemies[i].CompareTag("Player"))
                    enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange);
                else
                    enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce * playerKnockBackMulti, transform.position, explosionRange);
            }


        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Bullet")) return;

        collisions++;

        if(collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("Squad") || collision.collider.CompareTag("Player"))
            Instantiate(Resources.Load("Prefabs/Blood"), transform.position, Quaternion.identity);
        else
            Instantiate(Resources.Load("Prefabs/Sparks"), transform.position, Quaternion.identity);

        if (collision.collider.CompareTag("Enemy") && destroyOnTouch) Explode();
    }

    private void Setup()
    {
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;

        GetComponent<SphereCollider>().material = physics_mat;

        rb.useGravity = useGravity;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
