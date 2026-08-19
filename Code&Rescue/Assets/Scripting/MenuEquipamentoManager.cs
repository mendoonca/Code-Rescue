using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MenuEquipamentoManager : MonoBehaviour
{

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            VoltarAoMenuPrincipal();
        }
    }

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