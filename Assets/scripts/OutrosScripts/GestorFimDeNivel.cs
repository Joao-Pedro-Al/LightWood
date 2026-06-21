using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GestorFimDeNivel : MonoBehaviour
{
    [Header("Dialogo")]
    private Dialogo DM;

    [Header("Objetivos")]
    private Pausa_Ecra PE;

    [Header("Configura��es do N�vel")]
    [Tooltip("O n�mero total de pistas que o jogador precisa de encontrar neste n�vel.")]
    public int totalDePistasNoNivel = 8; // Altera para 7, 8 ou o teu total de pistas

    [Header("Cena de Destino")]
    [Tooltip("O nome exato da tua cena de cr�ditos (como est� nas Build Settings).")]
    public string nomeDaCenaCreditos = "Creditos";

    private BillboardManager billboard;
    private bool nivelFinalizado = false;

    void Start()
    {
        // Acede ao Sistema de Diálogo
        DM = Dialogo.Instance;

        // Acede ao Objetivo no Menu de Pausa
        PE = Pausa_Ecra.Instance;

        // Encontra o BillboardManager automaticamente na cena
        billboard = FindFirstObjectByType<BillboardManager>();

        if (billboard == null)
        {
            Debug.LogError("GestorFimDeNivel: N�o foi encontrado o BillboardManager na cena!");
            return;
        }

        // Inicia a verifica��o em segundo plano para poupar processamento (roda a cada 1 segundo em vez de cada frame)
        StartCoroutine(RotinaVerificarPistas());
    }

    IEnumerator RotinaVerificarPistas()
    {
        while (!nivelFinalizado)
        {
            if (billboard != null)
            {
                // Usamos reflex�o em C# para ler a lista privada 'cartoesInstanciados' do teu BillboardManager
                // Assim n�o precisas de alterar absolutamente nada no teu c�digo antigo!
                var campoLista = typeof(BillboardManager).GetField("cartoesInstanciados",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (campoLista != null)
                {
                    // Obt�m a lista e extrai a contagem de itens dentro dela
                    var lista = campoLista.GetValue(billboard) as System.Collections.IList;
                    int pistasAtuais = (lista != null) ? lista.Count : 0;

                    // Se apanhou todas as pistas, ativa o temporizador de 1 minuto
                    if (pistasAtuais >= totalDePistasNoNivel)
                    {
                        nivelFinalizado = true;
                        PE.AtualizarObjetivo("Analisa as pistas no Billboard.");
                        StartCoroutine(TemporizadorMudarDeCena());
                        yield break; // Para o Loop de verifica��o
                    }
                }
            }

            // Espera 1 segundo antes de verificar novamente (�timo para performance)
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator TemporizadorMudarDeCena()
    {
        Debug.Log("Todas as pistas encontradas! O n�vel vai terminar em 60 segundos...");

        // Espera exatamente 60 segundos (1 minuto)
        yield return new WaitForSeconds(15f);

        Debug.Log("A carregar a cena de cr�ditos: " + nomeDaCenaCreditos);

        
        DM.Destruir();
        PE.Destruir_MenuPausa();

        // Carrega a cena dos cr�ditos
        SceneManager.LoadScene(nomeDaCenaCreditos);
    }
}