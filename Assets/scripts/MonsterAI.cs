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
    public Camera mainCamera; // ← NOVO: câmera para o billboard

    // ── Silhueta escura que aparece na árvore durante a Fase 1 ──────────
    [Header("Silhueta (Fase 1)")]
    [Tooltip("GameObject filho com um MeshRenderer (Quad) e material escuro/semi-transparente.")]
    public GameObject monsterSilhouette;

    [Tooltip("Escala da silhueta no mundo (ajusta ao tamanho do teu monstro).")]
    public Vector3 silhouetteScale = new Vector3(1f, 2f, 1f);

    [Tooltip("Deslocamento vertical para que a silhueta não flutue nem fique enterrada.")]
    public float silhouetteYOffset = 1f;
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Fase 1 - Escondido nas Árvores")]
    public float teleportRadius = 12f;
    public float treeSearchRadius = 3f;

    [Header("Fase 2 - Perseguição")]
    public float attackDistance = 1.2f;

    [Header("Deteção de Flashlight")]
    public float flashlightAngle = 35f;
    public float flashlightRange = 12f;

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

    private Renderer[] monsterRenderers;
    private NavMeshAgent agent;
    private float phase1Timer;
    private bool isDisappearing = false;
    private bool hasValidNavMesh = false;

    private Vector3 lastHidingPosition;

    void Start()
    {
        // ← NOVO: encontra a câmera automaticamente se não estiver atribuída
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

        SetVisible(false);
        SetSilhouetteVisible(false);

        if (autoSpawnOnStart)
            StartCoroutine(AutoSpawnLoop());
    }

    // Ciclo de spawn automático: espera e aparece em loop
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
            // Espera até o monstro ficar Hidden novamente, depois aguarda o ciclo
            yield return new WaitUntil(() => currentState == MonsterState.Hidden && !isDisappearing);
            float wait = Random.Range(hiddenCycleDelayMin, hiddenCycleDelayMax);
            Debug.Log($"[Monstro] 💤 A aguardar {wait:F1}s antes do próximo spawn.");
            yield return new WaitForSeconds(wait);
        }
    }

    void Update()
    {
        // ← NOVO: Billboard — a silhueta olha sempre para a câmera, em todos os ângulos
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

        if (IsIlluminatedByFlashlight())
        {
            Debug.Log("[Monstro] Apanhado pela lanterna! A fugir...");
            StartCoroutine(DisappearAndReturn());
            return;
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

            // Silhueta SÓ aparece se o monstro está mesmo atrás de uma árvore
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

            // Monstro olha para o jogador enquanto está escondido
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    // Retorna true se encontrou posição junto a uma árvore, false se usou fallback aleatório
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
                        return true; // ← encontrou árvore
                    }
                }
            }
        }

        result = FindRandomNavMeshPosition();
        return false; // ← sem árvore, usou posição aleatória
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

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = phase2Speed;
        }

        Debug.Log("[Monstro] ⚡ FASE 2 — A perseguir desde a árvore!");
    }

    void UpdatePhase2()
    {
        if (!hasValidNavMesh) return;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.SetDestination(player.position);
        else
            return;

        if (IsIlluminatedByFlashlight())
        {
            StartCoroutine(DisappearAndReturn());
            return;
        }

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
            AttackPlayer();
    }

    // ═══════════════════════════════════════════════════════
    // DETECÇÃO DE FLASHLIGHT
    // ═══════════════════════════════════════════════════════
    bool IsIlluminatedByFlashlight()
    {
        if (flashlight == null || !flashlight.FlashlightActive) return false;

        Transform lightT = flashlight.GetLightTransform();
        if (lightT == null) return false;

        Vector3 toMonster = transform.position - lightT.position;
        float dist = toMonster.magnitude;

        if (dist > flashlightRange) return false;

        float angle = Vector3.Angle(lightT.forward, toMonster);
        if (angle > flashlightAngle) return false;

        LayerMask blockingLayers = ~treeLayer;

        if (Physics.Raycast(lightT.position, toMonster.normalized, out RaycastHit hit, dist, blockingLayers))
        {
            if (hit.transform.root != transform)
                return false;
        }

        return true;
    }

    // ═══════════════════════════════════════════════════════
    // SILHUETA
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Move a silhueta para a posição do esconderijo.
    /// A rotação (billboard) é tratada no Update() para ser contínua.
    /// </summary>
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

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.speed = 0f;
        }

        currentState = MonsterState.Hidden;
        phase1Timer = 0f;

        yield return new WaitForSeconds(Random.Range(reappearDelayMin, reappearDelayMax));

        TeleportToTree();

        currentState = MonsterState.Phase1;
        SetVisible(false);
        isDisappearing = false;
        Debug.Log("[Monstro] Voltou — Fase 1 (escondido).");
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