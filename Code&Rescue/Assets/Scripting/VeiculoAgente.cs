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
            // Caso estejas a testar diretamente na cena sem vir do Menu
            isDrone = false; // Altera aqui para 'false' se quiseres forçar teste de Robô diretamente
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isDrone ? spriteDrone : spriteRobo;
            spriteRenderer.sortingOrder = 2; // Fica visível à frente do chão

            // Ajusta a escala para 1 célula
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

    // Posiciona o veículo numa coordenada da matriz
    public void PosicionarNaGrelha(Vector2Int coord)
    {
        PosicaoAtual = coord;
        transform.position = GridManager.Instance.ObterPosicaoMundo(coord);
    }

    // Movimenta o veículo 1 bloco
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

    // Robô coloca saco de areia à frente
    public void ExecutarColocarSacoAreia()
    {
        if (isDrone) return;
        Vector2Int alvo = PosicaoAtual + DirecaoOlhar;
        GridManager.Instance.ColocarSacoDeAreia(alvo);
    }

    // Sensores booleanos para os Ifs
    public bool TemAguaAFrente() => GridManager.Instance.ObterTipoElemento(PosicaoAtual + DirecaoOlhar) == TipoElemento.AguaInundacao;
    public bool TemObstaculoAFrente() => !GridManager.Instance.PodeMoverPara(PosicaoAtual + DirecaoOlhar, isDrone);
    public bool TemVitimaAFrente()
    {
        TipoElemento elem = GridManager.Instance.ObterTipoElemento(PosicaoAtual + DirecaoOlhar);
        return elem == TipoElemento.Pessoas || elem == TipoElemento.PessoasInundacao;
    }

    // Valida o progresso de resgates do nível atual
    private void VerificarInteracoes()
    {
        TipoElemento elem = GridManager.Instance.ObterTipoElemento(PosicaoAtual);
        int totalNecessario = GridManager.Instance.TotalVitimasNivel;

        // Regra do Drone
        if (isDrone && (elem == TipoElemento.Pessoas || elem == TipoElemento.PessoasInundacao))
        {
            GridManager.Instance.LimparCelula(PosicaoAtual);
            vitimasResgatadas++;
            Debug.Log($"Kit entregue! Vítimas salvas: {vitimasResgatadas}/{totalNecessario}");

            if (vitimasResgatadas >= totalNecessario)
            {
                Debug.Log("Todas as vítimas foram salvas pelo Drone! Missão Concluída!");
                if (PlayerProgressManager.Instance != null)
                    PlayerProgressManager.Instance.FinalizarMissao(true, 100f);
            }
        }
        // Regra do Robô
        else if (!isDrone)
        {
            if (elem == TipoElemento.Pessoas || elem == TipoElemento.PessoasInundacao)
            {
                GridManager.Instance.LimparCelula(PosicaoAtual);
                vitimasEmTransporte++;
                Debug.Log($"Vítima a bordo! Leva ao Hospital ({vitimasEmTransporte} a bordo).");
            }
            else if (elem == TipoElemento.Hospital && vitimasEmTransporte > 0)
            {
                vitimasResgatadas += vitimasEmTransporte;
                vitimasEmTransporte = 0;
                Debug.Log($"Vítimas entregues no Hospital! Total salvo: {vitimasResgatadas}/{totalNecessario}");

                if (vitimasResgatadas >= totalNecessario)
                {
                    Debug.Log("Todas as vítimas estão a salvo no Hospital! Missão Concluída!");
                    if (PlayerProgressManager.Instance != null)
                        PlayerProgressManager.Instance.FinalizarMissao(true, 100f);
                }
            }
        }
    }
}