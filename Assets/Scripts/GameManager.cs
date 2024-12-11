using UnityEngine;
using System;

[Serializable]
public class PlayerData
{
    public float latitude = 34.264967705635449f;  // Default values
    public float longitude = -118.77450914718295f;
    // Add other player data fields as needed
}

public class GameManager : MonoBehaviour
{
    public GameObject aircraftPrefab;
    private PlayerData playerData;

    void Start()
    {
        // Initialize player data first
        InitializePlayerFromURL();

        // Check if Online Maps is available
        if (OnlineMaps.instance == null)
        {
            Debug.LogError("Online Maps instance not found! Please add Online Maps component to a GameObject in the scene.");
            return;
        }

        // Set the starting position on the map using player data
        OnlineMaps.instance.SetPosition(playerData.longitude, playerData.latitude);
        OnlineMaps.instance.zoom = 14;

        SpawnAircraft();
    }

    private void InitializePlayerFromURL()
    {
        // Initialize default player data
        playerData = new PlayerData();

        // Get player data from URL
        string encodedData = GetPlayerDataFromURL();
        if (!string.IsNullOrEmpty(encodedData))
        {
            try
            {
                // Decode base64 player data
                byte[] decodedBytes = Convert.FromBase64String(encodedData);
                string jsonData = System.Text.Encoding.UTF8.GetString(decodedBytes);
                
                // Parse JSON data into playerData
                playerData = JsonUtility.FromJson<PlayerData>(jsonData);
                Debug.Log($"Player initialized at: Lat {playerData.latitude}, Lon {playerData.longitude}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error initializing player: {e.Message}");
                // Keep using default values if there's an error
            }
        }
        else
        {
            Debug.Log("No player data in URL, using defaults");
        }
    }

    private string GetPlayerDataFromURL()
    {
        string url = Application.absoluteURL;
        if (url.Contains("?"))
        {
            string[] parameters = url.Split('?')[1].Split('&');
            foreach (string param in parameters)
            {
                string[] keyValue = param.Split('=');
                if (keyValue[0] == "player")
                {
                    return keyValue[1];
                }
            }
        }
        return null;
    }

    private void SpawnAircraft()
    {
        // Option 1: Load from Resources
        GameObject prefab = Resources.Load("Aircraft") as GameObject;
        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab);
            PositionAircraft(instance);
        }
        
        // Option 2: Use the prefab assigned in Inspector
        if (aircraftPrefab != null)
        {
            GameObject instance = Instantiate(aircraftPrefab);
            PositionAircraft(instance);
        }
    }

    private void PositionAircraft(GameObject aircraft)
    {
        if (aircraft != null)
        {
            double x, y;
            OnlineMaps.instance.projection.CoordinatesToTile(
                playerData.longitude, 
                playerData.latitude, 
                OnlineMaps.instance.zoom, 
                out x, 
                out y
            );
            aircraft.transform.position = new Vector3((float)x, aircraft.transform.position.y, (float)y);
        }
    }
} 