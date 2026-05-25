using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GeradorInventario : MonoBehaviour
{
    public GameObject painelInventario; 
    public GameObject slotItemPrefab;   
    public GameObject jogadorPlayer; // O objeto que tem o script Player_Teste_Alves

    private bool inventarioAberto = false;
    
    // Lista atualizada para guardar os Sprites recolhidos
    private List<Sprite> itensNaMochila = new List<Sprite>(); 

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
                Player_Teste_Alves scriptNovo = jogadorPlayer.GetComponent<Player_Teste_Alves>();
                
                if (scriptNovo != null)
                {
                    if (inventarioAberto)
                    {
                        // Limpa os eixos do rato mesmo antes de travar para evitar o "salto" da visão
                        Input.ResetInputAxes(); 
                        scriptNovo.cameraTravada = true; 
                        
                        Time.timeScale = 0f; // Congela o tempo do mundo
                    }
                    else
                    {
                        scriptNovo.cameraTravada = false; 
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        
                        Time.timeScale = 1f; // Descongela o mundo
                    }
                }
            }
        }
    }

    // Função modificada: agora recebe diretamente o Sprite do objeto apanhado
    public void AdicionarAoInventario(Sprite fotoDoObjeto)
    {
        if (fotoDoObjeto == null)
        {
            Debug.LogWarning("Aviso: O objeto que apanhaste não tem nenhuma imagem atribuída no Inspector!");
            return;
        }

        itensNaMochila.Add(fotoDoObjeto);
        
        GameObject novoSlot = Instantiate(slotItemPrefab, painelInventario.transform);
        novoSlot.SetActive(true);

        Image imagemDoSlot = novoSlot.GetComponent<Image>();
        if (imagemDoSlot != null)
        {
            // Aplica a foto que veio diretamente do objeto
            imagemDoSlot.sprite = fotoDoObjeto; 
        }
    }
}