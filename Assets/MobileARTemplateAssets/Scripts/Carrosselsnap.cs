using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Transforma um ScrollRect horizontal comum em um carrossel com "snap"
/// (a rolagem sempre para exatamente em um painel) + atualiza indicadores
/// de bolinha (dots), como no padrão de referência (seção "Características").
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class CarrosselSnap : MonoBehaviour, IEndDragHandler
{
    [Header("Referências")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;

    [Header("Indicadores (dots)")]
    [Tooltip("Arraste os GameObjects das bolinhas, na mesma ordem dos painéis.")]
    [SerializeField] private List<Image> dots = new List<Image>();
    [SerializeField] private Color corDotAtivo = Color.white;
    [SerializeField] private Color corDotInativo = new Color(1, 1, 1, 0.4f);

    [Header("Comportamento")]
    [SerializeField] private float velocidadeSnap = 10f;
    [Tooltip("Distância mínima de arrasto (em % da largura do painel) para considerar troca de página.")]
    [SerializeField] private float limiarTroca = 0.2f;

    private int paginaAtual = 0;
    private int totalPaginas = 0;
    private float larguraPainel;
    private bool animando = false;
    private float posicaoAlvo;

    void Awake()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (content == null) content = scrollRect.content;
    }

    void Start()
    {
        totalPaginas = content.childCount;
        if (totalPaginas > 0)
        {
            var primeiroPainel = content.GetChild(0) as RectTransform;
            larguraPainel = primeiroPainel.rect.width;
        }
        AtualizarDots();
    }

    void Update()
    {
        if (!animando) return;

        Vector2 posAtual = content.anchoredPosition;
        float novoX = Mathf.Lerp(posAtual.x, posicaoAlvo, Time.deltaTime * velocidadeSnap);
        content.anchoredPosition = new Vector2(novoX, posAtual.y);

        if (Mathf.Abs(novoX - posicaoAlvo) < 0.5f)
        {
            content.anchoredPosition = new Vector2(posicaoAlvo, posAtual.y);
            animando = false;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (totalPaginas <= 1 || larguraPainel <= 0f) return;

        float deslocamento = -content.anchoredPosition.x - (paginaAtual * larguraPainel);

        if (Mathf.Abs(deslocamento) > larguraPainel * limiarTroca)
        {
            if (deslocamento > 0 && paginaAtual < totalPaginas - 1)
                paginaAtual++;
            else if (deslocamento < 0 && paginaAtual > 0)
                paginaAtual--;
        }

        IrParaPagina(paginaAtual);
    }

    /// <summary>Vai para uma página específica (também pode ser chamado por um botão de seta, se quiser).</summary>
    public void IrParaPagina(int indice)
    {
        indice = Mathf.Clamp(indice, 0, totalPaginas - 1);
        paginaAtual = indice;
        posicaoAlvo = -paginaAtual * larguraPainel;
        animando = true;
        AtualizarDots();
    }

    private void AtualizarDots()
    {
        for (int i = 0; i < dots.Count; i++)
        {
            if (dots[i] == null) continue;
            dots[i].color = (i == paginaAtual) ? corDotAtivo : corDotInativo;
        }
    }
}