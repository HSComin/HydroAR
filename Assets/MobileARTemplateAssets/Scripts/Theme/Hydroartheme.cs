using UnityEngine;

[CreateAssetMenu(fileName = "HydroARTheme", menuName = "HydroAR/Theme")]
public class HydroARTheme : ScriptableObject
{
    [Header("Primária (verde)")]
    public Color primary900 = new Color32(0x0B, 0x4A, 0x16, 0xFF);
    public Color primary700 = new Color32(0x17, 0x8C, 0x1E, 0xFF);
    public Color primary500 = new Color32(0x2F, 0xA8, 0x3A, 0xFF);
    public Color primary100 = new Color32(0xDF, 0xF4, 0xE1, 0xFF);

    [Header("Secundária (accent — resolve o problema dos cards azuis)")]
    public Color accent700 = new Color32(0x0E, 0x7C, 0x86, 0xFF);
    public Color accent100 = new Color32(0xDF, 0xF0, 0xF2, 0xFF);

    [Header("Neutros")]
    public Color surface = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    public Color background = new Color32(0xF4, 0xF6, 0xF5, 0xFF);
    public Color textPrimary = new Color32(0x1A, 0x1F, 0x1C, 0xFF);
    public Color textSecondary = new Color32(0x5B, 0x67, 0x5E, 0xFF);
    public Color border = new Color32(0xE2, 0xE7, 0xE3, 0xFF);

    [Header("Sombra")]
    [Range(0f, 1f)] public float shadowOpacity = 0.12f;
}