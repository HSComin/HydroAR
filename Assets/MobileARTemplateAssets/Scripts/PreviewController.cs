using UnityEngine;
using UnityEngine.UI;

public class PreviewController : MonoBehaviour
{
    [Header("UI")]
    public RawImage displayUI;
    public RenderTexture rt;

    [Header("Rotação")]
    public float rotationSpeed = 0.3f;
    public bool autoRotate = true;
    public float autoRotateSpeed = 20f;

    [Header("Zoom")]
    public float minZoom = 1f;
    public float maxZoom = 5f;

    [Header("AR")]
    public ARTemplateMenuManager arMenuManager;

    private string nomeModeloAtual;

    private GameObject previewScene;
    private Transform previewRoot;
    private Camera previewCamera;
    private Light previewLight;
    private GameObject modeloAtual;

    private Vector3 cameraPosInicial;
    private Quaternion cameraRotInicial;

    private Vector2 lastTouchPos;
    private bool isDragging;

    private static readonly Vector3 PREVIEW_OFFSET = new Vector3(5000, 5000, 5000);

    void Awake()
    {
        CriarCena();
    }

    public void SelecionarModelo(GameObject prefab)
    {
        MostrarModelo(prefab);
        nomeModeloAtual = prefab.name;
    }

    public void VerEmAR()
    {
        if (arMenuManager == null || string.IsNullOrEmpty(nomeModeloAtual))
        {
            Debug.LogWarning("ARMenuManager não atribuído ou nenhum modelo selecionado.");
            return;
        }

        arMenuManager.AbrirARComModelo(nomeModeloAtual);
    }

    void CriarCena()
    {
        previewScene = new GameObject("_PreviewScene3D");
        previewScene.transform.position = PREVIEW_OFFSET;

        var rootGO = new GameObject("PreviewRoot");
        rootGO.transform.SetParent(previewScene.transform);
        rootGO.transform.localPosition = Vector3.zero;
        previewRoot = rootGO.transform;

        var camGO = new GameObject("PreviewCamera");
        camGO.transform.SetParent(previewScene.transform);
        camGO.transform.localPosition = new Vector3(0, 2, -5);

        previewCamera = camGO.AddComponent<Camera>();
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0, 0, 0, 0);
        previewCamera.fieldOfView = 60f;
        previewCamera.cullingMask = ~0;
        previewCamera.allowHDR = true;
        previewCamera.allowMSAA = true;
        previewCamera.depth = 10;

        previewCamera.targetTexture = rt;
        displayUI.texture = rt;

        Debug.Log("RT atribuída: " + (rt != null ? rt.name : "NULO"));

        var lightGO = new GameObject("PreviewLight");
        lightGO.transform.SetParent(previewScene.transform);
        lightGO.transform.localPosition = new Vector3(2, 5, -3);
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
        previewLight = lightGO.AddComponent<Light>();
        previewLight.type = LightType.Directional;
        previewLight.intensity = 1.5f;
    }

    void Update()
    {
        if (modeloAtual == null) return;

        HandleInput();

        if (autoRotate && !isDragging)
            previewRoot.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime, Space.World);
    }

    public void MostrarModelo(GameObject prefab)
    {
        if (modeloAtual != null)
            Destroy(modeloAtual);

        modeloAtual = Instantiate(prefab, previewRoot);
        modeloAtual.transform.localPosition = Vector3.zero;
        modeloAtual.transform.localRotation = Quaternion.identity;
        modeloAtual.transform.localScale = Vector3.one;

        foreach (var t in modeloAtual.GetComponentsInChildren<Transform>(true))
            t.gameObject.SetActive(true);

        RemoverComponentesAR(modeloAtual);
        AjustarCamera();
    }

    void RemoverComponentesAR(GameObject obj)
    {
        var rb = obj.GetComponentInChildren<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        var xr = obj.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (xr != null) xr.enabled = false;

        foreach (var col in obj.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    void AjustarCamera()
    {
        Renderer[] renderers = modeloAtual.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = new Bounds(modeloAtual.transform.position, Vector3.zero);
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);

        modeloAtual.transform.position -= (bounds.center - previewRoot.position);

        float tamanhoAlvo = 1.0f;
        float tamanhoAtual = bounds.extents.magnitude;
        float fatorEscala = tamanhoAlvo / tamanhoAtual;
        modeloAtual.transform.localScale = Vector3.one * fatorEscala;

        float distanciaFixa = 1.5f;
        float alturaFixa = 0.5f;
        previewCamera.transform.localPosition = new Vector3(0, alturaFixa, -distanciaFixa);
        previewCamera.transform.LookAt(previewRoot.position);

        cameraPosInicial = previewCamera.transform.localPosition;
        cameraRotInicial = previewCamera.transform.localRotation;
    }

    void HandleInput()
    {
        if (Application.isEditor)
        {
            HandleMouse();
            return;
        }

        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                if (!TouchDentroDoPreview(t.position)) return;
                lastTouchPos = t.position;
                isDragging = true;
            }
            else if (t.phase == TouchPhase.Moved && isDragging)
            {
                RotateRoot(t.position - lastTouchPos);
                lastTouchPos = t.position;
            }
            else if (t.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }
        else if (Input.touchCount == 2 && isDragging)
        {
            PinchZoom();
        }
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!TouchDentroDoPreview(Input.mousePosition)) return;
            lastTouchPos = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            RotateRoot((Vector2)Input.mousePosition - lastTouchPos);
            lastTouchPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && TouchDentroDoPreview(Input.mousePosition))
            ZoomCamera(scroll * -2f);
    }

    bool TouchDentroDoPreview(Vector2 screenPos)
    {
        if (displayUI == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(
            displayUI.rectTransform,
            screenPos,
            null
        );
    }

    void RotateRoot(Vector2 delta)
    {
        previewRoot.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
        previewRoot.Rotate(Vector3.right, delta.y * rotationSpeed, Space.World);
    }

    void PinchZoom()
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        float before = Vector2.Distance(t0.position - t0.deltaPosition, t1.position - t1.deltaPosition);
        float after = Vector2.Distance(t0.position, t1.position);

        ZoomCamera((before - after) * 0.01f);
    }

    void ZoomCamera(float delta)
    {
        Vector3 pos = previewCamera.transform.localPosition;
        pos.z = Mathf.Clamp(pos.z + delta, -maxZoom, -minZoom);
        previewCamera.transform.localPosition = pos;
    }

    public void ResetarRotacao()
    {
        previewRoot.localRotation = Quaternion.identity;
        previewCamera.transform.localPosition = cameraPosInicial;
        previewCamera.transform.localRotation = cameraRotInicial;
    }

    public void ToggleAutoRotate()
    {
        autoRotate = !autoRotate;
    }

    void OnDestroy()
    {
        if (previewScene != null)
            Destroy(previewScene);
    }
}