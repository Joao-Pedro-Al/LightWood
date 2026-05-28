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
        
        // Ativa o rato para poderes clicar e selecionar os cartões à vontade
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void FecharQuadro()
    {
        if (painelFundoPreto) painelFundoPreto.SetActive(false);
        
        // Prende o rato novamente para o jogador voltar ao modo de exploração 3D
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AdicionarPistaAoQuadro(Sprite foto, string nome, string descricao, int numero)
    {
        if (prefabCard == null || gridDoQuadro == null) return;

        GameObject novoCard = Instantiate(prefabCard, gridDoQuadro);
        
        RectTransform rt = novoCard.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
        }
        
        PistaCard scriptCard = novoCard.GetComponent<PistaCard>();
        if (scriptCard != null)
        {
            scriptCard.Setup(foto, nome, descricao, numero, this);
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

        // Se forem as pulseiras (8) e o telemóvel (7) por exemplo
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