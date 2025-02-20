using UnityEngine;

public class Sector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameController gameController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            gameController.UpdateSector(name);
        }
        if(other.tag == "NPC") {
            gameController.UpdateNpcLap(gameObject, other.gameObject);
        }
    }
}
