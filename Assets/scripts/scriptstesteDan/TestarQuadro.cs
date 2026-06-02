using UnityEngine;

public class TestarQuadro : MonoBehaviour
{
    public Sprite fotoTeste;
    private BillboardManager manager;

    void Start()
    {
        manager = GetComponent<BillboardManager>();
    }

    void Update()
    {
        // Carrega no Espaço para simular que encontraste as pistas 1, 2 e 3!
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Simulando descoberta de pistas...");
            manager.AdicionarPistaAoQuadro(fotoTeste, "Suspeito A", "Visto perto do local.", 1);
            manager.AdicionarPistaAoQuadro(fotoTeste, "Pegada", "Tamanho 42 na lama.", 2);
            manager.AdicionarPistaAoQuadro(fotoTeste, "Horário", "O crime foi às 22h.", 3);
        }
    }
}