using UnityEngine;

public class OlharRato : MonoBehaviour
{
    public float sensibilidadeRato = 100f;
    public Transform corpoPlayer; 

    private float rotacaoX = 0f;
    private bool cameraTravada = false; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // SE A CÂMARA ESTIVER TRAVADA: Força a rotação atual a ficar congelada e não deixa mexer nada!
        if (cameraTravada)
        {
            transform.localRotation = Quaternion.Euler(rotacaoX, 0f, 0f);
            return; 
        }

        // Movimento normal quando o inventário está fechado
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadeRato * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadeRato * Time.deltaTime;

        rotacaoX -= mouseY;
        rotacaoX = Mathf.Clamp(rotacaoX, -90f, 90f);

        // Aplica a rotação vertical (Cima/Baixo) na própria Câmara/Pivot
        transform.localRotation = Quaternion.Euler(rotacaoX, 0f, 0f);
        
        // Aplica a rotação horizontal (Lados) no corpo do Player
        if (corpoPlayer != null)
        {
            corpoPlayer.Rotate(Vector3.up * mouseX);
        }
    }

    // Função que o inventário chama para travar
    public void SetTravarCamera(bool travar)
    {
        cameraTravada = travar;
    }
}