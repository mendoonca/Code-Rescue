using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuEquipamentoManager : MonoBehaviour
{

    [SerializeField] private string drone;
    [SerializeField] private string robo;

    public void jogarDrone()
    {
        SceneManager.LoadScene(drone);
    }

    public void jogarRobo()
    {
        SceneManager.LoadScene(robo);
    }

    public void VoltarAoMenuPrincipal()
    {
        SceneManager.LoadScene("Menu");
    }
}