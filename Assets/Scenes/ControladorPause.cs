using UnityEngine;

public class ControladorPause : MonoBehaviour
{
    public GameObject botaoPause;       // Botao_Pause da imagem
    public GameObject botaoContinuar;   // Botao_Continuar da imagem

    void Start()
    {
        // Quando o jogo começa: o botão de Pause fica visível e o de Continuar fica escondido
        if (botaoPause != null) botaoPause.SetActive(true);
        if (botaoContinuar != null) botaoContinuar.SetActive(false);
    }

    public void PausarJogo()
    {
        botaoPause.SetActive(false);       // Esconde o botão Pause
        botaoContinuar.SetActive(true);    // Mostra o botão Continuar
        Time.timeScale = 0f;               // Congela o jogo
    }

    public void ContinuarJogo()
    {
        botaoPause.SetActive(true);        // Reaparece o botão Pause
        botaoContinuar.SetActive(false);   // Esconde o botão Continuar
        Time.timeScale = 1f;               // O jogo volta ao normal
    }
}