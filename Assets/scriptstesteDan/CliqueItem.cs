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

        // Procura o billboard
        billboard = FindObjectOfType<BillboardManager>();

        if (billboard == null)
        {
            Debug.LogWarning("BillboardManager não encontrado na cena!");
        }

        // Renderers
        meusRenderers = GetComponentsInChildren<Renderer>();

        if (meusRenderers != null && meusRenderers.Length > 0)
        {
            coresOriginais = new Color[meusRenderers.Length];

            for (int i = 0; i < meusRenderers.Length; i++)
            {
                if (meusRenderers[i] != null &&
                    meusRenderers[i].material != null &&
                    meusRenderers[i].material.HasProperty("_Color"))
                {
                    coresOriginais[i] = meusRenderers[i].material.color;
                }
                else
                {
                    coresOriginais[i] = Color.white;
                }
            }
        }
    }

    public void AoOlharEntrar()
    {
        if (jaFoiRegistado || jaEstaBrilhando) return;

        jaEstaBrilhando = true;

        if (meusRenderers != null)
        {
            foreach (Renderer r in meusRenderers)
            {
                if (r != null &&
                    r.material != null &&
                    r.material.HasProperty("_Color"))
                {
                    r.material.color = Color.white * 1.5f;
                }
            }
        }
    }

    public void AoOlharSair()
    {
        if (jaFoiRegistado || !jaEstaBrilhando) return;

        jaEstaBrilhando = false;

        if (meusRenderers != null)
        {
            for (int i = 0; i < meusRenderers.Length; i++)
            {
                if (meusRenderers[i] != null &&
                    meusRenderers[i].material != null &&
                    meusRenderers[i].material.HasProperty("_Color"))
                {
                    meusRenderers[i].material.color = coresOriginais[i];
                }
            }
        }
    }

    public void ColetarPista()
    {
        if (jaFoiRegistado) return;

        // =========================
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