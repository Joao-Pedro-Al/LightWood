using UnityEngine;
using System.Collections.Generic;

public class BillboardManager : MonoBehaviour
{
    [Header("Diálogo")]
    private Dialogo DM; // Para aceder ao Script de Diálogo

    [Header("Configurações do Quadro")]
    public GameObject prefabCard;     
    public GameObject painelFundoPreto; 

    [Header("Configurações do Caos")]
    public float limitesX = 400f;
    public float limitesY = 200f;

    [Header("Customização do Fio Vermelho (3D Real)")]
    [Range(0.005f, 0.1f)] public float espessuraDoFio = 0.015f; // Grossura perfeita de um cordel fino
    public Color corDoFio = new Color(0.75f, 0.05f, 0.05f);     // Vermelho vivo de investigação

    private Transform gridDoQuadro;   
    private List<PistaCard> cartoesInstanciados = new List<PistaCard>();
    private Material materialFioNativo;

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

    // Start para o Instance, pois se colocar no Wake poderá não associar no caso que Dialogo.cs ainda não iniciou
    void Start()
    {
        DM = Dialogo.Instance;
    }

    void GerarMaterialDoFio()
    {
        // Usa um shader simples não afetado por luzes para a cor ficar sempre viva
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
            DM.AtivarDialogo(14);
        }

        // Interligar 5 + 6
        if (!ligou56 && mapaPistas.ContainsKey(5) && mapaPistas.ContainsKey(6))
        {
            ligou56 = true;
            CriarLinhaFio3D(mapaPistas[5].gameObject, mapaPistas[6].gameObject);
            DM.AtivarDialogo(15);
        }

        // Interligar 7 + 8
        if (!ligou78 && mapaPistas.ContainsKey(7) && mapaPistas.ContainsKey(8))
        {
            ligou78 = true;
            CriarLinhaFio3D(mapaPistas[7].gameObject, mapaPistas[8].gameObject);
            DM.AtivarDialogo(16);
        }
    }

    void CriarLinhaFio3D(GameObject objA, GameObject objB)
    {
        // Cria um objeto para o fio e junta-o diretamente como filho do gestor do quadro
        GameObject goLinha = new GameObject("Fio3D_Conexao", typeof(LineRenderer));
        goLinha.transform.SetParent(transform, true);

        LineRenderer lr = goLinha.GetComponent<LineRenderer>();
        
        // Define o uso do espaço global do mundo para evitar desvios por causa de rotações locais
        lr.useWorldSpace = true;
        lr.positionCount = 2;

        // Modifica a grossura nas duas pontas uniformemente
        lr.startWidth = espessuraDoFio;
        lr.endWidth = espessuraDoFio;

        lr.material = materialFioNativo;
        lr.startColor = corDoFio;
        lr.endColor = corDoFio;

        // CORREÇÃO GEOMÉTRICA: Captura a posição exata no mundo 3D de cada cartão
        Vector3 posA = objA.transform.position;
        Vector3 posB = objB.transform.position;

        // Cria um recuo muito ligeiro para a frente (usando o eixo Z global ou a frente do quadro)
        // para garantir que o barbante não se esconde por trás da textura da cortiça
        Vector3 avançoFrente = -transform.forward * 0.02f;

        // Força as pontas a ligarem exatamente onde os cartões estão renderizados na tela
        lr.SetPosition(0, posA + avançoFrente);
        lr.SetPosition(1, posB + avançoFrente);
    }
}