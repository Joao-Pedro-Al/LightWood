using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BillboardManager : MonoBehaviour
{
    [Header("Configurações do Quadro")]
    public GameObject prefabCard;     
    public Transform gridDoQuadro;    
    public LineRenderer fioPrefab;    

    [Header("Textos e UI")]
    public TextMeshProUGUI textoConclusao;
    public GameObject painelFundoPreto; 

    private List<PistaCard> selecionados = new List<PistaCard>();
    private bool segredoRevelado = false;

    void Start()
    {
        if (textoConclusao != null) textoConclusao.text = "";
        FecharQuadro();
    }

    public void AbrirQuadro()
    {
        if (painelFundoPreto != null) painelFundoPreto.SetActive(true);
    }

    public void FecharQuadro()
    {
        if (painelFundoPreto != null) painelFundoPreto.SetActive(false);
    }

    public void AdicionarPistaAoQuadro(Sprite f, string n, string d, int num)
    {
        if (prefabCard == null || gridDoQuadro == null) return;

        GameObject go = Instantiate(prefabCard, gridDoQuadro);
        PistaCard scriptCard = go.GetComponent<PistaCard>();
        if (scriptCard != null)
        {
            scriptCard.Setup(f, n, d, num, this);
        }
    }

    public void SelecionarPista(PistaCard card)
    {
        if (selecionados.Contains(card)) return;
        selecionados.Add(card);

        if (selecionados.Count == 2)
        {
            Verificar();
        }
    }

    void Verificar()
    {
        int n1 = selecionados[0].meuNumero;
        int n2 = selecionados[1].meuNumero;

        if ((n1 == 7 && n2 == 8) || (n1 == 8 && n2 == 7))
        {
            if (!segredoRevelado) {
                Concluir("Estes estavam próximos, pertenciam à mesma pessoa.", selecionados[0], selecionados[1]);
                segredoRevelado = true;
            } else {
                Concluir("Ela deve ter ido buscar estas pulseiras, mas aonde...?", selecionados[0], selecionados[1]);
            }
        }
        else
        {
            Invoke("LimparSelecao", 0.5f);
        }
    }

    void Concluir(string txt, PistaCard c1, PistaCard c2)
    {
        if (textoConclusao != null) textoConclusao.text = txt;

        if (fioPrefab != null)
        {
            // Cria o fio como filho do quadro para herdar a rotação de 239 graus
            LineRenderer fio = Instantiate(fioPrefab, transform);
            fio.useWorldSpace = true;

            Vector3 pos1 = c1.transform.position;
            Vector3 pos2 = c2.transform.position;

            // Afasta ligeiramente o fio para a frente do quadro usando a direção frontal do modelo
            Vector3 offsetFrente = transform.forward * -0.02f;

            fio.SetPosition(0, pos1 + offsetFrente);
            fio.SetPosition(1, pos2 + offsetFrente);
        }

        LimparSelecao();
    }

    void LimparSelecao()
    {
        selecionados.Clear();
    }
}