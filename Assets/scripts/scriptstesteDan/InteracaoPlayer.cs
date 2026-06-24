using UnityEngine;
using System.Collections;

public class PlayerInteracao : MonoBehaviour
{
    [Header("Configurações do Raio")]
    public float distanciaDoRaio = 6f;
    public LayerMask layerDasPistas;

    [Header("Interface UI (A Bola da Mira)")]
    [Tooltip("Coloca aqui o objeto UI da tua mira (a bola) do Canvas.")]
    public GameObject miraUI;

    [Header("Tecla do Quadro/Inventário")]
    public KeyCode teclaVerQuadro = KeyCode.E;

    [Header("Lanterna e Monstro")]
    [Tooltip("Arrasta aqui o script Flashlight do jogador.")]
    public Flashlight flashlightScript;
    [Tooltip("Tag do CliqueItem que representa a lanterna no chão.")]
    public string tagLanterna = "Lanterna";
    [Tooltip("Arrasta aqui o GameObject do monstro (deve estar desativado no início).")]
    public GameObject monstro;

    private Camera cam;
    private CliqueItem itemSendoOlhado;
    private BillboardManager billboardAtivo;
    private bool quadroAberto = false;

    void Start()
    {
        cam = Camera.main;
        //billboard = FindObjectOfType<BillboardManager>();

        // Mantém a mira base visível para o jogador saber para onde aponta,
        // mas garante que ela começa no estado normal/desativada do brilho.
        if (miraUI) miraUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaVerQuadro))
        {
            ToggleQuadro();
        }

        if (quadroAberto)
        {
            if (miraUI && miraUI.activeSelf) miraUI.SetActive(false);
            return;
        }

        if (cam == null) return;

        Ray raio = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(raio, out hit, distanciaDoRaio, layerDasPistas))
        {
            CliqueItem itemDetetado = hit.transform.GetComponent<CliqueItem>();

            if (itemDetetado != null && itemDetetado.enabled)
            {
                if (itemSendoOlhado != null && itemSendoOlhado != itemDetetado)
                {
                    itemSendoOlhado.AoOlharSair();
                }

                itemSendoOlhado = itemDetetado;
                itemSendoOlhado.AoOlharEntrar();

                // ==========================================================
                // PRIMEIRA ALTERAÇÃO: Ativa a bola da mira ao olhar para o item
                // ==========================================================
                if (miraUI && !miraUI.activeSelf)
                {
                    miraUI.SetActive(true);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    CliqueItem itemParaColetar = itemSendoOlhado;
                    LimparVisao();

                    // Verifica se o item coletado é a lanterna
                    if (itemParaColetar.CompareTag(tagLanterna) && flashlightScript != null)
                    {
                        flashlightScript.ColetarLanterna();

                        // Spana o monstro e inicia a Fase 1 (adiado um frame para o Start() terminar)
                        if (monstro != null)
                        {
                            monstro.SetActive(true);
                            StartCoroutine(AtivarMonstro());
                        }
                    }

                    itemParaColetar.ColetarPista();
                    return;
                }
            }
            else
            {
                LimparVisao();
            }
        }
        else
        {
            LimparVisao();
        }
    }

    void LimparVisao()
    {
        if (itemSendoOlhado != null)
        {
            itemSendoOlhado.AoOlharSair();
            itemSendoOlhado = null;
        }

        // Quando não olhas para nada interativo, a bola da mira desativa-se
        if (miraUI && miraUI.activeSelf)
        {
            miraUI.SetActive(false);
        }
    }

    IEnumerator AtivarMonstro()
    {
        yield return null; // espera um frame para o Start() do MonsterAI terminar
        MonsterAI ai = monstro.GetComponent<MonsterAI>();
        if (ai != null) ai.Activate();
        else Debug.LogWarning("[Player] MonsterAI não encontrado no monstro!");
    }

    void ToggleQuadro()
    {
        // Encontra dinamicamente qual o Billboard do local mais próximo/ativo para não abrir o quadro errado
        BillboardManager[] quadros = FindObjectsOfType<BillboardManager>();
        if (quadros.Length == 0) return;

        // Se houver mais de um quadro, pega o mais perto do jogador
        BillboardManager quadroMaisProximo = quadros[0];
        float menorDistancia = Mathf.Infinity;
        foreach(var q in quadros)
        {
            float dist = Vector3.Distance(transform.position, q.transform.position);
            if(dist < menorDistancia)
            {
                menorDistancia = dist;
                quadroMaisProximo = q;
            }
        }
        
        billboardAtivo = quadroMaisProximo;
        if (billboardAtivo == null || billboardAtivo.painelFundoPreto == null) return;

        quadroAberto = !quadroAberto;
        billboardAtivo.painelFundoPreto.SetActive(quadroAberto);

        if (quadroAberto)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}