using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(MeshRenderer))]
public class FixARMaterial : MonoBehaviour
{
    [SerializeField] private Material[] targetMaterials;

    void Start()
    {
        StartCoroutine(AplicarMaterial());
    }

    private System.Collections.IEnumerator AplicarMaterial()
    {
        yield return new WaitUntil(() =>
            ARSession.state == ARSessionState.SessionTracking);

        GetComponent<MeshRenderer>().materials = targetMaterials;
    }
}