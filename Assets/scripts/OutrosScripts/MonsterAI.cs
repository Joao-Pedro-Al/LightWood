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

    [Header("Spawn Automático (ativa-se sozinho ao apanhar a lanterna)")]
    [Tooltip("Tempo de espera DEPOIS de apanhares a lanterna até o monstro aparecer pela primeira vez (segundos)")]
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
    private bool wasFlashlightActive = false;

    // Referência à coroutine do AutoSpawnLoop para não a matar acidentalmente
    private Coroutine autoSpawnCoroutine;

    private Vector3 lastHidingPosition;

    void Start()
    {
        MonsterAI[] todosMonstros = FindObjectsOfType<MonsterAI>();
        if (todosMonstros.Length > 1)
        {
            Debug.LogWarning($"[Monstro] ⚠️ Existem {todosMonstros.Length} objetos com MonsterAI ativos na cena ao mesmo tempo! " +
                "Verifica se há duplicados (pesquisa 't:MonsterAI' na Hierarchy) — só deve existir 1.");
        }

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
        phase1AudioSource.spatialBlend = 1f;
        phase1AudioSource.rolloffMode = AudioRolloffMode.Linear;
        phase1AudioSource.minDistance = audioMinDistance;
        phase1AudioSource.maxDistance = audioMaxDistance;
        phase1AudioSource.volume = phase1AudioVolume;
        phase1AudioSource.Stop();

        SetVisible(false);
        SetSilhouetteVisible(false);

        Debug.Log("[Monstro] 🟢 A aguardar que a lanterna seja apanhada para começar...");
        autoSpawnCoroutine = StartCoroutine(AutoSpawnLoop());
    }

    IEnumerator AutoSpawnLoop()
    {
        Debug.Log("[Monstro] 🟡 AutoSpawnLoop a correr — a aguardar que a lanterna seja apanhada...");

        yield return new WaitUntil(() => flashlight != null && flashlight.hasFlashlight);

        Debug.Log("[Monstro] 🟡 Lanterna detetada! A aguardar " + firstSpawnDelay + "s antes do primeiro spawn...");
        yield return new WaitForSeconds(firstSpawnDelay);
        Debug.Log("[Monstro] 🟡 Espera concluída — a tentar o primeiro spawn.");

        while (true)
        {
            if (currentState == MonsterState.Hidden && !isDisappearing)
            {
                ActivateInternal();
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
        if (flashlight == null || !flashlight.hasFlashlight)
        {
            if (Time.frameCount % 120 == 0)
                Debug.Log($"[Monstro] ⏸️ Update bloqueado — flashlight null? {flashlight == null} | hasFlashlight: {(flashlight != null ? flashlight.hasFlashlight.ToString() : "N/A")}");
            return;
        }

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
        StopPhase1Audio();

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
    // DETECÇÃO DE FLASHLIGHT
    // ═══════════════════════════════════════════════════════
    bool IsIlluminatedByFlashlight()
    {
        if (flashlight == null)
        {
            Debug.LogWarning("[Monstro] ⚠️ Flashlight não atribuída no Inspector!");
            return false;
        }

        // Verifica FlashlightActive E que a luz está mesmo acesa (pode estar a piscar)
        if (!flashlight.FlashlightActive) return false;

        Transform lightT = flashlight.GetLightTransform();
        if (lightT == null)
        {
            Debug.LogWarning("[Monstro] ⚠️ GetLightTransform() devolveu null!");
            return false;
        }

        // Verifica se a luz está fisicamente ativa (pode estar a piscar por bateria fraca)
        if (!lightT.gameObject.activeInHierarchy) return false;

        // Múltiplos pontos do corpo do monstro — pés, centro, peito, cabeça
        Vector3[] checkPoints = new Vector3[]
        {
            transform.position + Vector3.up * 0.2f,   // pés
            transform.position + Vector3.up * 0.9f,   // centro
            transform.position + Vector3.up * 1.5f,   // peito
            transform.position + Vector3.up * 1.9f,   // cabeça
        };

        foreach (Vector3 point in checkPoints)
        {
            Vector3 toPoint = point - lightT.position;
            float dist = toPoint.magnitude;

            // Fora do alcance -> skip
            if (dist > flashlightRange) continue;

            // Fora do cone -> skip
            float angle = Vector3.Angle(lightT.forward, toPoint);
            if (angle > flashlightAngle) continue;

            // Raycast para ver se há obstáculo entre a luz e este ponto
            bool blocked = false;
            if (flashlightBlockingLayers.value != 0)
            {
                if (Physics.Raycast(lightT.position, toPoint.normalized, out RaycastHit hit, dist, flashlightBlockingLayers))
                {
                    if (hit.transform.root != transform)
                        blocked = true;
                }
            }

            if (!blocked)
            {
                Debug.Log($"[Monstro] ✅ Iluminado pela lanterna!");
                return true;
            }
        }

        return false;
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
        StopPhase1Audio();

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

        // Garante que o AudioSource já está na posição correta antes de tocar
        yield return null;
        StartPhase1Audio();
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

        // FIX dano fantasma: collider só ativo quando visível (Fase 2)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = visible;
    }

    bool IsVisible()
    {
        foreach (var r in monsterRenderers)
            if (r.enabled) return true;
        return false;
    }

    void AttackPlayer()
    {
        Debug.Log("[Monstro] 💀 ATACOU O PLAYER!");

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(25f);
        }
        else
        {
            TAKING_DAMAGE td = player.GetComponent<TAKING_DAMAGE>();
            if (td != null) td.TakeDamage(1);
            else Debug.LogWarning("[Monstro] ⚠️ Nenhum script de dano encontrado no Player!");
        }

        ResetToPhase1AfterAttack();
    }

    void ResetToPhase1AfterAttack()
    {
        // FIX 2: Parar APENAS a coroutine de desaparecimento — o AutoSpawnLoop mantém-se vivo
        if (autoSpawnCoroutine != null) StopCoroutine(autoSpawnCoroutine);
        StopCoroutine("DisappearAndReturn");

        isDisappearing = false;
        StartCoroutine(DisappearAndReturn());

        // Reinicia o AutoSpawnLoop para o ciclo continuar após o ataque
        autoSpawnCoroutine = StartCoroutine(AutoSpawnLoop());

        Debug.Log("[Monstro] 🔄 Recuou para Fase 1 após ataque.");
    }

    // ═══════════════════════════════════════════════════════
    // BOTÕES DE TESTE
    // ═══════════════════════════════════════════════════════

    // Versão interna usada pelo AutoSpawnLoop (não para o loop)
    void ActivateInternal()
    {
        if (flashlight == null || !flashlight.hasFlashlight)
        {
            Debug.Log("[Monstro] 🚫 Ainda não pode ativar — o jogador ainda não apanhou a lanterna.");
            return;
        }

        isDisappearing = false;
        phase1Timer = 0f;
        currentState = MonsterState.Phase1;
        TeleportToTree();
        SetVisible(false);
        StartPhase1Audio();
        Debug.Log("[Monstro] FASE 1 ativada!");
    }

    [ContextMenu("▶ TESTE — Ativar Fase 1")]
    public void Activate()
    {
        if (flashlight == null || !flashlight.hasFlashlight)
        {
            Debug.Log("[Monstro] 🚫 Ainda não pode ativar — o jogador ainda não apanhou a lanterna.");
            return;
        }

        // Para o AutoSpawnLoop e reinicia-o depois para não criar duplicados
        if (autoSpawnCoroutine != null) StopCoroutine(autoSpawnCoroutine);
        StopCoroutine("DisappearAndReturn");

        isDisappearing = false;
        phase1Timer = 0f;
        currentState = MonsterState.Phase1;
        TeleportToTree();
        SetVisible(false);
        StartPhase1Audio();
        Debug.Log("[Monstro] FASE 1 ativada!");

        autoSpawnCoroutine = StartCoroutine(AutoSpawnLoop());
    }

    [ContextMenu("⚡ TESTE — Forçar Fase 2")]
    public void ForcePhase2()
    {
        if (autoSpawnCoroutine != null) StopCoroutine(autoSpawnCoroutine);
        StopCoroutine("DisappearAndReturn");
        isDisappearing = false;
        EnterPhase2();
        autoSpawnCoroutine = StartCoroutine(AutoSpawnLoop());
    }

    [ContextMenu("✕ TESTE — Esconder")]
    public void ForceHide()
    {
        if (autoSpawnCoroutine != null) StopCoroutine(autoSpawnCoroutine);
        StopCoroutine("DisappearAndReturn");
        isDisappearing = false;
        SetVisible(false);
        SetSilhouetteVisible(false);
        StopPhase1Audio();
        if (agent != null && agent.isOnNavMesh) agent.ResetPath();
        currentState = MonsterState.Hidden;
        Debug.Log("[Monstro] Escondido!");
        autoSpawnCoroutine = StartCoroutine(AutoSpawnLoop());
    }
}