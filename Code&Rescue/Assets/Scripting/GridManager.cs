using System.Collections.Generic;
using UnityEngine;

public enum TipoElemento
{
    Vazio,
    Casa,
    CasaIncendio,
    CasaInundacao,
    Chama,
    PredioSismoTerramoto,
    Destrocos,
    Arvores,
    AguaInundacao,
    MuroSacoAreia,
    Pessoas,
    PessoasInundacao,
    Hospital
}

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Configuração de Nível (1 = Incêndio, 2 = Inundação, 3 = Sismo/Incêndio)")]
    [Range(1, 3)] public int nivelAtual = 1;
    public float tamanhoBloco = 1f;

    [Header("Sprites de Cenário")]
    public Sprite spriteCelulaBase;
    public Sprite spriteCasa;
    public Sprite spriteCasaIncendio;
    public Sprite spriteCasaInundacao;
    public Sprite spriteChama;
    public Sprite spritePredioSismo;
    public Sprite spriteDestrocos;
    public Sprite spriteAguaInundacao;
    public Sprite spriteMuroSacoAreia;
    public Sprite spriteHospital;

    [Header("Sprites com Variações")]
    public Sprite[] spritesArvores;
    public Sprite[] spritesPessoas;
    public Sprite[] spritesPessoasInundacao;

    private int tamanhoGrelha;
    private TipoElemento[,] mapa;
    private GameObject[,] objetosNoMapa;
    private List<Vector2Int> posicoesLivres = new List<Vector2Int>();
    private List<Vector2Int> posicoesVitimas = new List<Vector2Int>();
    private Vector2Int posHospital;

    // Snapshot para restaurar o mapa original
    private struct DadosCelula
    {
        public TipoElemento tipo;
        public Sprite sprite;
    }
    private DadosCelula[,] mapaInicial;

    public int TotalVitimasNivel { get; private set; } = 1;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            nivelAtual = GameManager.Instance.NivelSelecionado;
        }

        GerarNivel(nivelAtual);
    }

    // Carrega o Nível 1 fixo ou gera os Níveis 2 e 3 aleatoriamente
    public void GerarNivel(int nivel)
    {
        nivelAtual = nivel;

        if (nivel == 1)
        {
            // Nível 1: Dimensão 5x5 fixa, desenhada à mão com 3 caminhos possíveis
            tamanhoGrelha = 5;
            TotalVitimasNivel = 1;
            LimparMapa();
            ConfigurarMatriz();
            MontarNivel1Fixo();
        }
        else
        {
            // Níveis 2 e 3: Procedurais e desafiantes
            tamanhoGrelha = (nivel == 2) ? 7 : 9;
            TotalVitimasNivel = (nivel == 2) ? 2 : 3;

            int tentativas = 0;
            bool mapaValido = false;

            while (!mapaValido && tentativas < 300)
            {
                tentativas++;
                LimparMapa();
                ConfigurarMatriz();

                if (nivel == 2) GerarNivelInundacaoAleatorio();
                else GerarNivelSismoAleatorio();

                mapaValido = ValidarMultiplosCaminhos(3);
            }
        }

        SalvarEstadoInicial();
        AjustarCamara();
    }

    private void SalvarEstadoInicial()
    {
        mapaInicial = new DadosCelula[tamanhoGrelha, tamanhoGrelha];
        for (int x = 0; x < tamanhoGrelha; x++)
        {
            for (int y = 0; y < tamanhoGrelha; y++)
            {
                mapaInicial[x, y].tipo = mapa[x, y];
                if (objetosNoMapa[x, y] != null)
                {
                    SpriteRenderer sr = objetosNoMapa[x, y].GetComponent<SpriteRenderer>();
                    if (sr != null) mapaInicial[x, y].sprite = sr.sprite;
                }
            }
        }
    }

    public void RestaurarMapaOriginal()
    {
        if (mapaInicial == null) return;

        for (int x = 0; x < tamanhoGrelha; x++)
        {
            for (int y = 0; y < tamanhoGrelha; y++)
            {
                // Apaga o objeto atual (ex: fogo apagado ou saco colocado)
                if (objetosNoMapa[x, y] != null)
                {
                    Destroy(objetosNoMapa[x, y]);
                    objetosNoMapa[x, y] = null;
                }

                // Recria o elemento original que estava lá no início
                mapa[x, y] = mapaInicial[x, y].tipo;
                if (mapaInicial[x, y].sprite != null)
                {
                    InstanciarElemento(new Vector2Int(x, y), mapa[x, y], mapaInicial[x, y].sprite);
                }
            }
        }
    }

    // Inicializa a matriz e o chão
    private void ConfigurarMatriz()
    {
        mapa = new TipoElemento[tamanhoGrelha, tamanhoGrelha];
        objetosNoMapa = new GameObject[tamanhoGrelha, tamanhoGrelha];
        posicoesLivres.Clear();
        posicoesVitimas.Clear();

        for (int x = 0; x < tamanhoGrelha; x++)
        {
            for (int y = 0; y < tamanhoGrelha; y++)
            {
                mapa[x, y] = TipoElemento.Vazio;
                if (spriteCelulaBase != null)
                {
                    CriarObjetoVisual(new Vector2Int(x, y), spriteCelulaBase, -1);
                }

                if ((x != 0 || y != 0) && (x != tamanhoGrelha - 1 || y != tamanhoGrelha - 1))
                {
                    posicoesLivres.Add(new Vector2Int(x, y));
                }
            }
        }

        bool isDrone = (GameManager.Instance != null && GameManager.Instance.EquipamentoSelecionado == TipoEquipamento.Drone);
        if (!isDrone)
        {
            posHospital = new Vector2Int(tamanhoGrelha - 1, tamanhoGrelha - 1);
            InstanciarElemento(posHospital, TipoElemento.Hospital, spriteHospital);
        }
    }

    // Nível 1: Layout fixo desenhado à mão com 3 rotas (desvio obrigatório)
    private void MontarNivel1Fixo()
    {
        Vector2Int v1 = new Vector2Int(0, 4);
        posicoesVitimas.Add(v1);
        InstanciarElemento(v1, TipoElemento.Pessoas, ObterSkinPessoas());

        InstanciarElemento(new Vector2Int(0, 1), TipoElemento.Chama, spriteChama);
        InstanciarElemento(new Vector2Int(0, 2), TipoElemento.Casa, spriteCasa);

        InstanciarElemento(new Vector2Int(1, 4), TipoElemento.CasaIncendio, spriteCasaIncendio);
        InstanciarElemento(new Vector2Int(1, 1), TipoElemento.Arvores, ObterSkinArvores());
        InstanciarElemento(new Vector2Int(1, 3), TipoElemento.Chama, spriteChama);


        InstanciarElemento(new Vector2Int(2, 1), TipoElemento.CasaIncendio, spriteCasaIncendio);
        InstanciarElemento(new Vector2Int(2, 3), TipoElemento.Chama, spriteChama);

        InstanciarElemento(new Vector2Int(3, 3), TipoElemento.Casa, spriteCasa);

        InstanciarElemento(new Vector2Int(4, 0), TipoElemento.Arvores, ObterSkinArvores());
        InstanciarElemento(new Vector2Int(4, 1), TipoElemento.Arvores, ObterSkinArvores());
        InstanciarElemento(new Vector2Int(4, 2), TipoElemento.Chama, spriteChama);
    }

    // Nível 2 (Inundação 7x7): Procedural
    private void GerarNivelInundacaoAleatorio()
    {
        for (int i = 0; i < 2; i++)
        {
            Vector2Int v = SortearPosicaoLivre();
            posicoesVitimas.Add(v);
            InstanciarElemento(v, TipoElemento.PessoasInundacao, ObterSkinPessoasInundacao());
        }

        for (int i = 0; i < 18; i++) InstanciarElemento(SortearPosicaoLivre(), TipoElemento.AguaInundacao, spriteAguaInundacao);
        for (int i = 0; i < 7; i++) InstanciarElemento(SortearPosicaoLivre(), TipoElemento.CasaInundacao, spriteCasaInundacao);
        for (int i = 0; i < 10; i++) InstanciarElemento(SortearPosicaoLivre(), TipoElemento.Arvores, ObterSkinArvores());
    }

    // Nível 3 (Sismo e Incêndio 9x9): Procedural
    private void GerarNivelSismoAleatorio()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2Int v = SortearPosicaoLivre();
            posicoesVitimas.Add(v);
            InstanciarElemento(v, TipoElemento.Pessoas, ObterSkinPessoas());
        }

        for (int i = 0; i < 11; i++) InstanciarElemento(SortearPosicaoLivre(), TipoElemento.PredioSismoTerramoto, spritePredioSismo);
        for (int i = 0; i < 8; i++) InstanciarElemento(SortearPosicaoLivre(), TipoElemento.Destrocos, spriteDestrocos);
        for (int i = 0; i < 12; i++) InstanciarElemento(SortearPosicaoLivre(), TipoElemento.Chama, spriteChama);
        for (int i = 0; i < 4; i++) InstanciarElemento(SortearPosicaoLivre(), TipoElemento.CasaIncendio, spriteCasaIncendio);
    }

    // Validação de múltiplas rotas para níveis aleatórios
    private bool ValidarMultiplosCaminhos(int minCaminhos)
    {
        bool isDrone = (GameManager.Instance != null && GameManager.Instance.EquipamentoSelecionado == TipoEquipamento.Drone);

        foreach (Vector2Int vitima in posicoesVitimas)
        {
            if (ContarRotas(new Vector2Int(0, 0), vitima, isDrone, minCaminhos) < minCaminhos)
                return false;
        }

        if (!isDrone)
        {
            foreach (Vector2Int vitima in posicoesVitimas)
            {
                if (ContarRotas(vitima, posHospital, isDrone, minCaminhos) < minCaminhos)
                    return false;
            }
        }

        return true;
    }

    // DFS para contagem de rotas
    private int ContarRotas(Vector2Int origem, Vector2Int destino, bool isDrone, int maxLimite)
    {
        int total = 0;
        HashSet<Vector2Int> visitados = new HashSet<Vector2Int>();

        void DFS(Vector2Int pos)
        {
            if (total >= maxLimite) return;
            if (pos == destino) { total++; return; }

            Vector2Int[] dirs = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };
            foreach (var d in dirs)
            {
                Vector2Int viz = pos + d;
                if (viz.x >= 0 && viz.x < tamanhoGrelha && viz.y >= 0 && viz.y < tamanhoGrelha && !visitados.Contains(viz))
                {
                    if (viz == destino || PodeMoverPara(viz, isDrone) || (!isDrone && mapa[viz.x, viz.y] == TipoElemento.AguaInundacao))
                    {
                        visitados.Add(viz);
                        DFS(viz);
                        visitados.Remove(viz);
                    }
                }
            }
        }

        visitados.Add(origem);
        DFS(origem);
        return total;
    }

    private Vector2Int SortearPosicaoLivre()
    {
        int idx = Random.Range(0, posicoesLivres.Count);
        Vector2Int pos = posicoesLivres[idx];
        posicoesLivres.RemoveAt(idx);
        return pos;
    }

    private void InstanciarElemento(Vector2Int coord, TipoElemento tipo, Sprite sprite)
    {
        if (sprite == null || coord.x < 0 || coord.x >= tamanhoGrelha || coord.y < 0 || coord.y >= tamanhoGrelha) return;
        mapa[coord.x, coord.y] = tipo;
        GameObject obj = CriarObjetoVisual(coord, sprite, 0);
        objetosNoMapa[coord.x, coord.y] = obj;
    }

    private GameObject CriarObjetoVisual(Vector2Int coord, Sprite sprite, int sortingOrder)
    {
        GameObject obj = new GameObject(sprite.name);
        obj.transform.parent = transform;
        obj.transform.position = ObterPosicaoMundo(coord);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;

        if (sprite != null && sprite.bounds.size.x > 0 && sprite.bounds.size.y > 0)
        {
            float escalaX = tamanhoBloco / sprite.bounds.size.x;
            float escalaY = tamanhoBloco / sprite.bounds.size.y;
            obj.transform.localScale = new Vector3(escalaX, escalaY, 1f);
        }

        return obj;
    }

    private void AjustarCamara()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // 1. Centro do tabuleiro no mundo
        float centroX = (tamanhoGrelha - 1) * tamanhoBloco / 2f;
        float centroY = -(tamanhoGrelha - 1) * tamanhoBloco / 2f;

        // 2. Altura da visão vertical com margem confortável
        float margemVertical = (tamanhoGrelha == 9) ? 2.2f : ((tamanhoGrelha == 7) ? 1.8f : 1.4f);
        cam.orthographicSize = (tamanhoGrelha * tamanhoBloco / 2f) + margemVertical;

        // 3. Posicionamento para a metade direita num ecrã 16:9
        float aspect169 = 16f / 9f;
        float larguraMundo = cam.orthographicSize * 2f * aspect169;
        float posXCamara = centroX - (larguraMundo / 4f);

        float offsetTopo = cam.orthographicSize * 0.08f;
        float posYCamara = centroY + offsetTopo;

        cam.transform.position = new Vector3(posXCamara, posYCamara, -10f);
    }

    private Sprite ObterSkinArvores() => (spritesArvores != null && spritesArvores.Length > 0) ? spritesArvores[Random.Range(0, spritesArvores.Length)] : null;
    private Sprite ObterSkinPessoas() => (spritesPessoas != null && spritesPessoas.Length > 0) ? spritesPessoas[Random.Range(0, spritesPessoas.Length)] : null;
    private Sprite ObterSkinPessoasInundacao() => (spritesPessoasInundacao != null && spritesPessoasInundacao.Length > 0) ? spritesPessoasInundacao[Random.Range(0, spritesPessoasInundacao.Length)] : null;

    public Vector3 ObterPosicaoMundo(Vector2Int coord) => new Vector3(coord.x * tamanhoBloco, -coord.y * tamanhoBloco, 0);

    public bool PodeMoverPara(Vector2Int coord, bool isDrone)
    {
        if (coord.x < 0 || coord.x >= tamanhoGrelha || coord.y < 0 || coord.y >= tamanhoGrelha)
            return false;

        TipoElemento elem = mapa[coord.x, coord.y];

        // Bloqueia colisões contra estruturas sólidas, fogo e água sem proteção
        if (elem == TipoElemento.Casa ||
            elem == TipoElemento.CasaIncendio ||
            elem == TipoElemento.CasaInundacao ||
            elem == TipoElemento.PredioSismoTerramoto ||
            elem == TipoElemento.Destrocos ||
            elem == TipoElemento.Arvores ||
            elem == TipoElemento.Chama)
        {
            return false;
        }

        // Se for água, o robô só passa se houver um saco de areia colocado (MuroSacoAreia). O drone passa sempre.
        if (elem == TipoElemento.AguaInundacao)
        {
            return isDrone;
        }

        return true;
    }

    public bool ColocarSacoDeAreia(Vector2Int coord)
    {
        if (coord.x < 0 || coord.x >= tamanhoGrelha || coord.y < 0 || coord.y >= tamanhoGrelha) return false;

        if (mapa[coord.x, coord.y] == TipoElemento.AguaInundacao)
        {
            if (objetosNoMapa[coord.x, coord.y] != null)
                Destroy(objetosNoMapa[coord.x, coord.y]);

            InstanciarElemento(coord, TipoElemento.MuroSacoAreia, spriteMuroSacoAreia);
            return true;
        }
        return false;
    }

    public TipoElemento ObterTipoElemento(Vector2Int coord)
    {
        if (coord.x < 0 || coord.x >= tamanhoGrelha || coord.y < 0 || coord.y >= tamanhoGrelha)
            return TipoElemento.Vazio;

        return mapa[coord.x, coord.y];
    }

    public void LimparCelula(Vector2Int coord)
    {
        if (coord.x < 0 || coord.x >= tamanhoGrelha || coord.y < 0 || coord.y >= tamanhoGrelha) return;

        if (objetosNoMapa[coord.x, coord.y] != null)
            Destroy(objetosNoMapa[coord.x, coord.y]);

        mapa[coord.x, coord.y] = TipoElemento.Vazio;
    }

    private void LimparMapa()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}