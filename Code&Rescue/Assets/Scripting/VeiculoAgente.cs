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

    public bool TeveDerrota { get; private set; } = false;

    public IEnumerator MoverUmBloco(Vector2Int direcao)
    {
        DirecaoOlhar = direcao;
        Vector2Int destino = PosicaoAtual + direcao;

        // Impede sair dos limites da grelha (fora do mapa)
        int tamanhoGrelha = (GridManager.Instance.nivelAtual == 1) ? 5 : ((GridManager.Instance.nivelAtual == 2) ? 7 : 9);
        if (destino.x < 0 || destino.x >= tamanhoGrelha || destino.y < 0 || destino.y >= tamanhoGrelha)
        {
            Debug.LogWarning($"Movimento bloqueado: {destino} está fora do mapa!");
            yield break;
        }

        TipoElemento elemDestino = GridManager.Instance.ObterTipoElemento(destino);

        // Bloqueia obstáculos intransponíveis
        if (elemDestino == TipoElemento.Casa ||
            elemDestino == TipoElemento.CasaIncendio ||
            elemDestino == TipoElemento.CasaInundacao ||
            elemDestino == TipoElemento.PredioSismoTerramoto ||
            elemDestino == TipoElemento.Destrocos ||
            elemDestino == TipoElemento.Arvores)
        {
            Debug.LogWarning($"Movimento bloqueado para obstáculo em {destino}!");
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

    public void ExecutarDerrota()
    {
        if (MissaoConcluida || TeveDerrota) return;

        TeveDerrota = true;
        StopAllCoroutines();

        if (PlayerProgressManager.Instance != null)
            PlayerProgressManager.Instance.FinalizarMissao(false, 0f);

        if (MenuModaisManager.Instance != null)
            MenuModaisManager.Instance.MostrarDerrota();
    }

    public void ExecutarColocarSacoAreia()
    {
        Vector2Int frente = PosicaoAtual + DirecaoOlhar;
        Vector2Int alvo = (GridManager.Instance.ObterTipoElemento(PosicaoAtual) == TipoElemento.AguaInundacao) ? PosicaoAtual : frente;

        if (GridManager.Instance.ColocarSacoDeAreia(alvo))
        {
            Debug.Log("Saco de areia colocado!");
        }
    }

    // Verifica se o robô está em cima de um perigo não neutralizado
    public bool EstaEmPerigoMortal()
    {
        TipoElemento elem = GridManager.Instance.ObterTipoElemento(PosicaoAtual);
        if (elem == TipoElemento.Chama) return true;
        if (!isDrone && elem == TipoElemento.AguaInundacao) return true;
        return false;
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
        Vector2Int frente = PosicaoAtual + DirecaoOlhar;
        Vector2Int alvo = (GridManager.Instance.ObterTipoElemento(PosicaoAtual) == TipoElemento.Chama) ? PosicaoAtual : frente;

        if (GridManager.Instance.ObterTipoElemento(alvo) == TipoElemento.Chama)
        {
            GridManager.Instance.LimparCelula(alvo);
            Debug.Log("Chama apagada com sucesso!");
        }
    }

    // Ação do Drone: Largar Kit Médico
    public void ExecutarLargarKit()
    {
        if (!isDrone) return;

        Vector2Int frente = PosicaoAtual + DirecaoOlhar;
        
        // Verifica se a vítima está na mesma célula ou na célula à frente
        Vector2Int alvo = (GridManager.Instance.ObterTipoElemento(PosicaoAtual) == TipoElemento.Pessoas ||
                           GridManager.Instance.ObterTipoElemento(PosicaoAtual) == TipoElemento.PessoasInundacao) 
                           ? PosicaoAtual : frente;

        TipoElemento elem = GridManager.Instance.ObterTipoElemento(alvo);

        if (elem == TipoElemento.Pessoas || elem == TipoElemento.PessoasInundacao)
        {
            GridManager.Instance.LimparCelula(alvo);
            vitimasResgatadas++;
            Debug.Log($"Kit entregue com sucesso! Salvas: {vitimasResgatadas}/{GridManager.Instance.TotalVitimasNivel}");
            VerificarVitoria();
        }
        else
        {
            Debug.LogWarning("Nenhuma vítima encontrada para entregar o kit!");
        }
    }

    // Ação do Robô: Resgatar Pessoa
    public void ExecutarResgatarPessoa()
    {
        if (isDrone) return;

        Vector2Int frente = PosicaoAtual + DirecaoOlhar;

        // Verifica se a vítima está na mesma célula ou na célula à frente
        Vector2Int alvo = (GridManager.Instance.ObterTipoElemento(PosicaoAtual) == TipoElemento.Pessoas ||
                           GridManager.Instance.ObterTipoElemento(PosicaoAtual) == TipoElemento.PessoasInundacao) 
                           ? PosicaoAtual : frente;

        TipoElemento elem = GridManager.Instance.ObterTipoElemento(alvo);

        if (elem == TipoElemento.Pessoas || elem == TipoElemento.PessoasInundacao)
        {
            GridManager.Instance.LimparCelula(alvo);
            vitimasEmTransporte++;
            Debug.Log($"Vítima recolhida! Vítimas a bordo: {vitimasEmTransporte}. Leva-a ao Hospital!");
        }
        else
        {
            Debug.LogWarning("Nenhuma vítima próxima para resgatar!");
        }
    }

    public bool MissaoConcluida { get; private set; } = false;

    // Repõe o veículo na posição inicial sem apagar o algoritmo da UI
    public void ReporPosicaoInicial()
    {
        if (MissaoConcluida) return;

        StopAllCoroutines();
        TeveDerrota = false;
        vitimasResgatadas = 0;
        vitimasEmTransporte = 0;
        DirecaoOlhar = new Vector2Int(0, -1);
        PosicionarNaGrelha(Vector2Int.zero);

        if (GridManager.Instance != null)
        {
            GridManager.Instance.RestaurarMapaOriginal();
        }
    }

    private void VerificarVitoria()
    {
        int totalNecessario = GridManager.Instance.TotalVitimasNivel;
        if (vitimasResgatadas >= totalNecessario)
        {
            MissaoConcluida = true;
            Debug.Log("Missão Concluída com Sucesso!");

            if (PlayerProgressManager.Instance != null)
                PlayerProgressManager.Instance.FinalizarMissao(true, 100f);

            // Abre o painel de vitória na UI
            if (MenuModaisManager.Instance != null)
                MenuModaisManager.Instance.MostrarVitoria();
        }
    }
}