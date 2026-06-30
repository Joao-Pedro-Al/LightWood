using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class Pausa_Ecra : MonoBehaviour
{
    public static Pausa_Ecra Instance;

    [Header("Overlay")]
    [SerializeField] private GameObject Overlay;
    [SerializeField] private TextMeshProUGUI Objetivo;
    [SerializeField] private GameObject Notificacao;
[SerializeField] private TextMeshProUGUI TextoSalvarFixo; // NOVO:texto fixo

    [Header("Valores Externos")]
    public Player_Teste_Alves ScriptPlayer;
    public Dialogo ScriptDialogo;

    [Header("Valores Locais")]
    private bool Pausado = false;
    private Coroutine ANotificar;

    void Awake()
    {
        if(Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        } else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        AtualizarObjetivo("Investiga a cena de crime.");
        // NOVO: Define a frase fixa para salvar o jogo no início
        if (TextoSalvarFixo != null)
        {
            TextoSalvarFixo.text = "Para salvar va ao butao do buildboard apos apanhar um pista";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ScriptPlayer != null)
            {
                if (!Pausado)
                {
                    Pausado = true;

                    if(Notificacao.activeInHierarchy && ANotificar != null)
                    {
                        StopCoroutine(ANotificar);
                        Notificacao.SetActive(false);
                    }

                    // Limpa os eixos do rato mesmo antes de travar para evitar o "salto" da visão
                    Input.ResetInputAxes(); 
                    ScriptPlayer.cameraTravada = true;
                    
                    Time.timeScale = 0f; // Congela o tempo do mundo

                    Overlay.SetActive(true);
                }
                // else
                // {
                //     Resumir();
                // }
            }
        }
    }

    public void AtualizarObjetivo(string NovoObjetivo)
    {
        Objetivo.text = NovoObjetivo;
        ANotificar = StartCoroutine(AvisarObjetivo());
    }

    private IEnumerator AvisarObjetivo()
    {
        Notificacao.SetActive(true);

        yield return new WaitForSeconds(4f);

        Notificacao.SetActive(false);
    }

    public void Resumir()
    {
        Pausado = false;

        ScriptPlayer.cameraTravada = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Time.timeScale = 1f; // Descongela o mundo

        Overlay.SetActive(false);
    }

    public void Sair()
    {
        ScriptDialogo.Destruir(); // Destruir o GameObject com o Diálogo
        Time.timeScale = 1f; // Descongela o mundo
        SceneManager.LoadScene("MenuInicial");
        Destruir_MenuPausa();
    }

    public void Destruir_MenuPausa()
    {
        Destroy(gameObject);
    }
}