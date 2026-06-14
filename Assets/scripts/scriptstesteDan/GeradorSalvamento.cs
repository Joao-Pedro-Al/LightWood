using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 

public class GeradorSalvamento : MonoBehaviour
{
    public static GeradorSalvamento Instance { get; private set; }

    [System.Serializable]
    public struct DadosPistaSalva
    {
        public Sprite foto;
        public string nome;
        public string descricao;
        public int numero;
    }

    public List<DadosPistaSalva> pistasSalvasPermanentes = new List<DadosPistaSalva>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Subscreve ao evento de carregamento de cenas do Unity
            SceneManager.sceneLoaded += AoCarregarNovaCena;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= AoCarregarNovaCena;
    }

    private void AoCarregarNovaCena(Scene cena, LoadSceneMode modo)
    {
        if (cena.name == "MenuInicial" || cena.name == "MenuInicial") 
        {
            LimparDadosDeSalvamento();
        }
    }

    public void LimparDadosDeSalvamento()
    {
        pistasSalvasPermanentes.Clear();
        Debug.Log("Salvo");
    }
}