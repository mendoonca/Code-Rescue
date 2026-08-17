using UnityEngine;

public enum TipoEquipamento
{
    Drone,
    Robo
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TipoEquipamento EquipamentoSelecionado { get; set; } = TipoEquipamento.Drone;

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
}