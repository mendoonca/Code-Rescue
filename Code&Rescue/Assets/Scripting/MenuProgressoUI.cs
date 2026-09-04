using UnityEngine;
using TMPro;

public class MenuProgressoUI : MonoBehaviour
{
    [Header("Textos (TMP)")]
    public TextMeshProUGUI txtMissoesCompletadas; // NumMissoesCompletadasText
    public TextMeshProUGUI txtMissoesTotais;      // NumMissoesTotaisText
    public TextMeshProUGUI txtPrecisaoMedia;      // NumPrecisaoMediaText
    public TextMeshProUGUI txtTempoTotal;         // NumTempoTotalText

    private void OnEnable()
    {
        AtualizarInterfaceProgresso();
    }

    public void AtualizarInterfaceProgresso()
    {
        if (PlayerProgressManager.Instance != null)
        {
            DadosProgresso dados = PlayerProgressManager.Instance.ObterDadosProgresso();

            if (txtMissoesCompletadas != null)
                txtMissoesCompletadas.text = dados.missoesCompletadas.ToString();

            if (txtMissoesTotais != null)
                txtMissoesTotais.text = dados.missoesTotais.ToString();

            if (txtPrecisaoMedia != null)
                txtPrecisaoMedia.text = $"{dados.precisaoMedia:F0}%";

            if (txtTempoTotal != null)
            {
                int minutos = Mathf.FloorToInt(dados.tempoTotalSegundos / 60f);
                txtTempoTotal.text = $"{minutos} min";
            }
        }
    }

    public void VoltarAoMenu()
    {
        if (MenuPrincipalManager.Instance != null)
        {
            MenuPrincipalManager.Instance.MostrarMenuPrincipal();
        }
    }
}