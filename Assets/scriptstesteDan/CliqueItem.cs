using UnityEngine;

public class CliqueItem : MonoBehaviour
{
    private GeradorInventario inventario;
    private Renderer meuRenderer;
    private Color corOriginal;
    private bool jogadorOlhando = false;

    void Start()
    {
        inventario = FindObjectOfType<GeradorInventario>();
        meuRenderer = GetComponent<Renderer>();
        
        if (meuRenderer != null)
        {
            corOriginal = meuRenderer.material.color;
        }
    }

    public void AoOlharEntrar()
    {
        jogadorOlhando = true;
        if (meuRenderer != null)
        {
            meuRenderer.material.color = corOriginal * 1.6f; 
        }
    }

    public void AoOlharSair()
    {
        jogadorOlhando = false;
        if (meuRenderer != null)
        {
            meuRenderer.material.color = corOriginal;
        }
    }

    void Update()
    {
        if (jogadorOlhando && Input.GetMouseButtonDown(0))
        {
            if (inventario != null)
            {
                inventario.AdicionarAoInventario(gameObject.name);
            }
            
            PlayerInteracao player = FindObjectOfType<PlayerInteracao>();
            if (player != null) player.ForcarResetMira();

            Destroy(gameObject);
        }
    }
}