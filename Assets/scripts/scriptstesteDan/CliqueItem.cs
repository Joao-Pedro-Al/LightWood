using UnityEngine;

public class CliqueItem : MonoBehaviour
{
    [Header("Configurações de Nível")]
    public int idNivel = 1;

    [Header("Configurações da Pista")]
    public Sprite imagemDoItem;
    public string nomeDaPista = "Nome do Item";
    [TextArea(2, 4)] public string descricaoDaPista = "Descrição no card.";
    public int numeroFixoDaPista = 1;

    [Header("Diálogo")]
    private Dialogo DM;
    public int Id_Dialogo;

    [Header("Tipo de Pista")]
    public bool naoColetavel = false;

    [Header("Mecânica de Bateria")]
    public bool eBateria = false;
    public float quantidadeCarga = 50f;

    [Header("Mecânica da Lanterna (Item Inicial)")]
    public bool eItemLanterna = false;

    [Header("Mecânicas Especiais (Nível 2)")]
    public bool eLanternaNelson = false;
    public bool eCorpoNelson = false;
    public bool ePortaTrancadaNivel2 = false;

    [Space(5)]
    public Sprite imagemChaveExterior;
    public Sprite imagemPilhasNelson;
    public GameObject portaObjetoFisico;

    private BillboardManager billboard;
    private Renderer[] meusRenderers;
    private Color[] coresOriginais;
    private bool jaEstaBrilhando = false;
    private bool jaFoiRegistado = false;

    void Start()
    {
        DM = Dialogo.Instance;

        meusRenderers = GetComponentsInChildren<Renderer>();
        coresOriginais = new Color[meusRenderers.Length];
        for (int i = 0; i < meusRenderers.Length; i++)
        {
            if (meusRenderers[i].material.HasProperty("_Color"))
                coresOriginais[i] = meusRenderers[i].material.color;
        }

        BillboardManager[] todosBillboards = FindObjectsOfType<BillboardManager>();
        foreach (var b in todosBillboards)
        {
            if (b.idNivel == idNivel) { billboard = b; break; }
        }

        // Baterias nunca persistem — reaparecem sempre
        if (eBateria) return;

        // ═══════════════════════════════════════════════════════
        // Verifica se este item já foi apanhado antes de morrer
        // ═══════════════════════════════════════════════════════
        if (GeradorSalvamento.Instance == null) return;

        // Lanterna
        if (eItemLanterna)
        {
            if (GeradorSalvamento.Instance.flashlightApanhada)
            {
                // Já tinha a lanterna — destrói o item sem mostrar nada
                // O GeradorSalvamento já tratou de restaurar no AoCarregarNovaCena
                Destroy(gameObject);
                return;
            }
        }
        // Pistas normais / especiais
        else
        {
            bool jaGuardada = GeradorSalvamento.Instance.pistasSalvasPermanentes.Exists(
                p => p.numero == numeroFixoDaPista && p.idNivel == idNivel);

            if (jaGuardada)
            {
                if (naoColetavel)
                {
                    jaFoiRegistado = true;
                    this.enabled = false;
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
            }
        }
    }

    public void AoOlharEntrar() => AoOlharEntrar(new Color(0.3f, 0.3f, 0.3f));

    public void AoOlharEntrar(Color corDoBrilho)
    {
        if (jaFoiRegistado || jaEstaBrilhando) return;
        jaEstaBrilhando = true;
        for (int i = 0; i < meusRenderers.Length; i++)
            if (meusRenderers[i] != null && meusRenderers[i].material.HasProperty("_Color"))
                meusRenderers[i].material.color = coresOriginais[i] + corDoBrilho;
    }

    public void AoOlharSair()
    {
        if (!jaEstaBrilhando) return;
        jaEstaBrilhando = false;
        for (int i = 0; i < meusRenderers.Length; i++)
            if (meusRenderers[i] != null && meusRenderers[i].material.HasProperty("_Color"))
                meusRenderers[i].material.color = coresOriginais[i];
    }

    public void ColetarPista() => AoClicar();

    public void AoClicar()
    {
        if (jaFoiRegistado) return;

        // ==========================================
        // MECÂNICA 0: ITEM DA LANTERNA
        // ==========================================
        if (eItemLanterna)
        {
            if (Flashlight.Instance != null) Flashlight.Instance.CollectFlashlight();

            // Guarda imediatamente que a lanterna foi apanhada
            if (GeradorSalvamento.Instance != null) GeradorSalvamento.Instance.GuardarFlashlight();

            if (DM != null) DM.AtivarDialogo(Id_Dialogo);
            if (billboard != null)
                billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);

            AoOlharSair();
            Destroy(gameObject);
            return;
        }

        // ==========================================
        // MECÂNICA 1: BATERIA COMUM (não persiste)
        // ==========================================
        if (eBateria)
        {
            if (Flashlight.Instance != null) Flashlight.Instance.Recharge(quantidadeCarga);
            AoOlharSair();
            Destroy(gameObject);
            return;
        }

        // ==========================================
        // MECÂNICA 2: LANTERNA DO NELSON
        // ==========================================
        if (eLanternaNelson)
        {
            if (Flashlight.Instance != null) Flashlight.Instance.Recharge(quantidadeCarga);
            if (billboard != null)
                billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);
            if (DM != null) DM.AtivarDialogo(Id_Dialogo);
            AoOlharSair();
            Destroy(gameObject);
            return;
        }

        // ==========================================
        // MECÂNICA 3: CORPO DE NELSON
        // ==========================================
        if (eCorpoNelson)
        {
            if (DM != null) DM.AtivarDialogo(Id_Dialogo);
            if (billboard != null)
            {
                billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);
                billboard.AdicionarPistaAoQuadro(imagemChaveExterior, "Chave para o Exterior", "Mm... precisastes das chaves para quê?", 16);
            }
            jaFoiRegistado = true;
            AoOlharSair();
            this.enabled = false;
            return;
        }

        // ==========================================
        // MECÂNICA 4: PORTA TRANCADA
        // ==========================================
        if (ePortaTrancadaNivel2)
        {
            bool temAChave = GeradorSalvamento.Instance != null &&
                GeradorSalvamento.Instance.pistasSalvasPermanentes.Exists(
                    p => p.numero == 16 && p.idNivel == idNivel);

            if (temAChave)
            {
                if (DM != null) DM.AtivarDialogo(Id_Dialogo);
                if (portaObjetoFisico != null) Destroy(portaObjetoFisico);
                if (billboard != null)
                    billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);
                jaFoiRegistado = true;
                AoOlharSair();
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("A porta está trancada por dentro.");
            }
            return;
        }

        // ==========================================
        // PISTAS NORMAIS
        // ==========================================
        if (DM != null) DM.AtivarDialogo(Id_Dialogo);
        if (billboard != null)
            billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);

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