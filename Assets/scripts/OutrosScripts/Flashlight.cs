using UnityEngine;
using UnityEngine.UI; // Necessário para a Image

public class Flashlight : MonoBehaviour
{
    // Singleton — garante que CliqueItem e MonsterAI apontam sempre para a MESMA lanterna,
    // mesmo que existam acidentalmente outras instâncias na cena.
    public static Flashlight Instance;

    [Header("Posse do Item")]
    [Tooltip("Fica TRUE assim que o jogador apanhar o item da lanterna no mundo. Enquanto for FALSE, a lanterna não pode ser ligada.")]
    public bool hasFlashlight = false;

    [Header("Luz")]
    [SerializeField] GameObject FlashlightLight;

    [Header("Bateria")]
    [SerializeField] float maxBattery = 100f;          // Carga máxima

    [Tooltip("Consumo por segundo. 0.666f faz a bateria durar exatamente 2.5 minutos (150 segundos).")]
    [SerializeField] float drainRate = 0.666f;         // ALTERADO: Ajustado para durar entre 2 a 3 minutos

    [SerializeField] float lowBatteryPercent = 0.2f;   // 20% = última parte da barra
    [SerializeField] float blinkInterval = 0.3f;       // Tempo entre piscadas

    [Header("UI - Imagem da Barra")]
    [SerializeField] Image batteryFillImage;           // ← Arraste aqui o objeto BatteryFill

    public bool FlashlightActive = false;

    // Evento disparado no momento exato em que a lanterna é ligada
    // O MonsterAI subscreve isto para deteção instantânea com um único flash
    public System.Action OnFlashlightTurnedOn;

    private float currentBattery;
    private float blinkTimer;
    private bool isBlinking = false;
    private bool lightCurrentlyOn = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Lanterna] ⚠️ Existe mais do que UM componente Flashlight na cena! " +
                "Isto causa bugs (ex: monstro nunca ativa). Apaga o duplicado: " + gameObject.name);
        }
        Instance = this;
    }

    void Start()
    {
        currentBattery = maxBattery;
        FlashlightLight.gameObject.SetActive(false);

        // Configura a barra visual (caso não esteja configurada no editor)
        if (batteryFillImage != null)
        {
            batteryFillImage.type = Image.Type.Filled;
            batteryFillImage.fillMethod = Image.FillMethod.Horizontal;
            batteryFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            batteryFillImage.fillAmount = 1f; // 100% no início
        }
    }

    void Update()
    {
        // 1. Entrada do jogador (ligar/desligar) — só funciona depois de apanhar a lanterna
        if (hasFlashlight && Input.GetKeyDown(KeyCode.F))
        {
            if (!FlashlightActive)
            {
                // Tentar ligar a lanterna
                if (currentBattery > 0)
                {
                    FlashlightActive = true;
                    ResetBlinkState();
                    OnFlashlightTurnedOn?.Invoke(); // ← notifica o monstro instantaneamente
                }
                else
                {
                    Debug.Log("Bateria esgotada! Não é possível ligar.");
                }
            }
            else
            {
                // Desligar lanterna
                FlashlightActive = false;
                FlashlightLight.SetActive(false);
                isBlinking = false;
            }
        }

        // 2. Consumo de bateria quando ligada e com carga
        if (FlashlightActive && currentBattery > 0)
        {
            currentBattery -= drainRate * Time.deltaTime;
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                // Bateria acabou: desliga completamente
                FlashlightActive = false;
                FlashlightLight.SetActive(false);
                isBlinking = false;
            }
            UpdateBatteryUI();
        }

        // 3. Controlar o comportamento da luz (acesa, apagada ou piscando)
        if (FlashlightActive && currentBattery > 0)
        {
            float batteryPercent = currentBattery / maxBattery;
            bool isLow = batteryPercent <= lowBatteryPercent;

            if (isLow)
            {
                // Modo de bateria fraca: PISCAR
                if (!isBlinking)
                {
                    // Iniciar o piscar
                    isBlinking = true;
                    blinkTimer = 0f;
                    lightCurrentlyOn = true;
                    FlashlightLight.SetActive(true);
                }

                blinkTimer += Time.deltaTime;
                if (blinkTimer >= blinkInterval)
                {
                    blinkTimer = 0f;
                    lightCurrentlyOn = !lightCurrentlyOn;
                    FlashlightLight.SetActive(lightCurrentlyOn);
                }
            }
            else
            {
                // Bateria normal: luz fixa acesa
                if (isBlinking)
                {
                    isBlinking = false;
                    if (!FlashlightLight.activeSelf)
                        FlashlightLight.SetActive(true);
                    lightCurrentlyOn = true;
                }
                else
                {
                    if (!FlashlightLight.activeSelf)
                        FlashlightLight.SetActive(true);
                }
            }
        }
        else
        {
            // Lanterna desligada ou sem bateria → luz apagada
            if (FlashlightLight.activeSelf)
                FlashlightLight.SetActive(false);
            isBlinking = false;
        }
    }

    // Atualiza a barra (imagem) com base na bateria atual
    private void UpdateBatteryUI()
    {
        if (batteryFillImage != null)
        {
            float percent = currentBattery / maxBattery;
            batteryFillImage.fillAmount = percent;

            // Muda a cor da barra quando está na última parte (baixa)
            if (percent <= lowBatteryPercent)
                batteryFillImage.color = Color.red;
            else
                batteryFillImage.color = Color.yellow; // ou outra cor, ex: verde
        }
    }

    // Reseta o estado do piscar (útil ao religar a lanterna)
    private void ResetBlinkState()
    {
        isBlinking = false;
        blinkTimer = 0f;
        lightCurrentlyOn = true;
        if (FlashlightLight != null)
            FlashlightLight.SetActive(true);
    }

    // Retorna a Transform da luz (direção do cone)
    public Transform GetLightTransform()
    {
        return FlashlightLight.transform;
    }

    // Método público para recarregar a bateria (opcional)
    public void Recharge(float amount)
    {
        currentBattery = Mathf.Clamp(currentBattery + amount, 0, maxBattery);
        UpdateBatteryUI();
    }

    // Chamado pelo CliqueItem quando o jogador apanha o ITEM da lanterna pela primeira vez.
    // A partir deste momento a tecla F passa a funcionar e o monstro pode começar a atuar.
    public void CollectFlashlight()
    {
        if (hasFlashlight) return; // já tinha sido apanhada, evita repetir o log

        hasFlashlight = true;
        Debug.Log("[Lanterna] 🔦 Lanterna apanhada! Já podes ligá-la com F.");
    }
}