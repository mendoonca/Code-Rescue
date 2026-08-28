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

    public Vector2Int PosicaoAtual { get; private set; } = Vector2Int.zero;
    public Vector2Int DirecaoOlhar { get; private set; } = new Vector2Int(0, -1);

    private SpriteRenderer spriteRenderer;
    private int vitimasResgatadas = 0;
    private int vitimasEmTransporte = 0;
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
        else
        {
            isDrone = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isDrone ? spriteDrone : spriteRobo;
            spriteRenderer.sortingOrder = 2;

            if (spriteRenderer.sprite != null && GridManager.Instance != null)
            {
                float tam = GridManager.Instance.tamanhoBloco;
                float escalaX = tam / spriteRenderer.sprite.bounds.size.x;
                float escalaY = tam / spriteRenderer.sprite.bounds.size.y;
                transform.localScale = new Vector3(escalaX, escalaY, 1f);
            }
        }

        PosicionarNaGrelha(Vector2Int.zero);
    }

    public void PosicionarNaGrelha(Vector2Int coord)
    {
        PosicaoAtual = coord;
        transform.position = GridManager.Instance.ObterPosicaoMundo(coord);
    }

    public IEnumerator MoverUmBloco(Vector2Int direcao)
    {
        DirecaoOlhar = direcao;
        Vector2Int destino = PosicaoAtual + direcao;

        if (!GridManager.Instance.PodeMoverPara(destino, isDrone))
        {
            Debug.LogWarning($"Movimento bloqueado para {destino}!");
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
        VerificarInteracoes();
    }

    public void ExecutarColocarSacoAreia()
    {
        if (isDrone) return;
        Vector2Int alvo = PosicaoAtual + DirecaoOlhar;
        GridManager.Instance.ColocarSacoDeAreia(alvo);
    }

    public bool TemAguaAFrente() => GridManager.Instance.ObterTipoElemento(PosicaoAtual + DirecaoOlhar) == TipoElemento.AguaInundacao;
    public bool TemObstaculoAFrente() => !GridManager.Instance.PodeMoverPara(PosicaoAtual + DirecaoOlhar, isDrone);
    public bool TemVitimaAFrente()
    {
        TipoElemento elem = GridManager.Instance.ObterTipoElemento(PosicaoAtual + DirecaoOlhar);
        return elem == TipoElemento.Pessoas || elem == TipoElemento.PessoasInundacao;
    }
    public bool TemFogoAFrente() => GridManager.Instance.ObterTipoElemento(PosicaoAtual + DirecaoOlhar) == TipoElemento.Chama;

    private void VerificarInteracoes()
    {
        TipoElemento elem = GridManager.Instance.ObterTipoElemento(PosicaoAtual);

        if (!isDrone && elem == TipoElemento.Hospital && vitimasEmTransporte > 0)
        {
            vitimasResgatadas += vitimasEmTransporte;
            vitimasEmTransporte = 0;
            int totalNecessario = GridManager.Instance.TotalVitimasNivel;
            Debug.Log($"Vítimas entregues no Hospital! Total salvo: {vitimasResgatadas}/{totalNecessario}");
            VerificarVitoria();
        }
    }

    public void ExecutarApagarFogo()
    {
        Vector2Int alvo = PosicaoAtual + DirecaoOlhar;
        if (GridManager.Instance.ObterTipoElemento(alvo) == TipoElemento.Chama)
        {
            GridManager.Instance.LimparCelula(alvo);
            Debug.Log("Chama apagada com sucesso!");
        }
    }

    public void ExecutarLargarKit()
    {
        if (!isDrone) return;
        Vector2Int alvo = PosicaoAtual + DirecaoOlhar;
        TipoElemento elem = GridManager.Instance.ObterTipoElemento(alvo);

        if (elem == TipoElemento.Pessoas || elem == TipoElemento.PessoasInundacao)
        {
            GridManager.Instance.LimparCelula(alvo);
            vitimasResgatadas++;
            int totalNecessario = GridManager.Instance.TotalVitimasNivel;
            Debug.Log($"Kit entregue! Vítimas salvas: {vitimasResgatadas}/{totalNecessario}");
            VerificarVitoria();
        }
    }

    public void ExecutarResgatarPessoa()
    {
        if (isDrone) return;
        Vector2Int alvo = PosicaoAtual + DirecaoOlhar;
        TipoElemento elem = GridManager.Instance.ObterTipoElemento(alvo);

        if (elem == TipoElemento.Pessoas || elem == TipoElemento.PessoasInundacao)
        {
            GridManager.Instance.LimparCelula(alvo);
            vitimasEmTransporte++;
            Debug.Log($"Vítima recolhida! Vítimas a bordo: {vitimasEmTransporte}");
        }
    }

    private void VerificarVitoria()
    {
        int totalNecessario = GridManager.Instance.TotalVitimasNivel;
        if (vitimasResgatadas >= totalNecessario)
        {
            Debug.Log("Missão Concluída com Sucesso!");
            if (PlayerProgressManager.Instance != null)
                PlayerProgressManager.Instance.FinalizarMissao(true, 100f);
        }
    }
}