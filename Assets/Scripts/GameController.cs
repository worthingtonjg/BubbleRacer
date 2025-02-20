using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Xml.Schema;

public class GameController : MonoBehaviour
{
    public Transform[] waypoints;

    public int bubblesCollected = 0;
    public int currentLap = 1;
    public int playerPlace = 1;
    public int MaxLaps = 3;

    // Reference to the TextMeshPro controls
    public TextMeshProUGUI bubblesText;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI placeText;

    public TextMeshProUGUI startMessage;

    public GameObject StartPanel;

    public GameObject Player;
    public GameObject[] Sectors;
    public GameObject[] Npcs;

    public GameObject BoostButton;
    public TextMeshProUGUI BoostButtonText;

    public bool UpgradeHandling1;
    public bool UpgradeHandling2;
    public bool UpgradeBoost1;
    public bool UpgradeBoost2;
    public bool UpgradeDurability1;
    public bool UpgradeDurability2;

    private List<GameObject> Finished;

    private int playerSectorIndex;
    private bool firstSector = true;
    public Dictionary<int, int> npcWaypoints = new Dictionary<int, int>();
    public Dictionary<int, int> npcLaps = new Dictionary<int, int>();
    public AudioSource audioSource;
    public AudioSource muscleCarBackgroundSounds;
    public AudioClip startClip;
    public AudioClip bubbleClip;
    public AudioClip lapFinishedClip;
    public AudioClip raceOverClip;
    private bool GameStarted = false;
    private bool GameEnded = false;
    
    void Start()
    {
        Finished = new List<GameObject>();

        foreach(var npc in Npcs)
        {
            npcWaypoints[npc.GetInstanceID()] = 0;
            npcLaps[npc.GetInstanceID()] = 0;
        }

        // Set initial values for the TextMeshPro controls
        UpdateBubbles(0);
        UpdateLap(1);
        UpdatePlace(1);
    }

    public void StartGame()
    {
        if(GameEnded) 
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("UpgradesScene");
        }

        if(GameStarted) return;
        GameStarted = true;
        Time.timeScale = 1;
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        audioSource.PlayOneShot(startClip);
        startMessage.text = "3";
        yield return new WaitForSeconds(1);
        startMessage.text = "2";
        yield return new WaitForSeconds(1);
        startMessage.text = "1";
        muscleCarBackgroundSounds.Play();
        yield return new WaitForSeconds(1);
        startMessage.text = "Go";
        yield return new WaitForSeconds(1);
        StartPanel.SetActive(false);

        foreach(var npc in Npcs)
        {
            if(npc.GetInstanceID() == Player.GetInstanceID())
            {
                npc.GetComponent<WheelController>().enabled = true;
            }
            else
            {
                npc.GetComponent<AICar>().enabled = true;
            }
        }
    }

    IEnumerator ShowLapFinished() {
        StartPanel.SetActive(true);

        if(currentLap < MaxLaps)
        {
            startMessage.text = $"Lap {currentLap}";
        }
        else
        {
            startMessage.text = $"Final Lap";
        }

        yield return new WaitForSeconds(2);
        StartPanel.SetActive(false);
    }

    void Update()
    {
        var kvpList = new List<KeyValuePair<int, float>>();
        foreach(var npc in Npcs)
        {
            if(npc.GetInstanceID() == Player.GetInstanceID())
            {
                kvpList.Add(new KeyValuePair<int, float>(npc.GetInstanceID(), npc.GetComponent<WheelController>().RaceProgress));
            }
            else
            {
                kvpList.Add(new KeyValuePair<int, float>(npc.GetInstanceID(), npc.GetComponent<AICar>().RaceProgress));
            }
        }

        kvpList = kvpList.OrderByDescending(kvp => kvp.Value).ToList();

        var index = 0;
        var playerPlace = 0;
        foreach(var kvp in kvpList)
        {
            if(kvp.Key == Player.GetInstanceID())
            {
                playerPlace = index + 1;
                UpdatePlace(playerPlace);
                break;
            }

            index++;
        }

        // If we are in first then make the other players faster
        foreach(var kvp in kvpList)
        {
            if(kvp.Key != Player.GetInstanceID())
            {
                bool overdrive = playerPlace < Npcs.Length;
                print($"Overdrive: {overdrive}");
                Npcs.First(nvp => nvp.GetInstanceID() == kvp.Key).GetComponent<AICar>().overdrive = overdrive;
                
            }
        }
    }

    public Transform GetNpcWaypoint(int npcId)
    {
        Transform target = waypoints[npcWaypoints[npcId]];        
        return target;      
    }

    public void UpdateNpcWaypoint(int npcId)
    {
        npcWaypoints[npcId] = (npcWaypoints[npcId] + 1) % waypoints.Length;
    }

    public int ClosestWaypointToPlayer() {
        int nearestIndex = -1;
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++) {
            float distance = Vector3.Distance(Player.transform.position, waypoints[i].position);
            if (distance < shortestDistance) {
                shortestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }    

    // Method to update bubbles collected
    public void UpdateBubbles(int amount)
    {
        bubblesCollected += amount;
        bubblesText.text = "Bubbles: " + bubblesCollected.ToString();
        audioSource.PlayOneShot(bubbleClip);
    }

    // Method to update lap
    public void UpdateLap(int? lap = null)
    {
        if(lap.HasValue)
        {
            currentLap = lap.Value;
        }
        else{
            currentLap++;
        }
        lapText.text = "Lap: " + currentLap.ToString();
    }

    // Method to update player's place
    public void UpdatePlace(int newPlace)
    {
        bool changed = playerPlace != newPlace;

        playerPlace = newPlace;
        placeText.text = "Place: " + playerPlace.ToString();
    }

    public void UpdateSector(string name)
    {
        if(Sectors[playerSectorIndex].name != name) return;

        print(name);

        if(!firstSector)
        {
            if(Sectors[playerSectorIndex].name == Sectors.First().name)
            {
                UpdateLap();

                if(currentLap > MaxLaps)
                {
                    audioSource.PlayOneShot(raceOverClip);
                    EndGame();
                }
                else
                {
                    audioSource.PlayOneShot(lapFinishedClip);
                    StartCoroutine(ShowLapFinished());
                }
            }
        }

        int nextSectorIndex;
        if(playerSectorIndex < Sectors.Length - 1)
        {
            nextSectorIndex = playerSectorIndex + 1;
        }
        else        
        {
            nextSectorIndex = 0;
        }

        playerSectorIndex = nextSectorIndex;
        firstSector = false;
    }

    public void EndGame()
    {
        Time.timeScale = 0;
        GameEnded = true;;
        StartPanel.SetActive(true);
        muscleCarBackgroundSounds.Stop();
        
        int totalBubbles = PlayerPrefs.GetInt("bubblesCollected", 0);
        PlayerPrefs.SetInt("bubblesCollected", totalBubbles + bubblesCollected);
        UpdatePlace(Finished.Count + 1);
        startMessage.text = $"You placed {playerPlace} of {Npcs.Length}! Click to continue.";
    }

    public void UpdateNpcLap(GameObject sector, GameObject other)
    {
        if(sector.name == Sectors.First().name)
        {
            if(!npcLaps.ContainsKey(other.GetInstanceID()))
            {
                npcLaps[other.GetInstanceID()] = 0;
            }

            ++npcLaps[other.GetInstanceID()];

            if(npcLaps[other.GetInstanceID()] > MaxLaps)
            {
             
                Finished.Add(other);
            }
        }
    }
}