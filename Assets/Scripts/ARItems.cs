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
    public float longPressTime = 1.0f;  // Time in seconds required for a long press
    public float doubleTapTimeThreshold = 0.5f; 
    public float doubleTapDistanceThreshold = 50f;  
    
    // Reference to the currently selected/placed object
    private GameObject currentObject;
    private bool isMoving = false;
    private Vector3 touchOffset;
    
    // Variables for gesture detection
    private float touchStartTime;
    private bool isLongPress = false;
    private float lastTapTime = 0;
    private Vector2 lastTapPosition;
    private int tapCount = 0;

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
            
            // Initial touch - start tracking touch time for long press detection
            if (touch.phase == TouchPhase.Began)
            {
                touchStartTime = Time.time;
                isLongPress = false;
                
                // Handle double-tap detection
                if (Time.time - lastTapTime < doubleTapTimeThreshold && 
                    Vector2.Distance(touch.position, lastTapPosition) < doubleTapDistanceThreshold)
                {
                    tapCount++;
                    if (tapCount == 2)
                    {
                        // Double tap detected - place object
                        HandleObjectPlacement(touch);
                        tapCount = 0;
                    }
                }
                else
                {
                    tapCount = 1;
                }
                
                lastTapTime = Time.time;
                lastTapPosition = touch.position;
            }
            // Check for long press and handle object movement
            else if (touch.phase == TouchPhase.Stationary)
            {
                if (!isLongPress && Time.time - touchStartTime > longPressTime)
                {
                    isLongPress = true;
                    HandleLongPress(touch);
                }
            }
            // Move the object if we're in moving mode
            else if (touch.phase == TouchPhase.Moved && isMoving && currentObject != null)
            {
                HandleObjectMovement(touch);
            }
            // End movement mode when touch ends
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
            {
                if (isMoving)
                {
                    isMoving = false;
                }
                
                // Reset long press
                isLongPress = false;
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
            
            // Note: We no longer set isMoving to true here
            // Instead, I'll wait for a long press to initiate movement
            
            // Calculate the offset from the touch to the object
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(currentObject.transform.position);
            touchOffset = currentObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, screenPoint.z));
            
            Debug.Log("Selected existing object: " + hitObject.name);
        }
        // We don't place objects on single tap anymore, that's handled by HandleObjectPlacement
    }
    
    // New method to handle object placement on double tap
    private void HandleObjectPlacement(Touch touch)
    {
        Debug.Log("Double tap detected at: " + touch.position);
        
        // Try to place a new object on a plane
        bool collision = raycastManager.Raycast(touch.position, raycastHits, TrackableType.PlaneWithinPolygon);
        
        // Debug raycast result
        Debug.Log("Raycast collision for placement: " + collision + ", Hits: " + raycastHits.Count);
        
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
            
            Debug.Log("Item instantiated on double tap: " + Items[selectedIndex].name + " (index: " + selectedIndex + ")");
            
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
    
    // Handle long press for object movement
    private void HandleLongPress(Touch touch)
    {
        Debug.Log("Long press detected at: " + touch.position + " - Current object: " + (currentObject != null ? currentObject.name : "none"));
        
        // Only proceed if we have a selected object
        if (currentObject != null)
        {
            // Enable movement mode
            isMoving = true;
            Debug.Log("Starting to move object: " + currentObject.name);
            
            // Try to find target location from raycast
            bool collision = raycastManager.Raycast(touch.position, raycastHits, TrackableType.PlaneWithinPolygon);
            
            if (collision)
            {
                // Update the touch offset
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(currentObject.transform.position);
                touchOffset = currentObject.transform.position - raycastHits[0].pose.position;
            }
        }
        else
        {
            // Try to select an object first
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;
            
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
                
                Debug.Log("Selected and moving object after long press: " + hitObject.name);
            }
        }
    }
    
    // Handle pinch gesture for scaling objects and rotation with two fingers
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
        
        // Handle rotation based on the change in angle between the two fingers
        Vector2 prevVector = touch1PrevPos - touch0PrevPos;
        Vector2 currentVector = touch1.position - touch0.position;
        
        // Calculate the angle between the previous and current vectors
        float prevAngle = Mathf.Atan2(prevVector.y, prevVector.x) * Mathf.Rad2Deg;
        float currentAngle = Mathf.Atan2(currentVector.y, currentVector.x) * Mathf.Rad2Deg;
        float rotationAngle = currentAngle - prevAngle;
        
        // Apply rotation around the Y-axis (up vector in world space)
        if (Mathf.Abs(rotationAngle) > 0.1f) // Small threshold to prevent tiny rotations
        {
            currentObject.transform.Rotate(Vector3.up, rotationAngle, Space.World);
            Debug.Log("Rotating object: " + currentObject.name + " by angle: " + rotationAngle);
        }
    }
    
    // Handle moving objects with touch
    private void HandleObjectMovement(Touch touch)
    {
        if (currentObject == null) 
            return;
        
        // Try to find a valid plane position
        bool collision = raycastManager.Raycast(touch.position, raycastHits, TrackableType.PlaneWithinPolygon);
        
        // If we have a collision with a plane, position the object on the plane
        if (collision)
        {
            // Get position from the raycast hit
            Vector3 newPosition = raycastHits[0].pose.position + touchOffset;
            
            // Smoothly move the object to the touch position
            currentObject.transform.position = Vector3.Lerp(
                currentObject.transform.position, 
                newPosition, 
                Time.deltaTime * 20f  // Adjust this value for smoother/faster movement
            );
        }
        else
        {
            // Fallback to screen-based movement if no plane is hit
            Vector3 screenPoint = new Vector3(touch.position.x, touch.position.y, Camera.main.WorldToScreenPoint(currentObject.transform.position).z);
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(screenPoint) + touchOffset;
            
            // Smoothly move the object to the touch position
            currentObject.transform.position = Vector3.Lerp(
                currentObject.transform.position, 
                newPosition, 
                Time.deltaTime * 20f  // Adjust this value for smoother/faster movement
            );
        }
        
        Debug.Log("Moving object to new position: " + currentObject.transform.position);
    }
}
