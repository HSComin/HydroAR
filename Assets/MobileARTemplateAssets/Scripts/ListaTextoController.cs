using TMPro;
using UnityEngine;

public class ListaTextoController : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public TMP_Text textoResultado;

    [TextArea]
    public string[] descricoes;

    public void AtualizarTexto()
    {
        int index = dropdown.value;

        if (index < descricoes.Length)
        {
            textoResultado.text = descricoes[index];
        }
        else
        {
            textoResultado.text = dropdown.options[index].text;
        }
    }
}