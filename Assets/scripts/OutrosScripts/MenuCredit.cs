using UnityEngine;
using UnityEngine.SceneManagement; // Permite ao Unity controlar e transitar entre as cenas do jogo

public class MenuCredit : MonoBehaviour
{
    
      public void Credit()
    {
        // Carrega a cena principal da tua floresta 3D
        SceneManager.LoadScene("Creditos");
    }// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
