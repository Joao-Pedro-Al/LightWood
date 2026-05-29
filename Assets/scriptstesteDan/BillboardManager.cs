using UnityEngine;
using System.Collections.Generic;

public class BillboardManager : MonoBehaviour
{
    [Header("Diálogo")]
    private Dialogo DM; 

    [Header("Configurações do Quadro")]
    public GameObject prefabCard;     
    public GameObject painelFundoPreto; 

    [Header("Configurações do Caos")]
    public float limitesX = 400f;
    public float limitesY = 200f;

    [Header("Customização do Fio Vermelho")]
    [Range(0.005f, 0.1f)] public float espessuraDoFio = 0.015f; 
    public Color corDoFio = new Color(0.75f, 0.05f, 0.05f);     

    private Transform gridDoQuadro;   
    private List<PistaCard> cartoesInstanciados = new List<PistaCard>();
    private Material materialFioNativo;

    // Estrutura para guardar e atualizar as conexões ativas
    private struct ConexaoFio
    {
        public LineRenderer lineRenderer;
        public GameObject objetoA;
        public GameObject objetoB;
    }
    private List<ConexaoFio> conexoesAtivas = new List<ConexaoFio>();

    private bool ligou123 = false;
    private bool ligou56 = false;
    private bool ligou78 = false;

    void Awake()
    {
        Canvas canvasInterno = GetComponentInChildren<Canvas>();
        if (canvasInterno != null) gridDoQuadro = canvasInterno.transform;
        if (painelFundoPreto) painelFundoPreto.SetActive(false);

        GerarMaterialDoFio();
    }

    void Start()
    {
        DM = Dialogo.Instance;
    }

    void Update()
    {
        // ATUALIZAÇÃO EM TEMPO REAL: Se moveres os cards, os fios acompanham!
        AtualizarPosicaoDosFios();
    }

    void GerarMaterialDoFio()
    {
        Shader shaderAlvo = Shader.Find("Sprites/Default");
        if (shaderAlvo == null) shaderAlvo = Shader.Find("Unlit/Color");
        
        materialFioNativo = new Material(shaderAlvo);
        materialFioNativo.color = corDoFio;
    }

    public void AbrirQuadro() { if (painelFundoPreto) painelFundoPreto.SetActive(true); Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    public void FecharQuadro() { if (painelFundoPreto) painelFundoPreto.SetActive(false); Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

    public void AdicionarPistaAoQuadro(Sprite foto, string nome, string descricao, int numero)
    {
        if (prefabCard == null || gridDoQuadro == null) return;

        GameObject novoCard = Instantiate(prefabCard, gridDoQuadro);
        RectTransform rt = novoCard.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            float aleatorioX = Random.Range(-limitesX, limitesX);
            float aleatorioY = Random.Range(-limitesY, limitesY);
            rt.localPosition = new Vector3(aleatorioX, aleatorioY, 0f);

            float inclinacaoAleatoria = Random.Range(-12f, 12f);
            rt.localRotation = Quaternion.Euler(0f, 0f, inclinacaoAleatoria);
        }
        
        PistaCard scriptCard = novoCard.GetComponent<PistaCard>();
        if (scriptCard != null)
        {
            scriptCard.Setup(foto, nome, descricao, numero, this);
            cartoesInstanciados.Add(scriptCard);
        }

        VerificarCombinacoesAutomaticas();
    }

    void VerificarCombinacoesAutomaticas()
    {
        Dictionary<int, PistaCard> mapaPistas = new Dictionary<int, PistaCard>();
        foreach (PistaCard card in cartoesInstanciados)
        {
            if (card != null && !mapaPistas.ContainsKey(card.meuNumero))
            {
                mapaPistas.Add(card.meuNumero, card);
            }
        }

        // Interligar 1 + 2 + 3
        if (!ligou123 && mapaPistas.ContainsKey(1) && mapaPistas.ContainsKey(2) && mapaPistas.ContainsKey(3))
        {
            ligou123 = true;
            CriarLinhaFio3D(mapaPistas[1].gameObject, mapaPistas[2].gameObject);
            CriarLinhaFio3D(mapaPistas[2].gameObject, mapaPistas[3].gameObject);
            if(DM != null) DM.AtivarDialogo(14);
        }

        // Interligar 5 + 6
        if (!ligou56 && mapaPistas.ContainsKey(5) && mapaPistas.ContainsKey(6))
        {
            ligou56 = true;
            CriarLinhaFio3D(mapaPistas[5].gameObject, mapaPistas[6].gameObject);
            if(DM != null) DM.AtivarDialogo(15);
        }

        // Interligar 7 + 8
        if (!ligou78 && mapaPistas.ContainsKey(7) && mapaPistas.ContainsKey(8))
        {
            ligou78 = true;
            CriarLinhaFio3D(mapaPistas[7].gameObject, mapaPistas[8].gameObject);
            if(DM != null) DM.AtivarDialogo(16);
        }
    }

    void CriarLinhaFio3D(GameObject objA, GameObject objB)
    {
        GameObject goLinha = new GameObject("FioVermelho_Link", typeof(LineRenderer));
        goLinha.transform.SetParent(gridDoQuadro, false); 

        LineRenderer lr = goLinha.GetComponent<LineRenderer>();
        
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.startWidth = espessuraDoFio;
        lr.endWidth = espessuraDoFio;
        lr.material = materialFioNativo;
        
        lr.sortingOrder = 5; 

        // CORRIGIDO: conexoesAtivas com "e"
        ConexaoFio novaConexao = new ConexaoFio { lineRenderer = lr, objetoA = objA, objetoB = objB };
        conexoesAtivas.Add(novaConexao);

        ConfigurarPosiçãoFio(novaConexao);
    }

    void AtualizarPosicaoDosFios()
    {
        // CORRIGIDO: conexoesAtivas com "e"
        conexoesAtivas.RemoveAll(c => c.objetoA == null || c.objetoB == null);

        // CORRIGIDO: conexoesAtivas com "e"
        foreach (var conexao in conexoesAtivas)
        {
            ConfigurarPosiçãoFio(conexao);
        }
    }

   
    void ConfigurarPosiçãoFio(ConexaoFio conexao)
    {
        // Pega no centro exato do RectTransform no espaço do mundo
        Vector3 posA = conexao.objetoA.transform.position;
        Vector3 posB = conexao.objetoB.transform.position;

        // Um ligeiro empurrão para a frente (Z negativo em direção à câmara) 
        // para evitar "Z-Fighting" (fio a piscar dentro do painel)
        Vector3 avancoFrente = -transform.forward * 0.05f; 

        conexao.lineRenderer.SetPosition(0, posA + avancoFrente);
        conexao.lineRenderer.SetPosition(1, posB + avancoFrente);
    }
}