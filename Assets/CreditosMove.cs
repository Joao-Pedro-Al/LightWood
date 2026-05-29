using UnityEngine;

public class CreditosMove : MonoBehaviour
{
    [Header("Configurações de Velocidade")]
    [SerializeField] private float velocidade = 50f;

    [Header("Configurações de Fim")]
    [SerializeField] private float posicaoFinalY = 1500f; 

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
      
        rectTransform.anchoredPosition += new Vector2(0, velocidade * Time.deltaTime);

       
        if (rectTransform.anchoredPosition.y >= posicaoFinalY)
        {
            Debug.Log("Os créditos terminaram!");
            
        }
    }
}