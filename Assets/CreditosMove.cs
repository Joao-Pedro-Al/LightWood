using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditosMove : MonoBehaviour
{
    [Header("Configurações de Velocidade")]
    [SerializeField] private float velocidade = 50f; // Velocidade com que o texto sobe
    [SerializeField] private float posicaoFinalY = 1200f; // Ponto no topo onde o texto para e sai

    [Header("Cena de Retorno")]
    [SerializeField] private string nomeMenuPrincipal = "SampleScene"; 

    [Header("Tempo no Ecrã Preto")]
    [SerializeField] private float tempoEsperaPreto = 3f; // Quantos segundos fica preto no fim

    private RectTransform rectTransform;
    private bool creditosTerminaram = false; // Garantir que a espera só ativa uma vez

    void Start()
    {
        // Pega no componente de posição do texto UI
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Se já terminaram e estamos a aguardar no preto, não mexe mais no texto
        if (creditosTerminaram) return;

        // Faz o texto subir verticalmente todos os frames
        rectTransform.anchoredPosition += new Vector2(0, velocidade * Time.deltaTime);

        // Se o texto passar da posição final, inicia a contagem do ecrã preto
        if (rectTransform.anchoredPosition.y >= posicaoFinalY)
        {
            StartCoroutine(EfeitoEcraPreto());
        }

        // Se o jogador carregar no "Esc" ou no "Espaço", pula os créditos imediatamente
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            VoltarParaMenu();
        }
    }

    // Coroutine para esperar com o ecrã preto
    private IEnumerator EfeitoEcraPreto()
    {
        creditosTerminaram = true;

        // Desativa o texto  para garantir que o ecrã fica todo preto
        gameObject.SetActive(false);

        // Espera os segundos configurados 
        yield return new WaitForSeconds(tempoEsperaPreto);

        //  volta para o menu
        VoltarParaMenu();
    }

    public void VoltarParaMenu()
    {
        SceneManager.LoadScene(nomeMenuPrincipal);
    }
}