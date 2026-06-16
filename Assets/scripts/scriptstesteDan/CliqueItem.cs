using UnityEngine;

public class CliqueItem : MonoBehaviour
{
    [Header("Configurações de Nível")]
    [Tooltip("A qual nível esta pista pertence? (Deve bater com o ID do BillboardManager do nível)")]
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
    [Tooltip("Se ativares isto, o objeto NÃO desaparece do chão ao clicar, mas vai para o quadro na mesma!")]
    public bool naoColetavel = false;

    [Header("Mecânica de Bateria")]
    public bool eBateria = false;
    public float quantidadeCarga = 50f;

    [Header("Mecânicas Especiais (Nível 2)")]
    public bool eLanternaNelson = false;
    public bool eCorpoNelson = false;
    public bool ePortaTrancadaNivel2 = false;
    
    [Space(5)]
    public Sprite imagemChaveExterior; // Atribui no inspector do Corpo do Nelson
    public Sprite imagemPilhasNelson;  // Atribui no inspector da Lanterna do Nelson
    public GameObject portaObjetoFisico; // Se for a porta trancada, arrasta o modelo da porta aqui para abrir/rodar/destruir

    private BillboardManager billboard;
    private Renderer[] meusRenderers;
    private Color[] coresOriginais;

    private bool jaEstaBrilhando = false;
    private bool jaFoiRegistado = false;

    void Start()
    {
        DM = Dialogo.Instance;

        if (DM == null)
        {
            Debug.LogWarning("Dialogo.Instance não foi encontrado na cena!");
        }

        meusRenderers = GetComponentsInChildren<Renderer>();
        coresOriginais = new Color[meusRenderers.Length];
        for (int i = 0; i < meusRenderers.Length; i++)
        {
            if (meusRenderers[i].material.HasProperty("_Color"))
            {
                coresOriginais[i] = meusRenderers[i].material.color;
            }
        }

        // Procura todos os billboards e escolhe o que tem o mesmo ID de Nível deste item
        BillboardManager[] todosBillboards = FindObjectsOfType<BillboardManager>();
        foreach (var b in todosBillboards)
        {
            if (b.idNivel == idNivel)
            {
                billboard = b;
                break;
            }
        }

        // SISTEMA DE SALVAMENTO ATUALIZADO COM ID DO NÍVEL
        if (!eBateria && GeradorSalvamento.Instance != null)
        {
            bool jaEstaSalvaNoQuadro = GeradorSalvamento.Instance.pistasSalvasPermanentes.Exists(p => p.numero == numeroFixoDaPista && p.idNivel == idNivel);

            if (jaEstaSalvaNoQuadro)
            {
                if (naoColetavel)
                {
                    jaFoiRegistado = true;
                    this.enabled = false;
                    return;
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }

    public void AoOlharEntrar() => AoOlharEntrar(new Color(0.3f, 0.3f, 0.3f));

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

    public void ColetarPista() => AoClicar();

    public void AoClicar()
    {
        if (jaFoiRegistado) return;

        // ==========================================
        // MECÂNICA 1: BATERIA COMUM
        // ==========================================
        if (eBateria)
        {
            Flashlight lanterna = FindObjectOfType<Flashlight>();
            if (lanterna != null) lanterna.Recharge(quantidadeCarga);
            AoOlharSair();
            Destroy(gameObject);
            return;
        }

        // ==========================================
        // MECÂNICA 2: LANTERNA DO NELSON (DÁ PILHAS)
        // ==========================================
        if (eLanternaNelson)
        {
            Flashlight lanterna = FindObjectOfType<Flashlight>();
            if (lanterna != null) lanterna.Recharge(quantidadeCarga); // Dá a carga das pilhas
            
            // Adiciona também a própria lanterna como pista ao quadro
            if (billboard != null)
            {
                billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);
            }
            
            // Opcional: Adicionar feedback visual ou inventário se usares
            Debug.Log("Obrigado... [Obteu pilhas]");
            
            if (DM != null) DM.AtivarDialogo(Id_Dialogo);
            AoOlharSair();
            Destroy(gameObject);
            return;
        }

        // ==========================================
        // MECÂNICA 3: CORPO DE NELSON (DÁ CHAVE - PISTA 16)
        // ==========================================
        if (eCorpoNelson)
        {
            if (DM != null) DM.AtivarDialogo(Id_Dialogo); // Texto base do corpo ("Yuck...")

            if (billboard != null)
            {
                // Envia o Corpo (Pista 9)
                billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);
                
                // Envia AUTOMATICAMENTE a Chave para o Exterior (Pista 16) para o quadro!
                billboard.AdicionarPistaAoQuadro(imagemChaveExterior, "Chave para o Exterior", "Mm... precisastes das chaves para quê?", 16);
            }
            
            jaFoiRegistado = true;
            AoOlharSair();
            this.enabled = false; // Corpo não some por ser um corpo, mas desativa interação
            return;
        }

        // ==========================================
        // MECÂNICA 4: PORTA TRANCADA (REQUER PISTA 16 NO QUADRO)
        // ==========================================
        if (ePortaTrancadaNivel2)
        {
            // Vamos verificar se o jogador já obteu a chave (pista 16) analisando o quadro
            bool temAChave = false;
            if (GeradorSalvamento.Instance != null)
            {
                temAChave = GeradorSalvamento.Instance.pistasSalvasPermanentes.Exists(p => p.numero == 16 && p.idNivel == idNivel);
            }

            if (temAChave)
            {
                Debug.Log("Então estivestes aqui... [A Porta Abre]");
                if (DM != null) DM.AtivarDialogo(Id_Dialogo); // Diálogo de abrir a porta
                
                if (portaObjetoFisico != null)
                {
                    // Abre a porta (podes destruir ou desativar o colisor/objeto)
                    Destroy(portaObjetoFisico); 
                }

                // Adiciona a própria porta como Pista Concluída (Pista 7)
                if (billboard != null)
                {
                    billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);
                }

                jaFoiRegistado = true;
                AoOlharSair();
                Destroy(gameObject); // Some o trigger de interação da porta
            }
            else
            {
                // Diálogo de que a porta está trancada e precisa de chave
                Debug.Log("A porta está trancada por dentro.");
            }
            return;
        }

        // ==========================================
        // COLETA E TRATAMENTO DE PISTAS NORMAIS
        // ==========================================
        if (DM != null) DM.AtivarDialogo(Id_Dialogo);

        if (billboard != null)
        {
            billboard.AdicionarPistaAoQuadro(imagemDoItem, nomeDaPista, descricaoDaPista, numeroFixoDaPista);
        }

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