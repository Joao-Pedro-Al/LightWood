using UnityEngine;

public class CliqueItem : MonoBehaviour
{
    [Header("Configurações da Pista")]
    public Sprite imagemDoItem; 
    public string nomeDaPista = "Nome do Item";
    [TextArea(2, 4)] public string descricaoDaPista = "Descrição no card.";
    public int numeroFixoDaPista = 1; 

    [Header("Tipo de Pista")]
    [Tooltip("Se ativares isto, o objeto NÃO desaparece do chão ao clicar, mas vai para o quadro na mesma!")]
    public bool naoColetavel = false; 

    [Header("Mecânica de Bateria")]
    [Tooltip("Ativa esta caixinha se este objeto for uma Bateria para recarregar a Lanterna!")]
    public bool eBateria = false;
    [Tooltip("Quantidade de carga que esta bateria vai dar à lanterna (ex: 50, 100).")]
    public float quantidadeCarga = 50f;

    private BillboardManager billboard;
    private Renderer[] meusRenderers; 
    private Color[] coresOriginais;    
    private bool jaEstaBrilhando = false;
    private bool jaFoiRegistado = false; 

    void Start()
    {
        billboard = FindObjectOfType<BillboardManager>();
        meusRenderers = GetComponentsInChildren<Renderer>();
        
        if (meusRenderers != null && meusRenderers.Length > 0)
        {
            coresOriginais = new Color[meusRenderers.Length];
            for (int i = 0; i < meusRenderers.Length; i++)
            {
                if (meusRenderers[i] != null && meusRenderers[i].material != null)
                {
                    if (meusRenderers[i].material.HasProperty("_Color"))
                    {
                        coresOriginais[i] = meusRenderers[i].material.color;
                    }
                    else
                    {
                        coresOriginais[i] = Color.white; 
                    }
                }
            }
        }
    }

    public void AoOlharEntrar()
    {
        if (jaFoiRegistado || jaEstaBrilhando) return;
        jaEstaBrilhando = true;

        if (meusRenderers != null)
        {
            foreach (Renderer r in meusRenderers)
            {
                if (r != null && r.material != null && r.material.HasProperty("_Color"))
                {
                    r.material.color = Color.white * 1.5f;
                }
            }
        }
    }

    public void AoOlharSair()
    {
        if (jaFoiRegistado || !jaEstaBrilhando) return;
        jaEstaBrilhando = false;

        if (meusRenderers != null)
        {
            for (int i = 0; i < meusRenderers.Length; i++)
            {
                if (meusRenderers[i] != null && meusRenderers[i].material != null && meusRenderers[i].material.HasProperty("_Color"))
                {
                    meusRenderers[i].material.color = coresOriginais[i];
                }
            }
        }
    }

    public void ColetarPista()
    {
        if (jaFoiRegistado) return;

        // SE FOR UMA BATERIA:
        if (eBateria)
        {
            // Encontra a lanterna no teu Player e dá-lhe a carga
            Flashlight lanterna = FindObjectOfType<Flashlight>();
            if (lanterna != null)
            {
                lanterna.Recharge(quantidadeCarga);
            }
            
            // Remove o brilho da mira, pula a parte de enviar para o quadro, e some com ela do chão
            AoOlharSair();
            Destroy(gameObject);
            return; // Interrompe o código aqui para não fazer mais nada
        }

        // SE FOR UMA PISTA NORMAL (Vai para o quadro):
        if (billboard != null)
        {
            billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);
        }

        if (naoColetavel)
        {
            jaFoiRegistado = true;
            AoOlharSair(); 
            this.enabled = false; 
        }
        else
        {
            AoOlharSair();
            Destroy(gameObject);
        }
    }
}