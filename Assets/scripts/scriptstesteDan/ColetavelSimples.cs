using UnityEngine;

public class ColetavelSimples : MonoBehaviour
{
    [Header("Configurações do Objeto")]
    public string nomeDoObjeto = "Item Comum";
    
    [Tooltip("Distância máxima que a câmara pode estar para conseguir coletar.")]
    public float distanciaDeColeta = 3.5f;

    [Header("Efeito de Brilho (Hover)")]
    [Tooltip("A cor que o objeto vai emitir quando olhares para ele.")]
    public Color corDoBrilho = new Color(0.3f, 0.3f, 0.3f); // Adiciona um tom claro

    private Transform cameraTransform;
    private Renderer objetoRenderer;
    private Color corOriginal;
    private bool estaOlhandoParaOObjeto = false;

    void Start()
    {
        // Procura a Main Camera
        if (Camera.main != null) cameraTransform = Camera.main.transform;
        else cameraTransform = GameObject.FindGameObjectWithTag("MainCamera")?.transform;

        // Pega o Renderer do objeto para conseguir mexer na cor/brilho
        objetoRenderer = GetComponent<Renderer>();
        if (objetoRenderer == null)
        {
            // Se não estiver no próprio objeto, procura nos filhos (comum em modelos 3D importados)
            objetoRenderer = GetComponentInChildren<Renderer>();
        }

        if (objetoRenderer != null)
        {
            // Guarda a cor original para saber como voltar ao normal depois
            corOriginal = objetoRenderer.material.color;
        }
    }

    void Update()
    {
        // Só permite coletar se estiver perto E se estiver a olhar para o objeto
        if (estaOlhandoParaOObjeto && Input.GetMouseButtonDown(0))
        {
            TentarColetar();
        }
    }

    void TentarColetar()
    {
        if (cameraTransform == null) return;

        float distancia = Vector3.Distance(transform.position, cameraTransform.position);

        if (distancia <= distanciaDeColeta)
        {
            EfetuarColeta();
        }
    }

    void EfetuarColeta()
    {
        Debug.Log($"[COLETA] Coletaste o objeto: {nomeDoObjeto} com o clique esquerdo! (Sem inventário, sem pista).");
        Destroy(gameObject);
    }

    // Chamado automaticamente pelo Unity quando o "rato/mira" passa por cima do Collider
    void OnMouseEnter()
    {
        if (objetoRenderer == null) return;

        estaOlhandoParaOObjeto = true;

        // Ativa o brilho somando a cor do brilho à cor original do material
        objetoRenderer.material.color = corOriginal + corDoBrilho;
    }

    // chama automaticamente pelo Unity quando o rato/mira sai de cima do objeto
    void OnMouseExit()
    {
        if (objetoRenderer == null) return;

        estaOlhandoParaOObjeto = false;

        // Restaura a cor normal do objeto
        objetoRenderer.material.color = corOriginal;
    }
}