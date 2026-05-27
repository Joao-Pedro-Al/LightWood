using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PistaCard : MonoBehaviour
{
    public Image displayFoto;
    public TextMeshProUGUI displayNome;
    public TextMeshProUGUI displayDesc;
    public TextMeshProUGUI displayNumero;

    [HideInInspector] public int meuNumero;
    private BillboardManager manager;

    public void Setup(Sprite foto, string nome, string desc, int num, BillboardManager m)
    {
        meuNumero = num;
        manager = m;
        if(displayFoto) displayFoto.sprite = foto;
        if(displayNome) displayNome.text = nome;
        if(displayDesc) displayDesc.text = desc;
        if(displayNumero) displayNumero.text = "#" + num;
    }

    public void ClicarNoCard() 
    { 
        if (manager != null)
        {
            manager.SelecionarPista(this); 
        }
    }
}