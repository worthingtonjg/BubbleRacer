using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class CameraController : MonoBehaviour
{
    public List<GameObject> locations = new List<GameObject>();
    public List<RaceCar> CarData = new List<RaceCar>();
    public float cameraHeight = 10f;
    public float cameraDistance = 10f;
    public float transitionTime = 2f;
    public Camera mainCamera; // Add this field
    public TextMeshProUGUI carNameText;
    public TextMeshProUGUI buyPriceText;
    public TextMeshProUGUI bubblesCollectedText;
    public Image[] statImages = new Image[5];
    public GameObject buyButton;
    public GameObject playButton;

    private int currentLocationIndex = 0;
    private Vector3 cameraTargetPosition;
    private int bubblesCollected = 0;
    private OwnedCarsWrapper ownedCars;

    void Start()
    {
        GetPlayerPrefs();

        UpdateCarInfo();
        UpdateBubblesCollected();
    }

    private void GetPlayerPrefs()
    {
        bubblesCollected = PlayerPrefs.GetInt("bubblesCollected", 0);
        string ownedJson = PlayerPrefs.GetString("ownedCars", "{\"cars\":[0]}");
        ownedCars = JsonUtility.FromJson<OwnedCarsWrapper>(ownedJson);        
    }


    void Update()
    {
        // Move camera to next location
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveCameraToNextLocation();
        }

        // Move camera to previous location
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveCameraToPreviousLocation();
        }

        if(Input.GetKeyDown(KeyCode.Alpha4)) 
        {
            bubblesCollected += 100;
            PlayerPrefs.SetInt("bubblesCollected", bubblesCollected);
            UpdateBubblesCollected();
        }

        if(Input.GetKeyDown(KeyCode.Delete))
        {
            PlayerPrefs.DeleteAll();
            GetPlayerPrefs();
            UpdateCarInfo();
            UpdateBubblesCollected();
        }

    }

    public void MoveCameraToNextLocation()
    {
        currentLocationIndex = (currentLocationIndex + 1) % locations.Count;
        MoveCameraToLocation(locations[currentLocationIndex]);
    }

    public void MoveCameraToPreviousLocation()
    {
        currentLocationIndex = (currentLocationIndex - 1 + locations.Count) % locations.Count;
        MoveCameraToLocation(locations[currentLocationIndex]);
    }


    void MoveCameraToLocation(GameObject location)
    {
        // Calculate camera target position
        cameraTargetPosition = location.transform.position + new Vector3(0, cameraHeight, -cameraDistance);

        // Smoothly move camera to target position
        StartCoroutine(MoveCameraToTargetPosition(cameraTargetPosition, transitionTime));

        UpdateCarInfo();
    }

    public void PlayGame()
    {
        PlayerPrefs.SetInt("carIndex", currentLocationIndex);
        SceneManager.LoadScene("RaceScene");
    }

    public void BuyCar()
    {
        if(bubblesCollected < CarData[currentLocationIndex].price) return;
        if(buyPriceText.text == "Locked") return;

        bubblesCollected -= CarData[currentLocationIndex].price;
        PlayerPrefs.SetInt("bubblesCollected", bubblesCollected);
        ownedCars.cars.Add(currentLocationIndex);
        PlayerPrefs.SetString("ownedCars", JsonUtility.ToJson(ownedCars));
        UpdateBubblesCollected();
        UpdateCarInfo();
    }

    private void UpdateBubblesCollected()
    {
        bubblesCollectedText.text = "Bubbles: " + bubblesCollected.ToString();
    }

    private IEnumerator MoveCameraToTargetPosition(Vector3 targetPosition, float transitionTime)
    {
        float elapsedTime = 0;
        Vector3 startPosition = mainCamera.transform.position; // Use mainCamera.transform instead of transform

        while (elapsedTime < transitionTime)
        {
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / transitionTime); // Use mainCamera.transform instead of transform
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetPosition; // Use mainCamera.transform instead of transform
    }

    private void UpdateCarInfo()
    {
        carNameText.text = CarData[currentLocationIndex].carName;

        if(!ownedCars.cars.Any(c => c == currentLocationIndex - 1))
        {
            buyPriceText.text = "Locked";
        }
        else
        {
            buyPriceText.text = $"{CarData[currentLocationIndex].price} Bubbles";
        }

        statImages[0].fillAmount = CarData[currentLocationIndex].speed / 10f;
        statImages[1].fillAmount = CarData[currentLocationIndex].acceleration / 10f;
        statImages[2].fillAmount = CarData[currentLocationIndex].traction / 10f;
        statImages[3].fillAmount = CarData[currentLocationIndex].handling / 10f;
        statImages[4].fillAmount = CarData[currentLocationIndex].durability / 10f;

        playButton.SetActive(ownedCars.cars.Contains(currentLocationIndex));
        buyButton.SetActive(!ownedCars.cars.Contains(currentLocationIndex));

    }

    [System.Serializable]
    public class OwnedCarsWrapper
    {
        public List<int> cars = new List<int>();
    }
}