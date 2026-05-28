using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PistaCard : MonoBehaviour
{
    [Header("Componentes de UI Internos (Do Teu Design)")]
    [Tooltip("Arrasta o componente TextMeshPro do Número (#) para aqui.")]
    public TextMeshProUGUI textoNumero;
    
    [Tooltip("Arrasta o componente TextMeshPro do Nome para aqui.")]
    public TextMeshProUGUI textoNome;
    
    [Tooltip("Arrasta o componente TextMeshPro da Descrição para aqui.")]
    public TextMeshProUGUI textoDescricao;
    
    [Tooltip("Arrasta o componente Image onde a foto do objeto vai aparecer.")]
    public Image imagemFotoItem;

    [HideInInspector]
    public int meuNumero; // Guarda o ID fixo da pista (Ex: 7 ou 8) para o sistema de fios

    private BillboardManager gerenciadorQuadro;
    private Button botaoInteracao;

    /// <summary>
    /// Esta função é chamada automaticamente pelo BillboardManager assim que o cartão nasce no quadro.
    /// Ela preenche o teu design com as informações reais do item coletado.
    /// </summary>
    public void Setup(Sprite foto, string nome, string descricao, int numero, BillboardManager manager)
    {
        gerenciadorQuadro = manager;
        meuNumero = numero;

        // Preenche os textos do teu design se os componentes estiverem arrastados
        if (textoNumero != null) textoNumero.text = "#" + numero.ToString();
        if (textoNome != null) textoNome.text = nome;
        if (textoDescricao != null) textoDescricao.text = descricao;

        // Aplica a foto real do objeto (Pedra, Telemóvel, Pulseira, etc.)
        if (imagemFotoItem != null)
        {
            if (foto != null)
            {
                imagemFotoItem.sprite = foto;
                imagemFotoItem.enabled = true;
            }
            else
            {
                // Se não houver foto, esconde o componente para não ficar um quadrado branco feio
                imagemFotoItem.enabled = false; 
            }
        }

        // Configura o botão dinamicamente para detetar cliques no quadro
        ConfigurarBotaoClique();
    }

    private void ConfigurarBotaoClique()
    {
        // Tenta encontrar um componente Button anexado a este cartão
        botaoInteracao = GetComponent<Button>();

        // Se não existir, adiciona um automaticamente para não precisares de o criar à mão
        if (botaoInteracao == null)
        {
            botaoInteracao = gameObject.AddComponent<Button>();
        }

        // Limpa cliques antigos por segurança e adiciona a função de seleção
        botaoInteracao.onClick.RemoveAllListeners();
        botaoInteracao.onClick.AddListener(ClicouNoCartao);
    }

    private void ClicouNoCartao()
    {
        if (gerenciadorQuadro != null)
        {
            // Avisa o cérebro do quadro que este cartão foi o selecionado para a combinação
            gerenciadorQuadro.SelecionarPista(this);
            
            // Pequeno feedback visual no console para saberes que o clique funcionou
            Debug.Log($"Pista selecionada no quadro: {textoNome.text} (ID: {meuNumero})");
        }
    }
}