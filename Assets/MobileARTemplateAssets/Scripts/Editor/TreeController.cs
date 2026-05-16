using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TreeController : EditorWindow
{
    [System.Serializable]
    public class TreeEntry
    {
        public GameObject prefab;
        public int quantidade = 10;
    }

    List<GameObject> planosAlvo = new List<GameObject>();
    Transform parentTrees;
    List<TreeEntry> trees = new List<TreeEntry>();

    float yOffset = 0f;
    Vector2 escalaAleatoria = new Vector2(0.02f, 0.02f);

    [MenuItem("Tools/Tree Controller")]
    public static void Open()
    {
        GetWindow<TreeController>("Tree Controller");
    }

    void OnGUI()
    {
        GUILayout.Label("Gerador de Árvores", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Planos Alvo", EditorStyles.boldLabel);

        for (int i = 0; i < planosAlvo.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            planosAlvo[i] = (GameObject)EditorGUILayout.ObjectField(
                planosAlvo[i], typeof(GameObject), true);

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                planosAlvo.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Adicionar Plano"))
        {
            planosAlvo.Add(null);
        }

        EditorGUILayout.Space();

        parentTrees = (Transform)EditorGUILayout.ObjectField(
            "Parent (Trees)", parentTrees, typeof(Transform), true);

        EditorGUILayout.Space();
        GUILayout.Label("Lista de Árvores", EditorStyles.boldLabel);

        for (int i = 0; i < trees.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");

            trees[i].prefab = (GameObject)EditorGUILayout.ObjectField(
                "Prefab", trees[i].prefab, typeof(GameObject), false);

            trees[i].quantidade = EditorGUILayout.IntField(
                "Quantidade", trees[i].quantidade);

            if (GUILayout.Button("Remover"))
            {
                trees.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Adicionar Árvore"))
        {
            trees.Add(new TreeEntry());
        }

        EditorGUILayout.Space();

        yOffset = EditorGUILayout.FloatField("YOffset", yOffset);
        escalaAleatoria = EditorGUILayout.Vector2Field("Escala Aleatória", escalaAleatoria);

        EditorGUILayout.Space();

        if (GUILayout.Button("Gerar Árvores"))
            Gerar();

        if (GUILayout.Button("Limpar Árvores"))
            Limpar();
    }

    void Gerar()
    {
        if (planosAlvo.Count == 0 || !parentTrees)
        {
            Debug.LogWarning("Planos alvo ou Parent não definido.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(parentTrees.gameObject, "Gerar Árvores");

        foreach (var plano in planosAlvo)
        {
            if (!plano) continue;

            MeshRenderer renderer = plano.GetComponentInChildren<MeshRenderer>();
            if (!renderer)
            {
                Debug.LogWarning($"Plano {plano.name} não possui MeshRenderer.");
                continue;
            }

            Bounds bounds = renderer.bounds;

            foreach (var entry in trees)
            {
                if (!entry.prefab || entry.quantidade <= 0)
                    continue;

                for (int i = 0; i < entry.quantidade; i++)
                {
                    Vector3 pos = new Vector3(
                        Random.Range(bounds.min.x, bounds.max.x),
                        bounds.center.y,
                        Random.Range(bounds.min.z, bounds.max.z)
                    );

                    GameObject t = (GameObject)PrefabUtility.InstantiatePrefab(entry.prefab, parentTrees);
                    t.transform.position = pos + Vector3.up * yOffset;
                    t.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                    float escala = Random.Range(escalaAleatoria.x, escalaAleatoria.y);
                    t.transform.localScale *= escala;
                }
            }
        }
    }

    void Limpar()
    {
        for (int i = parentTrees.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(parentTrees.GetChild(i).gameObject);
        }
    }
}
