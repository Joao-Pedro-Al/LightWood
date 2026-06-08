using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditosMove : MonoBehaviour
{
    [Header("Configurações de Velocidade")]
    [SerializeField] private float velocidade = 50f; // Velocidade com que o texto sobe
    [SerializeField] private float posicaoFinalY = 1200f; // Ponto no topo onde o texto para e sai

    [Header("Cena de Retorno")]
    [SerializeField] private string nomeMenuPrincipal = "SampleScene"; // Nome exato da cena do teu menu

    private RectTransform rectTransform;

    void Start()
    {
        // Pega no componente de posição do texto UI
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Faz o texto subir verticalmente todos os frames
        rectTransform.anchoredPosition += new Vector2(0, velocidade * Time.deltaTime);

        // Se o texto passar da posição final, volta automaticamente para o menu
        if (rectTransform.anchoredPosition.y >= posicaoFinalY)
        {
            VoltarParaMenu();
        }

        // Se o jogador carregar no "Esc" ou no "Espaço", também pula os créditos e volta para o menu
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            VoltarParaMenu();
        }
    }

    public void VoltarParaMenu()
    {
        SceneManager.LoadScene(nomeMenuPrincipal);
    }
}