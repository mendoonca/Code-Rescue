using UnityEngine;
using TMPro;

public class MenuProgressoUI : MonoBehaviour
{
    public TextMeshProUGUI textoMissoes;
    public TextMeshProUGUI textoPrecisao;
    public TextMeshProUGUI textoTempo;

    private void OnEnable()
    {
        AtualizarPainel();
    }

    public void AtualizarPainel()
    {
        if (PlayerProgressManager.Instance == null) return;

        int vitorias = PlayerProgressManager.Instance.ObterVitorias();
        float precisao = PlayerProgressManager.Instance.ObterPrecisaoMedia();
        int tempoMin = PlayerProgressManager.Instance.ObterTempoTotalMinutos();

        if (textoMissoes != null)
            textoMissoes.text = $"{vitorias}/3";

        if (textoPrecisao != null)
            textoPrecisao.text = $"{Mathf.RoundToInt(precisao)}%";

        if (textoTempo != null)
            textoTempo.text = $"{tempoMin}min";
    }
}