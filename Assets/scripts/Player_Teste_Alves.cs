using UnityEngine;

public class Player_Teste_Alves : MonoBehaviour
{
    // Créditos: https://discussions.unity.com/t/first-person-movement/677313
    #region "Variables"
    public Rigidbody Rigid;
    public float MouseSensitivity_Horizontal;
    public float MouseSensitivity_Vertical;
    public float MoveSpeed;
    public float RunSpeed;
    public float JumpForce;
    public Transform camera;
    #endregion

    private float verticalRotation = 0f;
    //dan inventario
    public bool cameraTravada = false;
//dan inventario start
void Start() 

{ 

    // Bloqueia o rato no centro do ecrã 

    Cursor.lockState = CursorLockMode.Locked; 

     

    // Torna o ponteiro do rato invisível para não estorvar a mira 

    Cursor.visible = false; 

} 
//dan inventario end

    void Update ()
    {
        //Dan inventario
        // Se a câmara estiver travada, não deixa o resto do código do rato rodar
         if (cameraTravada)
           {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Zera os inputs do rato neste frame para a câmara não dar saltos para baixo ou para os lados
        Input.ResetInputAxes();
        return; // Sai do Update e não move a câmara
          }
//Dan inventario fim

        // Camera-Horizontal
        Rigid.MoveRotation(Rigid.rotation * Quaternion.Euler(new Vector3(0, Input.GetAxis("Mouse X") * MouseSensitivity_Horizontal, 0)));

        // Camera-Vertical
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity_Vertical;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        camera.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        // Movimento
        float VelocidadeAtual = MoveSpeed;
        if(Input.GetKey("left shift"))
        {
            VelocidadeAtual = RunSpeed;
        }

        // Debug.Log(VelocidadeAtual);

        Rigid.MovePosition(transform.position + (transform.forward * Input.GetAxis("Vertical") * VelocidadeAtual) + (transform.right * Input.GetAxis("Horizontal") * VelocidadeAtual));
        // if (Input.GetKeyDown("space"))
        //     Rigid.AddForce(transform.up * JumpForce);
    }
}