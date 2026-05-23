using UnityEngine;
using UnityEngine.UI;

public class PlayerInteracao : MonoBehaviour
{
    public float distanciaInteracao = 3.5f; 
    public Image miraUI;                   

    private CliqueItem itemSendoFocado;

    void Start()
    {
        // Garante que começa totalmente desligada
        if (miraUI != null) miraUI.gameObject.SetActive(false);
    }

    void Update()
    {
        // Se o inventário estiver aberto, não faz sentido procurar itens
        if (Cursor.visible) return;

        Ray raio = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(raio, out hit, distanciaInteracao))
        {
            CliqueItem itemDetectado = hit.collider.GetComponent<CliqueItem>();

            if (itemDetectado != null)
            {
                if (itemSendoFocado != itemDetectado)
                {
                    if (itemSendoFocado != null) itemSendoFocado.AoOlharSair();
                    
                    itemSendoFocado = itemDetectado;
                    itemSendoFocado.AoOlharEntrar(); // Ativa o brilho no objeto
                    
                    // LIGA A MIRA APENAS AQUI
                    if (miraUI != null)
                    {
                        miraUI.gameObject.SetActive(true);
                        miraUI.color = Color.green;
                        miraUI.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    }
                }
            }
            else
            {
                LimparFoco();
            }
        }
        else
        {
            LimparFoco();
        }
    }

    void LimparFoco()
    {
        if (itemSendoFocado != null)
        {
            itemSendoFocado.AoOlharSair(); // Desliga o brilho no objeto
            itemSendoFocado = null;
        }
        
        // DESLIGA A MIRA COMPLEMENTAMENTE
        if (miraUI != null)
        {
            miraUI.gameObject.SetActive(false);
        }
    }

    public void ForcarResetMira()
    {
        itemSendoFocado = null;
        if (miraUI != null)
        {
            miraUI.gameObject.SetActive(false);
        }
    }
}