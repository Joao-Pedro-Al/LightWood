using UnityEngine;

public class PlayerInteracao : MonoBehaviour
{
    [Header("Configurações do Raio")]
    public float distanciaDoRaio = 6f;
    public LayerMask layerDasPistas;

    [Header("Interface UI (A Bola da Mira)")]
    public GameObject miraUI;

    [Header("Tecla do Quadro/Inventário")]
    public KeyCode teclaVerQuadro = KeyCode.E;

    private Camera cam;
    private CliqueItem itemSendoOlhado;
    private BillboardManager billboardAtivo;
    private bool quadroAberto = false;

    void Start()
    {
        cam = Camera.main;
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

                if (miraUI && !miraUI.activeSelf)
                {
                    miraUI.SetActive(true);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    CliqueItem itemParaColetar = itemSendoOlhado;
                    LimparVisao();
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

        if (miraUI && miraUI.activeSelf)
        {
            miraUI.SetActive(false);
        }
    }

    void ToggleQuadro()
    {
        // Encontra dinamicamente qual o Billboard do local mais próximo/ativo para não abrir o quadro errado
        BillboardManager[] quadros = FindObjectsOfType<BillboardManager>();
        if (quadros.Length == 0) return;

        // Se houver mais de um quadro, pega o mais perto do jogador
        BillboardManager quadroMaisProximo = quadros[0];
        float menorDistancia = Mathf.Infinity;
        foreach (var q in quadros)
        {
            float dist = Vector3.Distance(transform.position, q.transform.position);
            if (dist < menorDistancia)
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