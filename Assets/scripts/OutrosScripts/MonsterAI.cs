using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    public enum MonsterState { Hidden, Phase1, Phase2 }
    public MonsterState currentState = MonsterState.Hidden;

    [Header("Referências")]
    public Transform player;
    public Flashlight flashlight;
    public LayerMask treeLayer;
    public Camera mainCamera;

    [Header("Silhueta (Fase 1)")]
    [Tooltip("GameObject filho com um MeshRenderer (Quad) e material escuro/semi-transparente.")]
    public GameObject monsterSilhouette;

    [Tooltip("Escala da silhueta no mundo (ajusta ao tamanho do teu monstro).")]
    public Vector3 silhouetteScale = new Vector3(1f, 2f, 1f);

    [Tooltip("Deslocamento vertical para que a silhueta não flutue nem fique enterrada.")]
    public float silhouetteYOffset = 1f;

    [Header("Fase 1 - Escondido nas Árvores")]
    public float teleportRadius = 12f;
    public float treeSearchRadius = 3f;

    [Header("Fase 2 - Perseguição")]
    public float attackDistance = 1.2f;

    [Header("Deteção de Flashlight")]
    public float flashlightAngle = 35f;
    public float flashlightRange = 12f;

    [Tooltip("Layers que BLOQUEIAM o raio da lanterna (ex: paredes, obstáculos). NÃO incluas o player nem as árvores.")]
    public LayerMask flashlightBlockingLayers;

    [Header("Reaparecimento após Lanterna")]
    public float reappearDelayMin = 3f;
    public float reappearDelayMax = 6f;

    [Header("Spawn Automático")]
    [Tooltip("O monstro aparece sozinho ao início do jogo?")]
    public bool autoSpawnOnStart = false;

    [Tooltip("Tempo de espera antes do primeiro spawn (segundos)")]
    public float firstSpawnDelay = 10f;

    [Tooltip("Depois de se esconder, quanto tempo até reaparecer (mínimo)")]
    public float hiddenCycleDelayMin = 8f;

    [Tooltip("Depois de se esconder, quanto tempo até reaparecer (máximo)")]
    public float hiddenCycleDelayMax = 15f;

    [Header("Velocidade das Fases")]
    [Tooltip("Velocidade do monstro na Fase 1 — normalmente 0 (parado)")]
    public float phase1Speed = 0f;

    [Tooltip("Velocidade do monstro na Fase 2 (perseguição)")]
    [Range(1f, 20f)]
    public float phase2Speed = 5f;

    [Tooltip("Quanto tempo o monstro fica na Fase 1 antes de passar à Fase 2")]
    public float phase1Duration = 20f;

    [Header("Áudio — Fase 1")]
    [Tooltip("Som que o monstro emite enquanto está escondido (loop 3D espacial).")]
    public AudioClip phase1AudioClip;

    [Tooltip("Volume do som da Fase 1.")]
    [Range(0f, 1f)]
    public float phase1AudioVolume = 1f;

    [Tooltip("Distância mínima — até aqui o volume é máximo.")]
    public float audioMinDistance = 1f;

    [Tooltip("Distância máxima — a partir daqui o som deixa de se ouvir.")]
    public float audioMaxDistance = 20f;

    private AudioSource phase1AudioSource;

    private Renderer[] monsterRenderers;
    private NavMeshAgent agent;
    private float phase1Timer;
    private bool isDisappearing = false;
    private bool hasValidNavMesh = false;
    private bool wasFlashlightActive = false; // rastreia estado anterior da lanterna

    private Vector3 lastHidingPosition;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        monsterRenderers = GetComponentsInChildren<Renderer>();

        if (monsterSilhouette == null)
            monsterSilhouette = GameObject.Find("MonsterSilhouette");

        agent = GetComponent<NavMeshAgent>();

        hasValidNavMesh = NavMesh.SamplePosition(transform.position, out _, 50f, NavMesh.AllAreas) ||
                          (player != null && NavMesh.SamplePosition(player.position, out _, 50f, NavMesh.AllAreas));

        if (!hasValidNavMesh)
        {
            Debug.LogError("[Monstro] ❌ Nenhum NavMesh encontrado!");
            enabled = false;
            return;
        }

        if (agent != null)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.speed = 0f;
            }
            else
            {
                agent.enabled = false;
            }
        }

        lastHidingPosition = transform.position;

        // Configura o AudioSource 3D para o som da Fase 1
        phase1AudioSource = gameObject.AddComponent<AudioSource>();
        phase1AudioSource.clip = phase1AudioClip;
        phase1AudioSource.loop = false;
        phase1AudioSource.playOnAwake = false;
        phase1AudioSource.spatialBlend = 1f;        // 1 = 100% 3D espacial
        phase1AudioSource.rolloffMode = AudioRolloffMode.Linear;
        phase1AudioSource.minDistance = audioMinDistance;
        phase1AudioSource.maxDistance = audioMaxDistance;
        phase1AudioSource.volume = phase1AudioVolume;
        phase1AudioSource.Stop();

        SetVisible(false);
        SetSilhouetteVisible(false);

        if (autoSpawnOnStart)
            StartCoroutine(AutoSpawnLoop());
    }

    IEnumerator AutoSpawnLoop()
    {
        yield return new WaitForSeconds(firstSpawnDelay);
        while (true)
        {
            if (currentState == MonsterState.Hidden && !isDisappearing)
            {
                Activate();
                Debug.Log("[Monstro] 🔁 Spawn automático!");
            }
            yield return new WaitUntil(() => currentState == MonsterState.Hidden && !isDisappearing);
            float wait = Random.Range(hiddenCycleDelayMin, hiddenCycleDelayMax);
            Debug.Log($"[Monstro] 💤 A aguardar {wait:F1}s antes do próximo spawn.");
            yield return new WaitForSeconds(wait);
        }
    }

    void Update()
    {
        // Billboard — silhueta olha sempre para a câmera
        if (monsterSilhouette != null && monsterSilhouette.activeSelf && mainCamera != null)
        {
            monsterSilhouette.transform.rotation = Quaternion.LookRotation(
                monsterSilhouette.transform.position - mainCamera.transform.position
            );
        }

        if (Input.GetKeyDown(KeyCode.I)) Activate();
        if (Input.GetKeyDown(KeyCode.O)) ForcePhase2();
        if (Input.GetKeyDown(KeyCode.P)) ForceHide();

        if (isDisappearing) return;

        // Verifica deteção por lanterna todos os frames enquanto ela está ligada
        if (currentState != MonsterState.Hidden && IsIlluminatedByFlashlight())
        {
            Debug.Log("[Monstro] 🔦 Apanhado! A desaparecer...");
            StartCoroutine(DisappearAndReturn());
            return;
        }

        switch (currentState)
        {
            case MonsterState.Phase1: UpdatePhase1(); break;
            case MonsterState.Phase2: UpdatePhase2(); break;
        }
    }

    // ═══════════════════════════════════════════════════════
    // FASE 1 — Escondido, parado, à espreita
    // ═══════════════════════════════════════════════════════
    void UpdatePhase1()
    {
        if (!hasValidNavMesh) return;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        phase1Timer += Time.deltaTime;

        if (phase1Timer >= phase1Duration)
        {
            EnterPhase2();
        }
    }

    void TeleportToTree()
    {
        if (!hasValidNavMesh) return;

        bool foundTree = FindPositionNearTree(out Vector3 pos);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.Warp(pos);
            lastHidingPosition = pos;

            if (foundTree)
            {
                UpdateSilhouette(pos);
                Debug.Log("[Monstro] Escondido numa árvore → silhueta visível.");
            }
            else
            {
                SetSilhouetteVisible(false);
                Debug.Log("[Monstro] Sem árvore disponível → silhueta escondida.");
            }

            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);

            // Inicia o som 3D da Fase 1 na posição do esconderijo
            StartPhase1Audio();
        }
    }

    bool FindPositionNearTree(out Vector3 result)
    {
        Collider[] trees = Physics.OverlapSphere(player.position, teleportRadius, treeLayer);

        if (trees.Length > 0)
        {
            System.Collections.Generic.List<Collider> shuffled = new System.Collections.Generic.List<Collider>(trees);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                Collider tmp = shuffled[i]; shuffled[i] = shuffled[j]; shuffled[j] = tmp;
            }

            foreach (Collider tree in shuffled)
            {
                Vector3 awayFromPlayer = (tree.transform.position - player.position).normalized;
                Vector3 candidate = tree.transform.position + awayFromPlayer * treeSearchRadius;

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, treeSearchRadius + 1f, NavMesh.AllAreas))
                {
                    Vector3 toCandidate = hit.position - player.position;
                    if (Physics.Raycast(player.position, toCandidate.normalized, out RaycastHit rHit, toCandidate.magnitude + 1f, treeLayer))
                    {
                        result = hit.position;
                        return true;
                    }
                }
            }
        }

        result = FindRandomNavMeshPosition();
        return false;
    }

    Vector3 FindRandomNavMeshPosition()
    {
        for (int i = 0; i < 30; i++)
        {
            float angle = Random.Range(0f, 360f);
            float dist = Random.Range(4f, teleportRadius);
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 candidate = player.position + dir * dist;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                return hit.position;
        }

        if (NavMesh.SamplePosition(player.position, out NavMeshHit fallback, 10f, NavMesh.AllAreas))
            return fallback.position;

        return transform.position;
    }

    // ═══════════════════════════════════════════════════════
    // FASE 2 — Perseguição
    // ═══════════════════════════════════════════════════════
    void EnterPhase2()
    {
        if (!hasValidNavMesh) return;

        if (agent != null)
        {
            if (NavMesh.SamplePosition(lastHidingPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                if (!agent.enabled) agent.enabled = true;
                agent.Warp(hit.position);
            }
            else if (!agent.isOnNavMesh)
            {
                Debug.LogError("[Monstro] Impossível entrar na Fase 2 – nenhum NavMesh!");
                return;
            }
        }

        currentState = MonsterState.Phase2;

        SetSilhouetteVisible(false);
        SetVisible(true);
        StopPhase1Audio(); // Para o som da Fase 1 ao entrar na perseguição

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = phase2Speed;
        }

        Debug.Log("[Monstro] ⚡ FASE 2 — A perseguir!");
    }

    void UpdatePhase2()
    {
        if (!hasValidNavMesh) return;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.SetDestination(player.position);
        else
            return;

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
            AttackPlayer();
    }

    // ═══════════════════════════════════════════════════════
    // DETECÇÃO DE FLASHLIGHT — CORRIGIDA
    // ═══════════════════════════════════════════════════════
    bool IsIlluminatedByFlashlight()
    {
        // 1. Verifica se a lanterna existe e está ligada
        if (flashlight == null)
        {
            Debug.LogWarning("[Monstro] ⚠️ Flashlight não atribuída no Inspector!");
            return false;
        }

        if (!flashlight.FlashlightActive)
            return false;

        // 2. Obtém o Transform da luz
        Transform lightT = flashlight.GetLightTransform();
        if (lightT == null)
        {
            Debug.LogWarning("[Monstro] ⚠️ GetLightTransform() devolveu null!");
            return false;
        }

        // 3. Vetor da luz até ao monstro (centro do collider)
        // Usa o centro do monstro em vez da base para maior precisão
        Vector3 monsterCenter = transform.position + Vector3.up * 1f;
        Vector3 toMonster = monsterCenter - lightT.position;
        float dist = toMonster.magnitude;

        // 4. Verifica distância
        if (dist > flashlightRange)
            return false;

        // 5. Verifica ângulo do cone
        float angle = Vector3.Angle(lightT.forward, toMonster);
        if (angle > flashlightAngle)
            return false;

        // 6. Raycast para verificar obstáculos
        // CORRIGIDO: usa flashlightBlockingLayers definido no Inspector
        // em vez de ~treeLayer que bloqueava o próprio player e outros objetos
        if (flashlightBlockingLayers.value != 0)
        {
            if (Physics.Raycast(lightT.position, toMonster.normalized, out RaycastHit hit, dist, flashlightBlockingLayers))
            {
                // Se acertou em algo que não é o monstro → está bloqueado
                if (hit.transform.root != transform)
                {
                    Debug.Log($"[Monstro] 🚧 Lanterna bloqueada por: {hit.transform.name}");
                    return false;
                }
            }
        }

        // 7. Iluminado!
        Debug.Log("[Monstro] ✅ Iluminado pela lanterna!");
        return true;
    }

    // ═══════════════════════════════════════════════════════
    // SILHUETA
    // ═══════════════════════════════════════════════════════
    void UpdateSilhouette(Vector3 hidePos)
    {
        if (monsterSilhouette == null) return;

        monsterSilhouette.transform.position = hidePos + Vector3.up * silhouetteYOffset;
        monsterSilhouette.transform.localScale = silhouetteScale;
        monsterSilhouette.SetActive(true);
    }

    void SetSilhouetteVisible(bool visible)
    {
        if (monsterSilhouette != null)
            monsterSilhouette.SetActive(visible);
    }

    // ═══════════════════════════════════════════════════════
    // UTILITÁRIOS
    // ═══════════════════════════════════════════════════════
    IEnumerator DisappearAndReturn()
    {
        isDisappearing = true;
        SetVisible(false);
        SetSilhouetteVisible(false);
        StopPhase1Audio(); // Para o som ao desaparecer

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.speed = 0f;
        }

        currentState = MonsterState.Hidden;
        phase1Timer = 0f;

        float delay = Random.Range(reappearDelayMin, reappearDelayMax);
        Debug.Log($"[Monstro] 💨 Desapareceu! Volta em {delay:F1}s.");
        yield return new WaitForSeconds(delay);

        TeleportToTree();

        currentState = MonsterState.Phase1;
        SetVisible(false);
        isDisappearing = false;
        Debug.Log("[Monstro] 👁️ Voltou — Fase 1 (escondido na árvore).");
    }

    // ═══════════════════════════════════════════════════════
    // ÁUDIO
    // ═══════════════════════════════════════════════════════
    void StartPhase1Audio()
    {
        if (phase1AudioSource == null || phase1AudioClip == null) return;
        if (phase1AudioSource.isPlaying) phase1AudioSource.Stop();
        phase1AudioSource.Play();
        Debug.Log("[Monstro] 🔊 Som da Fase 1 iniciado.");
    }

    void StopPhase1Audio()
    {
        if (phase1AudioSource != null && phase1AudioSource.isPlaying)
        {
            phase1AudioSource.Stop();
            Debug.Log("[Monstro] 🔇 Som da Fase 1 parado.");
        }
    }

    void SetVisible(bool visible)
    {
        foreach (var r in monsterRenderers)
            r.enabled = visible;
    }

    bool IsVisible()
    {
        foreach (var r in monsterRenderers)
            if (r.enabled) return true;
        return false;
    }

    void AttackPlayer()
    {
        Debug.Log("[Monstro] 💀 ATACOU O PLAYER — GAME OVER");

        TAKING_DAMAGE td = player.GetComponent<TAKING_DAMAGE>();
        if (td != null)
        {
            td.TakeDamage(1);
        }
        else
        {
            Debug.LogWarning("[Monstro] ⚠️ Script TAKING_DAMAGE não encontrado no Player!");
        }
    }
    // ═══════════════════════════════════════════════════════
    // BOTÕES DE TESTE
    // ═══════════════════════════════════════════════════════
    [ContextMenu("▶ TESTE — Ativar Fase 1")]
    public void Activate()
    {
        StopAllCoroutines();
        isDisappearing = false;
        phase1Timer = 0f;
        currentState = MonsterState.Phase1;
        TeleportToTree();
        SetVisible(false);
        Debug.Log("[Monstro] FASE 1 ativada!");
    }

    [ContextMenu("⚡ TESTE — Forçar Fase 2")]
    public void ForcePhase2()
    {
        StopAllCoroutines();
        isDisappearing = false;
        EnterPhase2();
    }

    [ContextMenu("✕ TESTE — Esconder")]
    public void ForceHide()
    {
        StopAllCoroutines();
        isDisappearing = false;
        SetVisible(false);
        SetSilhouetteVisible(false);
        if (agent != null && agent.isOnNavMesh) agent.ResetPath();
        currentState = MonsterState.Hidden;
        Debug.Log("[Monstro] Escondido!");
    }
}