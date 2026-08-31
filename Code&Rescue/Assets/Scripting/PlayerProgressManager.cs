using UnityEngine;

[System.Serializable]
public struct DadosProgresso
{
    public int missoesCompletadas;
    public float precisaoMedia;
    public float tempoTotalSegundos;
}

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance { get; private set; }

    private int missoesCompletadas;
    private float precisaoMedia;
    private float tempoTotalSegundos;

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
        precisaoMedia = PlayerPrefs.GetFloat("Progresso_PrecisaoMedia", 0f);
        tempoTotalSegundos = PlayerPrefs.GetFloat("Progresso_TempoTotal", 0f);
    }

    private void SalvarDados()
    {
        PlayerPrefs.SetInt("Progresso_MissoesCompletadas", missoesCompletadas);
        PlayerPrefs.SetFloat("Progresso_PrecisaoMedia", precisaoMedia);
        PlayerPrefs.SetFloat("Progresso_TempoTotal", tempoTotalSegundos);
        PlayerPrefs.Save();
    }

    public void IniciarMissao() { }

    public void FinalizarMissao(bool vitoria, float pontuacao)
    {
        Debug.Log($"FinalizarMissao chamado! Vitória: {vitoria}, Pontuação: {pontuacao}");

        if (vitoria)
        {
            if (missoesCompletadas < 3)
            {
                missoesCompletadas++;
            }

            if (precisaoMedia <= 0f)
                precisaoMedia = pontuacao;
            else
                precisaoMedia = (precisaoMedia + pontuacao) / 2f;
        }
        else
        {
            // Se perdeu, entra na média com 0% de precisão para penalizar o progresso
            if (precisaoMedia <= 0f)
                precisaoMedia = 0f;
            else
                precisaoMedia = (precisaoMedia + 0f) / 2f;
        }

        SalvarDados();
        Debug.Log($"Dados guardados - Missões: {missoesCompletadas}, Precisão: {precisaoMedia}");
    }

    public DadosProgresso ObterDadosProgresso()
    {
        return new DadosProgresso
        {
            missoesCompletadas = this.missoesCompletadas,
            precisaoMedia = this.precisaoMedia,
            tempoTotalSegundos = this.tempoTotalSegundos
        };
    }

    public void ResetarProgressoTotal()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        missoesCompletadas = 0;
        precisaoMedia = 0f;
        tempoTotalSegundos = 0f;
        Debug.Log("Progresso reiniciado com sucesso.");
    }
}