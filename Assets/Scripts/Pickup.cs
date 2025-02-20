using UnityEngine;

public class Pickup : MonoBehaviour
{
    private GameController gameController;
    public GameObject bubbleParticles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            gameController.UpdateBubbles(1);
            Destroy(gameObject);
        }
    }
}
