using UnityEngine;

public class CliqueItem : MonoBehaviour
{
    [Header("Configurações Únicas da Pista")]
    public Sprite imagemDoItem; 
    public string nomeDaPista = "Nome do Item";
    [TextArea(2, 4)] public string descricaoDaPista = "Descrição que aparece no card.";
    public int numeroFixoDaPista = 1; // 3, 5, 7 ou 8

    private BillboardManager billboard;
    private Renderer[] meusRenderers; 
    private Color[] coresOriginais;    
    private bool jogadorOlhando = false;

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
        jogadorOlhando = true;
        if (meusRenderers != null)
        {
            for (int i = 0; i < meusRenderers.Length; i++)
            {
                if (meusRenderers[i] != null && meusRenderers[i].material != null)
                {
                    meusRenderers[i].material.color = coresOriginais[i] * 1.6f;
                }
            }
        }
    }

    public void AoOlharSair()
    {
        jogadorOlhando = false;
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

    void Update()
    {
        if (jogadorOlhando && Input.GetMouseButtonDown(0))
        {
            if (billboard != null)
            {
                billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);
            }
            
            PlayerInteracao player = FindObjectOfType<PlayerInteracao>();
            if (player != null) player.ForcarResetMira();

            Destroy(gameObject);
        }
    }
}