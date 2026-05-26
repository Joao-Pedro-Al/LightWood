using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PistaCard : MonoBehaviour
{
    [Header("Componentes Visuais do Card")]
    public Image displayFoto;
    public TextMeshProUGUI displayNome;
    public TextMeshProUGUI displayDescricao;
    public TextMeshProUGUI displayNumero; 

    [HideInInspector] public int numeroDaPista;
    private BillboardManager manager;

    public void ConfigurarCard(Sprite foto, string nome, string descricao, int numero, BillboardManager gerenciador)
    {
        numeroDaPista = numero;
        manager = gerenciador;

        if (displayFoto != null) displayFoto.sprite = foto;
        if (displayNome != null) displayNome.text = nome;
        if (displayDescricao != null) displayDescricao.text = descricao;
        if (displayNumero != null) displayNumero.text = "#" + numero.ToString();
    }

    // Deve ser chamado pelo componente Button do CardPista
    public void AoClicarNoCard()
    {
        if (manager != null)
        {
            manager.SelecionarPista(this);
        }
    }
}