using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using InfinityCode.OnlineMapsExamples;

public class GetBlockchainMarkers : MonoBehaviour
{
    public OnlineMaps map;
    public OnlineMapsControlBase control;
    public OnlineMapsMarkerManager markerMGR;
    public GameObject markerPrefab;
    public Texture2D markerTexture;
    public bool use3DMarkers = false;
    private string apiUrl = "http://localhost:3001/api/base_markers";
    public string bearerToken = "unityapp-api-key-654321";

    private bool isPaused = false;
    private double pausedLongitude;
    private double pausedLatitude;
    private int pausedZoom;

    void Start()
    {
        if (map == null) map = OnlineMaps.instance;
        if (control == null) control = OnlineMapsControlBase.instance;
        if (markerMGR == null) markerMGR = OnlineMapsMarkerManager.instance;

        if (map == null || control == null || markerMGR == null)
        {
            Debug.LogError("One or more required components are missing. Please check the inspector.");
            return;
        }

        Debug.Log("GetBlockchainMarkers Start method called.");
        StartCoroutine(FetchMarkers());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed");
            TogglePause();
        }

        if (isPaused)
        {
            EnforcePausedState();
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Debug.Log($"Pause toggled. isPaused: {isPaused}");

        if (isPaused)
        {
            // Store current state
            map.GetPosition(out pausedLongitude, out pausedLatitude);
            pausedZoom = map.zoom;
            Debug.Log($"Paused at position: Lon {pausedLongitude}, Lat {pausedLatitude}, Zoom {pausedZoom}");

            // Disable controls
            if (control != null)
            {
                control.enabled = false;  // Added this line
                control.allowUserControl = false;
                control.allowZoom = false;
            }

            // Force position
            map.SetPosition(pausedLongitude, pausedLatitude);
            map.zoom = pausedZoom;
            map.Redraw();
        }
        else
        {
            // Re-enable controls
            if (control != null)
            {
                control.enabled = true;  // Added this line
                control.allowUserControl = true;
                control.allowZoom = true;
            }

            // Restore position
            map.SetPosition(pausedLongitude, pausedLatitude);
            map.zoom = pausedZoom;
            map.Redraw();
            Debug.Log($"Unpaused. Position restored to: Lon {pausedLongitude}, Lat {pausedLatitude}, Zoom: {pausedZoom}");
        }
    }

    void EnforcePausedState()
    {
        if (!isPaused) return;

        double currentLon, currentLat;
        map.GetPosition(out currentLon, out currentLat);

        // Added small threshold for floating point comparison
        if (Mathf.Abs((float)(currentLon - pausedLongitude)) > 0.000001f || 
            Mathf.Abs((float)(currentLat - pausedLatitude)) > 0.000001f || 
            map.zoom != pausedZoom)
        {
            map.SetPosition(pausedLongitude, pausedLatitude);
            map.zoom = pausedZoom;
            map.Redraw();

            // Re-disable controls in case they got re-enabled
            if (control != null)
            {
                control.enabled = false;
                control.allowUserControl = false;
                control.allowZoom = false;
            }
        }
    }

    IEnumerator FetchMarkers()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Authorization", "Bearer " + bearerToken);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            JArray markers = JArray.Parse(jsonResponse);

            foreach (JObject marker in markers)
            {
                double latitude = marker["latitude"].ToObject<double>();
                double longitude = marker["longitude"].ToObject<double>();
                string label = marker["label"].ToString();

                if (use3DMarkers)
                    Add3DMarker(latitude, longitude, label);
                else
                    Add2DMarker(latitude, longitude, label);
            }

            // Replace this line:
            // control.Redraw();
            // With:
            map.Redraw();
        }
        else
        {
            Debug.LogError($"Error fetching markers: {request.error}");
        }
    }

    void Add2DMarker(double latitude, double longitude, string label)
    {
        if (markerMGR == null)
        {
            Debug.LogError("MarkerManager is null. Cannot add 2D marker.");
            return;
        }

        try
        {
            OnlineMapsMarker marker = markerMGR.Create(longitude, latitude, markerTexture);
            marker.label = label;
            marker.scale = 3.0f; // Adjust this value as needed
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error adding 2D marker: {e.Message}");
        }
    }

    void Add3DMarker(double latitude, double longitude, string label)
    {
        if (markerMGR == null || markerPrefab == null)
        {
            Debug.LogError("MarkerManager or markerPrefab is null. Cannot add 3D marker.");
            return;
        }

        try
        {
            OnlineMapsMarker3D marker3D = OnlineMapsMarker3DManager.CreateItem(longitude, latitude, markerPrefab);
            marker3D.label = label;
            marker3D.scale = 3.0f; // Adjust this float value as needed
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error adding 3D marker: {e.Message}");
        }
    }

    private void ParseJson(string jsonString)
    {
        JArray jsonArray = JArray.Parse(jsonString);
        // ... rest of the parsing code ...
    }

    // ... rest of your existing methods (ResumeAfterFrame, etc.) ...
}
