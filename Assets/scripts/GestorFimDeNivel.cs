using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GestorFimDeNivel : MonoBehaviour
{
    [Header("Configurações do Nível")]
    [Tooltip("O número total de pistas que o jogador precisa de encontrar neste nível.")]
    public int totalDePistasNoNivel = 8; // Altera para 7, 8 ou o teu total de pistas

    [Header("Cena de Destino")]
    [Tooltip("O nome exato da tua cena de créditos (como está nas Build Settings).")]
    public string nomeDaCenaCreditos = "Creditos";

    private BillboardManager billboard;
    private bool nivelFinalizado = false;

    void Start()
    {
        // Encontra o BillboardManager automaticamente na cena
        billboard = FindFirstObjectByType<BillboardManager>();

        if (billboard == null)
        {
            Debug.LogError("GestorFimDeNivel: Não foi encontrado o BillboardManager na cena!");
            return;
        }

        // Inicia a verificação em segundo plano para poupar processamento (roda a cada 1 segundo em vez de cada frame)
        StartCoroutine(RotinaVerificarPistas());
    }

    IEnumerator RotinaVerificarPistas()
    {
        while (!nivelFinalizado)
        {
            if (billboard != null)
            {
                // Usamos reflexão em C# para ler a lista privada 'cartoesInstanciados' do teu BillboardManager
                // Assim não precisas de alterar absolutamente nada no teu código antigo!
                var campoLista = typeof(BillboardManager).GetField("cartoesInstanciados",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (campoLista != null)
                {
                    // Obtém a lista e extrai a contagem de itens dentro dela
                    var lista = campoLista.GetValue(billboard) as System.Collections.IList;
                    int pistasAtuais = (lista != null) ? lista.Count : 0;

                    // Se apanhou todas as pistas, ativa o temporizador de 1 minuto
                    if (pistasAtuais >= totalDePistasNoNivel)
                    {
                        nivelFinalizado = true;
                        StartCoroutine(TemporizadorMudarDeCena());
                        yield break; // Para o Loop de verificação
                    }
                }
            }

            // Espera 1 segundo antes de verificar novamente (ótimo para performance)
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator TemporizadorMudarDeCena()
    {
        Debug.Log("Todas as pistas encontradas! O nível vai terminar em 60 segundos...");

        // Espera exatamente 60 segundos (1 minuto)
        yield return new WaitForSeconds(15f);

        Debug.Log("A carregar a cena de créditos: " + nomeDaCenaCreditos);

        // Carrega a cena dos créditos
        SceneManager.LoadScene(nomeDaCenaCreditos);
    }
}