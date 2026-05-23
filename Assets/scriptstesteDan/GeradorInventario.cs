using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GeradorInventario : MonoBehaviour
{
    public GameObject painelInventario; 
    public GameObject slotItemPrefab;   
    
    // NOVO: Aqui vais arrastar o teu objeto Player que tem o novo script!
    public GameObject jogadorPlayer; 

    private bool inventarioAberto = false;
    private List<string> itensNaMochila = new List<string>();

    void Start()
    {
        if (painelInventario != null) painelInventario.SetActive(false);
        if (slotItemPrefab != null) slotItemPrefab.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventarioAberto = !inventarioAberto;
            painelInventario.SetActive(inventarioAberto);

            if (jogadorPlayer != null)
            {
                // Procura pelo teu novo script no jogador
                Player_Teste_Alves scriptNovo = jogadorPlayer.GetComponent<Player_Teste_Alves>();
                
                if (scriptNovo != null)
                {
                    if (inventarioAberto)
                    {
                        scriptNovo.cameraTravada = true; // TRAVA O TEU NOVO SCRIPT
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        Time.timeScale = 0f; // Congela o mundo
                    }
                    else
                    {
                        scriptNovo.cameraTravada = false; // LIBERTA O TEU NOVO SCRIPT
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        Time.timeScale = 1f; // Descongela o mundo
                    }
                }
            }
        }
    }

    public void AdicionarAoInventario(string nomeDoObjeto)
    {
        itensNaMochila.Add(nomeDoObjeto);
        
        GameObject novoSlot = Instantiate(slotItemPrefab, painelInventario.transform);
        novoSlot.SetActive(true);

        Image imagemDoSlot = novoSlot.GetComponent<Image>();
        Sprite fotoDoItem = Resources.Load<Sprite>(nomeDoObjeto + "UI");

        if (imagemDoSlot != null && fotoDoItem != null)
        {
            imagemDoSlot.sprite = fotoDoItem;
        }
    }
}