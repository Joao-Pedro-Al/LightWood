using UnityEngine;

public class Player : MonoBehaviour
{
    // Créditos: https://discussions.unity.com/t/first-person-movement/677313
    #region "Variables"
    public Rigidbody Rigid;
    public float MouseSensitivity_Horizontal;
    public float MouseSensitivity_Vertical;
    public float MoveSpeed;
    public float JumpForce;
    public Transform camera;
    #endregion

    private float verticalRotation = 0f;

//novo de teste Dan 


// 
    void Update ()
    {
        // Camera-Horizontal
        Rigid.MoveRotation(Rigid.rotation * Quaternion.Euler(new Vector3(0, Input.GetAxis("Mouse X") * MouseSensitivity_Horizontal, 0)));

        // Camera-Vertical
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity_Vertical;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        camera.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        // Movimento
        Rigid.MovePosition(transform.position + (transform.forward * Input.GetAxis("Vertical") * MoveSpeed) + (transform.right * Input.GetAxis("Horizontal") * MoveSpeed));
        // if (Input.GetKeyDown("space"))
        //     Rigid.AddForce(transform.up * JumpForce);
    }
}