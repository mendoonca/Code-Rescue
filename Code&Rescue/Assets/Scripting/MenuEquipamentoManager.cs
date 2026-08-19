using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuEquipamentoManager : MonoBehaviour
{

    public void jogarDrone()
    {
        SceneManager.LoadScene("DroneNiveis");
    }

    public void jogarRobo()
    {
        SceneManager.LoadScene("RoboNiveis");
    }

    public void VoltarAoMenuPrincipal()
    {
        SceneManager.LoadScene("Menu");
    }
}