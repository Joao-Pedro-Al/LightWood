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
                    // PROTEÇÃO: Guarda a cor apenas se o material suportar a propriedade padrão
                    if (meusRenderers[i].material.HasProperty("_Color"))
                    {
                        coresOriginais[i] = meusRenderers[i].material.color;
                    }
                    else
                    {
                        coresOriginais[i] = Color.white; // Valor de segurança
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
                // PROTEÇÃO: Só tenta dar brilho se o material contiver a propriedade '_Color'
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
                // PROTEÇÃO: Só devolve a cor original se o material aceitar a propriedade
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