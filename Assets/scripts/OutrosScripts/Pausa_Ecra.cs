using UnityEngine;
using UnityEngine.SceneManagement;

public class Pausa_Ecra : MonoBehaviour
{
    [Header("Overlay")]
    [SerializeField] private GameObject Overlay;

    [Header("Valores Externos")]
    public Player_Teste_Alves ScriptPlayer;
    public Dialogo ScriptDialogo;

    [Header("Valores Locais")]
    private bool Pausado = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ScriptPlayer != null)
            {
                if (!Pausado)
                {
                    Pausado = true;

                    // Limpa os eixos do rato mesmo antes de travar para evitar o "salto" da visão
                    Input.ResetInputAxes(); 
                    ScriptPlayer.cameraTravada = true;
                    
                    Time.timeScale = 0f; // Congela o tempo do mundo

                    Overlay.SetActive(true);
                }
                // else
                // {
                //     Resumir();
                // }
            }
        }
    }

    public void Resumir()
    {
        Pausado = false;

        ScriptPlayer.cameraTravada = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Time.timeScale = 1f; // Descongela o mundo

        Overlay.SetActive(false);
    }

    public void Sair()
    {
        ScriptDialogo.Destruir(); // Destruir o GameObject com o Diálogo
        Time.timeScale = 1f; // Descongela o mundo
        SceneManager.LoadScene("MenuInicial");
    }
}