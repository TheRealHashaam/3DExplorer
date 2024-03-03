using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAi : MonoBehaviour
{
    private Animator animator;
    private Transform player;
    private float attackRange = 2.0f;
    private bool isAttacking = false;
    private bool isRetreating = false;
    private bool isHurt = false;
    private bool isDead = false;

    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    public float retreatDuration = 3.0f;
    public float retreatSpeed = -2.0f;
    public float howlDuration = 2.20f;
    public float health;
    public float MaxHealth;
    public AudioSource Howl;
    public Image Health;
    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(EnemyAction());
        health = MaxHealth;
    }

    void UpdateHealthBar()
    {
        // Calculate the fill amount based on the current health and max health
        float fillAmount = health / MaxHealth;

        // Set the fill amount for the health bar
        Health.fillAmount = fillAmount;
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isRetreating && distanceToPlayer < attackRange && !isAttacking)
        {
            StartCoroutine(Attack());
        }
        else if (!isHurt)
        {
            MoveTowardsPlayer(distanceToPlayer);
        }
    }

    void MoveTowardsPlayer(float distanceToPlayer)
    {
        if (!isRetreating)
        {
            if (distanceToPlayer > attackRange)
            {
                float speed = distanceToPlayer > runSpeed ? runSpeed : walkSpeed;

                animator.SetFloat("Speed", speed);

                Vector3 direction = (player.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

                transform.Translate(0, 0, speed * Time.deltaTime);
            }
            else
            {
                animator.SetFloat("Speed", 0f);
            }
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        int attackAnimationIndex = Random.Range(1, 3);
        animator.SetTrigger("Attack" + attackAnimationIndex);

        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);

        isAttacking = false;

        int r = Random.Range(0, 2);
        if(r==0)
        {
            StartCoroutine(Attack());
        }
        else
        {
            StartCoroutine(Retreat());
        }
    }

    IEnumerator Retreat()
    {
        Debug.Log("Retreat");
        isRetreating = true;
        animator.SetFloat("Speed", -walkSpeed); // Set negative speed for retreat
        //animator.applyRootMotion = true;
        //yield return new WaitForSeconds(retreatDuration);
        float retreatTimer = 0f;
        while (retreatTimer < retreatDuration)
        {
            animator.SetFloat("Speed", 5f);
            transform.Translate(0, 0, retreatSpeed * Time.deltaTime);
            retreatTimer += Time.deltaTime;
            yield return null;
        }
        if(Howl)
        {
            int r = Random.Range(0, 2);
            if(r ==1)
            {
                animator.SetTrigger("Howl");
                Howl.Play();
                yield return new WaitForSeconds(howlDuration);

            }
        }
        // Resume normal behavior
        animator.SetFloat("Speed", 0f);
        isRetreating = false;
    }

    IEnumerator EnemyAction()
    {
        while (true)
        {
            if (!isHurt && !isDead && !isRetreating)
            {
                animator.SetFloat("Speed", 0f);
                yield return new WaitForSeconds(Random.Range(2, 5));

                float randomAction = Random.Range(0f, 1f);
                if (randomAction < 0.5f)
                {
                    animator.SetFloat("Speed", walkSpeed);
                }
                else
                {
                    animator.SetFloat("Speed", runSpeed);
                }

                yield return new WaitForSeconds(Random.Range(2, 5));
            }
            else
            {
                yield return null;
            }
        }
    }
    public void TakeDamage()
    {
        if (isDead) return;

        health -= 30;
        UpdateHealthBar();
        // Check if the enemy's health is depleted
        if (health <= 0)
        {
            Die();
        }
        else
        {
            // Trigger the hurt animation
            isHurt = true;
            animator.SetTrigger("Hurt");

            StartCoroutine(RecoverFromHurt());
        }
    }

    IEnumerator RecoverFromHurt()
    {
        yield return new WaitForSeconds(1.0f);

        isHurt = false;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("Die");
        Destroy(Health.GetComponentInParent<Canvas>());
        Destroy(this);
    }
}
