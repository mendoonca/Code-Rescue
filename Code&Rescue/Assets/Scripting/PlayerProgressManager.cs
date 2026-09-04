using UnityEngine;

[System.Serializable]
public struct DadosProgresso
{
    public int missoesCompletadas;
    public int missoesTotais;
    public float precisaoMedia;
    public float tempoTotalSegundos;
}

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance { get; private set; }

    private int missoesCompletadas;
    private float somaPrecisoes;
    private int totalTentativas;
    private float tempoTotalSegundos;

    private float precisaoMedia => totalTentativas > 0 ? somaPrecisoes / totalTentativas : 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CarregarDados();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Contabiliza o tempo jogado globalmente
        tempoTotalSegundos += Time.unscaledDeltaTime;
    }

    private void CarregarDados()
    {
        missoesCompletadas = PlayerPrefs.GetInt("Progresso_MissoesCompletadas", 0);
        somaPrecisoes = PlayerPrefs.GetFloat("Progresso_SomaPrecisoes", 0f);
        totalTentativas = PlayerPrefs.GetInt("Progresso_TotalTentativas", 0);
        tempoTotalSegundos = PlayerPrefs.GetFloat("Progresso_TempoTotal", 0f);
    }

    private void SalvarDados()
    {
        PlayerPrefs.SetInt("Progresso_MissoesCompletadas", missoesCompletadas);
        PlayerPrefs.SetFloat("Progresso_SomaPrecisoes", somaPrecisoes);
        PlayerPrefs.SetInt("Progresso_TotalTentativas", totalTentativas);
        PlayerPrefs.SetFloat("Progresso_TempoTotal", tempoTotalSegundos);
        PlayerPrefs.Save();
    }

    public void IniciarMissao() { }

    public void FinalizarMissao(bool vitoria, float pontuacao)
    {
        Debug.Log($"FinalizarMissao chamado! Vitória: {vitoria}, Pontuação: {pontuacao}");

        if (vitoria && missoesCompletadas < 3)
        {
            missoesCompletadas++;
        }

        // Cada tentativa conta igual para a média: vitória entra com a sua
        // pontuação, derrota entra com 0%.
        somaPrecisoes += vitoria ? Mathf.Clamp(pontuacao, 0f, 100f) : 0f;
        totalTentativas++;

        SalvarDados();
        Debug.Log($"Dados guardados - Missões: {missoesCompletadas}, Precisão: {precisaoMedia}");
    }

    public DadosProgresso ObterDadosProgresso()
    {
        return new DadosProgresso
        {
            missoesCompletadas = this.missoesCompletadas,
            missoesTotais = this.totalTentativas,
            precisaoMedia = this.precisaoMedia,
            tempoTotalSegundos = this.tempoTotalSegundos
        };
    }

    public void ResetarProgressoTotal()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        missoesCompletadas = 0;
        somaPrecisoes = 0f;
        totalTentativas = 0;
        tempoTotalSegundos = 0f;
        Debug.Log("Progresso reiniciado com sucesso.");
    }
}