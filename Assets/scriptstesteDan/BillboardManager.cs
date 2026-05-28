using UnityEngine;
using System.Collections.Generic;

public class BillboardManager : MonoBehaviour
{
    [Header("Configurações do Quadro")]
    [Tooltip("Arrasta o teu Prefab do CardPista para aqui.")]
    public GameObject prefabCard;     
    [Tooltip("Arrasta o teu Prefab do FioVermelho (LineRenderer) para aqui.")]
    public LineRenderer fioPrefab;    

    [Header("UI do Inventário")]
    [Tooltip("Arrasta o teu objeto fundoinventario para aqui.")]
    public GameObject painelFundoPreto; 

    [Header("Configurações do Caos Máximo (Mural Livre)")]
    [Tooltip("Distância máxima para a esquerda e para a direita a partir do centro (Aumenta para espalhar mais nos lados).")]
    public float limitesX = 500f;
    [Tooltip("Distância máxima para cima e para baixo a partir do centro (Aumenta para espalhar mais na vertical).")]
    public float limitesY = 300f;

    private Transform gridDoQuadro;   
    private List<PistaCard> selecionados = new List<PistaCard>();

    void Awake()
    {
        Canvas canvasInterno = GetComponentInChildren<Canvas>();
        if (canvasInterno != null)
        {
            gridDoQuadro = canvasInterno.transform;
        }
        else
        {
            Debug.LogError("Aviso: O Canvas_Quadro precisa de ser filho do objeto buildboard2D!");
        }

        if (painelFundoPreto) painelFundoPreto.SetActive(false);
    }

    public void AbrirQuadro()
    {
        if (painelFundoPreto) painelFundoPreto.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void FecharQuadro()
    {
        if (painelFundoPreto) painelFundoPreto.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AdicionarPistaAoQuadro(Sprite foto, string nome, string descricao, int numero)
    {
        if (prefabCard == null || gridDoQuadro == null) return;

        // Instancia o cartão no quadro
        GameObject novoCard = Instantiate(prefabCard, gridDoQuadro);
        
        RectTransform rt = novoCard.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;

            // CAOS TOTAL: Cada cartão ganha uma posição absolutamente única e livre no quadro inteiro
            // Retirámos as amarras do ID na posição para que eles possam nascer em QUALQUER lugar da cortiça!
            float aleatorioX = Random.Range(-limitesX, limitesX);
            float aleatorioY = Random.Range(-limitesY, limitesY);

            rt.localPosition = new Vector3(aleatorioX, aleatorioY, 0f);

            // ROTAÇÃO REALISTA: Inclinação do cartão torto (efeito espetado com pionés)
            float inclinacaoAleatoria = Random.Range(-15f, 15f); // Aumentado para até 15 graus de inclinação
            rt.localRotation = Quaternion.Euler(0f, 0f, inclinacaoAleatoria);
        }
        
        PistaCard scriptCard = novoCard.GetComponent<PistaCard>();
        if (scriptCard != null)
        {
            scriptCard.Setup(foto, nome, descricao, numero, this);
        }

        // Mantém a organização interna para o sistema de ligações funcionar perfeitamente
        OrganizarHierarquiaInterna();
    }

    void OrganizarHierarquiaInterna()
    {
        if (gridDoQuadro == null) return;

        List<PistaCard> cartoesNoQuadro = new List<PistaCard>();
        foreach (Transform filho in gridDoQuadro)
        {
            PistaCard card = filho.GetComponent<PistaCard>();
            if (card != null) cartoesNoQuadro.Add(card);
        }

        // Ordenação lógica interna por ID
        cartoesNoQuadro.Sort((a, b) => a.meuNumero.CompareTo(b.meuNumero));

        for (int i = 0; i < cartoesNoQuadro.Count; i++)
        {
            cartoesNoQuadro[i].transform.SetSiblingIndex(i);
        }
    }

    public void SelecionarPista(PistaCard card)
    {
        if (selecionados.Contains(card)) return;
        selecionados.Add(card);

        if (selecionados.Count == 2)
        {
            VerificarLigacao();
        }
    }

    void VerificarLigacao()
    {
        int n1 = selecionados[0].meuNumero;
        int n2 = selecionados[1].meuNumero;

        // Mantém a tua mecânica de ligar o telemóvel (7) às pulseiras (8)
        if ((n1 == 7 && n2 == 8) || (n1 == 8 && n2 == 7))
        {
            DesenharFio(selecionados[0], selecionados[1]);
        }
        
        selecionados.Clear();
    }

    void DesenharFio(PistaCard c1, PistaCard c2)
    {
        if (!fioPrefab) return;

        LineRenderer fio = Instantiate(fioPrefab, transform);
        fio.useWorldSpace = true;
        fio.positionCount = 2;
        
        Vector3 offset = transform.forward * -0.05f; 
        fio.SetPosition(0, c1.transform.position + offset);
        fio.SetPosition(1, c2.transform.position + offset);
    }
}