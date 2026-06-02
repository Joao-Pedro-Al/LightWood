using UnityEngine;

public class PlayerInteracao : MonoBehaviour
{
    [Header("Configurações do Raio")]
    public float distanciaDoRaio = 6f; 
    public LayerMask layerDasPistas;  

    [Header("Interface UI")]
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
            CliqueItem itemDetetado = hit.collider.GetComponent<CliqueItem>();

            // Só interage se o script existir e estiver ATIVO (enabled)
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