using System.Collections.Generic;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager instance = null;

    public VehicleSpawnerScriptable vehicleLib;

    Dictionary<string, GameObject> vehicleDictionary;

    int vehicleCount;
    int selectedVehicle;

    //r- realtime vehicles (spawned).
    List<Vehicle> rVehicles;
    List<AIDriver> rAIDrivers;

    private void Awake()
    {
        if (instance is null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InitVehicleManager();
    }

    private void InitVehicleManager()
    {
        vehicleDictionary = new();
        rVehicles = new();
        rAIDrivers = new();

        for (int i = 0; i < vehicleLib.vehicles.Length; i++)
        {
            vehicleDictionary.Add(vehicleLib.vehicles[i].name, vehicleLib.vehicles[i].vehicle);
        }

        vehicleCount = vehicleLib.vehicles.Length;
    }

    [ContextMenu("Spawn Random Vehicle (DEBUG)")]
    public void SpawnRandomVehicle()
    {
        string vName = vehicleLib.vehicles[Random.Range(0, vehicleLib.vehicles.Length)].name;
        Vector3 pos = new(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        SpawnVehicle(vName, pos, pos * 0);
    }

    [ContextMenu("Spawn Random Vehicle with Driver Type (DEBUG)")]
    public void SpawnRandomVehicleAndDriverType()
    {
        string vName = vehicleLib.vehicles[Random.Range(0, vehicleLib.vehicles.Length)].name;
        int range = Random.Range(0, 4);
        Vector3 pos = new(Random.Range(-500, 500), 0, Random.Range(-500, 500));

        SpawnVehicleWithDriverType(vName, (DriverType)range, pos, pos * 0);
    }

    public void SpawnRandomRacer(Vector3 position, Vector3 rotation)
    {
        string vName = vehicleLib.vehicles[Random.Range(0, vehicleLib.vehicles.Length)].name;
        
        int range = Random.Range(0, 1); //0 and 1 are careful and reckless

        SpawnVehicleWithDriverType(vName, (DriverType)range, position, rotation);
    }

    public void SpawnVehicle(string name, Vector3 position, Vector3 rotation)
    {
        if (vehicleDictionary.TryGetValue(name, out GameObject vGO))
        {
            GameObject rVehicle = Instantiate(vGO, position, Quaternion.Euler(rotation));

            Vehicle v = rVehicle.GetComponent<Vehicle>();

            rVehicles.Add(v);
        }
    }
    public Vehicle SpawnVehicle(string name, Vector3[] pos_rot)
    {
        if (vehicleDictionary.TryGetValue(name, out GameObject vGO))
        {
            GameObject rVehicle = Instantiate(vGO, pos_rot[0], Quaternion.Euler(pos_rot[1]));

            Vehicle v = rVehicle.GetComponent<Vehicle>();

            rVehicles.Add(v);

            return v;
        }

        return null;
    }

    //LEFT RIGHT OPTIONS IN VEHICLE SELECT IN GARAGE.
    public void SpawnNextVehicleInGarage_LEFT()
    {
        selectedVehicle--;
        if (selectedVehicle < 0)
            selectedVehicle = vehicleCount - 1;

        string vehicleName;

        for (int i = 0; i < vehicleCount; i++)
        {
            if (i == selectedVehicle)
            {
                vehicleName = vehicleLib.vehicles[i].name;

                GarageManager.instance.SetDisplayVehicle(vehicleName);

                break;
            }
        }
    }

    public void SpawnNextVehicleInGarage_RIGHT()
    {
        selectedVehicle++;
        if (selectedVehicle >= vehicleCount)
            selectedVehicle = 0;

        string vehicleName;

        for (int i = 0; i < vehicleCount; i++)
        {
            if (i == selectedVehicle)
            {
                vehicleName = vehicleLib.vehicles[i].name;

                GarageManager.instance.SetDisplayVehicle(vehicleName);

                break;
            }
        }
    }

    public void SpawnVehicleWithDriverType(string name, DriverType driverType, Vector3 position, Vector3 rotation)
    {
        Vehicle v =
            SpawnVehicle
            (
                name,
                new Vector3[]
                {
                    position,
                    rotation
                }
             );

        AIDriver ai = v.gameObject.AddComponent<AIDriver>();

        ai.driver.driverType = driverType;

        rAIDrivers.Add(ai);
    }
}