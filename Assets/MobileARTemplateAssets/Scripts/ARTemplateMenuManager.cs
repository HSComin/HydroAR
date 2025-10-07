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
using System.Linq;

public interface IInstanciationController
{
    bool DeveBloquearInstanciacaoManual();
}

public class ARTemplateMenuManager : MonoBehaviour, IInstanciationController
{
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

    [SerializeField]
    private TMP_InputField campoPesquisa;

    [SerializeField]
    private Transform conteudoLista;

    [SerializeField]
    private ScrollRect scroll;

    [SerializeField]
    private List<Waypoint> waypoints;

    [SerializeField]
    private TMP_Text textoRioSelecionado;

    [SerializeField]
    private Button botaoVerModelo;

    [SerializeField]
    private GameObject allWaypoints;

    [SerializeField]
    private ARRaycastManager m_RaycastManager;

    [SerializeField]
    private List<ModeloEnchente> modelosEnchente;

    [SerializeField]
    private List<SelectionBoxEntry> selectionBoxes = new List<SelectionBoxEntry>();

    [SerializeField]
    private GameObject fundo;

    [SerializeField]
    private GameObject telaAbertura;

    [SerializeField]
    private GameObject telaUI;

    [SerializeField]
    private GameObject telaLista;

    [SerializeField]
    private GameObject telaMapa;

    [SerializeField]
    private GameObject cameraAR;

    [SerializeField]
    private GameObject[] dicas;

    [SerializeField]
    private GameObject firstHint;

    [SerializeField]
    private GameObject lastHint;

    [SerializeField]
    private GameObject nextButton;

    [SerializeField]
    Button m_CreateButtonAmazonas;

    [SerializeField]
    Button m_CreateButtonParana;

    [SerializeField]
    Button m_CreateButtonJacui;

    [SerializeField]
    Button m_DeleteButton;

    [SerializeField]
    Button m_BackButton;

    [SerializeField]
    Button m_FloodButton;

    [SerializeField]
    Button m_StopFloodButton;

    [SerializeField]
    Button m_OptionsButton;

    [SerializeField]
    Button m_HintsButton;

    [SerializeField]
    GameObject m_ModalMenu;

    [SerializeField]
    GameObject m_ObjectMenuAmazonas;

    [SerializeField]
    GameObject m_ObjectMenuParana;

    [SerializeField]
    GameObject m_ObjectMenuJacui;

    [SerializeField]
    Animator m_ObjectMenuAnimatorAmazonas;

    [SerializeField]
    Animator m_ObjectMenuAnimatorParana;

    [SerializeField]
    Animator m_ObjectMenuAnimatorJacui;

    [SerializeField]
    ObjectSpawner m_ObjectSpawner;

    [SerializeField]
    Button m_CancelButton;

    [SerializeField]
    XRInteractionGroup m_InteractionGroup;

    [SerializeField]
    DebugSlider m_DebugPlaneSlider;

    [SerializeField]
    GameObject m_DebugPlane;

    [SerializeField]
    ARPlaneManager m_PlaneManager;

    [SerializeField]
    ARDebugMenu m_DebugMenu;

    [SerializeField]
    DebugSlider m_DebugMenuSlider;

    [SerializeField]
    XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");

    [SerializeField]
    XRInputValueReader<Vector2> m_DragCurrentPositionInput = new XRInputValueReader<Vector2>("Drag Current Position");

    public Button createButtonAmazonas
    {
        get => m_CreateButtonAmazonas;
        set => m_CreateButtonAmazonas = value;
    }

    public Button createButtonParana
    {
        get => m_CreateButtonParana;
        set => m_CreateButtonParana = value;
    }

    public Button createButtonJacui
    {
        get => m_CreateButtonJacui;
        set => m_CreateButtonJacui = value;
    }

    public Button deleteButton
    {
        get => m_DeleteButton;
        set => m_DeleteButton = value;
    }

    public Button backButton
    {
        get => m_BackButton;
        set => m_BackButton = value;
    }

    public Button floodButton
    {
        get => m_FloodButton;
        set => m_FloodButton = value;
    }

    public Button stopFloodButton
    {
        get => m_StopFloodButton;
        set => m_StopFloodButton = value;
    }

    public Button optionsButton
    {
        get => m_OptionsButton;
        set => m_OptionsButton = value;
    }

    public Button hintsButton
    {
        get => m_HintsButton;
        set => m_HintsButton = value;
    }

    public GameObject objectMenuAmazonas
    {
        get => m_ObjectMenuAmazonas;
        set => m_ObjectMenuAmazonas = value;
    }

    public GameObject objectMenuParana
    {
        get => m_ObjectMenuParana;
        set => m_ObjectMenuParana = value;
    }

    public GameObject objectMenuJacui
    {
        get => m_ObjectMenuJacui;
        set => m_ObjectMenuJacui = value;
    }

    public GameObject modalMenu
    {
        get => m_ModalMenu;
        set => m_ModalMenu = value;
    }

    public Animator objectMenuAnimatorAmazonas
    {
        get => m_ObjectMenuAnimatorAmazonas;
        set => m_ObjectMenuAnimatorAmazonas = value;
    }

    public Animator objectMenuAnimatorParana
    {
        get => m_ObjectMenuAnimatorParana;
        set => m_ObjectMenuAnimatorParana = value;
    }

    public Animator objectMenuAnimatorJacui
    {
        get => m_ObjectMenuAnimatorJacui;
        set => m_ObjectMenuAnimatorJacui = value;
    }

    public ObjectSpawner objectSpawner
    {
        get => m_ObjectSpawner;
        set => m_ObjectSpawner = value;
    }

    public Button cancelButton
    {
        get => m_CancelButton;
        set => m_CancelButton = value;
    }

    public XRInteractionGroup interactionGroup
    {
        get => m_InteractionGroup;
        set => m_InteractionGroup = value;
    }

    public DebugSlider debugPlaneSlider
    {
        get => m_DebugPlaneSlider;
        set => m_DebugPlaneSlider = value;
    }

    public GameObject debugPlane
    {
        get => m_DebugPlane;
        set => m_DebugPlane = value;
    }

    public ARPlaneManager planeManager
    {
        get => m_PlaneManager;
        set => m_PlaneManager = value;
    }

    public ARDebugMenu debugMenu
    {
        get => m_DebugMenu;
        set => m_DebugMenu = value;
    }

    public DebugSlider debugMenuSlider
    {
        get => m_DebugMenuSlider;
        set => m_DebugMenuSlider = value;
    }

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

    bool m_IsPointerOverUI;
    bool m_ShowObjectMenuAmazonas;
    bool m_ShowObjectMenuParana;
    bool m_ShowObjectMenuJacui;
    bool m_ShowOptionsModal;
    bool m_InitializingDebugMenu;
    private bool tutorialAtivo = false;
    public float tempoAbertura = 4.5f;

    //Vector2 m_ObjectButtonOffsetAmazonas = Vector2.zero;
    //Vector2 m_ObjectButtonOffsetParana = Vector2.zero;
    //Vector2 m_ObjectMenuOffsetAmazonas = Vector2.zero;
    //Vector2 m_ObjectMenuOffsetParana = Vector2.zero;
    readonly List<ARFeatheredPlaneMeshVisualizerCompanion> featheredPlaneMeshVisualizerCompanions = new List<ARFeatheredPlaneMeshVisualizerCompanion>();
    private int indiceAtual = 0;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private ModeloEnchente modeloSelecionado;
    private Dictionary<PlanoAgua, float> alturasIniciais = new Dictionary<PlanoAgua, float>();
    private bool enchenteAtiva = false;
    private bool enchenteDescendo = false;
    private string nomeRioSelecionado = "";
    private string nomeModeloSelecionado = "";

    private bool aguardandoInstanciarModeloSelecionado = false;
    private List<GameObject> itensLista = new List<GameObject>();
    private GameObject objetoInstanciadoAtual;
    private Animator anim;

    public static System.Func<bool> DeveBloquearInstanciacao;
    public static bool InstanciacaoBloqueada = false;
    private bool bloquearInstanciacaoTemporaria = false;

    private string[] riosPermitidos = new string[] { "AMAZONAS", "PARANA", "JACUI" };

    private enum TelaAtiva { Nenhuma, Lista, Mapa }
    private TelaAtiva telaAtual = TelaAtiva.Nenhuma;
    private TelaAtiva telaAnterior = TelaAtiva.Nenhuma;

    void OnEnable()
    {
        //m_ShowObjectMenu = true;
        //m_CreateButton.onClick.AddListener(ShowMenu);
        //m_CancelButton.onClick.AddListener(HideMenu);
        //m_BackButton.onClick.AddListener(MostrarTelaLista);
        //m_DeleteButton.onClick.AddListener(DeleteFocusedObject);
        m_PlaneManager.trackablesChanged.AddListener(OnPlaneChanged);
    }

    void OnDisable()
    {
        //m_ShowObjectMenu = false;
        //m_CreateButton.onClick.RemoveListener(ShowMenu);
        //m_CancelButton.onClick.RemoveListener(HideMenu);
        //m_BackButton.onClick.RemoveListener(MostrarTelaLista);
        //m_DeleteButton.onClick.RemoveListener(DeleteFocusedObject);
        m_PlaneManager.trackablesChanged.RemoveListener(OnPlaneChanged);
    }

    public void MostrarTelaLista()
    {
        telaAtual = TelaAtiva.Lista;
        telaAnterior = telaAtual;
        InstanciacaoBloqueada = false;

        telaUI.SetActive(true);
        telaLista.SetActive(true);
        telaMapa.SetActive(false);
        telaAbertura.SetActive(false);
        fundo.SetActive(false);
        //cameraAR.SetActive(false);
        m_ObjectMenuAmazonas.SetActive(false);
        m_ObjectMenuParana.SetActive(false);
        m_ObjectMenuJacui.SetActive(false);

        m_CreateButtonAmazonas.gameObject.SetActive(false);
        m_CreateButtonParana.gameObject.SetActive(false);
        m_CreateButtonJacui.gameObject.SetActive(false);
        m_OptionsButton.gameObject.SetActive(false);
        m_BackButton.gameObject.SetActive(false);
        m_HintsButton.gameObject.SetActive(false);
        m_ModalMenu.gameObject.SetActive(false);
        m_FloodButton.gameObject.SetActive(false);
        m_StopFloodButton.gameObject.SetActive(false);
        allWaypoints.gameObject.SetActive(false);
        m_DeleteButton.gameObject.SetActive(false);

        foreach (var modelo in modelosEnchente)
        {
            if (modelo.instanciaAtual != null)
            {
                Destroy(modelo.instanciaAtual);
                modelo.instanciaAtual = null;
            }
        }

        if (objetoInstanciadoAtual != null)
        {
            Destroy(objetoInstanciadoAtual);
            objetoInstanciadoAtual = null;
        }

        modeloSelecionado = null;
        nomeModeloSelecionado = "";
        nomeRioSelecionado = "";

        if (m_ObjectSpawner != null)
        {
            m_ObjectSpawner.spawnOptionIndex = -1;
            Debug.Log("spawnOptionIndex resetado ao voltar para Tela Lista");
        }

        alturasIniciais.Clear();
        enchenteAtiva = false;
        enchenteDescendo = false;

        aguardandoInstanciarModeloSelecionado = false;
        bloquearInstanciacaoTemporaria = true;

        AtualizarSelectionBoxes("");

        Debug.Log("Tela Lista aberta: modelo selecionado e instanciacao resetados.");

        LimparEstadoCompleto();
    }


    public void MostrarTelaMapa()
    {
        telaAtual = TelaAtiva.Mapa;
        telaAnterior = telaAtual;
        InstanciacaoBloqueada = true;

        telaMapa.SetActive(true);
        telaLista.SetActive(false);
        telaAbertura.SetActive(false);
        fundo.SetActive(false);
        m_ObjectMenuAmazonas.SetActive(false);
        m_ObjectMenuParana.SetActive(false);
        m_ObjectMenuJacui.SetActive(false);

        m_CreateButtonAmazonas.gameObject.SetActive(false);
        m_CreateButtonParana.gameObject.SetActive(false);
        m_CreateButtonJacui.gameObject.SetActive(false);
        m_OptionsButton.gameObject.SetActive(false);
        m_BackButton.gameObject.SetActive(false);
        m_HintsButton.gameObject.SetActive(false);
        m_ModalMenu.gameObject.SetActive(false);
        m_FloodButton.gameObject.SetActive(false);
        m_StopFloodButton.gameObject.SetActive(false);
        allWaypoints.gameObject.SetActive(true);
        m_DeleteButton.gameObject.SetActive(false);

        foreach (var modelo in modelosEnchente)
        {
            if (modelo.instanciaAtual != null)
            {
                Destroy(modelo.instanciaAtual);
                modelo.instanciaAtual = null;
            }
        }

        if (objetoInstanciadoAtual != null)
        {
            Destroy(objetoInstanciadoAtual);
            objetoInstanciadoAtual = null;
        }

        modeloSelecionado = null;
        alturasIniciais.Clear();
        enchenteAtiva = false;
        enchenteDescendo = false;

        aguardandoInstanciarModeloSelecionado = false;
        bloquearInstanciacaoTemporaria = true;

        Debug.Log("Tela Mapa aberta: modo automático ativado.");
    }

    public void VoltarParaTelaAnterior()
    {
        Debug.Log($"Voltando da tela {telaAtual} para tela {telaAnterior}");

        switch (telaAnterior)
        {
            case TelaAtiva.Lista:
                MostrarTelaLista();
                break;

            case TelaAtiva.Mapa:
                MostrarTelaMapa();
                break;

            case TelaAtiva.Nenhuma:
            default:
                // Se não há tela anterior definida, volta para Lista como padrão
                Debug.Log("Nenhuma tela anterior definida, voltando para Lista como padrão");
                MostrarTelaLista();
                break;
        }
    }

    public void AbrirCameraARAmazonas()
    {
        telaAtual = TelaAtiva.Lista;
        InstanciacaoBloqueada = false;

        if (cameraAR != null)
        {
            LimparEstadoCompleto();
            LimparSelectionBoxesCompletamente();

            nomeRioSelecionado = "Amazonas";
            SelecionarModeloInicialAutomaticamente("Amazonas");

            if (m_ObjectSpawner != null)
            {
                m_ObjectSpawner.spawnOptionIndex = 0;
                Debug.Log("spawnOptionIndex resetado para Amazonas");
            }

            aguardandoInstanciarModeloSelecionado = true;
            bloquearInstanciacaoTemporaria = false;

            if (!string.IsNullOrEmpty(nomeModeloSelecionado))
            {
                AtualizarSelectionBoxes(nomeModeloSelecionado);
            }

            botaoVerModelo.interactable = false;
            m_DeleteButton.gameObject.SetActive(false);
            m_FloodButton.gameObject.SetActive(false);
            m_StopFloodButton.gameObject.SetActive(false);

            cameraAR.SetActive(true);
            telaLista.SetActive(false);
            telaMapa.SetActive(false);
            m_ObjectMenuAmazonas.SetActive(true);
            m_ObjectMenuParana.SetActive(false);
            m_ObjectMenuJacui.SetActive(false);
            allWaypoints.gameObject.SetActive(false);

            m_CreateButtonAmazonas.gameObject.SetActive(true);
            m_CreateButtonParana.gameObject.SetActive(false);
            m_CreateButtonJacui.gameObject.SetActive(false);
            m_OptionsButton.gameObject.SetActive(true);
            m_BackButton.gameObject.SetActive(true);
            m_HintsButton.gameObject.SetActive(true);
        }
    }

    public void AbrirCameraARParana()
    {
        telaAtual = TelaAtiva.Lista;
        InstanciacaoBloqueada = false;

        if (cameraAR != null)
        {
            LimparEstadoCompleto();
            LimparSelectionBoxesCompletamente();

            nomeRioSelecionado = "Parana";
            SelecionarModeloInicialAutomaticamente("Parana");

            if (m_ObjectSpawner != null)
            {
                m_ObjectSpawner.spawnOptionIndex = 3;
                Debug.Log("spawnOptionIndex resetado para Paraná");
            }

            aguardandoInstanciarModeloSelecionado = true;
            bloquearInstanciacaoTemporaria = false;

            if (!string.IsNullOrEmpty(nomeModeloSelecionado))
            {
                AtualizarSelectionBoxes(nomeModeloSelecionado);
            }

            botaoVerModelo.interactable = false;
            m_DeleteButton.gameObject.SetActive(false);
            m_FloodButton.gameObject.SetActive(false);
            m_StopFloodButton.gameObject.SetActive(false);

            cameraAR.SetActive(true);
            telaLista.SetActive(false);
            telaMapa.SetActive(false);
            m_ObjectMenuAmazonas.SetActive(false);
            m_ObjectMenuParana.SetActive(true);
            m_ObjectMenuJacui.SetActive(false);
            allWaypoints.gameObject.SetActive(false);

            m_CreateButtonAmazonas.gameObject.SetActive(false);
            m_CreateButtonParana.gameObject.SetActive(true);
            m_CreateButtonJacui.gameObject.SetActive(false);
            m_OptionsButton.gameObject.SetActive(true);
            m_BackButton.gameObject.SetActive(true);
            m_HintsButton.gameObject.SetActive(true);
        }
    }

    public void AbrirCameraARJacui()
    {
        telaAtual = TelaAtiva.Lista;
        InstanciacaoBloqueada = false;

        if (cameraAR != null)
        {
            LimparEstadoCompleto();
            LimparSelectionBoxesCompletamente();

            nomeRioSelecionado = "Jacui";
            SelecionarModeloInicialAutomaticamente("Jacui");

            if (m_ObjectSpawner != null)
            {
                m_ObjectSpawner.spawnOptionIndex = 6;
                Debug.Log("spawnOptionIndex resetado para Jacui");
            }

            aguardandoInstanciarModeloSelecionado = true;
            bloquearInstanciacaoTemporaria = false;

            if (!string.IsNullOrEmpty(nomeModeloSelecionado))
            {
                AtualizarSelectionBoxes(nomeModeloSelecionado);
            }

            botaoVerModelo.interactable = false;
            m_DeleteButton.gameObject.SetActive(false);
            m_FloodButton.gameObject.SetActive(false);
            m_StopFloodButton.gameObject.SetActive(false);

            cameraAR.SetActive(true);
            telaLista.SetActive(false);
            telaMapa.SetActive(false);
            m_ObjectMenuAmazonas.SetActive(false);
            m_ObjectMenuParana.SetActive(false);
            m_ObjectMenuJacui.SetActive(true);
            allWaypoints.gameObject.SetActive(false);

            m_CreateButtonAmazonas.gameObject.SetActive(false);
            m_CreateButtonParana.gameObject.SetActive(false);
            m_CreateButtonJacui.gameObject.SetActive(true);
            m_OptionsButton.gameObject.SetActive(true);
            m_BackButton.gameObject.SetActive(true);
            m_HintsButton.gameObject.SetActive(true);
        }
    }

    public void IniciarTutorial()
    {
        tutorialAtivo = true;

        indiceAtual = 0;
        nextButton.gameObject.SetActive(true);
        firstHint.gameObject.SetActive(true);

        m_CreateButtonAmazonas.gameObject.SetActive(false);
        m_CreateButtonParana.gameObject.SetActive(false);
        m_CreateButtonJacui.gameObject.SetActive(false);
        m_OptionsButton.gameObject.SetActive(false);
        m_BackButton.gameObject.SetActive(false);
        m_ModalMenu.gameObject.SetActive(false);
        m_HintsButton.gameObject.SetActive(false);
        m_DeleteButton.gameObject.SetActive(false);
        m_FloodButton.gameObject.SetActive(false);
        m_StopFloodButton.gameObject.SetActive(false);
        m_ObjectMenuAmazonas.SetActive(false);
        m_ObjectMenuParana.SetActive(false);
        m_ObjectMenuJacui.SetActive(false);
    }


    public void AtualizarDicas()
    {
        for (int i = 0; i < dicas.Length; i++)
        {
            dicas[i].SetActive(i == indiceAtual);
        }
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
                if (nomeRioSelecionado.ToUpper() == "AMAZONAS")
                {
                    m_CreateButtonAmazonas.gameObject.SetActive(true);
                    m_CreateButtonParana.gameObject.SetActive(false);
                    m_CreateButtonJacui.gameObject.SetActive(false);
                }
                else if (nomeRioSelecionado.ToUpper() == "PARANA")
                {
                    m_CreateButtonAmazonas.gameObject.SetActive(false);
                    m_CreateButtonParana.gameObject.SetActive(true);
                    m_CreateButtonJacui.gameObject.SetActive(false);
                }
                else if (nomeRioSelecionado.ToUpper() == "JACUI")
                {
                    m_CreateButtonAmazonas.gameObject.SetActive(false);
                    m_CreateButtonParana.gameObject.SetActive(false);
                    m_CreateButtonJacui.gameObject.SetActive(true);
                }
                else
                {
                    m_CreateButtonAmazonas.gameObject.SetActive(false);
                    m_CreateButtonParana.gameObject.SetActive(false);
                    m_CreateButtonJacui.gameObject.SetActive(false);
                }
            }
            else
            {
                m_CreateButtonAmazonas.gameObject.SetActive(false);
                m_CreateButtonParana.gameObject.SetActive(false);
                m_CreateButtonJacui.gameObject.SetActive(false);
            }

            m_OptionsButton.gameObject.SetActive(true);
            m_BackButton.gameObject.SetActive(true);

            if (objetoInstanciadoAtual != null && objetoInstanciadoAtual.activeInHierarchy)
            {
                m_DeleteButton.gameObject.SetActive(true);
                m_FloodButton.gameObject.SetActive(true);
                Debug.Log("Botões de deletar e enchente ativados após final das dicas.");
            }
            else
            {
                m_DeleteButton.gameObject.SetActive(false);
                m_FloodButton.gameObject.SetActive(false);
            }

            m_HintsButton.gameObject.SetActive(true);
            tutorialAtivo = false;
            return;
        }
        AtualizarDicas();
    }


    public void IniciarEnchenteNoSelecionado()
    {
        if (modeloSelecionado != null)
        {
            enchenteAtiva = true;
            m_FloodButton.gameObject.SetActive(false);
            m_StopFloodButton.gameObject.SetActive(true);
        }
    }

    public void PararEnchente()
    {
        enchenteAtiva = false;
        enchenteDescendo = true;

        m_FloodButton.gameObject.SetActive(true);
        m_StopFloodButton.gameObject.SetActive(false);

        Debug.Log("Parando enchente - iniciando descida suave dos planos.");
    }

    private void SelecionarRio(Waypoint selecionado)
    {
        nomeModeloSelecionado = selecionado.nomeRio;
        textoRioSelecionado.text = selecionado.nomeRioTexto;
        botaoVerModelo.interactable = true;

        foreach (var wp in waypoints)
        {
            bool ativo = wp == selecionado;
            if (wp.imagemIcone != null)
                wp.imagemIcone.sprite = ativo ? wp.spriteSelecionado : wp.spriteNormal;
        }
    }

    public void VerModelo()
    {
        telaAtual = TelaAtiva.Mapa;

        modeloSelecionado = modelosEnchente.Find(m => m.nome == nomeModeloSelecionado);

        if (modeloSelecionado == null || modeloSelecionado.prefab == null)
        {
            Debug.LogWarning("Modelo não encontrado ou prefab ausente para: " + nomeModeloSelecionado);
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
        m_ObjectMenuAmazonas.SetActive(false);
        m_ObjectMenuParana.SetActive(false);
        m_ObjectMenuJacui.SetActive(false);

        m_DeleteButton.gameObject.SetActive(false);
    }


    public void VerModeloSelecionadoAmazonas()
    {
        telaAtual = TelaAtiva.Lista;

        if (modeloSelecionado == null || modeloSelecionado.rio != "Amazonas")
        {
            Debug.LogWarning("Nenhum modelo válido selecionado para o rio Amazonas.");
            return;
        }

        aguardandoInstanciarModeloSelecionado = true;
        nomeModeloSelecionado = modeloSelecionado.nome;
        bloquearInstanciacaoTemporaria = false;

        cameraAR.SetActive(true);
        telaLista.SetActive(false);
        telaMapa.SetActive(false);
        m_ObjectMenuAmazonas.SetActive(true);
        m_ObjectMenuParana.SetActive(false);
        m_ObjectMenuJacui.SetActive(false);
        allWaypoints.gameObject.SetActive(false);

        m_CreateButtonAmazonas.gameObject.SetActive(true);
        m_CreateButtonParana.gameObject.SetActive(false);
        m_CreateButtonJacui.gameObject.SetActive(false);
        m_OptionsButton.gameObject.SetActive(true);
        m_BackButton.gameObject.SetActive(true);
        m_HintsButton.gameObject.SetActive(true);

        m_ShowObjectMenuAmazonas = true;
        m_ShowObjectMenuParana = false;
        m_ShowObjectMenuJacui = false;

        foreach (var modelo in modelosEnchente)
        {
            if (modelo.instanciaAtual != null)
            {
                bool isAmazonas = modelo.rio == "Amazonas";

                modelo.instanciaAtual.SetActive(isAmazonas);

                var selectionBox = modelo.instanciaAtual.transform.Find("SelectionBox");
                if (selectionBox != null)
                    selectionBox.gameObject.SetActive(isAmazonas);
            }
        }

        indiceAtual = 0;
        enchenteAtiva = false;
        enchenteDescendo = false;
        alturasIniciais.Clear();

    }


    public void VerModeloSelecionadoParana()
    {
        telaAtual = TelaAtiva.Lista;

        if (modeloSelecionado == null || modeloSelecionado.rio != "Parana")
        {
            Debug.LogWarning("Nenhum modelo válido selecionado para o rio Paraná.");
            return;
        }

        aguardandoInstanciarModeloSelecionado = true;
        nomeModeloSelecionado = modeloSelecionado.nome;
        bloquearInstanciacaoTemporaria = false;

        cameraAR.SetActive(true);
        telaLista.SetActive(false);
        telaMapa.SetActive(false);
        m_ObjectMenuAmazonas.SetActive(false);
        m_ObjectMenuParana.SetActive(true);
        m_ObjectMenuJacui.SetActive(false);
        allWaypoints.gameObject.SetActive(false);

        m_CreateButtonAmazonas.gameObject.SetActive(false);
        m_CreateButtonParana.gameObject.SetActive(true);
        m_CreateButtonJacui.gameObject.SetActive(false);
        m_OptionsButton.gameObject.SetActive(true);
        m_BackButton.gameObject.SetActive(true);
        m_HintsButton.gameObject.SetActive(true);

        m_ShowObjectMenuAmazonas = false;
        m_ShowObjectMenuParana = true;
        m_ShowObjectMenuJacui = false;

        foreach (var modelo in modelosEnchente)
        {
            if (modelo.instanciaAtual != null)
            {
                bool isParana = modelo.rio == "Parana";

                modelo.instanciaAtual.SetActive(isParana);

                var selectionBox = modelo.instanciaAtual.transform.Find("SelectionBox");
                if (selectionBox != null)
                    selectionBox.gameObject.SetActive(isParana);
            }
        }

        indiceAtual = 0;
        enchenteAtiva = false;
        enchenteDescendo = false;
        alturasIniciais.Clear();

    }

    public void VerModeloSelecionadoJacui()
    {
        telaAtual = TelaAtiva.Lista;

        if (modeloSelecionado == null || modeloSelecionado.rio != "Jacui")
        {
            Debug.LogWarning("Nenhum modelo válido selecionado para o rio Jacui.");
            return;
        }

        aguardandoInstanciarModeloSelecionado = true;
        nomeModeloSelecionado = modeloSelecionado.nome;
        bloquearInstanciacaoTemporaria = false;

        cameraAR.SetActive(true);
        telaLista.SetActive(false);
        telaMapa.SetActive(false);
        m_ObjectMenuAmazonas.SetActive(false);
        m_ObjectMenuParana.SetActive(false);
        m_ObjectMenuJacui.SetActive(true);
        allWaypoints.gameObject.SetActive(false);

        m_CreateButtonAmazonas.gameObject.SetActive(false);
        m_CreateButtonParana.gameObject.SetActive(false);
        m_CreateButtonJacui.gameObject.SetActive(true);
        m_OptionsButton.gameObject.SetActive(true);
        m_BackButton.gameObject.SetActive(true);
        m_HintsButton.gameObject.SetActive(true);

        m_ShowObjectMenuAmazonas = false;
        m_ShowObjectMenuParana = false;
        m_ShowObjectMenuJacui = true;

        foreach (var modelo in modelosEnchente)
        {
            if (modelo.instanciaAtual != null)
            {
                bool isJacui = modelo.rio == "Jacui";

                modelo.instanciaAtual.SetActive(isJacui);

                var selectionBox = modelo.instanciaAtual.transform.Find("SelectionBox");
                if (selectionBox != null)
                    selectionBox.gameObject.SetActive(isJacui);
            }
        }

        indiceAtual = 0;
        enchenteAtiva = false;
        enchenteDescendo = false;
        alturasIniciais.Clear();

    }

    public void FiltrarLista(string texto)
    {
        if (conteudoLista == null || scroll == null)
        {
            Debug.LogWarning("conteudoLista ou scroll nao estao atribuidos.");
            return;
        }

        string textoLower = texto.ToLower();
        GameObject primeiroEncontrado = null;

        foreach (Transform item in conteudoLista)
        {
            TMP_Text[] textos = item.GetComponentsInChildren<TMP_Text>(true);
            bool corresponde = false;

            foreach (TMP_Text txt in textos)
            {
                if (txt.text.ToLower().Contains(textoLower))
                {
                    corresponde = true;
                    break;
                }
            }

            item.gameObject.SetActive(corresponde);

            if (corresponde && primeiroEncontrado == null)
            {
                primeiroEncontrado = item.gameObject;
            }
        }

        if (primeiroEncontrado != null)
        {
            StartCoroutine(MoverScrollParaItem(primeiroEncontrado));
        }
    }

    private System.Collections.IEnumerator MoverScrollParaItem(GameObject item)
    {
        yield return null;

        RectTransform itemRect = item.GetComponent<RectTransform>();
        RectTransform contentRect = conteudoLista.GetComponent<RectTransform>();

        if (itemRect != null && contentRect != null)
        {
            float pos = itemRect.anchoredPosition.y / (contentRect.rect.height - itemRect.rect.height);
            scroll.verticalNormalizedPosition = 1f - Mathf.Clamp01(pos);
        }
    }


    private void OnObjectSpawned(GameObject objeto)
    {
        Debug.Log("OnObjectSpawned chamado");

        string nomeModelo = objeto.name.Replace("(Clone)", "").Trim();
        modeloSelecionado = modelosEnchente.Find(m => m.nome == nomeModelo);
        Debug.Log("Modelo instanciado detectado: " + nomeModelo);

        alturasIniciais.Clear();

        if (modeloSelecionado != null && modeloSelecionado.planosAgua != null)
        {
            Debug.Log("Modelo encontrado com " + modeloSelecionado.planosAgua.Count + " planos de agua");

            foreach (var plano in modeloSelecionado.planosAgua)
            {
                string nomePlano = plano.nomePlano;
                Debug.Log("Procurando plano: " + nomePlano);

                if (string.IsNullOrEmpty(nomePlano))
                {
                    Debug.LogWarning("Plano de agua sem nome definido");
                    continue;
                }

                Transform planoInstanciado = objeto.transform.Find(nomePlano);

                if (planoInstanciado == null)
                {
                    Transform[] filhos = objeto.GetComponentsInChildren<Transform>(true);
                    foreach (var filho in filhos)
                    {
                        if (filho.name == nomePlano)
                        {
                            planoInstanciado = filho;
                            break;
                        }
                    }
                }

                if (planoInstanciado != null)
                {
                    plano.plano = planoInstanciado;
                    alturasIniciais[plano] = planoInstanciado.localPosition.y;
                    Debug.Log("Plano encontrado: " + nomePlano + " | Altura inicial: " + planoInstanciado.position.y);
                }
                else
                {
                    Debug.LogWarning("Plano de agua '" + nomePlano + "' nao encontrado");
                }
            }

            m_FloodButton.gameObject.SetActive(true);
            Debug.Log("Botao de enchente ativado");
        }
        else
        {
            Debug.LogWarning("Modelo invalido ou sem planos de agua");
            m_FloodButton.gameObject.SetActive(false);
        }

        if (modeloSelecionado != null && telaAtual == TelaAtiva.Lista)
        {
            m_DeleteButton.gameObject.SetActive(true);
            Debug.Log("Botão de deletar ativado porque modelo foi instanciado (Tela Lista)");
        }
        else
        {
            m_DeleteButton.gameObject.SetActive(false);
            Debug.Log("Botão de deletar ocultado (Tela Mapa ou nenhum modelo selecionado)");
        }

        objetoInstanciadoAtual = objeto;
    }

    void ExecutarFadeOut()
    {
        anim.SetTrigger("FadeOutStart");
        Invoke(nameof(MostrarTelaLista), 0.9f);
    }

    private bool AlgumSelectionBoxAtivo()
    {
        foreach (var box in selectionBoxes)
        {
            if (box.selectionBox.activeSelf) return true;
        }
        return false;
    }

    private void SelecionarModeloInicialAutomaticamente(string rioDesejado)
    {
        Debug.Log($"Selecionando modelo inicial para rio: {rioDesejado}");

        if (string.IsNullOrEmpty(nomeRioSelecionado) || nomeRioSelecionado.ToUpper() != rioDesejado.ToUpper())
        {
            Debug.LogError($"ERRO: nomeRioSelecionado ({nomeRioSelecionado}) não corresponde ao rio desejado ({rioDesejado})");
            nomeRioSelecionado = rioDesejado;
        }

        foreach (var modelo in modelosEnchente)
        {
            Debug.Log($"Verificando: {modelo.nome} | Rio: {modelo.rio} | Disponível: {modelo.instanciaAtual == null}");

            if (modelo.instanciaAtual == null && modelo.rio.ToUpper() == rioDesejado.ToUpper())
            {
                modeloSelecionado = modelo;
                nomeModeloSelecionado = modelo.nome;

                Debug.Log($"Modelo selecionado: {modelo.nome} do rio {modelo.rio}");
                return;
            }
        }

        Debug.LogWarning($"Nenhum modelo disponível para o rio {rioDesejado}");
    }


    private void AtualizarSelectionBoxes(string nomeModelo)
    {
        foreach (var entry in selectionBoxes)
        {
            entry.selectionBox.SetActive(entry.nomeModelo == nomeModelo);
        }
    }

    private void LimparEstadoCompleto()
    {
        Debug.Log(" Limpando estado completo do sistema");

        modeloSelecionado = null;
        nomeModeloSelecionado = "";
        nomeRioSelecionado = "";

        alturasIniciais.Clear();
        enchenteAtiva = false;
        enchenteDescendo = false;
        aguardandoInstanciarModeloSelecionado = false;
        bloquearInstanciacaoTemporaria = true;

        if (m_ObjectSpawner != null)
        {
            m_ObjectSpawner.spawnOptionIndex = -1;
            Debug.Log("spawnOptionIndex resetado para -1");
        }

        LimparSelectionBoxesCompletamente();

        Debug.Log("Estado COMPLETAMENTE limpo");
    }

    public bool DeveBloquearInstanciacaoManual()
    {
        // Bloquear se estiver na Tela Mapa
        if (telaAtual == TelaAtiva.Mapa)
        {
            Debug.Log("Bloqueando instanciação: Tela Mapa ativa");
            return true;
        }

        // Bloquear se estiver em modo de instanciação bloqueada temporariamente
        if (bloquearInstanciacaoTemporaria)
        {
            Debug.Log("Bloqueando instanciação: Modo bloqueado temporariamente");
            return true;
        }

        // Bloquear se não estiver aguardando instanciação de modelo selecionado
        if (!aguardandoInstanciarModeloSelecionado)
        {
            Debug.Log("Bloqueando instanciação: Não aguardando modelo selecionado");
            return true;
        }

        return false;
    }

    private void LimparSelectionBoxesCompletamente()
    {
        Debug.Log("Limpando TODAS as selection boxes antes de mudar de rio");

        foreach (var entry in selectionBoxes)
        {
            if (entry.selectionBox != null)
            {
                entry.selectionBox.SetActive(false);
                Debug.Log($"Selection box desativada: {entry.nomeModelo}");
            }
        }
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

        if (modeloSelecionado.rio.ToUpper() != nomeRioSelecionado.ToUpper())
        {
            Debug.LogError($"INCONSISTÊNCIA: Modelo {modeloSelecionado.nome} ({modeloSelecionado.rio}) não pertence ao rio {nomeRioSelecionado}!");
            LimparEstadoCompleto();
            return false;
        }

        return true;
    }


    void Start()
    {
        m_DebugMenu.gameObject.SetActive(true);
        m_InitializingDebugMenu = true;
        m_ObjectSpawner.objectSpawned += OnObjectSpawned;

        anim = telaAbertura.GetComponent<Animator>();
        telaUI.SetActive(false);
        fundo.SetActive(true);
        Invoke(nameof(ExecutarFadeOut), tempoAbertura);

        //InitializeDebugMenuOffsets();
        HideMenu();
        m_PlaneManager.planePrefab = m_DebugPlane;

        foreach (var wp in waypoints)
        {
            wp.botao.onClick.AddListener(() => SelecionarRio(wp));
        }

        botaoVerModelo.interactable = false;

        foreach (Transform item in conteudoLista)
        {
            itensLista.Add(item.gameObject);
        }

        campoPesquisa.onValueChanged.AddListener(FiltrarLista);

    }

    void Update()
    {
        if (!string.IsNullOrEmpty(nomeRioSelecionado) && modeloSelecionado != null)
        {
            if (modeloSelecionado.rio.ToUpper() != nomeRioSelecionado.ToUpper())
            {
                Debug.LogError($"INCONSISTÊNCIA DETECTADA: Modelo {modeloSelecionado.nome} ({modeloSelecionado.rio}) != Rio selecionado ({nomeRioSelecionado})");

                LimparEstadoCompleto();
                return;
            }
        }

        if (string.IsNullOrEmpty(nomeModeloSelecionado))
        {
            bool algumBoxAtivo = false;
            foreach (var entry in selectionBoxes)
            {
                if (entry.selectionBox.activeSelf)
                {
                    algumBoxAtivo = true;
                    break;
                }
            }

            if (algumBoxAtivo)
            {
                Debug.LogWarning("Selection boxes ativas sem modelo selecionado - limpando...");
                LimparSelectionBoxesCompletamente();
            }
        }

        if (m_InitializingDebugMenu)
        {
            m_DebugMenu.gameObject.SetActive(false);
            m_InitializingDebugMenu = false;
        }

        // --- MODO AUTOMÁTICO: Tela Mapa ---
        if (telaAtual == TelaAtiva.Mapa &&
            modeloSelecionado != null &&
            modeloSelecionado.instanciaAtual == null &&
            aguardandoInstanciarModeloSelecionado)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (m_RaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes))
            {
                Pose pose = hits[0].pose;


                GameObject modelo = Instantiate(modeloSelecionado.prefab, pose.position, pose.rotation);
                modeloSelecionado.instanciaAtual = modelo;
                objetoInstanciadoAtual = modelo;

                bloquearInstanciacaoTemporaria = true;
                aguardandoInstanciarModeloSelecionado = false;

                Debug.Log("Modelo '" + modeloSelecionado.nome + "' instanciado automaticamente.");

                m_DeleteButton.gameObject.SetActive(false);
                OnObjectSpawned(modelo);
            }
        }

        // MODO 2 — Instanciação Manual via Toque (válido apenas para Lista)
        else if (telaAtual == TelaAtiva.Lista &&
                 aguardandoInstanciarModeloSelecionado &&
                 !bloquearInstanciacaoTemporaria &&
                 ValidarConsistenciaRioModelo() && // Nova validação
                 modeloSelecionado.instanciaAtual == null &&
                 !string.IsNullOrEmpty(nomeRioSelecionado))
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    // Validação extra antes da instanciação
                    if (!ValidarConsistenciaRioModelo())
                    {
                        return;
                    }

                    List<ARRaycastHit> hits = new List<ARRaycastHit>();
                    if (m_RaycastManager.Raycast(touch.position, hits, TrackableType.Planes))
                    {
                        Pose pose = hits[0].pose;

                        GameObject modelo = Instantiate(modeloSelecionado.prefab, pose.position, pose.rotation);
                        modeloSelecionado.instanciaAtual = modelo;
                        objetoInstanciadoAtual = modelo;

                        bloquearInstanciacaoTemporaria = true;
                        aguardandoInstanciarModeloSelecionado = false;

                        Debug.Log($"Modelo '{modeloSelecionado.nome}' do rio '{modeloSelecionado.rio}' instanciado via TOQUE.");

                        OnObjectSpawned(modelo);
                    }
                }
            }
        }

        // (Opcional) Log se já houver instância
        else if (modeloSelecionado != null && modeloSelecionado.instanciaAtual != null)
        {
            Debug.Log("Modelo já instanciado: " + modeloSelecionado.nome);
        }

        if (enchenteAtiva && modeloSelecionado != null)
        {
            Debug.Log("Enchente ativa - atualizando planos");

            foreach (var plano in modeloSelecionado.planosAgua)
            {
                if (!alturasIniciais.ContainsKey(plano))
                {
                    Debug.LogWarning("Plano '" + plano.nomePlano + "' nao encontrado em alturasIniciais");
                    continue;
                }

                if (plano.plano != null && alturasIniciais.TryGetValue(plano, out float alturaInicial))
                {
                    float alturaAlvo = alturaInicial + plano.alturaMaxima;
                    float atual = plano.plano.localPosition.y;

                    Debug.Log("Atualizando plano: " + plano.nomePlano + " | Atual: " + atual + " | Alvo: " + alturaAlvo);

                    if (Mathf.Abs(atual - alturaAlvo) < 0.0001f)
                    {
                        Debug.Log("Plano '" + plano.nomePlano + "' ja atingiu a altura alvo");
                        continue;
                    }

                    Vector3 posLocal = plano.plano.localPosition;
                    posLocal.y += plano.velocidadeSubida * Time.deltaTime;
                    posLocal.y = Mathf.Min(posLocal.y, alturaAlvo);
                    plano.plano.localPosition = posLocal;

                    Debug.Log("Nova altura de '" + plano.nomePlano + "': " + plano.plano.localPosition.y);
                }
                else
                {
                    Debug.LogWarning("Plano '" + plano.nomePlano + "' esta nulo ou altura inicial nao encontrada");
                }
            }
        }

        if (enchenteDescendo && modeloSelecionado != null)
        {
            Debug.Log("Enchente descendo - resetando planos");

            bool algumPlanoAindaDescendo = false;

            foreach (var plano in modeloSelecionado.planosAgua)
            {
                if (plano.plano != null && alturasIniciais.TryGetValue(plano, out float alturaOriginal))
                {
                    float atual = plano.plano.localPosition.y;

                    if (Mathf.Abs(atual - alturaOriginal) > 0.0001f)
                    {
                        algumPlanoAindaDescendo = true;

                        Vector3 pos = plano.plano.localPosition;
                        pos.y -= plano.velocidadeSubida * Time.deltaTime;
                        pos.y = Mathf.Max(pos.y, alturaOriginal);
                        plano.plano.localPosition = pos;

                        Debug.Log("Descendo plano: " + plano.nomePlano + " | Atual: " + pos.y + " | Original: " + alturaOriginal);
                    }
                }
            }

            if (!algumPlanoAindaDescendo)
            {
                enchenteDescendo = false;
                Debug.Log("Enchente totalmente revertida.");
            }
        }

        bool algumMenuAberto = m_ShowObjectMenuAmazonas || m_ShowObjectMenuParana || m_ShowObjectMenuJacui || m_ShowOptionsModal;

        if (algumMenuAberto)
        {
            if (!m_IsPointerOverUI && (m_TapStartPositionInput.TryReadValue(out _) || m_DragCurrentPositionInput.TryReadValue(out _)))
            {
                if (m_ShowObjectMenuAmazonas)
                {
                    HideMenu();
                    m_ShowObjectMenuAmazonas = false;
                }

                if (m_ShowObjectMenuParana)
                {
                    HideMenu();
                    m_ShowObjectMenuParana = false;
                }

                if (m_ShowObjectMenuJacui)
                {
                    HideMenu();
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
                bool temFoco = m_InteractionGroup?.focusInteractable != null;
                bool temModeloInstanciado = objetoInstanciadoAtual != null && objetoInstanciadoAtual.activeInHierarchy;

                if (!temModeloInstanciado && modeloSelecionado != null && modeloSelecionado.planosAgua != null)
                {
                    foreach (var plano in modeloSelecionado.planosAgua)
                    {
                        if (plano.plano != null && plano.plano.gameObject.activeInHierarchy)
                        {
                            temModeloInstanciado = true;
                            break;
                        }
                    }
                }

                if (telaAtual == TelaAtiva.Mapa)
                {
                    m_DeleteButton.gameObject.SetActive(false);
                }
                else
                {
                    m_DeleteButton.gameObject.SetActive(temFoco || temModeloInstanciado);
                }
            }
            else
            {
                m_DeleteButton.gameObject.SetActive(false);
            }
        }

        if (!m_IsPointerOverUI && m_ShowOptionsModal)
        {
            m_IsPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
        }
    }


    public void SetObjectToSpawn(int objectIndex)
    {
        if (m_ObjectSpawner == null)
        {
            Debug.LogWarning("Object Spawner not configured correctly: no ObjectSpawner set.");
        }
        else
        {
            if (m_ObjectSpawner.objectPrefabs.Count > objectIndex)
            {
                m_ObjectSpawner.spawnOptionIndex = objectIndex;
            }
            else
            {
                Debug.LogWarning("Object Spawner not configured correctly: object index larger than number of Object Prefabs.");
            }
        }

        HideMenu();
    }

    public void ShowMenuAmazonas()
    {
        m_ShowObjectMenuAmazonas = true;
        m_ObjectMenuAmazonas.SetActive(true);
        if (!m_ObjectMenuAnimatorAmazonas.GetBool("Show"))
        {
            m_ObjectMenuAnimatorAmazonas.SetBool("Show", true);
        }
        //AdjustARDebugMenuPosition();
    }

    public void ShowMenuParana()
    {
        m_ShowObjectMenuParana = true;
        m_ObjectMenuParana.SetActive(true);
        if (!m_ObjectMenuAnimatorParana.GetBool("Show"))
        {
            m_ObjectMenuAnimatorParana.SetBool("Show", true);
        }
        //AdjustARDebugMenuPosition();
    }

    public void ShowMenuJacui()
    {
        m_ShowObjectMenuJacui = true;
        m_ObjectMenuJacui.SetActive(true);
        if (!m_ObjectMenuAnimatorJacui.GetBool("Show"))
        {
            m_ObjectMenuAnimatorJacui.SetBool("Show", true);
        }
        //AdjustARDebugMenuPosition();
    }

    public void ShowHideModal()
    {
        if (m_ModalMenu.activeSelf)
        {
            m_ShowOptionsModal = false;
            m_ModalMenu.SetActive(false);
        }
        else
        {
            m_ShowOptionsModal = true;
            m_ModalMenu.SetActive(true);
        }
    }

    public void ShowHideDebugPlane()
    {
        if (m_DebugPlaneSlider.value == 1)
        {
            m_DebugPlaneSlider.value = 0;
            ChangePlaneVisibility(false);
        }
        else
        {
            m_DebugPlaneSlider.value = 1;
            ChangePlaneVisibility(true);
        }
    }

    public void ShowHideDebugMenu()
    {
        if (m_DebugMenu.gameObject.activeSelf)
        {
            m_DebugMenuSlider.value = 0;
            m_DebugMenu.gameObject.SetActive(false);
        }
        else
        {
            m_DebugMenuSlider.value = 1;
            m_DebugMenu.gameObject.SetActive(true);
            //AdjustARDebugMenuPosition();
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

        Debug.Log("Todos os objetos instanciados foram destruídos e estado limpo.");
    }


    public void HideMenu()
    {
        m_ObjectMenuAnimatorAmazonas.SetBool("Show", false);
        m_ObjectMenuAnimatorParana.SetBool("Show", false);
        m_ObjectMenuAnimatorJacui.SetBool("Show", false);
        m_ShowObjectMenuAmazonas = false;
        m_ShowObjectMenuParana = false;
        m_ShowObjectMenuJacui = false;
        //AdjustARDebugMenuPosition();
    }

    void ChangePlaneVisibility(bool setVisible)
    {
        var count = featheredPlaneMeshVisualizerCompanions.Count;
        for (int i = 0; i < count; ++i)
        {
            featheredPlaneMeshVisualizerCompanions[i].visualizeSurfaces = setVisible;
        }
    }

    public void DeletarObjetoInstanciado()
    {
        if (objetoInstanciadoAtual != null)
        {
            Destroy(objetoInstanciadoAtual);
            objetoInstanciadoAtual = null;

            if (modeloSelecionado != null)
            {
                modeloSelecionado.instanciaAtual = null;
                modeloSelecionado = null;
            }

            enchenteAtiva = false;
            m_FloodButton.gameObject.SetActive(false);
            m_StopFloodButton.gameObject.SetActive(false);
            m_DeleteButton.gameObject.SetActive(false);

            Debug.Log("Objeto instanciado deletado e modelo selecionado limpo");
        }
    }

    void OnPlaneChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
    {
        if (eventArgs.added.Count > 0)
        {
            foreach (var plane in eventArgs.added)
            {
                if (plane.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                {
                    featheredPlaneMeshVisualizerCompanions.Add(visualizer);
                    visualizer.visualizeSurfaces = (m_DebugPlaneSlider.value != 0);
                }
            }
        }

        if (eventArgs.removed.Count > 0)
        {
            foreach (var plane in eventArgs.removed)
            {
                if (plane.Value != null && plane.Value.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                    featheredPlaneMeshVisualizerCompanions.Remove(visualizer);
            }
        }

        if (m_PlaneManager.trackables.count != featheredPlaneMeshVisualizerCompanions.Count)
        {
            featheredPlaneMeshVisualizerCompanions.Clear();
            foreach (var trackable in m_PlaneManager.trackables)
            {
                if (trackable.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                {
                    featheredPlaneMeshVisualizerCompanions.Add(visualizer);
                    visualizer.visualizeSurfaces = (m_DebugPlaneSlider.value != 0);
                }
            }
        }
    }
}