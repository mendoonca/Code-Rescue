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
            VoltarMenuPrincipal();
        }
    }

    // Chamar no On Click () do botão do Drone
    public void EscolherDrone()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EquipamentoSelecionado = TipoEquipamento.Drone;
        }

        SceneManager.LoadScene("DroneNiveis");
    }

    // Chamar no On Click () do botão do Robô
    public void EscolherRobo()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EquipamentoSelecionado = TipoEquipamento.Robo;
        }

        SceneManager.LoadScene("RoboNiveis");
    }

    // Botão Voltar ao Menu Principal
    public void VoltarMenuPrincipal()
    {
        SceneManager.LoadScene("Menu");
    }
}