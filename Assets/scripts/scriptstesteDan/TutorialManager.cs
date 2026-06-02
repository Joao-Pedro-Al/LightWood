using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Textos do Tutorial (Arraste os objetos da UI aqui)")]
    [SerializeField] private GameObject movimentoTexto;
    [SerializeField] private GameObject corridaTexto;
    [SerializeField] private GameObject lanternaTexto;

    // Controla em qual passo do tutorial o jogador está
    // 0 = Movimento, 1 = Corrida, 2 = Lanterna, 3 = Fim
    private int tutorialStep = 0;

    // Variáveis para registar as teclas de movimento
    private bool moveuW = false;
    private bool moveuA = false;
    private bool moveuS = false;
    private bool moveuD = false;

    void Start()
    {
        // No início, APENAS o texto de movimento fica ativo
        if (movimentoTexto != null) movimentoTexto.SetActive(true);
        if (corridaTexto != null) corridaTexto.SetActive(false);
        if (lanternaTexto != null) lanternaTexto.SetActive(false);
    }

    void Update()
    {
        switch (tutorialStep)
        {
            case 0: // PASSO 0: Movimento (W, A, S, D)
                if (movimentoTexto != null)
                {
                    if (Input.GetKeyDown(KeyCode.W)) moveuW = true;
                    if (Input.GetKeyDown(KeyCode.A)) moveuA = true;
                    if (Input.GetKeyDown(KeyCode.S)) moveuS = true;
                    if (Input.GetKeyDown(KeyCode.D)) moveuD = true;

                    // Se carregou nas 4 teclas, avança
                    if (moveuW && moveuA && moveuS && moveuD)
                    {
                        movimentoTexto.SetActive(false); // Esconde o atual
                        
                        if (corridaTexto != null) 
                        {
                            corridaTexto.SetActive(true); // Mostra o próximo
                            tutorialStep = 1;             // Avança a fase
                        }
                        else
                        {
                            tutorialStep = 2; // Se não houver texto de corrida, pula para a lanterna
                        }
                    }
                }
                break;

            case 1: // PASSO 1: Corrida (Shift)
                if (corridaTexto != null)
                {
                    if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                    {
                        corridaTexto.SetActive(false); // Esconde o atual
                        
                        if (lanternaTexto != null)
                        {
                            lanternaTexto.SetActive(true); // Mostra o próximo
                            tutorialStep = 2;              // Avança a fase
                        }
                        else
                        {
                            tutorialStep = 3; // Fim do tutorial
                        }
                    }
                }
                break;

            case 2: // PASSO 2: Lanterna (F)
                if (lanternaTexto != null)
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        lanternaTexto.SetActive(false); // Esconde o último texto
                        tutorialStep = 3;               // Fim do tutorial
                    }
                }
                break;

            case 3: // PASSO 3: Tutorial Concluído
                Debug.Log("Tutorial concluído com sucesso!");
                this.enabled = false; // Desativa o script para poupar memória
                break;
        }
    }
}