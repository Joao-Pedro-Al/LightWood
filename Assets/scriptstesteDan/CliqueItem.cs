using UnityEngine;

public class CliqueItem : MonoBehaviour
{
    public Sprite imagemNoInventario; 

    private GeradorInventario inventario;
    private Renderer[] meusRenderers; 
    private Color[] coresOriginais;    
    private bool jogadorOlhando = false;

    void Start()
    {
        inventario = FindObjectOfType<GeradorInventario>();
        
        // Pega todos os renderers dos filhos para fazer o brilho
        meusRenderers = GetComponentsInChildren<Renderer>();
        
        if (meusRenderers != null && meusRenderers.Length > 0)
        {
            coresOriginais = new Color[meusRenderers.Length];
            for (int i = 0; i < meusRenderers.Length; i++)
            {
                // Evita erros se houver materiais vazios
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
            if (inventario != null)
            {
                inventario.AdicionarAoInventario(imagemNoInventario);
            }
            
            PlayerInteracao player = FindObjectOfType<PlayerInteracao>();
            if (player != null) player.ForcarResetMira();

            Destroy(gameObject);
        }
    }
}