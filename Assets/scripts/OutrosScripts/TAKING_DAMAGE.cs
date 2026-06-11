using UnityEngine;
using UnityEngine.SceneManagement;

public class TAKING_DAMAGE : MonoBehaviour
{
    private Dialogo DM;
    private PlayerHealth playerHealth;

    public int health = 1;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Start()
    {
        DM = Dialogo.Instance;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("DAMAGE"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        // Mostra efeito visual ANTES de morrer
        if (playerHealth != null)
            playerHealth.TakeDamage(damage * 25f);

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (DM != null) DM.Destruir();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}