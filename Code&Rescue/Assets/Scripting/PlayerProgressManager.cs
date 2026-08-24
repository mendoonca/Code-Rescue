using UnityEngine;

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance { get; private set; }

    // Chaves de armazenamento
    private const string KEY_TOTAL_JOGOS = "TotalJogos";
    private const string KEY_VITORIAS = "TotalVitorias";
    private const string KEY_DERROTAS = "TotalDerrotas";
    private const string KEY_TEMPO_TOTAL = "TempoTotalSegundos";
    private const string KEY_PRECISAO_ACUMULADA = "PrecisaoAcumulada";

    private float tempoInicioMissao;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Chama quando uma missão/nível é iniciado.
    /// </summary>
    public void IniciarMissao()
    {
        tempoInicioMissao = Time.time;
        int totalJogos = PlayerPrefs.GetInt(KEY_TOTAL_JOGOS, 0) + 1;
        PlayerPrefs.SetInt(KEY_TOTAL_JOGOS, totalJogos);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Regista a conclusão da missão (Vitória ou Game Over) e atualiza métricas.
    /// </summary>
    /// <param name="vitoria">true se salvou as vítimas / concluiu; false se falhou.</param>
    /// <param name="precisaoNivel">Percentagem de precisão calculada para este nível (0 a 100).</param>
    public void FinalizarMissao(bool vitoria, float precisaoNivel)
    {
        float tempoGasto = Time.time - tempoInicioMissao;

        // Atualizar vitórias / derrotas
        if (vitoria)
        {
            int vit = PlayerPrefs.GetInt(KEY_VITORIAS, 0) + 1;
            PlayerPrefs.SetInt(KEY_VITORIAS, vit);
        }
        else
        {
            int der = PlayerPrefs.GetInt(KEY_DERROTAS, 0) + 1;
            PlayerPrefs.SetInt(KEY_DERROTAS, der);
        }

        // Atualizar tempo acumulado
        float tempoAcumulado = PlayerPrefs.GetFloat(KEY_TEMPO_TOTAL, 0f) + tempoGasto;
        PlayerPrefs.SetFloat(KEY_TEMPO_TOTAL, tempoAcumulado);

        // Atualizar precisão acumulada
        float precAcumulada = PlayerPrefs.GetFloat(KEY_PRECISAO_ACUMULADA, 0f) + precisaoNivel;
        PlayerPrefs.SetFloat(KEY_PRECISAO_ACUMULADA, precAcumulada);

        PlayerPrefs.Save();
    }

    // Métodos para obter os dados no ecrã de Progresso
    public int ObterTotalJogos() => PlayerPrefs.GetInt(KEY_TOTAL_JOGOS, 0);
    public int ObterVitorias() => PlayerPrefs.GetInt(KEY_VITORIAS, 0);
    public int ObterDerrotas() => PlayerPrefs.GetInt(KEY_DERROTAS, 0);

    public float ObterPrecisaoMedia()
    {
        int total = ObterTotalJogos();
        if (total == 0) return 0f;
        return PlayerPrefs.GetFloat(KEY_PRECISAO_ACUMULADA, 0f) / total;
    }

    public int ObterTempoTotalMinutos()
    {
        float totalSegundos = PlayerPrefs.GetFloat(KEY_TEMPO_TOTAL, 0f);
        return Mathf.FloorToInt(totalSegundos / 60f);
    }

    public void ResetarProgresso()
    {
        PlayerPrefs.DeleteKey(KEY_TOTAL_JOGOS);
        PlayerPrefs.DeleteKey(KEY_VITORIAS);
        PlayerPrefs.DeleteKey(KEY_DERROTAS);
        PlayerPrefs.DeleteKey(KEY_TEMPO_TOTAL);
        PlayerPrefs.DeleteKey(KEY_PRECISAO_ACUMULADA);
        PlayerPrefs.Save();
    }
}