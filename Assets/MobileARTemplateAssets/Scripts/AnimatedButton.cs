using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AnimatedButton : MonoBehaviour
{
    public Animator animator;
    public float tempoAnimacao = 0.2f;

    public UnityEvent acaoBotao;

    public void Clicar()
    {
        StartCoroutine(Executar());
    }

    IEnumerator Executar()
    {
        if (animator != null)
            animator.SetTrigger("Click");

        yield return new WaitForSeconds(tempoAnimacao);

        acaoBotao.Invoke();
    }
}
