using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreviewManager : MonoBehaviour
{
    [System.Serializable]
    public class ModeloData
    {
        public string nome;
        public GameObject prefab;
        public Button botao;
        public GameObject selectionBox;
    }

    [Header("Referência 3D")]
    public PreviewController previewController;

    [Header("Modelos & Botões do Scroll")]
    public List<ModeloData> modelos = new List<ModeloData>();

    [Header("UI — Topo")]
    public TextMeshProUGUI txtNomeModelo;

    [Header("UI — Botões de controle")]
    public Button btnReset;
    public Button btnAutoRotate;
    public Sprite iconeRotateOn;
    public Sprite iconeRotateOff;

    [Header("Highlight do botão selecionado")]
    public Color corSelecionado = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color corNormal = Color.white;

    private Image imgBtnAutoRotate;
    private Button botaoSelecionado;

    void Start()
    {
        btnReset?.onClick.RemoveAllListeners();
        btnReset?.onClick.AddListener(previewController.ResetarRotacao);

        if (btnAutoRotate != null)
        {
            btnAutoRotate.onClick.RemoveAllListeners();
            imgBtnAutoRotate = btnAutoRotate.GetComponent<Image>();
            btnAutoRotate.onClick.AddListener(OnToggleAutoRotate);
        }

        foreach (ModeloData m in modelos)
        {
            if (m.botao == null || m.prefab == null) continue;

            m.botao.onClick.RemoveAllListeners();
            ModeloData captura = m;
            m.botao.onClick.AddListener(() => SelecionarModelo(captura));
        }

        if (modelos.Count > 0 && modelos[0].prefab != null)
            SelecionarModelo(modelos[0]);
    }

    private void SelecionarModelo(ModeloData modelo)
    {
        previewController.SelecionarModelo(modelo.prefab); // salva o nome e mostra o modelo

        if (txtNomeModelo != null)
            txtNomeModelo.text = modelo.nome;

        AtualizarHighlight(modelo.botao);
    }

    private void AtualizarHighlight(Button novoBotao)
    {
        if (botaoSelecionado != null)
        {
            ModeloData anterior = modelos.Find(m => m.botao == botaoSelecionado);
            if (anterior?.selectionBox != null)
                anterior.selectionBox.SetActive(false);
        }

        botaoSelecionado = novoBotao;

        if (botaoSelecionado != null)
        {
            ModeloData atual = modelos.Find(m => m.botao == botaoSelecionado);
            if (atual?.selectionBox != null)
                atual.selectionBox.SetActive(true);
        }
    }

    private void OnToggleAutoRotate()
    {
        previewController.ToggleAutoRotate();

        if (imgBtnAutoRotate != null)
            imgBtnAutoRotate.sprite = previewController.autoRotate
                ? iconeRotateOn
                : iconeRotateOff;
    }

    public void Fechar()
    {
        AtualizarHighlight(null);
        gameObject.SetActive(false);
    }
}