using UnityEngine;
using System.Collections.Generic;

public class BillboardManager : MonoBehaviour
{
    [Header("Identificação do Nível")]
    [Tooltip("Define se este quadro é do Nível 1, Nível 2, etc.")]
    public int idNivel = 1;

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

    private struct ConexaoFio
    {
        public LineRenderer lineRenderer;
        public GameObject objetoA;
        public GameObject objetoB;
    }
    private List<ConexaoFio> conexoesAtivas = new List<ConexaoFio>();

    // Trincas de controle do Nível 1
    private bool ligou123 = false;
    private bool ligou56 = false;
    private bool ligou78 = false;

    // Trincas de controle do Nível 2
    private bool ligou15_Nivel2 = false;
    private bool ligou36_Nivel2 = false;
    private bool ligou789_Nivel2 = false;

    private bool ligou9161110_Nivel2 = false;
    private bool ligou131415_Nivel2 = false;

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

        if (idNivel == 1)
        {
            // Interligar 1 + 2 + 3 (Nível 1)
            if (!ligou123 && mapaPistas.ContainsKey(1) && mapaPistas.ContainsKey(2) && mapaPistas.ContainsKey(3))
            {
                ligou123 = true;
                CriarLinhaFio3D(mapaPistas[1].gameObject, mapaPistas[2].gameObject);
                CriarLinhaFio3D(mapaPistas[2].gameObject, mapaPistas[3].gameObject);
                if(DM != null) DM.AtivarDialogo(14);
            }

            // Interligar 5 + 6 (Nível 1)
            if (!ligou56 && mapaPistas.ContainsKey(5) && mapaPistas.ContainsKey(6))
            {
                ligou56 = true;
                CriarLinhaFio3D(mapaPistas[5].gameObject, mapaPistas[6].gameObject);
                if(DM != null) DM.AtivarDialogo(15);
            }

            // Interligar 7 + 8 (Nível 1)
            if (!ligou78 && mapaPistas.ContainsKey(7) && mapaPistas.ContainsKey(8))
            {
                ligou78 = true;
                CriarLinhaFio3D(mapaPistas[7].gameObject, mapaPistas[8].gameObject);
                if(DM != null) DM.AtivarDialogo(16);
            }
        }
        else if (idNivel == 2)
        {
            // Interligar 1 + 5: "Mesma vítima, locais diferentes..."
            if (!ligou15_Nivel2 && mapaPistas.ContainsKey(1) && mapaPistas.ContainsKey(5))
            {
                ligou15_Nivel2 = true;
                CriarLinhaFio3D(mapaPistas[1].gameObject, mapaPistas[5].gameObject);
                if(DM != null) DM.AtivarDialogo(48);
                // NOTA: Altera o ID do diálogo (ex: 20) conforme o teu sistema de IDs do Nível 2
                
            }

            // Interligar 3 + 6: "O alicate; o livro; e caixa de luz..."
            if (!ligou36_Nivel2 && mapaPistas.ContainsKey(3) && mapaPistas.ContainsKey(6))
            {
                ligou36_Nivel2 = true;
                CriarLinhaFio3D(mapaPistas[3].gameObject, mapaPistas[6].gameObject);
                if(DM != null) DM.AtivarDialogo(49);
            }

           
           // Interligar 7 + 8+ 9: "O alicate; o livro; e caixa de luz..."
            if (!ligou789_Nivel2 && mapaPistas.ContainsKey(7) && mapaPistas.ContainsKey(8) && mapaPistas.ContainsKey(9))
            {
                ligou789_Nivel2 = true;
                CriarLinhaFio3D(mapaPistas[7].gameObject, mapaPistas[8].gameObject);
                CriarLinhaFio3D(mapaPistas[8].gameObject, mapaPistas[9].gameObject);
                if(DM != null) DM.AtivarDialogo(50);
            }
            
            // Interligar 9 + 16 + 11 + 10: "Trancaste-te dentro do escritório..."
            // NOTA: Como a pista 16 vem de uma interação mecânica, ela precisa de ser adicionada ao quadro via script para ligar!
            if (!ligou9161110_Nivel2 && mapaPistas.ContainsKey(9) && mapaPistas.ContainsKey(16) && mapaPistas.ContainsKey(11) && mapaPistas.ContainsKey(10))
            {
                ligou9161110_Nivel2 = true;
                CriarLinhaFio3D(mapaPistas[9].gameObject, mapaPistas[16].gameObject);
                CriarLinhaFio3D(mapaPistas[16].gameObject, mapaPistas[11].gameObject);
                CriarLinhaFio3D(mapaPistas[11].gameObject, mapaPistas[10].gameObject);
                if(DM != null) DM.AtivarDialogo(51);
            }

            // Interligar 13 + 14 + 15: "Alguém parece ter saltado da varanda..."
            if (!ligou131415_Nivel2 && mapaPistas.ContainsKey(13) && mapaPistas.ContainsKey(14) && mapaPistas.ContainsKey(15))
            {
                ligou131415_Nivel2 = true;
                CriarLinhaFio3D(mapaPistas[13].gameObject, mapaPistas[14].gameObject);
                CriarLinhaFio3D(mapaPistas[14].gameObject, mapaPistas[15].gameObject);
                if(DM != null) DM.AtivarDialogo(52);
            }
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

        ConexaoFio novaConexao = new ConexaoFio { lineRenderer = lr, objetoA = objA, objetoB = objB };
        conexoesAtivas.Add(novaConexao);

        ConfigurarPosiçãoFio(novaConexao);
    }

    void AtualizarPosicaoDosFios()
    {
        conexoesAtivas.RemoveAll(c => c.objetoA == null || c.objetoB == null);

        foreach (var conexao in conexoesAtivas)
        {
            ConfigurarPosiçãoFio(conexao);
        }
    }
   
    void ConfigurarPosiçãoFio(ConexaoFio conexao)
    {
        Vector3 posA = conexao.objetoA.transform.position;
        Vector3 posB = conexao.objetoB.transform.position;

        Vector3 avancoFrente = -transform.forward * 0.05f; 

        conexao.lineRenderer.SetPosition(0, posA + avancoFrente);
        conexao.lineRenderer.SetPosition(1, posB + avancoFrente);
    }
}