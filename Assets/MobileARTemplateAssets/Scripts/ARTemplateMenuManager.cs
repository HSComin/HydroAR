using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using TMPro;
using UnityEngine.XR.ARSubsystems;

// ─────────────────────────────────────────────────────────────────────────────
// Interface
// ─────────────────────────────────────────────────────────────────────────────

public interface IInstanciationController
{
    bool DeveBloquearInstanciacaoManual();
}

// ─────────────────────────────────────────────────────────────────────────────
// ARTemplateMenuManager
// ─────────────────────────────────────────────────────────────────────────────

public class ARTemplateMenuManager : MonoBehaviour, IInstanciationController
{
    // =========================================================================
    // Tipos de dados serializados
    // =========================================================================

    [System.Serializable]
    public class PlanoAgua
    {
        public string nomePlano;
        public Transform plano;
        public float alturaMaxima = 1f;
        public float velocidadeSubida = 0.1f;
    }

    [System.Serializable]
    public class ModeloEnchente
    {
        public string nome;
        public string rio;
        public List<PlanoAgua> planosAgua;
        public GameObject prefab;

        [HideInInspector]
        public GameObject instanciaAtual;
    }

    [System.Serializable]
    public class Waypoint
    {
        public string nomeRio;
        public string nomeRioTexto;
        public Button botao;
        public Image imagemIcone;
        public Sprite spriteNormal;
        public Sprite spriteSelecionado;
    }

    [System.Serializable]
    public struct SelectionBoxEntry
    {
        public string nomeModelo;
        public GameObject selectionBox;
    }

    private enum TelaAtiva { Nenhuma, Lista, Mapa, Preview }

    // =========================================================================
    // Campos serializados — UI principal
    // =========================================================================

    [Header("Pesquisa e Lista")]
    [SerializeField] private TMP_InputField campoPesquisa;
    [SerializeField] private Transform conteudoLista;
    [SerializeField] private ScrollRect scroll;

    [Header("Waypoints e Mapa")]
    [SerializeField] private List<Waypoint> waypoints;
    [SerializeField] private TMP_Text textoRioSelecionado;
    [SerializeField] private Button botaoVerModelo;
    [SerializeField] private GameObject allWaypoints;

    [Header("AR")]
    [SerializeField] private ARRaycastManager m_RaycastManager;
    [SerializeField] private List<ModeloEnchente> modelosEnchente;
    [SerializeField] private List<SelectionBoxEntry> selectionBoxes = new List<SelectionBoxEntry>();

    [Header("Telas")]
    [SerializeField] private GameObject fundo;
    [SerializeField] private GameObject telaAbertura;
    [SerializeField] private GameObject telaUI;
    [SerializeField] private GameObject telaLista;
    [SerializeField] private GameObject telaMapa;
    [SerializeField] private GameObject telaPreview;

    [Header("Painéis de Detalhes")]
    [SerializeField] private GameObject detalhesAmazonas;
    [SerializeField] private GameObject detalhesParana;
    [SerializeField] private GameObject detalhesJacui;

    [Header("Camera e Dicas")]
    [SerializeField] private GameObject cameraAR;
    [SerializeField] private GameObject[] dicas;
    [SerializeField] private GameObject firstHint;
    [SerializeField] private GameObject lastHint;
    [SerializeField] private GameObject nextButton;

    // =========================================================================
    // Campos serializados — Botões
    // =========================================================================

    [Header("Botões de Criação")]
    [SerializeField] Button m_CreateButtonAmazonas;
    [SerializeField] Button m_CreateButtonParana;
    [SerializeField] Button m_CreateButtonJacui;

    [Header("Botões de Ação")]
    [SerializeField] Button m_DeleteButton;
    [SerializeField] Button m_BackButton;
    [SerializeField] Button m_FloodButton;
    [SerializeField] Button m_StopFloodButton;
    [SerializeField] Button m_OptionsButton;
    [SerializeField] Button m_HintsButton;
    [SerializeField] Button m_CancelButton;

    // =========================================================================
    // Campos serializados — Menus de Objetos
    // =========================================================================

    [Header("Menus de Objetos")]
    [SerializeField] GameObject m_ModalMenu;
    [SerializeField] GameObject m_ObjectMenuAmazonas;
    [SerializeField] GameObject m_ObjectMenuParana;
    [SerializeField] GameObject m_ObjectMenuJacui;
    [SerializeField] Animator m_ObjectMenuAnimatorAmazonas;
    [SerializeField] Animator m_ObjectMenuAnimatorParana;
    [SerializeField] Animator m_ObjectMenuAnimatorJacui;

    // =========================================================================
    // Campos serializados — Sistema AR/XR
    // =========================================================================

    [Header("Spawner e Interação")]
    [SerializeField] ObjectSpawner m_ObjectSpawner;
    [SerializeField] XRInteractionGroup m_InteractionGroup;

    [Header("Debug")]
    [SerializeField] DebugSlider m_DebugPlaneSlider;
    [SerializeField] GameObject m_DebugPlane;
    [SerializeField] ARPlaneManager m_PlaneManager;
    [SerializeField] ARDebugMenu m_DebugMenu;
    [SerializeField] DebugSlider m_DebugMenuSlider;

    [Header("Input")]
    [SerializeField] XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");
    [SerializeField] XRInputValueReader<Vector2> m_DragCurrentPositionInput = new XRInputValueReader<Vector2>("Drag Current Position");

    // =========================================================================
    // Propriedades públicas
    // =========================================================================

    public Button createButtonAmazonas { get => m_CreateButtonAmazonas; set => m_CreateButtonAmazonas = value; }
    public Button createButtonParana { get => m_CreateButtonParana; set => m_CreateButtonParana = value; }
    public Button createButtonJacui { get => m_CreateButtonJacui; set => m_CreateButtonJacui = value; }
    public Button deleteButton { get => m_DeleteButton; set => m_DeleteButton = value; }
    public Button backButton { get => m_BackButton; set => m_BackButton = value; }
    public Button floodButton { get => m_FloodButton; set => m_FloodButton = value; }
    public Button stopFloodButton { get => m_StopFloodButton; set => m_StopFloodButton = value; }
    public Button optionsButton { get => m_OptionsButton; set => m_OptionsButton = value; }
    public Button hintsButton { get => m_HintsButton; set => m_HintsButton = value; }
    public Button cancelButton { get => m_CancelButton; set => m_CancelButton = value; }
    public GameObject objectMenuAmazonas { get => m_ObjectMenuAmazonas; set => m_ObjectMenuAmazonas = value; }
    public GameObject objectMenuParana { get => m_ObjectMenuParana; set => m_ObjectMenuParana = value; }
    public GameObject objectMenuJacui { get => m_ObjectMenuJacui; set => m_ObjectMenuJacui = value; }
    public GameObject modalMenu { get => m_ModalMenu; set => m_ModalMenu = value; }
    public Animator objectMenuAnimatorAmazonas { get => m_ObjectMenuAnimatorAmazonas; set => m_ObjectMenuAnimatorAmazonas = value; }
    public Animator objectMenuAnimatorParana { get => m_ObjectMenuAnimatorParana; set => m_ObjectMenuAnimatorParana = value; }
    public Animator objectMenuAnimatorJacui { get => m_ObjectMenuAnimatorJacui; set => m_ObjectMenuAnimatorJacui = value; }
    public ObjectSpawner objectSpawner { get => m_ObjectSpawner; set => m_ObjectSpawner = value; }
    public XRInteractionGroup interactionGroup { get => m_InteractionGroup; set => m_InteractionGroup = value; }
    public DebugSlider debugPlaneSlider { get => m_DebugPlaneSlider; set => m_DebugPlaneSlider = value; }
    public GameObject debugPlane { get => m_DebugPlane; set => m_DebugPlane = value; }
    public ARPlaneManager planeManager { get => m_PlaneManager; set => m_PlaneManager = value; }
    public ARDebugMenu debugMenu { get => m_DebugMenu; set => m_DebugMenu = value; }
    public DebugSlider debugMenuSlider { get => m_DebugMenuSlider; set => m_DebugMenuSlider = value; }

    public XRInputValueReader<Vector2> tapStartPositionInput
    {
        get => m_TapStartPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
    }

    public XRInputValueReader<Vector2> dragCurrentPositionInput
    {
        get => m_DragCurrentPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_DragCurrentPositionInput, value, this);
    }

    // =========================================================================
    // Estado público estático
    // =========================================================================

    public static System.Func<bool> DeveBloquearInstanciacao;
    public static bool InstanciacaoBloqueada = false;

    // =========================================================================
    // Configuração pública
    // =========================================================================

    public float tempoAbertura = 4.5f;

    // =========================================================================
    // Estado privado
    // =========================================================================

    // — Telas —
    private TelaAtiva telaAtual = TelaAtiva.Nenhuma;
    private TelaAtiva telaAnterior = TelaAtiva.Nenhuma;

    // — Menus —
    private bool m_IsPointerOverUI;
    private bool m_ShowObjectMenuAmazonas;
    private bool m_ShowObjectMenuParana;
    private bool m_ShowObjectMenuJacui;
    private bool m_ShowOptionsModal;
    private bool m_InitializingDebugMenu;

    // — Tutorial —
    private bool tutorialAtivo = false;
    private int indiceAtual = 0;

    // — Modelos e Enchente —
    private ModeloEnchente modeloSelecionado;
    private ModeloEnchente modeloFixadoParaAR = null;
    private string nomeRioSelecionado = "";
    private string nomeModeloSelecionado = "";
    private bool aguardandoInstanciarModeloSelecionado = false;
    private bool bloquearInstanciacaoTemporaria = false;
    private GameObject objetoInstanciadoAtual;
    private Dictionary<PlanoAgua, float> alturasIniciais = new Dictionary<PlanoAgua, float>();
    private bool enchenteAtiva = false;
    private bool enchenteDescendo = false;
    private bool enchenteEmAndamento = false;

    // — Lista —
    private List<GameObject> itensLista = new List<GameObject>();
    private float listaOffsetMaxY;
    // Cache de TMP_Text por item da lista (evita GetComponentsInChildren todo frame)
    private List<TMP_Text[]> cacheTextoLista = new List<TMP_Text[]>();

    // — Planos AR —
    private readonly List<ARFeatheredPlaneMeshVisualizerCompanion> featheredPlaneMeshVisualizerCompanions
        = new List<ARFeatheredPlaneMeshVisualizerCompanion>();

    // — Reutilização de lista de raycast (evita alloc no Update) —
    private readonly List<ARRaycastHit> hitsReutilizavel = new List<ARRaycastHit>();

    // — Animação de abertura —
    private Animator anim;

    // =========================================================================
    // Unity: OnEnable / OnDisable
    // =========================================================================

    void OnEnable() => m_PlaneManager.trackablesChanged.AddListener(OnPlaneChanged);
    void OnDisable() => m_PlaneManager.trackablesChanged.RemoveListener(OnPlaneChanged);

    // =========================================================================
    // Unity: Start
    // =========================================================================

    void Start()
    {
        m_DebugMenu.gameObject.SetActive(true);
        m_InitializingDebugMenu = true;
        m_ObjectSpawner.objectSpawned += OnObjectSpawned;

        anim = telaAbertura.GetComponent<Animator>();
        telaUI.SetActive(false);
        fundo.SetActive(true);
        Invoke(nameof(ExecutarFadeOut), tempoAbertura);

        listaOffsetMaxY = conteudoLista.GetComponent<RectTransform>().offsetMax.y;

        HideMenu();
        m_PlaneManager.planePrefab = m_DebugPlane;

        // Registra listeners dos waypoints
        foreach (var wp in waypoints)
        {
            // Captura local necessária para o closure dentro do loop
            Waypoint wpLocal = wp;
            wp.botao.onClick.AddListener(() => SelecionarRio(wpLocal));
        }

        botaoVerModelo.interactable = false;

        // Preenche cache de itens e textos da lista
        foreach (Transform item in conteudoLista)
        {
            itensLista.Add(item.gameObject);
            cacheTextoLista.Add(item.GetComponentsInChildren<TMP_Text>(true));
        }

        campoPesquisa.onValueChanged.AddListener(FiltrarLista);
    }

    // =========================================================================
    // Unity: Update
    // =========================================================================

    void Update()
    {
        // — Inicialização do Debug Menu (uma única vez) —
        if (m_InitializingDebugMenu)
        {
            m_DebugMenu.gameObject.SetActive(false);
            m_InitializingDebugMenu = false;
        }

        // — Tecla Escape —
        ProcessarEscape();

        // — Verificação de consistência de estado (barata: sem log por padrão) —
        if (!string.IsNullOrEmpty(nomeRioSelecionado) && modeloSelecionado != null)
        {
            if (!string.Equals(modeloSelecionado.rio, nomeRioSelecionado, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError($"INCONSISTÊNCIA: Modelo '{modeloSelecionado.nome}' ({modeloSelecionado.rio}) != rio selecionado ({nomeRioSelecionado})");
                LimparEstadoCompleto();
                return;
            }
        }

        // — Garante que selection boxes não fiquem ativas sem modelo selecionado —
        if (string.IsNullOrEmpty(nomeModeloSelecionado) && AlgumSelectionBoxAtivo())
        {
            LimparSelectionBoxesCompletamente();
        }

        // — Lógica de colocação/reposicionamento AR —
        ProcessarInstanciacaoAR();

        // — Animação de enchente —
        if (enchenteAtiva) ProcessarEnchenteSubindo();
        if (enchenteDescendo) ProcessarEnchenteDescendo();

        // — Fechar menus ao tocar fora —
        ProcessarFechamentoDeMenus();
    }

    // =========================================================================
    // Submétodos do Update
    // =========================================================================

    private void ProcessarEscape()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (m_ModalMenu.activeSelf)
        {
            m_ModalMenu.SetActive(false);
            m_ShowOptionsModal = false;
        }
        else if (m_ShowObjectMenuAmazonas || m_ShowObjectMenuParana || m_ShowObjectMenuJacui)
        {
            HideMenu();
        }
        else if (telaAtual == TelaAtiva.Lista || telaAtual == TelaAtiva.Mapa)
        {
            MostrarTelaLista();
        }
    }

    private void ProcessarInstanciacaoAR()
    {
        // MODO 1 — Instanciação Automática (Tela Mapa, sem instância)
        if (telaAtual == TelaAtiva.Mapa &&
            modeloSelecionado != null &&
            modeloSelecionado.instanciaAtual == null &&
            aguardandoInstanciarModeloSelecionado)
        {
            if (m_RaycastManager.Raycast(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f),
                                         hitsReutilizavel, TrackableType.Planes))
            {
                Pose pose = hitsReutilizavel[0].pose;
                GameObject modelo = Instantiate(modeloSelecionado.prefab, pose.position, pose.rotation);
                modeloSelecionado.instanciaAtual = modelo;
                objetoInstanciadoAtual = modelo;
                aguardandoInstanciarModeloSelecionado = true;
                enchenteEmAndamento = false;
                enchenteAtiva = false;
                enchenteDescendo = false;
                bloquearInstanciacaoTemporaria = false;
                m_DeleteButton.gameObject.SetActive(false);
                OnObjectSpawned(modelo);
            }
            return;
        }

        // MODO 2 — Reposicionamento via toque (Tela Mapa, com instância)
        if (telaAtual == TelaAtiva.Mapa &&
            modeloSelecionado != null &&
            modeloSelecionado.instanciaAtual != null &&
            aguardandoInstanciarModeloSelecionado &&
            !bloquearInstanciacaoTemporaria &&
            !enchenteEmAndamento)
        {
            ProcessarToqueReposicionamentoMapa();
            return;
        }

        // MODO 3 — Instanciação Manual via toque (Tela Lista)
        if (telaAtual == TelaAtiva.Lista &&
            aguardandoInstanciarModeloSelecionado &&
            !bloquearInstanciacaoTemporaria &&
            !string.IsNullOrEmpty(nomeRioSelecionado))
        {
            ProcessarToqueInstanciacaoLista();
        }
    }

    private void ProcessarToqueReposicionamentoMapa()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;

        if (m_RaycastManager.Raycast(touch.position, hitsReutilizavel, TrackableType.Planes))
        {
            Pose pose = hitsReutilizavel[0].pose;
            Destroy(modeloSelecionado.instanciaAtual);
            modeloSelecionado.instanciaAtual = null;
            objetoInstanciadoAtual = null;
            alturasIniciais.Clear();
            enchenteAtiva = false;
            enchenteDescendo = false;

            GameObject modelo = Instantiate(modeloSelecionado.prefab, pose.position, pose.rotation);
            modeloSelecionado.instanciaAtual = modelo;
            objetoInstanciadoAtual = modelo;
            OnObjectSpawned(modelo);
        }
    }

    private void ProcessarToqueInstanciacaoLista()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;

        ModeloEnchente modeloParaInstanciar = modeloFixadoParaAR ?? modeloSelecionado;
        if (modeloParaInstanciar == null || modeloParaInstanciar.prefab == null) return;

        if (m_RaycastManager.Raycast(touch.position, hitsReutilizavel, TrackableType.Planes))
        {
            Pose pose = hitsReutilizavel[0].pose;

            DestruirTodosModelos();

            enchenteAtiva = false;
            enchenteDescendo = false;
            alturasIniciais.Clear();

            GameObject novoObjeto = Instantiate(modeloParaInstanciar.prefab, pose.position, pose.rotation);
            modeloParaInstanciar.instanciaAtual = novoObjeto;
            objetoInstanciadoAtual = novoObjeto;
            modeloSelecionado = modeloParaInstanciar;
            nomeModeloSelecionado = modeloParaInstanciar.nome;
            bloquearInstanciacaoTemporaria = false;
            aguardandoInstanciarModeloSelecionado = true;

            OnObjectSpawned(novoObjeto);
        }
    }

    private void ProcessarEnchenteSubindo()
    {
        if (modeloSelecionado == null) return;

        foreach (var plano in modeloSelecionado.planosAgua)
        {
            if (plano.plano == null || !alturasIniciais.TryGetValue(plano, out float alturaInicial)) continue;

            float alturaAlvo = alturaInicial + plano.alturaMaxima;
            float atual = plano.plano.localPosition.y;
            if (Mathf.Abs(atual - alturaAlvo) < 0.0001f) continue;

            Vector3 pos = plano.plano.localPosition;
            pos.y = Mathf.Min(pos.y + plano.velocidadeSubida * Time.deltaTime, alturaAlvo);
            plano.plano.localPosition = pos;
        }
    }

    private void ProcessarEnchenteDescendo()
    {
        if (modeloSelecionado == null) return;

        bool algumDescendo = false;

        foreach (var plano in modeloSelecionado.planosAgua)
        {
            if (plano.plano == null || !alturasIniciais.TryGetValue(plano, out float alturaOriginal)) continue;

            float atual = plano.plano.localPosition.y;
            if (Mathf.Abs(atual - alturaOriginal) <= 0.0001f) continue;

            algumDescendo = true;
            Vector3 pos = plano.plano.localPosition;
            pos.y = Mathf.Max(pos.y - plano.velocidadeSubida * Time.deltaTime, alturaOriginal);
            plano.plano.localPosition = pos;
        }

        if (!algumDescendo)
        {
            enchenteDescendo = false;
            enchenteEmAndamento = false;

            if (telaAtual == TelaAtiva.Mapa)
            {
                bloquearInstanciacaoTemporaria = false;
                aguardandoInstanciarModeloSelecionado = true;
            }
            else if (telaAtual == TelaAtiva.Lista)
            {
                bloquearInstanciacaoTemporaria = false;
            }
        }
    }

    private void ProcessarFechamentoDeMenus()
    {
        bool algumMenuAberto = m_ShowObjectMenuAmazonas || m_ShowObjectMenuParana
                             || m_ShowObjectMenuJacui || m_ShowOptionsModal;

        if (algumMenuAberto)
        {
            m_IsPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);

            bool tocando = (m_TapStartPositionInput.TryReadValue(out Vector2 tapPos) && tapPos != Vector2.zero)
                        || (m_DragCurrentPositionInput.TryReadValue(out Vector2 dragPos) && dragPos != Vector2.zero);

            if (!m_IsPointerOverUI && tocando)
            {
                if (m_ShowObjectMenuAmazonas || m_ShowObjectMenuParana || m_ShowObjectMenuJacui)
                {
                    HideMenu();
                    m_ShowObjectMenuAmazonas = false;
                    m_ShowObjectMenuParana = false;
                    m_ShowObjectMenuJacui = false;
                }

                if (m_ShowOptionsModal)
                {
                    m_ModalMenu.SetActive(false);
                    m_ShowOptionsModal = false;
                }
            }

            m_DeleteButton.gameObject.SetActive(false);
            m_IsPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
        }
        else
        {
            m_IsPointerOverUI = false;

            if (!tutorialAtivo)
            {
                if (telaAtual == TelaAtiva.Mapa)
                {
                    m_DeleteButton.gameObject.SetActive(false);
                }
                else
                {
                    bool temObjetoReal = objetoInstanciadoAtual != null && objetoInstanciadoAtual.activeInHierarchy;
                    m_DeleteButton.gameObject.SetActive(temObjetoReal);

                    if (!temObjetoReal)
                    {
                        m_FloodButton.gameObject.SetActive(false);
                        m_StopFloodButton.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                m_DeleteButton.gameObject.SetActive(false);
            }
        }

        if (!m_IsPointerOverUI && m_ShowOptionsModal)
            m_IsPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
    }

    // =========================================================================
    // Navegação de Telas
    // =========================================================================

    public void MostrarTelaLista()
    {
        telaAtual = TelaAtiva.Lista;
        telaAnterior = telaAtual;
        InstanciacaoBloqueada = false;
        modeloFixadoParaAR = null;

        if (m_ObjectSpawner != null)
        {
            m_ObjectSpawner.enabled = true;
            m_ObjectSpawner.objectSpawned -= OnObjectSpawned;
            m_ObjectSpawner.objectSpawned += OnObjectSpawned;
        }

        AtivarTelaPrincipal(mostrarLista: true, mostrarMapa: false, mostrarPreview: false);
        OcultarTodosMenusAR();
        OcultarBotoesAR();
        allWaypoints.gameObject.SetActive(false);

        DestruirTodosModelos();
        ResetarEstadoModelo();
        AtualizarSelectionBoxes("");
        LimparEstadoCompleto();
    }

    public void MostrarTelaPreview()
    {
        telaAtual = TelaAtiva.Preview;
        telaAnterior = telaAtual;
        InstanciacaoBloqueada = false;

        AtivarTelaPrincipal(mostrarLista: false, mostrarMapa: false, mostrarPreview: true);
        OcultarTodosMenusAR();
        OcultarBotoesAR();
        allWaypoints.gameObject.SetActive(false);

        DestruirTodosModelos();
        ResetarEstadoModelo();
        AtualizarSelectionBoxes("");
        LimparEstadoCompleto();
    }

    public void MostrarTelaMapa()
    {
        telaAtual = TelaAtiva.Mapa;
        telaAnterior = telaAtual;
        InstanciacaoBloqueada = true;
        enchenteEmAndamento = false;

        AtivarTelaPrincipal(mostrarLista: false, mostrarMapa: true, mostrarPreview: false);
        OcultarTodosMenusAR();
        OcultarBotoesAR();
        allWaypoints.gameObject.SetActive(true);

        DestruirTodosModelos();
        ResetarWaypoints();
        alturasIniciais.Clear();
        enchenteAtiva = false;
        enchenteDescendo = false;
        aguardandoInstanciarModeloSelecionado = false;
        bloquearInstanciacaoTemporaria = true;
    }

    public void VoltarParaTelaAnterior()
    {
        switch (telaAnterior)
        {
            case TelaAtiva.Lista: MostrarTelaLista(); break;
            case TelaAtiva.Mapa: MostrarTelaMapa(); break;
            default: MostrarTelaLista(); break;
        }
    }

    // ─── Telas de Detalhes ────────────────────────────────────────────────────

    public void DetalhesAmazonas() => MostrarDetalhes(detalhesAmazonas);
    public void DetalhesParana() => MostrarDetalhes(detalhesParana);
    public void DetalhesJacui() => MostrarDetalhes(detalhesJacui);

    private void MostrarDetalhes(GameObject painelAlvo)
    {
        LimparEstadoCompleto();
        DestruirTodosModelos();

        enchenteAtiva = false;
        enchenteDescendo = false;
        m_FloodButton.gameObject.SetActive(false);
        m_StopFloodButton.gameObject.SetActive(false);
        m_DeleteButton.gameObject.SetActive(false);

        detalhesAmazonas.SetActive(painelAlvo == detalhesAmazonas);
        detalhesParana.SetActive(painelAlvo == detalhesParana);
        detalhesJacui.SetActive(painelAlvo == detalhesJacui);
        telaLista.SetActive(false);
        telaMapa.SetActive(false);
    }

    // =========================================================================
    // Abertura de Câmera AR por Rio
    // =========================================================================

    public void AbrirCameraARAmazonas() => AbrirCameraARParaRio("Amazonas", spawnIndex: 0);
    public void AbrirCameraARParana() => AbrirCameraARParaRio("Parana", spawnIndex: 3);
    public void AbrirCameraARJacui() => AbrirCameraARParaRio("Jacui", spawnIndex: 6);

    private void AbrirCameraARParaRio(string rio, int spawnIndex)
    {
        if (cameraAR == null) return;

        telaAtual = TelaAtiva.Lista;
        InstanciacaoBloqueada = false;

        LimparEstadoCompleto();
        LimparSelectionBoxesCompletamente();

        nomeRioSelecionado = rio;
        SelecionarModeloInicialAutomaticamente(rio);

        if (m_ObjectSpawner != null)
            m_ObjectSpawner.spawnOptionIndex = spawnIndex;

        aguardandoInstanciarModeloSelecionado = true;
        bloquearInstanciacaoTemporaria = false;

        if (!string.IsNullOrEmpty(nomeModeloSelecionado))
            AtualizarSelectionBoxes(nomeModeloSelecionado);

        botaoVerModelo.interactable = false;
        m_DeleteButton.gameObject.SetActive(false);
        m_FloodButton.gameObject.SetActive(false);
        m_StopFloodButton.gameObject.SetActive(false);

        cameraAR.SetActive(true);
        telaLista.SetActive(false);
        telaMapa.SetActive(false);
        allWaypoints.gameObject.SetActive(false);

        // Ativa apenas o menu do rio correto
        m_ObjectMenuAmazonas.SetActive(rio == "Amazonas");
        m_ObjectMenuParana.SetActive(rio == "Parana");
        m_ObjectMenuJacui.SetActive(rio == "Jacui");

        m_CreateButtonAmazonas.gameObject.SetActive(rio == "Amazonas");
        m_CreateButtonParana.gameObject.SetActive(rio == "Parana");
        m_CreateButtonJacui.gameObject.SetActive(rio == "Jacui");

        m_OptionsButton.gameObject.SetActive(true);
        m_BackButton.gameObject.SetActive(true);
        m_HintsButton.gameObject.SetActive(true);

        detalhesAmazonas.SetActive(false);
        detalhesParana.SetActive(false);
        detalhesJacui.SetActive(false);
    }

    // =========================================================================
    // Visualização de Modelos Selecionados
    // =========================================================================

    public void VerModeloSelecionadoAmazonas() => VerModeloSelecionadoParaRio("Amazonas");
    public void VerModeloSelecionadoParana() => VerModeloSelecionadoParaRio("Parana");
    public void VerModeloSelecionadoJacui() => VerModeloSelecionadoParaRio("Jacui");

    private void VerModeloSelecionadoParaRio(string rio)
    {
        telaAtual = TelaAtiva.Lista;

        if (modeloSelecionado == null || modeloSelecionado.rio != rio)
        {
            Debug.LogWarning($"Nenhum modelo válido selecionado para o rio {rio}.");
            return;
        }

        aguardandoInstanciarModeloSelecionado = true;
        nomeModeloSelecionado = modeloSelecionado.nome;
        bloquearInstanciacaoTemporaria = false;

        cameraAR.SetActive(true);
        telaLista.SetActive(false);
        telaMapa.SetActive(false);
        allWaypoints.gameObject.SetActive(false);

        m_ObjectMenuAmazonas.SetActive(rio == "Amazonas");
        m_ObjectMenuParana.SetActive(rio == "Parana");
        m_ObjectMenuJacui.SetActive(rio == "Jacui");

        m_CreateButtonAmazonas.gameObject.SetActive(rio == "Amazonas");
        m_CreateButtonParana.gameObject.SetActive(rio == "Parana");
        m_CreateButtonJacui.gameObject.SetActive(rio == "Jacui");

        m_OptionsButton.gameObject.SetActive(true);
        m_BackButton.gameObject.SetActive(true);
        m_HintsButton.gameObject.SetActive(true);

        m_ShowObjectMenuAmazonas = rio == "Amazonas";
        m_ShowObjectMenuParana = rio == "Parana";
        m_ShowObjectMenuJacui = rio == "Jacui";

        // Mostra/oculta instâncias conforme o rio
        foreach (var modelo in modelosEnchente)
        {
            if (modelo.instanciaAtual == null) continue;
            bool visivel = modelo.rio == rio;
            modelo.instanciaAtual.SetActive(visivel);
            var selectionBox = modelo.instanciaAtual.transform.Find("SelectionBox");
            if (selectionBox != null) selectionBox.gameObject.SetActive(visivel);
        }

        indiceAtual = 0;
        enchenteAtiva = false;
        enchenteDescendo = false;
        alturasIniciais.Clear();
    }

    // =========================================================================
    // Seleção e Instanciação de Modelos
    // =========================================================================

    public void SelecionarModeloAR(string nomeModelo)
    {
        var modelo = modelosEnchente.Find(m => m.nome == nomeModelo);
        if (modelo == null)
        {
            Debug.LogWarning("Modelo não encontrado: " + nomeModelo);
            return;
        }

        modeloSelecionado = modelo;
        nomeModeloSelecionado = nomeModelo;
        nomeRioSelecionado = modelo.rio;
        aguardandoInstanciarModeloSelecionado = true;
        bloquearInstanciacaoTemporaria = false;
        AtualizarSelectionBoxes(nomeModelo);
    }

    public void VerARModeloDoScroll()
    {
        if (string.IsNullOrEmpty(nomeModeloSelecionado))
        {
            Debug.LogWarning("Nenhum modelo selecionado no scroll.");
            return;
        }
        AbrirARComModelo(nomeModeloSelecionado);
    }

    public void AbrirARComModelo(string nomeModelo)
    {
        if (m_ObjectSpawner != null)
        {
            m_ObjectSpawner.objectSpawned -= OnObjectSpawned;
            m_ObjectSpawner.enabled = false;
            m_ObjectSpawner.spawnOptionIndex = -1;
        }

        var modelo = modelosEnchente.Find(m => m.nome == nomeModelo);
        if (modelo == null || modelo.prefab == null)
        {
            Debug.LogWarning("Modelo não encontrado: " + nomeModelo);
            return;
        }

        modeloFixadoParaAR = modelo;
        LimparEstadoCompleto();
        LimparSelectionBoxesCompletamente();

        nomeModeloSelecionado = modelo.nome;
        nomeRioSelecionado = modelo.rio;
        modeloSelecionado = modelo;
        aguardandoInstanciarModeloSelecionado = true;
        bloquearInstanciacaoTemporaria = false;
        telaAtual = TelaAtiva.Lista;

        cameraAR.SetActive(true);
        telaPreview.SetActive(false);
        telaLista.SetActive(false);
        telaMapa.SetActive(false);
        allWaypoints.gameObject.SetActive(false);

        OcultarTodosMenusAR();
        m_CreateButtonAmazonas.gameObject.SetActive(false);
        m_CreateButtonParana.gameObject.SetActive(false);
        m_CreateButtonJacui.gameObject.SetActive(false);

        m_OptionsButton.gameObject.SetActive(true);
        m_BackButton.gameObject.SetActive(true);
        m_HintsButton.gameObject.SetActive(true);
        m_DeleteButton.gameObject.SetActive(false);
        m_FloodButton.gameObject.SetActive(false);
        m_StopFloodButton.gameObject.SetActive(false);

        detalhesAmazonas.SetActive(false);
        detalhesParana.SetActive(false);
        detalhesJacui.SetActive(false);

        AtualizarSelectionBoxes(nomeModeloSelecionado);
    }

    public void VerModelo()
    {
        telaAtual = TelaAtiva.Mapa;
        modeloSelecionado = modelosEnchente.Find(m => m.nome == nomeModeloSelecionado);

        if (modeloSelecionado == null || modeloSelecionado.prefab == null)
        {
            Debug.LogWarning("Modelo não encontrado ou prefab ausente: " + nomeModeloSelecionado);
            return;
        }

        aguardandoInstanciarModeloSelecionado = true;
        bloquearInstanciacaoTemporaria = true;

        cameraAR.SetActive(true);
        telaLista.SetActive(false);
        telaMapa.SetActive(false);
        allWaypoints.gameObject.SetActive(false);

        m_CreateButtonAmazonas.gameObject.SetActive(false);
        m_CreateButtonParana.gameObject.SetActive(false);
        m_CreateButtonJacui.gameObject.SetActive(false);
        m_OptionsButton.gameObject.SetActive(true);
        m_BackButton.gameObject.SetActive(true);
        m_HintsButton.gameObject.SetActive(true);

        OcultarTodosMenusAR();
        m_DeleteButton.gameObject.SetActive(false);
    }

    // =========================================================================
    // Enchente
    // =========================================================================

    public void IniciarEnchenteNoSelecionado()
    {
        if (modeloSelecionado == null) return;
        enchenteAtiva = true;
        enchenteEmAndamento = true;
        bloquearInstanciacaoTemporaria = true;
        m_FloodButton.gameObject.SetActive(false);
        m_StopFloodButton.gameObject.SetActive(true);
    }

    public void PararEnchente()
    {
        enchenteAtiva = false;
        enchenteDescendo = true;
        enchenteEmAndamento = true;
        bloquearInstanciacaoTemporaria = true;
        m_FloodButton.gameObject.SetActive(true);
        m_StopFloodButton.gameObject.SetActive(false);
    }

    // =========================================================================
    // Tutorial / Dicas
    // =========================================================================

    public void IniciarTutorial()
    {
        tutorialAtivo = true;
        indiceAtual = 0;
        nextButton.gameObject.SetActive(true);
        firstHint.gameObject.SetActive(true);

        OcultarBotoesAR();
        m_CreateButtonAmazonas.gameObject.SetActive(false);
        m_CreateButtonParana.gameObject.SetActive(false);
        m_CreateButtonJacui.gameObject.SetActive(false);
        OcultarTodosMenusAR();
    }

    public void AtualizarDicas()
    {
        for (int i = 0; i < dicas.Length; i++)
            dicas[i].SetActive(i == indiceAtual);
    }

    public void MostrarProximaDica()
    {
        indiceAtual++;

        if (indiceAtual >= dicas.Length)
        {
            nextButton.gameObject.SetActive(false);
            lastHint.gameObject.SetActive(false);

            m_ObjectMenuAmazonas.SetActive(true);
            m_ObjectMenuParana.SetActive(true);
            m_ObjectMenuJacui.SetActive(true);

            if (telaAtual == TelaAtiva.Lista)
            {
                string rioUp = nomeRioSelecionado.ToUpper();
                m_CreateButtonAmazonas.gameObject.SetActive(rioUp == "AMAZONAS");
                m_CreateButtonParana.gameObject.SetActive(rioUp == "PARANA");
                m_CreateButtonJacui.gameObject.SetActive(rioUp == "JACUI");
            }
            else
            {
                m_CreateButtonAmazonas.gameObject.SetActive(false);
                m_CreateButtonParana.gameObject.SetActive(false);
                m_CreateButtonJacui.gameObject.SetActive(false);
            }

            m_OptionsButton.gameObject.SetActive(true);
            m_BackButton.gameObject.SetActive(true);

            bool temObjeto = objetoInstanciadoAtual != null && objetoInstanciadoAtual.activeInHierarchy;
            m_DeleteButton.gameObject.SetActive(temObjeto);
            m_FloodButton.gameObject.SetActive(temObjeto);
            m_StopFloodButton.gameObject.SetActive(false);
            m_HintsButton.gameObject.SetActive(true);

            tutorialAtivo = false;
            return;
        }

        AtualizarDicas();
    }

    // =========================================================================
    // Waypoints e Mapa
    // =========================================================================

    private void SelecionarRio(Waypoint selecionado)
    {
        nomeModeloSelecionado = selecionado.nomeRio;
        textoRioSelecionado.text = selecionado.nomeRioTexto;
        botaoVerModelo.interactable = true;

        foreach (var wp in waypoints)
        {
            if (wp.imagemIcone == null) continue;
            wp.imagemIcone.sprite = (wp == selecionado) ? wp.spriteSelecionado : wp.spriteNormal;
        }
    }

    public void ResetarWaypoints()
    {
        nomeModeloSelecionado = null;
        textoRioSelecionado.text = "";
        botaoVerModelo.interactable = false;

        foreach (var wp in waypoints)
        {
            if (wp.imagemIcone != null)
                wp.imagemIcone.sprite = wp.spriteNormal;
        }
    }

    // =========================================================================
    // Lista / Pesquisa
    // =========================================================================

    public void FiltrarLista(string texto)
    {
        if (conteudoLista == null || scroll == null) return;

        if (string.IsNullOrEmpty(texto))
        {
            foreach (var item in itensLista)
                item.SetActive(true);

            scroll.vertical = true;
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f;
            return;
        }

        string textoLower = texto.ToLower();
        int indiceEncontrado = -1;

        for (int i = 0; i < itensLista.Count; i++)
        {
            // Usa cache de TMP_Text para evitar GetComponentsInChildren por frame
            TMP_Text[] textos = cacheTextoLista[i];
            bool corresponde = false;

            foreach (var t in textos)
            {
                if (t.text.ToLower().Contains(textoLower))
                {
                    corresponde = true;
                    break;
                }
            }

            itensLista[i].SetActive(corresponde);

            if (corresponde && indiceEncontrado == -1)
                indiceEncontrado = i;
        }

        if (indiceEncontrado >= 0)
            StartCoroutine(IrParaIndice(indiceEncontrado));
    }

    // =========================================================================
    // Menus de Objetos
    // =========================================================================

    public void ShowMenuAmazonas()
    {
        m_ShowObjectMenuAmazonas = true;
        m_ObjectMenuAmazonas.SetActive(true);
        if (!m_ObjectMenuAnimatorAmazonas.GetBool("Show"))
            m_ObjectMenuAnimatorAmazonas.SetBool("Show", true);
    }

    public void ShowMenuParana()
    {
        m_ShowObjectMenuParana = true;
        m_ObjectMenuParana.SetActive(true);
        if (!m_ObjectMenuAnimatorParana.GetBool("Show"))
            m_ObjectMenuAnimatorParana.SetBool("Show", true);
    }

    public void ShowMenuJacui()
    {
        m_ShowObjectMenuJacui = true;
        m_ObjectMenuJacui.SetActive(true);
        if (!m_ObjectMenuAnimatorJacui.GetBool("Show"))
            m_ObjectMenuAnimatorJacui.SetBool("Show", true);
    }

    public void HideMenu()
    {
        m_ObjectMenuAnimatorAmazonas.SetBool("Show", false);
        m_ObjectMenuAnimatorParana.SetBool("Show", false);
        m_ObjectMenuAnimatorJacui.SetBool("Show", false);
        m_ShowObjectMenuAmazonas = false;
        m_ShowObjectMenuParana = false;
        m_ShowObjectMenuJacui = false;
    }

    public void ShowHideModal()
    {
        bool aberto = m_ModalMenu.activeSelf;
        m_ShowOptionsModal = !aberto;
        m_ModalMenu.SetActive(!aberto);
    }

    public void SetObjectToSpawn(int objectIndex)
    {
        if (m_ObjectSpawner == null)
        {
            Debug.LogWarning("Object Spawner não configurado.");
            return;
        }

        if (m_ObjectSpawner.objectPrefabs.Count > objectIndex)
            m_ObjectSpawner.spawnOptionIndex = objectIndex;
        else
            Debug.LogWarning("Object Spawner: índice maior que o número de prefabs.");

        HideMenu();
    }

    // =========================================================================
    // Debug Plane / Debug Menu
    // =========================================================================

    public void ShowHideDebugPlane()
    {
        bool ativo = m_DebugPlaneSlider.value == 1;
        m_DebugPlaneSlider.value = ativo ? 0 : 1;
        ChangePlaneVisibility(!ativo);
    }

    public void ShowHideDebugMenu()
    {
        bool ativo = m_DebugMenu.gameObject.activeSelf;
        m_DebugMenuSlider.value = ativo ? 0 : 1;
        m_DebugMenu.gameObject.SetActive(!ativo);
    }

    private void ChangePlaneVisibility(bool setVisible)
    {
        int count = featheredPlaneMeshVisualizerCompanions.Count;
        for (int i = 0; i < count; i++)
            featheredPlaneMeshVisualizerCompanions[i].visualizeSurfaces = setVisible;
    }

    // =========================================================================
    // Deletar / Limpar Objetos
    // =========================================================================

    public void DeletarObjetoInstanciado()
    {
        if (objetoInstanciadoAtual != null)
        {
            Destroy(objetoInstanciadoAtual);
            objetoInstanciadoAtual = null;
        }

        foreach (var m in modelosEnchente)
        {
            if (m.instanciaAtual != null)
            {
                Destroy(m.instanciaAtual);
                m.instanciaAtual = null;
            }
        }

        enchenteAtiva = false;
        enchenteDescendo = false;
        alturasIniciais.Clear();
        m_FloodButton.gameObject.SetActive(false);
        m_StopFloodButton.gameObject.SetActive(false);
        m_DeleteButton.gameObject.SetActive(false);

        // Restaura para permitir reposicionamento
        ModeloEnchente modeloParaRestaurar = modeloFixadoParaAR ?? modeloSelecionado;
        if (modeloParaRestaurar != null)
        {
            modeloSelecionado = modeloParaRestaurar;
            nomeModeloSelecionado = modeloParaRestaurar.nome;
            nomeRioSelecionado = modeloParaRestaurar.rio;
            aguardandoInstanciarModeloSelecionado = true;
            bloquearInstanciacaoTemporaria = false;
        }
    }

    public void ClearAllObjects()
    {
        foreach (var modelo in modelosEnchente)
        {
            if (modelo.instanciaAtual != null)
            {
                Destroy(modelo.instanciaAtual);
                modelo.instanciaAtual = null;
            }
        }

        objetoInstanciadoAtual = null;
        modeloSelecionado = null;
        nomeModeloSelecionado = "";
        nomeRioSelecionado = "";
        enchenteAtiva = false;
        enchenteDescendo = false;
        alturasIniciais.Clear();

        m_FloodButton.gameObject.SetActive(false);
        m_StopFloodButton.gameObject.SetActive(false);
        m_DeleteButton.gameObject.SetActive(false);
    }

    // =========================================================================
    // IInstanciationController
    // =========================================================================

    public bool DeveBloquearInstanciacaoManual()
    {
        if (telaAtual == TelaAtiva.Mapa) return true;
        if (bloquearInstanciacaoTemporaria) return true;
        if (!aguardandoInstanciarModeloSelecionado) return true;
        return false;
    }

    // =========================================================================
    // Callbacks de Spawner e Planos AR
    // =========================================================================

    private void OnObjectSpawned(GameObject objeto)
    {
        // Rejeita objetos que não correspondam ao modelo fixado
        if (modeloFixadoParaAR != null)
        {
            string nomeSpawnado = objeto.name.Replace("(Clone)", "").Trim();
            if (nomeSpawnado != modeloFixadoParaAR.nome)
            {
                Destroy(objeto);
                return;
            }
        }

        // Destrói instâncias anteriores
        if (objetoInstanciadoAtual != null && objetoInstanciadoAtual != objeto)
        {
            Destroy(objetoInstanciadoAtual);
            objetoInstanciadoAtual = null;
        }

        foreach (var m in modelosEnchente)
        {
            if (m.instanciaAtual != null && m.instanciaAtual != objeto)
            {
                Destroy(m.instanciaAtual);
                m.instanciaAtual = null;
            }
        }

        enchenteAtiva = false;
        enchenteDescendo = false;
        alturasIniciais.Clear();
        m_FloodButton.gameObject.SetActive(false);
        m_StopFloodButton.gameObject.SetActive(false);

        string nomeModelo = objeto.name.Replace("(Clone)", "").Trim();
        modeloSelecionado = modelosEnchente.Find(m => m.nome == nomeModelo);

        // Registra alturas iniciais dos planos de água
        if (modeloSelecionado != null && modeloSelecionado.planosAgua != null)
        {
            foreach (var plano in modeloSelecionado.planosAgua)
            {
                if (string.IsNullOrEmpty(plano.nomePlano)) continue;

                Transform planoInstanciado = BuscarFilhoPorNome(objeto.transform, plano.nomePlano);
                if (planoInstanciado != null)
                {
                    plano.plano = planoInstanciado;
                    alturasIniciais[plano] = planoInstanciado.localPosition.y;
                }
                else
                {
                    Debug.LogWarning($"Plano '{plano.nomePlano}' não encontrado");
                }
            }

            m_FloodButton.gameObject.SetActive(true);
        }
        else
        {
            m_FloodButton.gameObject.SetActive(false);
        }

        bool mostrarDelete = modeloSelecionado != null && telaAtual == TelaAtiva.Lista;
        m_DeleteButton.gameObject.SetActive(mostrarDelete);

        objetoInstanciadoAtual = objeto;
    }

    void OnPlaneChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
    {
        bool visivel = m_DebugPlaneSlider.value != 0;

        foreach (var plane in eventArgs.added)
        {
            if (plane.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var viz))
            {
                featheredPlaneMeshVisualizerCompanions.Add(viz);
                viz.visualizeSurfaces = visivel;
            }
        }

        foreach (var plane in eventArgs.removed)
        {
            if (plane.Value != null &&
                plane.Value.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var viz))
                featheredPlaneMeshVisualizerCompanions.Remove(viz);
        }

        // Reconcilia lista com trackables reais se houver divergência
        if (m_PlaneManager.trackables.count != featheredPlaneMeshVisualizerCompanions.Count)
        {
            featheredPlaneMeshVisualizerCompanions.Clear();
            foreach (var trackable in m_PlaneManager.trackables)
            {
                if (trackable.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var viz))
                {
                    featheredPlaneMeshVisualizerCompanions.Add(viz);
                    viz.visualizeSurfaces = visivel;
                }
            }
        }
    }

    // =========================================================================
    // Corrotinas de Scroll
    // =========================================================================

    private IEnumerator IrParaIndice(int indice)
    {
        scroll.vertical = true;
        scroll.StopMovement();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        RectTransform contentRect = conteudoLista.GetComponent<RectTransform>();
        float alturaItem = conteudoLista.GetChild(0).GetComponent<RectTransform>().rect.height;

        float spacing = 0f;
        var layout = conteudoLista.GetComponent<VerticalLayoutGroup>();
        if (layout != null) spacing = layout.spacing;

        float posY = indice * (alturaItem + spacing);
        contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, posY);

        scroll.StopMovement();
        scroll.vertical = false;
    }

    private IEnumerator TravaNaTopo()
    {
        scroll.vertical = true;
        scroll.StopMovement();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(conteudoLista.GetComponent<RectTransform>());

        yield return new WaitForEndOfFrame();

        scroll.verticalNormalizedPosition = 1f;

        yield return new WaitForEndOfFrame();
        scroll.StopMovement();
        scroll.vertical = false;
    }

    private IEnumerator RolarParaItem(RectTransform itemRect)
    {
        scroll.vertical = true;
        scroll.StopMovement();

        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(conteudoLista.GetComponent<RectTransform>());

        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        RectTransform contentRect = conteudoLista.GetComponent<RectTransform>();
        RectTransform viewportRect = scroll.viewport;

        float contentHeight = contentRect.rect.height;
        float viewportHeight = viewportRect.rect.height;
        float scrollableHeight = contentHeight - viewportHeight;

        if (scrollableHeight > 0f)
        {
            Vector2 itemPosWorld = itemRect.TransformPoint(Vector2.zero);
            Vector2 itemPosLocal = contentRect.InverseTransformPoint(itemPosWorld);
            float itemTopFromContentTop = (contentHeight * 0.5f) - (itemPosLocal.y + itemRect.rect.height);
            float normalizado = 1f - Mathf.Clamp01(itemTopFromContentTop / scrollableHeight);
            scroll.verticalNormalizedPosition = normalizado;
        }
        else
        {
            scroll.verticalNormalizedPosition = 1f;
        }

        yield return new WaitForEndOfFrame();
        scroll.StopMovement();
        scroll.vertical = false;
    }

    // =========================================================================
    // Métodos auxiliares privados
    // =========================================================================

    private void ExecutarFadeOut()
    {
        anim.SetTrigger("FadeOutStart");
        Invoke(nameof(MostrarTelaLista), 0.9f);
    }

    /// <summary>Ativa/oculta telas principais de UI.</summary>
    private void AtivarTelaPrincipal(bool mostrarLista, bool mostrarMapa, bool mostrarPreview)
    {
        telaUI.SetActive(mostrarLista || mostrarMapa || mostrarPreview);
        telaLista.SetActive(mostrarLista);
        telaMapa.SetActive(mostrarMapa);
        telaPreview.SetActive(mostrarPreview);
        telaAbertura.SetActive(false);
        fundo.SetActive(false);

        detalhesAmazonas.SetActive(false);
        detalhesParana.SetActive(false);
        detalhesJacui.SetActive(false);
    }

    /// <summary>Desativa todos os menus de objetos AR.</summary>
    private void OcultarTodosMenusAR()
    {
        m_ObjectMenuAmazonas.SetActive(false);
        m_ObjectMenuParana.SetActive(false);
        m_ObjectMenuJacui.SetActive(false);
        m_ModalMenu.SetActive(false);
    }

    /// <summary>Desativa todos os botões de ação AR.</summary>
    private void OcultarBotoesAR()
    {
        m_CreateButtonAmazonas.gameObject.SetActive(false);
        m_CreateButtonParana.gameObject.SetActive(false);
        m_CreateButtonJacui.gameObject.SetActive(false);
        m_OptionsButton.gameObject.SetActive(false);
        m_BackButton.gameObject.SetActive(false);
        m_HintsButton.gameObject.SetActive(false);
        m_FloodButton.gameObject.SetActive(false);
        m_StopFloodButton.gameObject.SetActive(false);
        m_DeleteButton.gameObject.SetActive(false);
    }

    /// <summary>Reseta o estado de modelo/enchente sem destruir instâncias.</summary>
    private void ResetarEstadoModelo()
    {
        modeloSelecionado = null;
        nomeModeloSelecionado = "";
        nomeRioSelecionado = "";

        if (m_ObjectSpawner != null)
            m_ObjectSpawner.spawnOptionIndex = -1;

        alturasIniciais.Clear();
        enchenteAtiva = false;
        enchenteDescendo = false;
        aguardandoInstanciarModeloSelecionado = false;
        bloquearInstanciacaoTemporaria = true;
    }

    /// <summary>Destrói todas as instâncias de modelos ativas.</summary>
    private void DestruirTodosModelos()
    {
        if (objetoInstanciadoAtual != null)
        {
            Destroy(objetoInstanciadoAtual);
            objetoInstanciadoAtual = null;
        }

        foreach (var modelo in modelosEnchente)
        {
            if (modelo.instanciaAtual != null)
            {
                Destroy(modelo.instanciaAtual);
                modelo.instanciaAtual = null;
            }
        }
    }

    private void LimparEstadoCompleto()
    {
        modeloSelecionado = null;
        nomeModeloSelecionado = "";
        nomeRioSelecionado = "";
        alturasIniciais.Clear();
        enchenteAtiva = false;
        enchenteDescendo = false;
        aguardandoInstanciarModeloSelecionado = false;
        bloquearInstanciacaoTemporaria = true;

        if (m_ObjectSpawner != null)
            m_ObjectSpawner.spawnOptionIndex = -1;

        LimparSelectionBoxesCompletamente();
    }

    private void AtualizarSelectionBoxes(string nomeModelo)
    {
        foreach (var entry in selectionBoxes)
        {
            if (entry.selectionBox != null)
                entry.selectionBox.SetActive(entry.nomeModelo == nomeModelo);
        }
    }

    private void LimparSelectionBoxesCompletamente()
    {
        foreach (var entry in selectionBoxes)
        {
            if (entry.selectionBox != null)
                entry.selectionBox.SetActive(false);
        }
    }

    private bool AlgumSelectionBoxAtivo()
    {
        foreach (var box in selectionBoxes)
        {
            if (box.selectionBox != null && box.selectionBox.activeSelf) return true;
        }
        return false;
    }

    private void SelecionarModeloInicialAutomaticamente(string rioDesejado)
    {
        if (string.IsNullOrEmpty(nomeRioSelecionado) ||
            !string.Equals(nomeRioSelecionado, rioDesejado, System.StringComparison.OrdinalIgnoreCase))
            nomeRioSelecionado = rioDesejado;

        foreach (var modelo in modelosEnchente)
        {
            if (modelo.instanciaAtual == null &&
                string.Equals(modelo.rio, rioDesejado, System.StringComparison.OrdinalIgnoreCase))
            {
                modeloSelecionado = modelo;
                nomeModeloSelecionado = modelo.nome;
                return;
            }
        }

        Debug.LogWarning($"Nenhum modelo disponível para o rio {rioDesejado}");
    }

    private bool ValidarConsistenciaRioModelo()
    {
        if (string.IsNullOrEmpty(nomeRioSelecionado))
        {
            Debug.LogError("Nenhum rio selecionado!");
            return false;
        }

        if (modeloSelecionado == null)
        {
            Debug.LogError("Nenhum modelo selecionado!");
            return false;
        }

        if (!string.Equals(modeloSelecionado.rio, nomeRioSelecionado, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError($"INCONSISTÊNCIA: Modelo {modeloSelecionado.nome} ({modeloSelecionado.rio}) != rio {nomeRioSelecionado}");
            LimparEstadoCompleto();
            return false;
        }

        return true;
    }

    /// <summary>Busca filho por nome em toda a hierarquia, com verificação direta primeiro.</summary>
    private static Transform BuscarFilhoPorNome(Transform pai, string nome)
    {
        Transform direto = pai.Find(nome);
        if (direto != null) return direto;

        foreach (var filho in pai.GetComponentsInChildren<Transform>(true))
        {
            if (filho.name == nome) return filho;
        }

        return null;
    }
}