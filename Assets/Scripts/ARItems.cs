using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;

public class ARItems : MonoBehaviour
{
    public GameObject[] Items; 

    [Header("AR Components")]
    public XROrigin xrOrigin;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    
    [Header("Gesture Settings")]
    public float rotationSpeed = 100f;
    public float scaleMinimum = 0.5f;
    public float scaleMaximum = 3.0f;
    public float scaleSensitivity = 0.01f;
    public float moveSensitivity = 0.01f;
    
    // Reference to the currently selected/placed object
    private GameObject currentObject;
    private bool isMoving = false;
    private Vector3 touchOffset;

    // Setter method for a specific Item GameObject in the array
    public void SetItem(GameObject newItem, int index)
    {
        if (Items != null && index >= 0 && index < Items.Length)
        {
            Items[index] = newItem;
            Debug.Log("Item " + index + " changed to: " + (newItem != null ? newItem.name : "null"));
        }
        else
        {
            Debug.LogError("Invalid index or Items array not initialized");
        }
    }

    void Start()
    {
        // Validate the Items array
        ValidateItemsArray();
        
        // Log the selected item from Auth
        Debug.Log("Current selected item from Auth: " + Auth.selected);
    }

    // Validate that the Items array is properly set up
    private void ValidateItemsArray()
    {
        if (Items == null || Items.Length == 0)
        {
            Debug.LogWarning("Items array is not initialized in Inspector. Initializing with default size of 3.");
            Items = new GameObject[3]; // Default size for white, brown, light brown sofas
        }
        
        // Check for null elements
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] == null)
            {
                Debug.LogWarning("Items[" + i + "] is null. Please assign all prefabs in the Inspector.");
            }
        }
    }

    void Update()
    {
        // Handle multi-touch gestures (scaling)
        if (Input.touchCount == 2)
        {
            HandlePinchToScale();
        }
        // Handle single touch gestures (placement, selection, movement)
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            
            // Initial touch - place object or select existing one
            if (touch.phase == TouchPhase.Began)
            {
                HandleTouchBegan(touch);
            }
            // Move the object if we're in moving mode
            else if (touch.phase == TouchPhase.Moved && isMoving && currentObject != null)
            {
                HandleObjectMovement(touch);
            }
            // End movement mode when touch ends
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isMoving)
            {
                isMoving = false;
            }
        }
    }
    
    private void HandleTouchBegan(Touch touch)
    {
        Debug.Log("Touch detected at: " + touch.position);
        
        // Try to select an existing object first
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        
        // Check if we hit an existing item
        if (Physics.Raycast(ray, out hit))
        {
            // If the hit object is one of our placed items
            GameObject hitObject = hit.transform.gameObject;
            
            // Select this object for manipulation
            currentObject = hitObject;
            isMoving = true;
            
            // Calculate the offset from the touch to the object
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(currentObject.transform.position);
            touchOffset = currentObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, screenPoint.z));
            
            Debug.Log("Selected existing object: " + hitObject.name);
            return;
        }
        
        // If we didn't hit an object, try to place a new one on a plane
        bool collision = raycastManager.Raycast(touch.position, raycastHits, TrackableType.PlaneWithinPolygon);
        
        // Debug raycast result
        Debug.Log("Raycast collision: " + collision + ", Hits: " + raycastHits.Count);
        
        // Get selected index from Auth class
        int selectedIndex = Auth.selected;
        
        // Check if selection is valid and item exists
        if (collision && Items != null && Items.Length > 0 && 
            selectedIndex >= 0 && selectedIndex < Items.Length && 
            Items[selectedIndex] != null)
        {
            // Place the selected item
            GameObject _object = Instantiate(Items[selectedIndex]);
            _object.transform.position = raycastHits[0].pose.position;
            _object.transform.rotation = raycastHits[0].pose.rotation;
            
            // Add a collider if it doesn't have one (needed for selection)
            if (_object.GetComponent<Collider>() == null)
            {
                _object.AddComponent<BoxCollider>();
            }
            
            // Make this the current object
            currentObject = _object;
            
            Debug.Log("Item instantiated: " + Items[selectedIndex].name + " (index: " + selectedIndex + ")");
            
            // Disable all detected planes after successful placement
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(true);
            }
            
            planeManager.enabled = true;
        }
        else
        {
            // Error handling with more detailed messages
            if (Items == null || Items.Length == 0)
            {
                Debug.LogError("No Items array initialized or it's empty!");
            }
            else if (selectedIndex < 0 || selectedIndex >= Items.Length)
            {
                Debug.LogError("Invalid selection index: " + selectedIndex + ". Valid range: 0-" + (Items.Length - 1));
            }
            else if (Items[selectedIndex] == null)
            {
                Debug.LogError("Selected Item at index " + selectedIndex + " is null!");
            }
        }
    }
    
    // Handle pinch gesture for scaling objects
    private void HandlePinchToScale()
    {
        // Only scale if we have an object selected
        if (currentObject == null)
            return;
            
        // Get both touches
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        
        // Find the position in the previous frame
        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        
        // Calculate the magnitude of the distance between touches, both current and previous
        float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
        float touchDeltaMag = (touch0.position - touch1.position).magnitude;
        
        // Find the difference in distances between frames
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
        
        // Calculate the new scale based on the pinch distance
        Vector3 currentScale = currentObject.transform.localScale;
        float newScale = currentScale.x - deltaMagnitudeDiff * scaleSensitivity;
        
        // Clamp the scale to our min/max values
        newScale = Mathf.Clamp(newScale, scaleMinimum, scaleMaximum);
        
        // Apply the new scale uniformly
        currentObject.transform.localScale = new Vector3(newScale, newScale, newScale);
        
        // Debug scaling
        if (Mathf.Abs(deltaMagnitudeDiff) > 1.0f)
        {
            Debug.Log("Scaling object: " + currentObject.name + " to scale: " + newScale);
        }
    }
    
    // Handle moving objects with touch
    private void HandleObjectMovement(Touch touch)
    {
        if (currentObject == null) 
            return;
            
        // Convert the touch position to world position at the same depth as the object
        Vector3 screenPoint = new Vector3(touch.position.x, touch.position.y, Camera.main.WorldToScreenPoint(currentObject.transform.position).z);
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(screenPoint) + touchOffset;
        
        // Smoothly move the object to the touch position
        currentObject.transform.position = Vector3.Lerp(
            currentObject.transform.position, 
            newPosition, 
            Time.deltaTime * 20f  // Adjust this value for smoother/faster movement
        );
    }
}
