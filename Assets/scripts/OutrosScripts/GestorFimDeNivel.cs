using UnityEngine;
using UnityEngine.UI;
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
    private string CenaAtual; // A cena que está aberta no momento
    public string nomeDoSegundoNivel = "Nivel2";
    public string nomeDaCenaCreditos = "Creditos";

    [Header("Player")]
    [SerializeField] private Image FadeIn_Out;

    private BillboardManager billboard;
    private bool nivelFinalizado = false;

    void Start()
    {
        // Acede ao Sistema de Diálogo
        DM = Dialogo.Instance;

        // Acede ao Objetivo no Menu de Pausa
        PE = Pausa_Ecra.Instance;

        // Buscar o Nome da Cena Atual
        Scene Cena = SceneManager.GetActiveScene();
        CenaAtual = Cena.name;

        // Encontra o BillboardManager automaticamente na cena
        billboard = FindFirstObjectByType<BillboardManager>();

        if (billboard == null)
        {
            Debug.LogError("GestorFimDeNivel: N�o foi encontrado o BillboardManager na cena!");
            return;
        }

        // Inicia a verifica��o em segundo plano para poupar processamento (roda a cada 1 segundo em vez de cada frame)
        StartCoroutine(RotinaVerificarPistas());

        StartCoroutine(FadeIn(true));
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
        Debug.Log("Todas as pistas encontradas! O nível vai terminar em 60 segundos...");

        // Verificar qual é a cena atual e definir qual deve ser aberta
        string ProxCena = null;
        switch(CenaAtual)
        {
            case "Nivel1":
                ProxCena = nomeDoSegundoNivel;
                break;
            case "Nivel2":
                ProxCena = nomeDaCenaCreditos;
                break;
            default:
                ProxCena = nomeDaCenaCreditos;
                break;
        }

        Debug.Log("A carregar a próxima cena: " + ProxCena);

        // Espera exatamente 60 segundos (1 minuto)
        yield return new WaitForSeconds(5f);

        yield return StartCoroutine(FadeIn(false));
        
        DM.Destruir();
        PE.Destruir_MenuPausa();

        // Carrega a cena dos cr�ditos
        SceneManager.LoadScene(ProxCena);
    }

    private IEnumerator FadeIn(bool s)
    {
        float timer = 0f;
        float duracao = 1.2f;
        float NovoAlpha = 255;
        int Alpha = 255;
        int Target = 0;

        if(s)
        {
            Alpha = 255;
            Target = 0;
        }
        else
        {
            Alpha = 0;
            Target = 255;
        }
        while (timer < duracao) // Durante o tempo estimado em Duração
        {
            NovoAlpha = Mathf.Lerp(Alpha, Target, timer / duracao); // Gradualmente altera o Valor da Velocidade_Idle para o valor inserido durante o while inteiro
            FadeIn_Out.color = new Color32(0, 0, 0, (byte)NovoAlpha);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        FadeIn_Out.color = new Color32(0, 0, 0, (byte)NovoAlpha);
    }
}