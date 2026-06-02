using UnityEngine;
using UnityEngine.SceneManagement;

public class BotaoMenu : MonoBehaviour
{
    public string nomeDaCenaMenu = "MenuPrincipal";

    void Start()
    {
        // OBRIGATÓRIO PARA CRÉDITOS: Força o rato a aparecer e a ficar solto
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Garante que o tempo do jogo não ficou pausado da cena anterior
        Time.timeScale = 1f;
    }

    public void VoltarAoMenu()
    {
        Debug.Log("A voltar para o menu...");
        SceneManager.LoadScene("MenuInicial");
    }
}