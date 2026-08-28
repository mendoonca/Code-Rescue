using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuModaisManager : MonoBehaviour
{
    public static MenuModaisManager Instance { get; private set; }

    [Header("Paineis Popups")]
    public GameObject popupCiclo;
    public GameObject popupAcao;
    public GameObject popupIf;

    [Header("Paineis Fim de Jogo")]
    public GameObject painelVitoria;
    public GameObject painelDerrota;

    [Header("Botoes Exclusivos no Popup de Acao")]
    public GameObject btnColocarSaco;
    public GameObject btnResgatarPessoa;
    public GameObject btnLargarKitMedico;
    public GameObject btnApagarFogo;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FecharTodosPopups();
        if (painelVitoria != null) painelVitoria.SetActive(false);
        if (painelDerrota != null) painelDerrota.SetActive(false);
    }

    // --- POPUPS DE PROGRAMAÇÃO ---
    public void AbrirPopupCiclo()
    {
        FecharTodosPopups();
        if (popupCiclo != null) popupCiclo.SetActive(true);
    }

    public void AbrirPopupAcao()
    {
        FecharTodosPopups();
        if (popupAcao != null) popupAcao.SetActive(true);

        bool isDrone = (GameManager.Instance == null || GameManager.Instance.EquipamentoSelecionado == TipoEquipamento.Drone);

        if (btnApagarFogo != null) btnApagarFogo.SetActive(true);
        if (btnLargarKitMedico != null) btnLargarKitMedico.SetActive(isDrone);
        if (btnColocarSaco != null) btnColocarSaco.SetActive(!isDrone);
        if (btnResgatarPessoa != null) btnResgatarPessoa.SetActive(!isDrone);
    }

    public void AbrirPopupIf()
    {
        FecharTodosPopups();
        if (popupIf != null) popupIf.SetActive(true);
    }

    public void FecharTodosPopups()
    {
        if (popupCiclo != null) popupCiclo.SetActive(false);
        if (popupAcao != null) popupAcao.SetActive(false);
        if (popupIf != null) popupIf.SetActive(false);
    }

    // --- SELEÇÕES CICLO ---
    public void SelecionarCiclo(int repeticoes)
    {
        ExecutadorComandos.Instance.InserirCiclo(repeticoes);
        FecharTodosPopups();
    }

    // --- SELEÇÕES AÇÃO ---
    public void SelecionarApagarFogo()    { ExecutadorComandos.Instance.InserirAcao(TipoComando.ApagarFogo); FecharTodosPopups(); }
    public void SelecionarColocarSaco()   { ExecutadorComandos.Instance.InserirAcao(TipoComando.ColocarSaco); FecharTodosPopups(); }
    public void SelecionarResgatarPessoa(){ ExecutadorComandos.Instance.InserirAcao(TipoComando.ResgatarPessoa); FecharTodosPopups(); }
    public void SelecionarLargarKit()     { ExecutadorComandos.Instance.InserirAcao(TipoComando.LargarKitMedico); FecharTodosPopups(); }

    // --- SELEÇÕES IF ---
    public void SelecionarIfFrenteFogo()       { ExecutadorComandos.Instance.InserirSe(TipoCondicao.FrenteTemFogo); FecharTodosPopups(); }
    public void SelecionarIfFrenteAgua()       { ExecutadorComandos.Instance.InserirSe(TipoCondicao.FrenteTemAgua); FecharTodosPopups(); }
    public void SelecionarIfFrentePessoa()     { ExecutadorComandos.Instance.InserirSe(TipoCondicao.FrenteTemPessoa); FecharTodosPopups(); }
    public void SelecionarIfCaminhoBloqueado() { ExecutadorComandos.Instance.InserirSe(TipoCondicao.CaminhoBloqueado); FecharTodosPopups(); }

    // --- TELAS DE FIM DE JOGO ---
    public void MostrarVitoria()
    {
        FecharTodosPopups();
        if (painelVitoria != null) painelVitoria.SetActive(true);
    }

    public void MostrarDerrota()
    {
        FecharTodosPopups();
        if (painelDerrota != null) painelDerrota.SetActive(true);
    }

    public void ReiniciarMissao()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarMenuNiveis()
    {
        if (GameManager.Instance != null && GameManager.Instance.EquipamentoSelecionado == TipoEquipamento.Drone)
            SceneManager.LoadScene("DroneNiveis");
        else
            SceneManager.LoadScene("RoboNiveis");
    }
}