using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Componente único e reutilizável para todos os botões do HydroAR
/// (botões de ação, botões pill, itens de navbar superior).
///
/// Recursos:
/// - Cor controlada pelo HydroARTheme (nunca hardcoded)
/// - Raio de borda editável em tempo real via shader (sem precisar de sprite pronta)
/// - Animação de toque (scale) configurável
/// - Suporte a estado "selecionado" (para itens de navbar tipo Rios/Modelos/Mapa)
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ThemedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public enum Estilo
    {
        Primario,     // fundo cheio, cor de marca
        Outline,      // fundo transparente, borda colorida
        Secundario,   // fundo cheio, cor de apoio (accent)
        Fantasma,     // sem fundo/borda, só texto/ícone (ex: botão "Cancelar" discreto)
        NavItem       // item de navbar superior, com estado selecionado/não selecionado
    }

    // ─────────────────────────────────────────────────────────────────
    // Referências
    // ─────────────────────────────────────────────────────────────────

    [Header("Referências")]
    [SerializeField] private HydroARTheme theme;
    [SerializeField] private Image fundo;                  // Image com o shader UIRoundedRect aplicado
    [SerializeField] private Graphic[] textosEIcones;       // TMP_Text e/ou Image de ícone filhos
    [SerializeField] private RectTransform rectTransform;

    // ─────────────────────────────────────────────────────────────────
    // Estilo visual
    // ─────────────────────────────────────────────────────────────────

    [Header("Estilo")]
    [SerializeField] private Estilo estilo = Estilo.Primario;

    [Header("Formato (raio de borda)")]
    [Tooltip("Raio em pixels. Use um valor alto (ex: 999) para formato pill/cápsula.")]
    [SerializeField] private float cornerRadius = 16f;
    [SerializeField] private float borderWidth = 2f;

    // ─────────────────────────────────────────────────────────────────
    // Animação
    // ─────────────────────────────────────────────────────────────────

    [Header("Animação de toque")]
    [SerializeField] private bool animarToque = true;
    [SerializeField] private float escalaPressionado = 0.96f;
    [SerializeField] private float duracaoAnimacao = 0.08f;
    [SerializeField] private AnimationCurve curvaAnimacao = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ─────────────────────────────────────────────────────────────────
    // Estado (para NavItem)
    // ─────────────────────────────────────────────────────────────────

    [Header("Somente para NavItem")]
    [SerializeField] private bool selecionado = false;
    [SerializeField] private GameObject indicadorSelecionado; // ex: barra verde embaixo do ícone

    // ─────────────────────────────────────────────────────────────────
    // Internos
    // ─────────────────────────────────────────────────────────────────

    private Material materialInstancia;
    private Coroutine animacaoAtual;
    private Vector3 escalaOriginal;
    private static readonly int ID_Size = Shader.PropertyToID("_Size");
    private static readonly int ID_Radius = Shader.PropertyToID("_Radius");
    private static readonly int ID_BorderWidth = Shader.PropertyToID("_BorderWidth");
    private static readonly int ID_BorderColor = Shader.PropertyToID("_BorderColor");

    // ─────────────────────────────────────────────────────────────────
    // Ciclo de vida
    // ─────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        escalaOriginal = rectTransform.localScale;

        // Cria uma instância única do material para este botão,
        // já que _Size e _Radius variam por botão.
        if (fundo != null && fundo.material != null)
        {
            materialInstancia = new Material(fundo.material);
            fundo.material = materialInstancia;
        }
    }

    void OnEnable()
    {
        AplicarEstilo();
        // Layout Groups (Horizontal/Vertical Layout Group, Content Size Fitter)
        // só resolvem o tamanho real do RectTransform DEPOIS do Awake/OnEnable,
        // no mesmo frame. Sem isso, botões não-quadrados (ex: 50x150) recebem
        // o _Size errado no primeiro frame e o raio fica desproporcional.
        StartCoroutine(ReaplicarAposLayout());
    }

    private IEnumerator ReaplicarAposLayout()
    {
        yield return new WaitForEndOfFrame();
        AplicarEstilo();
    }

    void OnRectTransformDimensionsChange()
    {
        // Atualiza o _Size do shader sempre que o botão for redimensionado
        // (ex: layout responsivo, Content Size Fitter)
        if (materialInstancia != null && rectTransform != null)
        {
            var size = rectTransform.rect.size;
            materialInstancia.SetVector(ID_Size, new Vector4(size.x, size.y, 0, 0));
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Aplicação de estilo
    // ─────────────────────────────────────────────────────────────────

    [ContextMenu("Aplicar Estilo Agora")]
    public void AplicarEstilo()
    {
        if (theme == null)
        {
            Debug.LogWarning($"[ThemedButton] Theme não atribuído em {name}");
            return;
        }

        Color corFundo, corTexto, corBorda;
        float larguraBorda = 0f;

        switch (estilo)
        {
            case Estilo.Primario:
                corFundo = theme.primary700;
                corTexto = Color.white;
                corBorda = Color.clear;
                break;

            case Estilo.Outline:
                corFundo = Color.clear;
                corTexto = theme.primary700;
                corBorda = theme.primary700;
                larguraBorda = borderWidth;
                break;

            case Estilo.Secundario:
                corFundo = theme.accent700;
                corTexto = Color.white;
                corBorda = Color.clear;
                break;

            case Estilo.Fantasma:
                corFundo = Color.clear;
                corTexto = theme.textSecondary;
                corBorda = Color.clear;
                break;

            case Estilo.NavItem:
                corFundo = Color.clear;
                corTexto = selecionado ? theme.primary700 : theme.textSecondary;
                corBorda = Color.clear;
                if (indicadorSelecionado != null)
                    indicadorSelecionado.SetActive(selecionado);
                break;

            default:
                corFundo = theme.primary700;
                corTexto = Color.white;
                corBorda = Color.clear;
                break;
        }

        if (fundo != null) fundo.color = corFundo;
        SetTextColor(corTexto);

        if (materialInstancia != null)
        {
            var size = rectTransform.rect.size;
            materialInstancia.SetVector(ID_Size, new Vector4(size.x, size.y, 0, 0));
            materialInstancia.SetFloat(ID_Radius, cornerRadius);
            materialInstancia.SetFloat(ID_BorderWidth, larguraBorda);
            materialInstancia.SetColor(ID_BorderColor, corBorda);
        }
    }

    private void SetTextColor(Color cor)
    {
        if (textosEIcones == null) return;
        foreach (var g in textosEIcones)
        {
            if (g != null) g.color = cor;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // API pública — usada pelo ARTemplateMenuManager para trocar estado
    // ─────────────────────────────────────────────────────────────────

    /// <summary>Define se este NavItem está selecionado (ex: aba "Rios" ativa).</summary>
    public void SetSelecionado(bool valor)
    {
        selecionado = valor;
        AplicarEstilo();
    }

    /// <summary>Troca o raio de borda em runtime (ex: transformar em pill dinamicamente).</summary>
    public void SetCornerRadius(float novoRaio)
    {
        cornerRadius = novoRaio;
        AplicarEstilo();
    }

    // ─────────────────────────────────────────────────────────────────
    // Animação de toque
    // ─────────────────────────────────────────────────────────────────

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!animarToque) return;
        IniciarAnimacao(escalaOriginal * escalaPressionado);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!animarToque) return;
        IniciarAnimacao(escalaOriginal);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Reservado para feedback extra (ex: som de clique), se necessário no futuro.
    }

    private void IniciarAnimacao(Vector3 alvo)
    {
        if (animacaoAtual != null) StopCoroutine(animacaoAtual);
        animacaoAtual = StartCoroutine(AnimarEscala(alvo));
    }

    private IEnumerator AnimarEscala(Vector3 alvo)
    {
        Vector3 inicio = rectTransform.localScale;
        float tempo = 0f;

        while (tempo < duracaoAnimacao)
        {
            tempo += Time.unscaledDeltaTime;
            float t = curvaAnimacao.Evaluate(Mathf.Clamp01(tempo / duracaoAnimacao));
            rectTransform.localScale = Vector3.LerpUnclamped(inicio, alvo, t);
            yield return null;
        }

        rectTransform.localScale = alvo;
    }
}