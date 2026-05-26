using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BillboardManager : MonoBehaviour
{
    [Header("Configurações do Quadro")]
    public GameObject cardPistaPrefab;    
    public Transform gridDoQuadro;        
    public LineRenderer fioVermelhoPrefab; 

    [Header("UI de Conclusões")]
    public TextMeshProUGUI textoConclusaoUI; 

    private List<PistaCard> pistasNoQuadro = new List<PistaCard>();
    private List<PistaCard> pistasSelecionadas = new List<PistaCard>();
    private bool primeiraCombinacaoFeita = false; 

    void Start()
    {
        if (textoConclusaoUI != null) textoConclusaoUI.text = "";
    }

    public void AdicionarPistaAoQuadro(Sprite foto, string nome, string descricao, int numeroFixo)
    {
        if (cardPistaPrefab == null || gridDoQuadro == null) return;

        GameObject novoCard = Instantiate(cardPistaPrefab, gridDoQuadro);
        PistaCard scriptCard = novoCard.GetComponent<PistaCard>();

        if (scriptCard != null)
        {
            scriptCard.ConfigurarCard(foto, nome, descricao, numeroFixo, this);
            pistasNoQuadro.Add(scriptCard);
        }
    }

    public void SelecionarPista(PistaCard cardClicado)
    {
        if (pistasSelecionadas.Contains(cardClicado))
        {
            pistasSelecionadas.Remove(cardClicado);
            return;
        }

        pistasSelecionadas.Add(cardClicado);

        if (pistasSelecionadas.Count == 2)
        {
            VerificarCombinacaoDupla();
        }
    }

    void VerificarCombinacaoDupla()
    {
        int n1 = pistasSelecionadas[0].numeroDaPista;
        int n2 = pistasSelecionadas[1].numeroDaPista;

        // Combinação das pistas 7 (Telemóvel) e 8 (Pulseiras)
        if ((n1 == 7 && n2 == 8) || (n1 == 8 && n2 == 7))
        {
            if (!primeiraCombinacaoFeita)
            {
                SucessoCombinacao("Estes estavam próximos, possivelmente pertenciam à mesma pessoa.");
                primeiraCombinacaoFeita = true;
            }
            else
            {
                SucessoCombinacao("Ela deve ter ido buscar estas pulseiras, mas aonde...?");
            }
        }
        else
        {
            // Se errar a combinação, pisca ou limpa a seleção
            Invoke("LimparSelecao", 0.3f);
        }
    }

    void SucessoCombinacao(string textoResultado)
    {
        if (fioVermelhoPrefab != null && pistasSelecionadas.Count == 2)
        {
            CriarFioVermelho(pistasSelecionadas[0].transform.position, pistasSelecionadas[1].transform.position);
        }

        if (textoConclusaoUI != null)
        {
            textoConclusaoUI.text = textoResultado;
        }

        LimparSelecao();
    }

    void LimparSelecao()
    {
        pistasSelecionadas.Clear();
    }

    void CriarFioVermelho(Vector3 pos1, Vector3 pos2)
    {
        LineRenderer novoFio = Instantiate(fioVermelhoPrefab, transform);
        // O eixo Z recua -0.02f para o fio ficar colado à frente dos papéis sem os atravessar
        novoFio.SetPosition(0, new Vector3(pos1.x, pos1.y, pos1.z - 0.02f)); 
        novoFio.SetPosition(1, new Vector3(pos2.x, pos2.y, pos2.z - 0.02f));
    }
}