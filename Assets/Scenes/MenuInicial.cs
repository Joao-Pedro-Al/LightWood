using UnityEngine;
using UnityEngine.SceneManagement; // Permite ao Unity controlar e transitar entre as cenas do jogo

public class MenuInicial : MonoBehaviour
{
    public void JogarJogo()
    {
        // Carrega a cena principal da tua floresta 3D
        SceneManager.LoadScene("Nivel1_teste");
    }

    public void SairDoJogo()
    {
        // Fecha o jogo 
        Debug.Log("O jogador clicou em Sair!");
        Application.Quit();
    }
}
