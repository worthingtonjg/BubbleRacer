using UnityEngine;

public class AICar : MonoBehaviour {
    public float speed = 10f;
    public float overrdriveSpeed = 20f;
    public float turnSpeed = 5f;
    public float RaceProgress;
    public bool overdrive = false;

    private GameController gameController;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();   
    }

    void Update() {
        if (gameController.waypoints.Length == 0) return;

        Transform target = gameController.GetNpcWaypoint(gameObject.GetInstanceID());

        target.position = new Vector3(target.position.x, transform.position.y, target.position.z);

        Vector3 direction = (target.position - transform.position).normalized;

        // Rotate towards the waypoint
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);

        // Move forward

        if(overdrive)
        {
            transform.Translate(Vector3.forward * overrdriveSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        

        // Check if close to the waypoint
        if (Vector3.Distance(transform.position, target.position) < 2f) {
            gameController.UpdateNpcWaypoint(gameObject.GetInstanceID());   
        }

        int waypoint = gameController.npcWaypoints[gameObject.GetInstanceID()];
        float totalWaypoints = gameController.waypoints.Length * gameController.MaxLaps; 
        float currentWaypoint = ((gameController.npcLaps[gameObject.GetInstanceID()] - 1) * gameController.waypoints.Length) + waypoint;
        RaceProgress = (currentWaypoint/totalWaypoints)*100;
    }
}
