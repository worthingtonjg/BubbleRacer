using UnityEngine;

[CreateAssetMenu(fileName = "NewRaceCar", menuName = "RaceCar")]
public class RaceCar : ScriptableObject
{
    public string carName;
    public int speed;
    public int acceleration;
    public int traction;
    public int handling;
    public int durability;
    public int price;
    public GameObject carPrefab;
}