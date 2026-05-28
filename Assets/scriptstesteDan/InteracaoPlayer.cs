using UnityEngine;

public class PlayerInteracao : MonoBehaviour
{
    [Header("Configurações do Raio")]
    public float distanciaDoRaio = 6f; 
    public LayerMask layerDasPistas;  

    [Header("Interface UI")]
    public GameObject miraUI; 

    [Header("Tecla do Quadro/Inventário")]
    public KeyCode teclaVerQuadro = KeyCode.E; // Carrega em E para abrir ou fechar o quadro de pistas

    private Camera cam;
    private CliqueItem itemSendoOlhado;
    private BillboardManager billboard;
    private bool quadroAberto = false;

    void Start()
    {
        cam = Camera.main;
        billboard = FindObjectOfType<BillboardManager>();
        
        if (miraUI) miraUI.SetActive(false);
        
        // Garante que o jogo começa com o rato escondido e focado
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Deteta se o jogador quer abrir ou fechar o quadro de pistas manuamente
        if (Input.GetKeyDown(teclaVerQuadro))
        {
            ToggleQuadro();
        }

        // Se o quadro estiver aberto, o jogador está a mexer no inventário, logo não faz Raycast no chão
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
            CliqueItem itemDetetado = hit.collider.GetComponent<CliqueItem>();

            if (itemDetetado != null)
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

                // CLIQUE INFALÍVEL
                if (Input.GetMouseButtonDown(0))
                {
                    CliqueItem itemParaColetar = itemSendoOlhado;
                    LimparVisao(); // Esconde a mira no exato frame da recolha
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
        if (billboard == null) return;

        quadroAberto = !quadroAberto;

        if (quadroAberto)
        {
            LimparVisao();
            billboard.AbrirQuadro();
        }
        else
        {
            billboard.FecharQuadro();
        }
    }
}