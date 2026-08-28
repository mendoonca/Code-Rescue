using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuModaisManager : MonoBehaviour
{
    public static MenuModaisManager Instance { get; private set; }

    [Header("Paineis Popups")]
    public GameObject popupCiclo;
    public GameObject popupAcao;
    public GameObject popupIf;

    [Header("Botoes Exclusivos no Popup de Acao")]
    public GameObject btnColocarSaco;      // Robo
    public GameObject btnResgatarPessoa;   // Robo
    public GameObject btnLargarKitMedico;  // Drone
    public GameObject btnApagarFogo;       // Ambos

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FecharTodosPopups();
    }

    // --- ABRIR POPUPS ---
    public void AbrirPopupCiclo()
    {
        FecharTodosPopups();
        popupCiclo.SetActive(true);
    }

    public void AbrirPopupAcao()
    {
        FecharTodosPopups();
        popupAcao.SetActive(true);

        bool isDrone = (GameManager.Instance == null || GameManager.Instance.EquipamentoSelecionado == TipoEquipamento.Drone);

        // Filtra os botões de acordo com o veículo ativo
        btnApagarFogo.SetActive(true);
        btnLargarKitMedico.SetActive(isDrone);
        btnColocarSaco.SetActive(!isDrone);
        btnResgatarPessoa.SetActive(!isDrone);
    }

    public void AbrirPopupIf()
    {
        FecharTodosPopups();
        popupIf.SetActive(true);
    }

    public void FecharTodosPopups()
    {
        if (popupCiclo != null) popupCiclo.SetActive(false);
        if (popupAcao != null) popupAcao.SetActive(false);
        if (popupIf != null) popupIf.SetActive(false);
    }

    // --- SELEÇÕES DO POPUP CICLO (2 a 5) ---
    public void SelecionarCiclo(int repeticoes)
    {
        ExecutadorComandos.Instance.InserirCiclo(repeticoes);
        FecharTodosPopups();
    }

    // --- SELEÇÕES DO POPUP AÇÃO ---
    public void SelecionarApagarFogo()
    {
        ExecutadorComandos.Instance.InserirAcao(TipoComando.ApagarFogo);
        FecharTodosPopups();
    }

    public void SelecionarColocarSaco()
    {
        ExecutadorComandos.Instance.InserirAcao(TipoComando.ColocarSaco);
        FecharTodosPopups();
    }

    public void SelecionarResgatarPessoa()
    {
        ExecutadorComandos.Instance.InserirAcao(TipoComando.ResgatarPessoa);
        FecharTodosPopups();
    }

    public void SelecionarLargarKit()
    {
        ExecutadorComandos.Instance.InserirAcao(TipoComando.LargarKitMedico);
        FecharTodosPopups();
    }

    // --- SELEÇÕES DO POPUP IF ---
    public void SelecionarIfFrenteFogo()
    {
        ExecutadorComandos.Instance.InserirSe(TipoCondicao.FrenteTemFogo);
        FecharTodosPopups();
    }

    public void SelecionarIfFrenteAgua()
    {
        ExecutadorComandos.Instance.InserirSe(TipoCondicao.FrenteTemAgua);
        FecharTodosPopups();
    }

    public void SelecionarIfFrentePessoa()
    {
        ExecutadorComandos.Instance.InserirSe(TipoCondicao.FrenteTemPessoa);
        FecharTodosPopups();
    }

    public void SelecionarIfCaminhoBloqueado()
    {
        ExecutadorComandos.Instance.InserirSe(TipoCondicao.CaminhoBloqueado);
        FecharTodosPopups();
    }

    public void VoltarMenuNiveis()
    {
        // Verifica qual equipamento estava selecionado para voltar ao menu correspondente
        if (GameManager.Instance != null && GameManager.Instance.EquipamentoSelecionado == TipoEquipamento.Drone)
        {
            SceneManager.LoadScene("DroneNiveis");
        }
        else
        {
            SceneManager.LoadScene("RoboNiveis");
        }
    }
}