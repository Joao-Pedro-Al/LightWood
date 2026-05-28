using UnityEngine;

public class CliqueItem : MonoBehaviour
{
    [Header("Configurações da Pista")]
    public Sprite imagemDoItem; 
    public string nomeDaPista = "Nome do Item";
    [TextArea(2, 4)] public string descricaoDaPista = "Descrição no card.";
    public int numeroFixoDaPista = 1; 

    private BillboardManager billboard;
    private Renderer[] meusRenderers; 
    private Color[] coresOriginais;    
    private bool jaEstaBrilhando = false;

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
                    coresOriginais[i] = meusRenderers[i].material.color;
                }
            }
        }
    }

    public void AoOlharEntrar()
    {
        if (jaEstaBrilhando) return;
        jaEstaBrilhando = true;

        if (meusRenderers != null)
        {
            foreach (Renderer r in meusRenderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.color = Color.white * 1.5f;
                }
            }
        }
    }

    public void AoOlharSair()
    {
        if (!jaEstaBrilhando) return;
        jaEstaBrilhando = false;

        if (meusRenderers != null)
        {
            for (int i = 0; i < meusRenderers.Length; i++)
            {
                if (meusRenderers[i] != null && meusRenderers[i].material != null)
                {
                    meusRenderers[i].material.color = coresOriginais[i];
                }
            }
        }
    }

    public void ColetarPista()
    {
        if (billboard != null)
        {
            // O item é adicionado ao quadro silenciosamente em segundo plano
            billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);
        }
        AoOlharSair();
        Destroy(gameObject);
    }
}