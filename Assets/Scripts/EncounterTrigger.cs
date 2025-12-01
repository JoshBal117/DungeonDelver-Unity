using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterTrigger : MonoBehaviour
{
    [SerializeField] private string battleSceneName = "BattleTestScene";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Encounter triggered! Loading battle scene...");
            SceneManager.LoadScene(battleSceneName);
        }
    }
}