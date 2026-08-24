using System.Collections;
using UnityEngine;

public class VeiculoAgente : MonoBehaviour
{
    public static VeiculoAgente Instance { get; private set; }

    [Header("Configurações")]
    public float velocidadeMovimento = 4f;

    [Header("Sprites")]
    public Sprite spriteDrone;
    public Sprite spriteRobo;

    public Vector2Int PosicaoAtual { get; private set; }
    public Vector2Int DirecaoOlhar { get; private set; } = new Vector2Int(0, -1); // Por defeito: Cima

    private SpriteRenderer spriteRenderer;
    private bool temVitima = false;
    private bool isDrone = true;

    private void Awake()
    {
        Instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            isDrone = (GameManager.Instance.EquipamentoSelecionado == TipoEquipamento.Drone);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isDrone ? spriteDrone : spriteRobo;
        }

        PosicionarNaGrelha(GridManager.Instance.PosicaoInicialJogador);
    }

    // Coloca o veículo imediatamente numa coordenada da grelha
    public void PosicionarNaGrelha(Vector2Int coord)
    {
        PosicaoAtual = coord;
        transform.position = GridManager.Instance.ObterPosicaoMundo(coord);
    }

    // Move exatamente 1 bloco numa direção e atualiza a orientação
    public IEnumerator MoverUmBloco(Vector2Int direcao)
    {
        DirecaoOlhar = direcao;
        Vector2Int destino = PosicaoAtual + direcao;

        // Valida se o movimento é permitido segundo as regras da grelha
        if (!GridManager.Instance.PodeMoverPara(destino, isDrone))
        {
            Debug.LogWarning($"Caminho bloqueado ou inválido em {destino}!");
            yield break;
        }

        PosicaoAtual = destino;
        Vector3 posMundoFinal = GridManager.Instance.ObterPosicaoMundo(destino);

        while (Vector3.Distance(transform.position, posMundoFinal) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, posMundoFinal, velocidadeMovimento * Time.deltaTime);
            yield return null;
        }

        transform.position = posMundoFinal;
        VerificarInteracaoCelula();
    }

    // Ação do Robô: coloca o saco de areia no bloco para onde está virado
    public void ExecutarColocarSacoAreia()
    {
        if (isDrone) return;

        Vector2Int alvo = PosicaoAtual + DirecaoOlhar;
        bool sucesso = GridManager.Instance.ColocarSacoDeAreia(alvo);
        if (sucesso)
        {
            Debug.Log($"Saco de areia colocado em {alvo}!");
        }
    }

    // Sensores para os blocos 'Se / If'
    public bool TemAguaAFrente() => GridManager.Instance.ObterTipoElemento(PosicaoAtual + DirecaoOlhar) == TipoElemento.AguaInundacao;
    public bool TemObstaculoAFrente() => !GridManager.Instance.PodeMoverPara(PosicaoAtual + DirecaoOlhar, isDrone);
    public bool TemVitimaAFrente()
    {
        TipoElemento elem = GridManager.Instance.ObterTipoElemento(PosicaoAtual + DirecaoOlhar);
        return elem == TipoElemento.Pessoas || elem == TipoElemento.PessoasInundacao;
    }

    // Valida o objetivo de resgate na célula
    private void VerificarInteracaoCelula()
    {
        TipoElemento elemento = GridManager.Instance.ObterTipoElemento(PosicaoAtual);

        // Regra do Drone
        if (isDrone && (elemento == TipoElemento.Pessoas || elemento == TipoElemento.PessoasInundacao))
        {
            GridManager.Instance.LimparCelula(PosicaoAtual);
            Debug.Log("Drone entregou o kit! Missão Cumprida!");
            if (PlayerProgressManager.Instance != null)
                PlayerProgressManager.Instance.FinalizarMissao(true, 100f);
        }
        // Regra do Robô
        else if (!isDrone)
        {
            if ((elemento == TipoElemento.Pessoas || elemento == TipoElemento.PessoasInundacao) && !temVitima)
            {
                temVitima = true;
                GridManager.Instance.LimparCelula(PosicaoAtual);
                Debug.Log("Robô resgatou a vítima! Leva-a para o Hospital.");
            }
            else if (elemento == TipoElemento.Hospital && temVitima)
            {
                Debug.Log("Vítima entregue no Hospital! Missão Cumprida!");
                if (PlayerProgressManager.Instance != null)
                    PlayerProgressManager.Instance.FinalizarMissao(true, 100f);
            }
        }
    }
}