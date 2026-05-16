using UnityEngine;
using UnityEngine.InputSystem;

public class AndroidBackHandler : MonoBehaviour
{
    [Header("Referência ao manager principal")]
    [SerializeField] private ARTemplateMenuManager menuManager;

    [Header("Diálogo de confirmação de saída")]
    [SerializeField] private GameObject dialogoSaida;

    [Header("Telas de UI")]
    [SerializeField] private GameObject telaLista;
    [SerializeField] private GameObject telaMapa;

    [Header("Modal de opções")]
    [SerializeField] private GameObject modalOpcoes;

    private bool dialogoSaidaAberto = false;
    private bool backSolicitadoNesteFrame = false;
    private InputAction androidBackAction;

    void OnEnable()
    {
#if !UNITY_EDITOR
        androidBackAction = new InputAction("AndroidBack", binding: "<Android>/back");
        androidBackAction.performed += ctx => backSolicitadoNesteFrame = true;
        androidBackAction.Enable();
#endif
    }

    void OnDisable()
    {
#if !UNITY_EDITOR
        androidBackAction?.Disable();
        androidBackAction?.Dispose();
        androidBackAction = null;
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.B))
            ProcessarBotaoBack();
#else
        if (backSolicitadoNesteFrame)
        {
            backSolicitadoNesteFrame = false;
            ProcessarBotaoBack();
        }
#endif
    }

    private void ProcessarBotaoBack()
    {
        // 1. Diálogo de saída aberto ? fecha
        if (dialogoSaidaAberto)
        {
            FecharDialogoSaida();
            return;
        }

        // 2. Modal de opções aberto ? fecha
        // Usa activeSelf pois o modal é ativado/desativado diretamente
        if (modalOpcoes != null && modalOpcoes.activeSelf)
        {
            menuManager.ShowHideModal();
            return;
        }

        // 3. Menu de objetos aberto ? fecha
        // Usa as propriedades públicas do manager (flags booleanas reais),
        // NÃO o activeSelf dos GameObjects (que ficam sempre ativos por causa do Animator)
        if (menuManager.objectMenuAnimatorAmazonas.GetBool("Show") ||
            menuManager.objectMenuAnimatorParana.GetBool("Show") ||
            menuManager.objectMenuAnimatorJacui.GetBool("Show"))
        {
            menuManager.HideMenu();
            return;
        }

        // 4. Tela Lista ativa ? confirmar saída
        if (telaLista != null && telaLista.activeSelf)
        {
            AbrirDialogoSaida();
            return;
        }

        // 5. Tela Mapa ativa ? confirmar saída
        if (telaMapa != null && telaMapa.activeSelf)
        {
            AbrirDialogoSaida();
            return;
        }

        // 6. Nenhuma tela UI visível = câmera AR ? voltar
        menuManager.VoltarParaTelaAnterior();
    }

    private void AbrirDialogoSaida()
    {
        dialogoSaidaAberto = true;
        if (dialogoSaida != null)
            dialogoSaida.SetActive(true);
        else
            SairDoApp();
    }

    private void FecharDialogoSaida()
    {
        dialogoSaidaAberto = false;
        if (dialogoSaida != null)
            dialogoSaida.SetActive(false);
    }

    public void ConfirmarSaida() => SairDoApp();
    public void CancelarSaida() => FecharDialogoSaida();

    private void SairDoApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}