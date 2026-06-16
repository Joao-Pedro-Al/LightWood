using UnityEngine;
using System.Collections.Generic;
using System.Reflection; 

public class CuboSalvarQuadro : MonoBehaviour
{
    [Header("Referências")]
    public BillboardManager billboardManager;

    [Header("Configuração de Proximidade")]
    public float distanciaAtivacao = 5.0f;

    [Header("Efeito de Brilho (Hover)")]
    public Color corDoBrilho = Color.white;
    [Range(1f, 5f)] public float intensidadeDoBrilho = 3.5f;

    private Transform cameraTransform;
    private Renderer cuboRenderer;
    private bool estaOlhandoParaOCubo = false;
    private bool jogadorPerto = false;
    private bool estaBrilhando = false;

    void Start()
    {
        if (Camera.main != null) cameraTransform = Camera.main.transform;
        else cameraTransform = GameObject.FindGameObjectWithTag("MainCamera")?.transform;

        cuboRenderer = GetComponent<Renderer>();

        Invoke("RecarregarPistasSalvas", 0.05f);
    }

    void Update()
    {
        if (cameraTransform == null || billboardManager == null || cuboRenderer == null) return;

        PistaCard[] pistasAtuais = billboardManager.GetComponentsInChildren<PistaCard>();
        bool existemPistas = (pistasAtuais.Length > 0);

        cuboRenderer.enabled = existemPistas;

        if (!existemPistas)
        {
            if (estaOlhandoParaOCubo) LimparOlhar();
            return;
        }

        float distancia = Vector3.Distance(transform.position, cameraTransform.position);
        jogadorPerto = (distancia <= distanciaAtivacao);

        if (jogadorPerto)
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray, distanciaAtivacao);
            
            bool encontrouEsteCubo = false;
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == this.transform)
                {
                    encontrouEsteCubo = true;
                    break;
                }
            }

            if (encontrouEsteCubo)
            {
                if (!estaOlhandoParaOCubo)
                {
                    estaOlhandoParaOCubo = true;
                    AtivarBrilho();
                }
            }
            else
            {
                if (estaOlhandoParaOCubo) LimparOlhar();
            }
        }
        else
        {
            if (estaOlhandoParaOCubo) LimparOlhar();
        }

        if (estaOlhandoParaOCubo && Input.GetMouseButtonDown(0))
        {
            ClicouSalvarPistas(pistasAtuais);
        }
    }

    void AtivarBrilho()
    {
        if (cuboRenderer == null || !cuboRenderer.enabled || estaBrilhando) return;
        estaBrilhando = true;
        cuboRenderer.material.EnableKeyword("_EMISSION");
        cuboRenderer.material.SetColor("_EmissionColor", corDoBrilho * intensidadeDoBrilho);
    }

    void LimparOlhar()
    {
        estaOlhandoParaOCubo = false;
        estaBrilhando = false;
        if (cuboRenderer != null)
        {
            cuboRenderer.material.SetColor("_EmissionColor", Color.black);
            cuboRenderer.material.DisableKeyword("_EMISSION");
        }
    }

    void ClicouSalvarPistas(PistaCard[] pistasAtuais)
    {
        if (GeradorSalvamento.Instance == null) return;

        bool salvouAlgo = false;
        int nivelAtual = billboardManager.idNivel;

        foreach (PistaCard pista in pistasAtuais)
        {
            // Verificação baseada em Número E Nível para evitar colisão de IDs entre mapas
            bool jaSalva = GeradorSalvamento.Instance.pistasSalvasPermanentes.Exists(p => p.numero == pista.meuNumero && p.idNivel == nivelAtual);
            
            if (!jaSalva)
            {
                GeradorSalvamento.DadosPistaSalva novosDados = new GeradorSalvamento.DadosPistaSalva
                {
                    foto = pista.imgFoto != null ? pista.imgFoto.sprite : null,
                    nome = pista.txtNome != null ? pista.txtNome.text : "",
                    descricao = pista.txtDescricao != null ? pista.txtDescricao.text : "",
                    numero = pista.meuNumero,
                    idNivel = nivelAtual // Atribui o ID do nível correto no struct global
                };
                
                GeradorSalvamento.Instance.pistasSalvasPermanentes.Add(novosDados);
                salvouAlgo = true;
            }
        }

        if (salvouAlgo)
        {
            Debug.Log("[SALVAMENTO] Pistas do Nível " + nivelAtual + " salvas com sucesso!");
            StartCoroutine(FeedbackVisualSalvar());
        }
    }

    void RecarregarPistasSalvas()
    {
        if (GeradorSalvamento.Instance == null || billboardManager == null) return;

        PistaCard[] clonesIniciais = billboardManager.GetComponentsInChildren<PistaCard>();
        foreach (PistaCard clone in clonesIniciais)
        {
            Destroy(clone.gameObject);
        }

        FieldInfo campoLista = typeof(BillboardManager).GetField("cartoesInstanciados", BindingFlags.NonPublic | BindingFlags.Instance);
        if (campoLista != null)
        {
            var listaOriginal = campoLista.GetValue(billboardManager) as List<PistaCard>;
            if (listaOriginal != null)
            {
                listaOriginal.Clear();
            }
        }

        // Filtra para só carregar neste quadro as pistas que pertencem ao ID deste nível
        foreach (var dados in GeradorSalvamento.Instance.pistasSalvasPermanentes)
        {
            if (dados.idNivel == billboardManager.idNivel)
            {
                billboardManager.AdicionarPistaAoQuadro(dados.foto, dados.nome, dados.descricao, dados.numero);
            }
        }
    }

    System.Collections.IEnumerator FeedbackVisualSalvar()
    {
        if (cuboRenderer != null)
        {
            cuboRenderer.material.SetColor("_EmissionColor", Color.green * 6f);
            yield return new WaitForSeconds(0.4f);
            if (estaOlhandoParaOCubo)
                cuboRenderer.material.SetColor("_EmissionColor", corDoBrilho * intensidadeDoBrilho);
            else
                LimparOlhar();
        }
    }
}