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
        public int idNivel;
    }

    public List<DadosPistaSalva> pistasSalvasPermanentes = new List<DadosPistaSalva>();

    // Guarda se o jogador já tinha a lanterna
    public bool flashlightApanhada = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        if (cena.name == "MenuInicial")
        {
            LimparDadosDeSalvamento();
            return;
        }

        // Restaura o estado após recarregar a cena (morte)
        // Usa um pequeno delay para garantir que todos os Start() já correram
        StartCoroutine(RestaurarEstadoAposCarregamento());
    }

    private System.Collections.IEnumerator RestaurarEstadoAposCarregamento()
    {
        // Espera dois frames para todos os objetos da cena estarem prontos
        yield return null;
        yield return null;

        // 1. Restaurar a lanterna
        if (flashlightApanhada && Flashlight.Instance != null)
        {
            Flashlight.Instance.CollectFlashlight();
            Debug.Log("[Salvamento] 🔦 Lanterna restaurada.");
        }

        // 2. Restaurar pistas no quadro
        if (pistasSalvasPermanentes.Count > 0)
        {
            BillboardManager[] billboards = FindObjectsOfType<BillboardManager>();

            foreach (var pista in pistasSalvasPermanentes)
            {
                foreach (var board in billboards)
                {
                    if (board.idNivel == pista.idNivel)
                    {
                        board.AdicionarPistaAoQuadro(pista.foto, pista.nome, pista.descricao, pista.numero);
                        break;
                    }
                }
            }

            Debug.Log($"[Salvamento] 📋 {pistasSalvasPermanentes.Count} pistas restauradas no quadro.");
        }
    }

    // Chamado pelo BillboardManager quando adiciona uma pista
    public void GuardarPista(Sprite foto, string nome, string descricao, int numero, int idNivel)
    {
        // Evita duplicados
        bool jaExiste = pistasSalvasPermanentes.Exists(p => p.numero == numero && p.idNivel == idNivel);
        if (jaExiste) return;

        pistasSalvasPermanentes.Add(new DadosPistaSalva
        {
            foto = foto,
            nome = nome,
            descricao = descricao,
            numero = numero,
            idNivel = idNivel
        });

        Debug.Log($"[Salvamento] 💾 Pista {numero} (nível {idNivel}) guardada.");
    }

    // Chamado quando o jogador apanha a lanterna
    public void GuardarFlashlight()
    {
        flashlightApanhada = true;
        Debug.Log("[Salvamento] 💾 Flashlight guardada.");
    }

    public void LimparDadosDeSalvamento()
    {
        pistasSalvasPermanentes.Clear();
        flashlightApanhada = false;
        Debug.Log("[Salvamento] 🗑️ Dados de salvamento limpos.");
    }
}