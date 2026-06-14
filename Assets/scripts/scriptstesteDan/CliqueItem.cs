using UnityEngine;

public class CliqueItem : MonoBehaviour
{
    [Header("Configurações da Pista")]
    public Sprite imagemDoItem;
    public string nomeDaPista = "Nome do Item";
    [TextArea(2, 4)] public string descricaoDaPista = "Descrição no card.";
    public int numeroFixoDaPista = 1;

    [Header("Diálogo")]
    private Dialogo DM;
    public int Id_Dialogo;

    [Header("Tipo de Pista")]
    [Tooltip("Se ativares isto, o objeto NÃO desaparece do chão ao clicar, mas vai para o quadro na mesma!")]
    public bool naoColetavel = false;

    [Header("Mecânica de Bateria")]
    [Tooltip("Ativa esta caixinha se este objeto for uma Bateria para recarregar a Lanterna!")]
    public bool eBateria = false;

    [Tooltip("Quantidade de carga que esta bateria vai dar à lanterna.")]
    public float quantidadeCarga = 50f;

    private BillboardManager billboard;
    private Renderer[] meusRenderers;
    private Color[] coresOriginais;

    private bool jaEstaBrilhando = false;
    private bool jaFoiRegistado = false;

    void Start()
    {
        // Procura o sistema de diálogo
        DM = Dialogo.Instance;

        if (DM == null)
        {
            Debug.LogWarning("Dialogo.Instance não foi encontrado na cena!");
        }

        // Cache de renderers para o efeito de brilho
        meusRenderers = GetComponentsInChildren<Renderer>();
        coresOriginais = new Color[meusRenderers.Length];
        for (int i = 0; i < meusRenderers.Length; i++)
        {
            if (meusRenderers[i].material.HasProperty("_Color"))
            {
                coresOriginais[i] = meusRenderers[i].material.color;
            }
        }

        // Procura o BillboardManager na cena
        billboard = FindObjectOfType<BillboardManager>();

        // =========================================================================
        // CORREÇÃO PARA O CASO DE TESTE DA BATERIA (RESPAWN / MORTE)
        // Só faz sumir se NÃO for uma bateria, evitando que o GeradorBaterias quebre!
        // =========================================================================
        if (!eBateria && GeradorSalvamento.Instance != null)
        {
            bool jaEstaSalvaNoQuadro = GeradorSalvamento.Instance.pistasSalvasPermanentes.Exists(p => p.numero == numeroFixoDaPista);

            if (jaEstaSalvaNoQuadro)
            {
                if (naoColetavel)
                {
                    // Se for a tenda (não coletável), remove a interação para sempre
                    jaFoiRegistado = true;
                    this.enabled = false;
                    return;
                }
                else
                {
                    // Se for o marshmallow (coletável), desaparece do chão imediatamente
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }

    public void AoOlharEntrar()
    {
        AoOlharEntrar(new Color(0.3f, 0.3f, 0.3f));
    }

    public void AoOlharEntrar(Color corDoBrilho)
    {
        if (jaFoiRegistado || jaEstaBrilhando) return;

        jaEstaBrilhando = true;

        for (int i = 0; i < meusRenderers.Length; i++)
        {
            if (meusRenderers[i] != null && meusRenderers[i].material.HasProperty("_Color"))
            {
                meusRenderers[i].material.color = coresOriginais[i] + corDoBrilho;
            }
        }
    }

    public void AoOlharSair()
    {
        if (!jaEstaBrilhando) return;

        jaEstaBrilhando = false;

        for (int i = 0; i < meusRenderers.Length; i++)
        {
            if (meusRenderers[i] != null && meusRenderers[i].material.HasProperty("_Color"))
            {
                meusRenderers[i].material.color = coresOriginais[i];
            }
        }
    }

    public void ColetarPista()
    {
        AoClicar();
    }

    public void AoClicar()
    {
        if (jaFoiRegistado) return;

        // BATERIA
        // =========================
        if (eBateria)
        {
            Flashlight lanterna = FindObjectOfType<Flashlight>();

            if (lanterna != null)
            {
                lanterna.Recharge(quantidadeCarga);
            }
            else
            {
                Debug.LogWarning("Flashlight não encontrada!");
            }

            AoOlharSair();
            Destroy(gameObject);
            return;
        }

        // =========================
        // DIÁLOGO
        // =========================
        if (DM != null)
        {
            DM.AtivarDialogo(Id_Dialogo);
        }
        else
        {
            Debug.LogWarning("Não foi possível iniciar diálogo porque DM está null.");
        }

        // =========================
        // BILLBOARD
        // =========================
        if (billboard != null)
        {
            billboard.AdicionarPistaAoQuadro(
                imagemDoItem,
                nomeDaPista,
                descricaoDaPista,
                numeroFixoDaPista
            );
        }

        // =========================
        // COLETA
        // =========================
        if (naoColetavel)
        {
            jaFoiRegistado = true;
            AoOlharSair();

            this.enabled = false;
        }
        else
        {
            AoOlharSair();
            Destroy(gameObject);
        }
    }
}