using UnityEngine;

public class BrilhoBateria : MonoBehaviour
{
    [Header("Configurações do Brilho")]
    public Color corDoBrilho = new Color(1f, 0.92f, 0.016f); // Amarelo brilhante por padrão
    public float intensidadeLuz = 3f;
    public float raioDoBrilho = 8f;

    void Start()
    {
        // 1. Cria um novo objeto vazio para segurar a luz
        GameObject objetoLuz = new GameObject("Luz_Indicadora_Bateria");
        objetoLuz.transform.SetParent(this.transform);
        objetoLuz.transform.localPosition = Vector3.zero; // Fica exatamente no centro da bateria

        // Luz
        Light luz = objetoLuz.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = corDoBrilho;
        luz.intensity = intensidadeLuz;
        luz.range = raioDoBrilho;

       
        luz.shadows = LightShadows.None;

        
        luz.renderMode = LightRenderMode.ForcePixel;
    }
}