using UnityEngine;
using UnityEngine.UI;

public class PlayerInteracao : MonoBehaviour
{
    public float distanciaInteracao = 3.5f; 
    public Image miraUI;                   

    private CliqueItem itemSendoFocado;

    void Start()
    {
        if (miraUI != null) miraUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Cursor.visible) return;

        Ray raio = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(raio, out hit, distanciaInteracao))
        {
            // SOLUÇÃO AQUI: Procura o script no objeto onde o raio bateu OU em qualquer PAI acima dele!
            CliqueItem itemDetectado = hit.collider.GetComponentInParent<CliqueItem>();

            if (itemDetectado != null)
            {
                if (itemSendoFocado != itemDetectado)
                {
                    if (itemSendoFocado != null) itemSendoFocado.AoOlharSair();
                    
                    itemSendoFocado = itemDetectado;
                    itemSendoFocado.AoOlharEntrar(); 
                    
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
            itemSendoFocado.AoOlharSair(); 
            itemSendoFocado = null;
        }
        
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