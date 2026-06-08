
using UnityEngine;
using UnityEngine.SceneManagement;

public class TAKING_DAMAGE : MonoBehaviour
{
    private Dialogo DM; // Instanciar o Diálogo

    public int health = 1;

    void Start()
    {
        // Acede ao Sistema de Diálogo
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
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        DM.Destruir(); // Destruir o GameObject com o Diálogo
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
