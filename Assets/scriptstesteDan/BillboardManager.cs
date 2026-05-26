using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BillboardManager : MonoBehaviour
{
    [Header("Configurações do Quadro")]
    public GameObject prefabCard;
    public Transform gridDoQuadro;
    public LineRenderer fioPrefab;
    public TextMeshProUGUI textoConclusao;

    [Header("UI Sistema de Fundo")]
    public GameObject painelFundoPreto; // O fundo escuro que vamos ativar/desativar

    private List<PistaCard> selecionados = new List<PistaCard>();
    private bool segredoRevelado = false;

    void Start()
    {
        if (textoConclusao != null) textoConclusao.text = "";
        FecharQuadro(); // Garante que começa fechado
    }

    // Funções para abrir e fechar o quadro com o fundo preto
    public void AbrirQuadro()
    {
        if (painelFundoPreto != null) painelFundoPreto.SetActive(true);
        // Ativa o rato e congela o tempo se necessário no teu script de input
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
            // Se errar, limpa a seleção após meio segundo
            Invoke("LimparSelecao", 0.5f);
        }
    }

    void Concluir(string txt, PistaCard c1, PistaCard c2)
    {
        if (textoConclusao != null) textoConclusao.text = txt;

        if (fioPrefab != null)
        {
            // Cria o fio diretamente dentro do Grid ou do Canvas para herdar a escala correta da UI
            LineRenderer fio = Instantiate(fioPrefab, gridDoQuadro);
            
            // Posições locais baseadas no RectTransform dos cartões
            Vector3 pos1 = c1.transform.position;
            Vector3 pos2 = c2.transform.position;

            // Puxa ligeiramente para a frente no eixo Z do mundo do Canvas
            fio.SetPosition(0, new Vector3(pos1.x, pos1.y, pos1.z - 0.1f));
            fio.SetPosition(1, new Vector3(pos2.x, pos2.y, pos2.z - 0.1f));
        }

        LimparSelecao();
    }

    void LimparSelecao()
    {
        selecionados.Clear();
    }
}