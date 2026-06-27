using UnityEngine;

/// <summary>
/// Coloca este script num GameObject com um BoxCollider (Is Trigger = ON)
/// cobrindo o segundo andar da cabana.
/// Quando o player entra, o monstro passa imediatamente à Fase 2 (perseguição).
/// </summary>
public class SecondFloorTrigger : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arrastar o GameObject do monstro aqui.")]
    public MonsterAI monster;

    [Tooltip("Tag do player (normalmente 'Player').")]
    public string playerTag = "Player";

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;
        Debug.Log("[SecondFloor] 🏠 Player no segundo andar — monstro a subir!");

        if (monster != null)
            monster.ForcePhase2();
        else
            Debug.LogWarning("[SecondFloor] ⚠️ MonsterAI não atribuído no Inspector!");
    }

    void OnTriggerExit(Collider other)
    {
        // Opcional: reset quando o player sai do segundo andar
        // triggered = false;
    }
}