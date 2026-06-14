using UnityEngine;

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

    private Camera cam;
    private CliqueItem itemSendoOlhado;
    private BillboardManager billboard;
    private bool quadroAberto = false;

    void Start()
    {
        cam = Camera.main;
        billboard = FindObjectOfType<BillboardManager>();
        
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

        // Executa o raio para detetar os objetos na Layer correta
        if (Physics.Raycast(raio, out hit, distanciaDoRaio, layerDasPistas))
        {
            CliqueItem itemDetetado = hit.transform.GetComponent<CliqueItem>();

            // Se encontrou um item interativo e o script dele está ativo
            if (itemDetetado != null && itemDetetado.enabled)
            {
                if (itemSendoOlhado != null && itemSendoOlhado != itemDetetado)
                {
                    itemSendoOlhado.AoOlharSair();
                }

                // Guarda o item e ativa o Brilho 3D no próprio modelo
                itemSendoOlhado = itemDetetado;
                itemSendoOlhado.AoOlharEntrar();

                // ==========================================================
                // PRIMEIRA ALTERAÇÃO: Ativa a bola da mira ao olhar para o item
                // ==========================================================
                if (miraUI && !miraUI.activeSelf) 
                {
                    miraUI.SetActive(true);
                }

                // Se o jogador clicar com o botão esquerdo
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

    // Função que limpa o brilho do objeto e esconde/reseta a bola da mira
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

    void ToggleQuadro()
    {
        if (billboard == null) return;

        quadroAberto = !quadroAberto;
        billboard.painelFundoPreto.SetActive(quadroAberto);

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