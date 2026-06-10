using UnityEngine;
using System.Collections;

public class GeradorBaterias : MonoBehaviour
{
    [Header("Configurações do Spawn")]
    [Tooltip("O Prefab da bateria 3D (com o teu script CliqueItem anexado).")]
    public GameObject prefabBateria;

    [Tooltip("O Transform da tua fogueira no cenário.")]
    public Transform posicaoFogueira;

    [Tooltip("Raio máximo de distância a partir da fogueira para espalhar as baterias.")]
    public float raioMaximoFogueira = 60f;

    [Header("Configurações de Proximidade")]
    [Tooltip("Distância máxima permitida entre a bateria e o jogador (Alterado para 5 metros).")]
    public float distanciaMaxPlayer = 5f;

    private Transform playerTransform;
    private int bateriasGeradasContador = 0;
    private const int LIMITE_TOTAL_BATERIAS = 6; // O número total de baterias que queremos gerar no nível

    private GameObject bateriaAtualNoCenario;
    private bool aguardandoColeta = false;

    void Start()
    {
        // Encontra o teu jogador automaticamente pelo nome exacto
        GameObject playerObj = GameObject.Find("Player_Teste_Alves");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("GeradorBaterias: Não foi possível encontrar o 'Player_Teste_Alves' na cena!");
        }

        if (posicaoFogueira == null)
        {
            Debug.LogError("GeradorBaterias: Por favor, arrasta o objeto da Fogueira para o Inspector!");
            return;
        }

        // Inicia o ciclo gerando a primeira bateria perto do jogador
        GerarProximaBateria();
    }

    void Update()
    {
        // Deteta automaticamente quando a bateria atual foi coletada (destruída pelo teu CliqueItem)
        if (aguardandoColeta && bateriaAtualNoCenario == null)
        {
            aguardandoColeta = false;
            GerarProximaBateria();
        }
    }

    void GerarProximaBateria()
    {
        // Se já gerou e coletou as 7 baterias, para o sistema
        if (bateriasGeradasContador >= LIMITE_TOTAL_BATERIAS)
        {
            Debug.Log("Todas as 7 baterias do nível foram coletadas com sucesso!");
            return;
        }

        if (prefabBateria == null)
        {
            Debug.LogError("GeradorBaterias: O Prefab da Bateria não foi associado no Inspector!");
            return;
        }

        Vector3 posicaoValida = EncontrarPosicaoValida();

        // Cria a bateria na cena com rotação padrão
        bateriaAtualNoCenario = Instantiate(prefabBateria, posicaoValida, Quaternion.identity);
        
        bateriasGeradasContador++;
        aguardandoColeta = true;

        Debug.Log($"Bateria {bateriasGeradasContador}/{LIMITE_TOTAL_BATERIAS} gerada a 5m do player na posição: {posicaoValida}");
    }

    Vector3 EncontrarPosicaoValida()
    {
        Vector3 pontoSorteado = Vector3.zero;
        bool pontoEncontrado = false;
        int tentativas = 0;
        int maxTentativas = 150;

        // Tenta encontrar um ponto perfeito que respeite a fogueira E os 5 metros do jogador
        while (!pontoEncontrado && tentativas < maxTentativas)
        {
            tentativas++;

            // 1. Sorteia um ponto dentro do raio da fogueira
            Vector2 offsetFogueira = Random.insideUnitCircle * raioMaximoFogueira;
            pontoSorteado = new Vector3(
                posicaoFogueira.position.x + offsetFogueira.x,
                posicaoFogueira.position.y + 50f, // Altura segura para o raio descer
                posicaoFogueira.position.z + offsetFogueira.y
            );

            // 2. Cola o ponto ao chão do terreno usando Física (Raycast)
            if (Physics.Raycast(pontoSorteado, Vector3.down, out RaycastHit hit, 150f))
            {
                pontoSorteado.y = hit.point.y + 0.15f; // Evita que a bateria fique enterrada na relva
            }
            else
            {
                pontoSorteado.y = posicaoFogueira.position.y + 0.15f;
            }

            // 3. Validação dos 5 metros em relação ao Player_Teste_Alves
            if (playerTransform != null)
            {
                float distanciaAoPlayer = Vector3.Distance(pontoSorteado, playerTransform.position);
                
                // Se estiver dentro do limite dos 5 metros, aceita a posição
                if (distanciaAoPlayer <= distanciaMaxPlayer)
                {
                    pontoEncontrado = true;
                }
            }
            else
            {
                pontoEncontrado = true; 
            }
        }

        
        if (!pontoEncontrado && playerTransform != null)
        {
            Vector3 pontoFrentePlayer = playerTransform.position + playerTransform.forward * 3f;
            
            // Ajusta a altura deste ponto de emergência ao terreno
            if (Physics.Raycast(new Vector3(pontoFrentePlayer.x, pontoFrentePlayer.y + 20f, pontoFrentePlayer.z), Vector3.down, out RaycastHit hitEmergencia, 50f))
            {
                pontoFrentePlayer.y = hitEmergencia.point.y + 0.15f;
            }
            pontoSorteado = pontoFrentePlayer;
            Debug.LogWarning("GeradorBaterias: Ponto ideal não encontrado no raio da fogueira. Gerada bateria de segurança perto do jogador.");
        }

        return pontoSorteado;
    }
}