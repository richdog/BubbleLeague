using UnityEngine;
using UnityEngine.SceneManagement;

namespace Main_Menu
{
    /// <summary>
    ///     The input manager on MainMenu will spawn this player, instantly loading the join scene. Works as a "press any
    ///     button" deal.
    /// </summary>
    public class MainMenuPlayer : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            SceneManager.LoadScene("Scenes/Game/JoinMenu");
        }
    }
}