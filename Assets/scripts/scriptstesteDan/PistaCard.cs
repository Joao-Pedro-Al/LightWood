using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PistaCard : MonoBehaviour
{
    public TextMeshProUGUI txtNumero;
    public TextMeshProUGUI txtNome;
    public TextMeshProUGUI txtDescricao;
    public Image imgFoto;

    [HideInInspector] public int meuNumero;
    private BillboardManager manager;

    public void Setup(Sprite foto, string nome, string descricao, int numero, BillboardManager bManager)
    {
        meuNumero = numero;
        manager = bManager;

        if (txtNumero != null) txtNumero.text = "P" + numero.ToString();
        if (txtNome != null) txtNome.text = nome;
        if (txtDescricao != null) txtDescricao.text = descricao;
        if (imgFoto != null && foto != null) imgFoto.sprite = foto;
    }
}