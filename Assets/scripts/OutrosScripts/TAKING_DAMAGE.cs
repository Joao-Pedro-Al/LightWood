using UnityEngine;
using UnityEngine.SceneManagement;

public class TAKING_DAMAGE : MonoBehaviour
{
    [Header("Dialogo")]
    private Dialogo DM;

    [Header("Objetivos")]
    private Pausa_Ecra PE;

    private PlayerHealth playerHealth;

    public int health = 1;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Start()
    {
        DM = Dialogo.Instance;
        PE = Pausa_Ecra.Instance;
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
        if (DM != null)
        {DM.Destruir();}
        if (playerHealth != null)
        {PE.Destruir_MenuPausa();}
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}