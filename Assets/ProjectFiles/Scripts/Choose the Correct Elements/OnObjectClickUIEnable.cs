//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;
//using DG.Tweening;

//#if ENABLE_INPUT_SYSTEM
//using UnityEngine.InputSystem;
//#endif

//public class OnObjectClickUIEnable : MonoBehaviour
//{
//    // ============================================================
//    // UI DISPLAY TYPE
//    // ============================================================

//    public enum UIDisplayType
//    {
//        Normal,
//        Correct,
//        Wrong
//    }

//    // ============================================================
//    // OBJECT DATA
//    // ============================================================

//    [System.Serializable]
//    public class ObjectUIData
//    {
//        [Header("3D CLICKABLE OBJECT")]
//        [Tooltip("Main 3D object that should be clicked.")]
//        public GameObject clickableObject;

//        [Header("CLICK COLLIDER")]
//        [Tooltip("Collider used for detecting this object. Leave empty to auto-find.")]
//        public Collider clickCollider;

//        [Header("UI")]
//        [Tooltip("Normal UI. Used only when UI Type = Normal.")]
//        public GameObject uiObject;

//        [Tooltip("Correct UI. Used only when UI Type = Correct.")]
//        public GameObject correctUI;

//        [Tooltip("Wrong UI. Used only when UI Type = Wrong.")]
//        public GameObject wrongUI;

//        [Header("UI TYPE")]
//        [Tooltip("Normal = Normal UI only | Correct = Correct UI only | Wrong = Wrong UI only")]
//        public UIDisplayType uiType = UIDisplayType.Normal;

//        [Header("FINAL UI SCALE")]
//        [Tooltip("Final scale of the selected UI.")]
//        public Vector3 uiScale = Vector3.one;
//    }

//    // ============================================================
//    // INSPECTOR
//    // ============================================================

//    [Header("OBJECT + COLLIDER + UI LIST")]
//    [Tooltip("Add all clickable objects here.")]
//    public List<ObjectUIData> objectUIList =
//        new List<ObjectUIData>();

//    // ============================================================
//    // CAMERA
//    // ============================================================

//    [Header("CAMERA SETTINGS")]
//    [Tooltip("Camera used for 3D raycast. Leave empty to use Main Camera.")]
//    public Camera raycastCamera;

//    // ============================================================
//    // RAYCAST
//    // ============================================================

//    [Header("RAYCAST SETTINGS")]
//    [Tooltip("Maximum distance for object detection.")]
//    public float raycastDistance = 1000f;

//    [Tooltip("Layers that can be clicked.")]
//    public LayerMask clickableLayerMask = ~0;

//    // ============================================================
//    // UI ANIMATION
//    // ============================================================

//    [Header("UI SCALE ANIMATION")]

//    [Tooltip("Starting scale of UI.")]
//    public Vector3 uiStartScale =
//        new Vector3(0.1f, 0.1f, 0.1f);

//    [Tooltip("Time taken to grow to final scale.")]
//    public float uiAnimationDuration = 0.3f;

//    [Tooltip("DOTween ease type.")]
//    public Ease uiAnimationEase = Ease.OutBack;

//    // ============================================================
//    // DEBUG
//    // ============================================================

//    [Header("DEBUG")]

//    [Tooltip("Shows click/raycast information in Console.")]
//    public bool enableDebugLog = true;

//    // ============================================================
//    // PRIVATE
//    // ============================================================

//    private readonly Dictionary<int, Tween>
//        activeAnimations =
//        new Dictionary<int, Tween>();

//    // ============================================================
//    // AWAKE
//    // ============================================================

//    private void Awake()
//    {
//        FindCamera();
//        SetupColliders();
//    }

//    // ============================================================
//    // START
//    // ============================================================

//    private void Start()
//    {
//        DisableAllUI();
//    }

//    // ============================================================
//    // FIND CAMERA
//    // ============================================================

//    private void FindCamera()
//    {
//        if (raycastCamera != null)
//            return;

//        raycastCamera = Camera.main;

//        if (raycastCamera == null)
//        {
//            Debug.LogError(
//                "OnObjectClickUIEnable: Main Camera not found. " +
//                "Please assign Raycast Camera manually."
//            );
//        }
//    }

//    // ============================================================
//    // SETUP COLLIDERS
//    // ============================================================

//    private void SetupColliders()
//    {
//        if (objectUIList == null)
//            return;

//        for (int i = 0; i < objectUIList.Count; i++)
//        {
//            ObjectUIData data =
//                objectUIList[i];

//            if (data == null)
//                continue;

//            if (data.clickableObject == null)
//            {
//                Debug.LogWarning(
//                    "Element " + i +
//                    ": Clickable Object is missing."
//                );

//                continue;
//            }

//            // If manually assigned, don't replace it
//            if (data.clickCollider != null)
//                continue;

//            // Check root object
//            data.clickCollider =
//                data.clickableObject.GetComponent<Collider>();

//            // Check children
//            if (data.clickCollider == null)
//            {
//                Collider[] childColliders =
//                    data.clickableObject
//                        .GetComponentsInChildren<Collider>(true);

//                if (childColliders.Length > 0)
//                {
//                    data.clickCollider =
//                        childColliders[0];
//                }
//            }

//            if (data.clickCollider == null)
//            {
//                Debug.LogWarning(
//                    "Element " + i +
//                    ": No Collider found for " +
//                    data.clickableObject.name
//                );
//            }
//        }
//    }

//    // ============================================================
//    // UPDATE
//    // ============================================================

//    private void Update()
//    {
//        if (raycastCamera == null)
//        {
//            FindCamera();

//            if (raycastCamera == null)
//                return;
//        }

//        Vector2 screenPosition;

//        if (GetPointerDown(
//            out screenPosition))
//        {
//            DetectObject(screenPosition);
//        }
//    }

//    // ============================================================
//    // INPUT SYSTEM
//    // ============================================================

//    private bool GetPointerDown(
//        out Vector2 screenPosition)
//    {
//        screenPosition = Vector2.zero;

//#if ENABLE_INPUT_SYSTEM

//        // TOUCH
//        if (Touchscreen.current != null)
//        {
//            var touch =
//                Touchscreen.current.primaryTouch;

//            if (touch.press.wasPressedThisFrame)
//            {
//                screenPosition =
//                    touch.position.ReadValue();

//                if (enableDebugLog)
//                {
//                    Debug.Log(
//                        "Touch detected at: " +
//                        screenPosition
//                    );
//                }

//                return true;
//            }
//        }

//        // MOUSE
//        if (Mouse.current != null)
//        {
//            if (Mouse.current.leftButton
//                .wasPressedThisFrame)
//            {
//                screenPosition =
//                    Mouse.current.position.ReadValue();

//                if (enableDebugLog)
//                {
//                    Debug.Log(
//                        "Mouse click detected at: " +
//                        screenPosition
//                    );
//                }

//                return true;
//            }
//        }

//#else

//        // OLD INPUT SYSTEM FALLBACK
//        if (Input.GetMouseButtonDown(0))
//        {
//            screenPosition =
//                Input.mousePosition;

//            return true;
//        }

//        if (Input.touchCount > 0)
//        {
//            Touch touch =
//                Input.GetTouch(0);

//            if (touch.phase ==
//                TouchPhase.Began)
//            {
//                screenPosition =
//                    touch.position;

//                return true;
//            }
//        }

//#endif

//        return false;
//    }

//    // ============================================================
//    // DETECT OBJECT
//    // ============================================================

//    private void DetectObject(
//        Vector2 screenPosition)
//    {
//        if (raycastCamera == null)
//            return;

//        Ray ray =
//            raycastCamera.ScreenPointToRay(
//                screenPosition
//            );

//        RaycastHit[] hits =
//            Physics.RaycastAll(
//                ray,
//                raycastDistance,
//                clickableLayerMask,
//                QueryTriggerInteraction.Collide
//            );

//        if (enableDebugLog)
//        {
//            Debug.Log(
//                "Raycast hit count: " +
//                hits.Length
//            );
//        }

//        if (hits == null ||
//            hits.Length == 0)
//        {
//            return;
//        }

//        for (int h = 0;
//             h < hits.Length;
//             h++)
//        {
//            Collider hitCollider =
//                hits[h].collider;

//            if (hitCollider == null)
//                continue;

//            for (int i = 0;
//                 i < objectUIList.Count;
//                 i++)
//            {
//                ObjectUIData data =
//                    objectUIList[i];

//                if (data == null)
//                    continue;

//                if (data.clickableObject == null)
//                    continue;

//                // DIRECT COLLIDER MATCH
//                if (data.clickCollider != null &&
//                    hitCollider ==
//                    data.clickCollider)
//                {
//                    ObjectClicked(i);
//                    return;
//                }

//                // ROOT OBJECT MATCH
//                if (hitCollider.transform ==
//                    data.clickableObject.transform)
//                {
//                    ObjectClicked(i);
//                    return;
//                }

//                // CHILD COLLIDER MATCH
//                if (hitCollider.transform.IsChildOf(
//                    data.clickableObject.transform))
//                {
//                    ObjectClicked(i);
//                    return;
//                }
//            }
//        }

//        if (enableDebugLog)
//        {
//            Debug.Log(
//                "Raycast hit an object, but it was not " +
//                "matched with the Object + Collider list."
//            );
//        }
//    }

//    // ============================================================
//    // OBJECT CLICKED
//    // ============================================================

//    private void ObjectClicked(int index)
//    {
//        if (objectUIList == null)
//            return;

//        if (index < 0 ||
//            index >= objectUIList.Count)
//            return;

//        if (enableDebugLog)
//        {
//            Debug.Log(
//                "OBJECT CLICKED SUCCESSFULLY: Element " +
//                index
//            );
//        }

//        // IMPORTANT:
//        // Previous UIs will NOT be hidden.
//        EnableUI(index);
//    }

//    // ============================================================
//    // GET SELECTED UI
//    // ============================================================

//    private GameObject GetSelectedUI(
//        ObjectUIData data)
//    {
//        if (data == null)
//            return null;

//        switch (data.uiType)
//        {
//            case UIDisplayType.Normal:
//                return data.uiObject;

//            case UIDisplayType.Correct:
//                return data.correctUI;

//            case UIDisplayType.Wrong:
//                return data.wrongUI;
//        }

//        return null;
//    }

//    // ============================================================
//    // ENABLE UI
//    // ============================================================

//    public void EnableUI(int index)
//    {
//        if (objectUIList == null)
//            return;

//        if (index < 0 ||
//            index >= objectUIList.Count)
//            return;

//        ObjectUIData data =
//            objectUIList[index];

//        if (data == null)
//            return;

//        StopScaleAnimation(index);

//        // Disable only this object's UI variants
//        if (data.uiObject != null)
//        {
//            data.uiObject.SetActive(false);
//        }

//        if (data.correctUI != null)
//        {
//            data.correctUI.SetActive(false);
//        }

//        if (data.wrongUI != null)
//        {
//            data.wrongUI.SetActive(false);
//        }

//        // Get selected UI
//        GameObject selectedUI =
//            GetSelectedUI(data);

//        if (selectedUI == null)
//        {
//            Debug.LogWarning(
//                "Element " +
//                index +
//                ": Selected UI is EMPTY. " +
//                "UI Type = " +
//                data.uiType
//            );

//            return;
//        }

//        // Enable selected UI
//        selectedUI.SetActive(true);

//        // Start DOTween scale animation
//        StartScaleAnimation(
//            index,
//            selectedUI.transform,
//            data.uiScale
//        );
//    }

//    // ============================================================
//    // DISABLE UI
//    // ============================================================

//    public void DisableUI(int index)
//    {
//        if (objectUIList == null)
//            return;

//        if (index < 0 ||
//            index >= objectUIList.Count)
//            return;

//        ObjectUIData data =
//            objectUIList[index];

//        if (data == null)
//            return;

//        StopScaleAnimation(index);

//        if (data.uiObject != null)
//        {
//            data.uiObject.SetActive(false);
//        }

//        if (data.correctUI != null)
//        {
//            data.correctUI.SetActive(false);
//        }

//        if (data.wrongUI != null)
//        {
//            data.wrongUI.SetActive(false);
//        }
//    }

//    // ============================================================
//    // DISABLE ALL UI
//    // ============================================================

//    public void DisableAllUI()
//    {
//        if (objectUIList == null)
//            return;

//        for (int i = 0;
//             i < objectUIList.Count;
//             i++)
//        {
//            DisableUI(i);
//        }
//    }

//    // ============================================================
//    // DOTWEEN SCALE ANIMATION
//    // ============================================================

//    private void StartScaleAnimation(
//        int index,
//        Transform target,
//        Vector3 finalScale)
//    {
//        if (target == null)
//            return;

//        StopScaleAnimation(index);

//        // Start scale
//        target.localScale =
//            uiStartScale;

//        // No animation
//        if (uiAnimationDuration <= 0f)
//        {
//            target.localScale =
//                finalScale;

//            return;
//        }

//        Tween scaleTween =
//            target.DOScale(
//                finalScale,
//                uiAnimationDuration
//            )
//            .SetEase(uiAnimationEase)
//            .OnComplete(() =>
//            {
//                activeAnimations.Remove(index);
//            });

//        activeAnimations[index] =
//            scaleTween;
//    }

//    // ============================================================
//    // STOP DOTWEEN ANIMATION
//    // ============================================================

//    private void StopScaleAnimation(
//        int index)
//    {
//        if (activeAnimations.TryGetValue(
//            index,
//            out Tween tween))
//        {
//            if (tween != null &&
//                tween.IsActive())
//            {
//                tween.Kill();
//            }

//            activeAnimations.Remove(index);
//        }
//    }

//    // ============================================================
//    // PUBLIC FUNCTIONS
//    // ============================================================

//    public void ShowNormalUI(int index)
//    {
//        if (!IsValidIndex(index))
//            return;

//        objectUIList[index].uiType =
//            UIDisplayType.Normal;

//        EnableUI(index);
//    }

//    public void ShowCorrectUI(int index)
//    {
//        if (!IsValidIndex(index))
//            return;

//        objectUIList[index].uiType =
//            UIDisplayType.Correct;

//        EnableUI(index);
//    }

//    public void ShowWrongUI(int index)
//    {
//        if (!IsValidIndex(index))
//            return;

//        objectUIList[index].uiType =
//            UIDisplayType.Wrong;

//        EnableUI(index);
//    }

//    // ============================================================
//    // VALIDATE INDEX
//    // ============================================================

//    private bool IsValidIndex(int index)
//    {
//        return objectUIList != null &&
//               index >= 0 &&
//               index < objectUIList.Count;
//    }

//    // ============================================================
//    // REFRESH COLLIDERS
//    // ============================================================

//    public void RefreshColliders()
//    {
//        SetupColliders();

//        if (enableDebugLog)
//        {
//            Debug.Log(
//                "OnObjectClickUIEnable: " +
//                "Colliders refreshed."
//            );
//        }
//    }

//    // ============================================================
//    // LAST EVENT
//    // ============================================================

//    [Header("EVENT")]

//    [Tooltip("Assign any Unity Event reference here.")]
//    public UnityEvent onEvent;

//    // ============================================================
//    // PUBLIC EVENT TRIGGER
//    // ============================================================

//    public void TriggerEvent()
//    {
//        onEvent?.Invoke();
//    }

//    // ============================================================
//    // CLEANUP
//    // ============================================================

//    private void OnDestroy()
//    {
//        foreach (var animation in activeAnimations)
//        {
//            if (animation.Value != null &&
//                animation.Value.IsActive())
//            {
//                animation.Value.Kill();
//            }
//        }

//        activeAnimations.Clear();
//    }
//}





using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class OnObjectClickUIEnable : MonoBehaviour
{
    // ============================================================
    // UI DISPLAY TYPE
    // ============================================================

    public enum UIDisplayType
    {
        Normal,
        Correct,
        Wrong
    }

    // ============================================================
    // OBJECT DATA
    // ============================================================

    [System.Serializable]
    public class ObjectUIData
    {
        [Header("3D CLICKABLE OBJECT")]
        [Tooltip("Main 3D object that should be clicked.")]
        public GameObject clickableObject;

        [Header("CLICK COLLIDER")]
        [Tooltip("Collider used for detecting this object. Leave empty to auto-find.")]
        public Collider clickCollider;

        [Header("UI")]
        public GameObject uiObject;

        public GameObject correctUI;

        public GameObject wrongUI;

        [Header("UI TYPE")]
        public UIDisplayType uiType =
            UIDisplayType.Normal;

        [Header("FINAL UI SCALE")]
        public Vector3 uiScale =
            Vector3.one;
    }

    // ============================================================
    // OBJECT LIST
    // ============================================================

    [Header("OBJECT + COLLIDER + UI LIST")]
    public List<ObjectUIData> objectUIList =
        new List<ObjectUIData>();

    // ============================================================
    // CAMERA
    // ============================================================

    [Header("CAMERA SETTINGS")]
    public Camera raycastCamera;

    // ============================================================
    // RAYCAST
    // ============================================================

    [Header("RAYCAST SETTINGS")]
    public float raycastDistance =
        1000f;

    public LayerMask clickableLayerMask =
        ~0;

    // ============================================================
    // UI ANIMATION
    // ============================================================

    [Header("UI SCALE ANIMATION")]

    public Vector3 uiStartScale =
        new Vector3(
            0.1f,
            0.1f,
            0.1f
        );

    public float uiAnimationDuration =
        0.3f;

    public Ease uiAnimationEase =
        Ease.OutBack;

    // ============================================================
    // EVENT SETTINGS
    // ============================================================

    [Header("EVENT SETTINGS")]

    [Tooltip(
        "Enter how many Correct UIs are required before the Event triggers."
    )]
    [Min(1)]
    public int correctCountRequired =
        2;

    [Tooltip(
        "This event triggers when the required number of Correct UIs are active."
    )]
    public UnityEvent onCorrectCountReached;

    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]

    public bool enableDebugLog =
        true;

    // ============================================================
    // PRIVATE
    // ============================================================

    private readonly Dictionary<int, Tween>
        activeAnimations =
        new Dictionary<int, Tween>();

    private bool eventTriggered =
        false;

    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        FindCamera();
        SetupColliders();
    }

    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        DisableAllUI();
    }

    // ============================================================
    // FIND CAMERA
    // ============================================================

    private void FindCamera()
    {
        if (raycastCamera != null)
            return;

        raycastCamera =
            Camera.main;

        if (raycastCamera == null)
        {
            Debug.LogError(
                "OnObjectClickUIEnable: Main Camera not found."
            );
        }
    }

    // ============================================================
    // SETUP COLLIDERS
    // ============================================================

    private void SetupColliders()
    {
        if (objectUIList == null)
            return;

        for (int i = 0;
             i < objectUIList.Count;
             i++)
        {
            ObjectUIData data =
                objectUIList[i];

            if (data == null)
                continue;

            if (data.clickableObject == null)
            {
                Debug.LogWarning(
                    "Element " +
                    i +
                    ": Clickable Object is missing."
                );

                continue;
            }

            // Don't replace manually assigned collider
            if (data.clickCollider != null)
                continue;

            // Check root
            data.clickCollider =
                data.clickableObject
                    .GetComponent<Collider>();

            // Check children
            if (data.clickCollider == null)
            {
                Collider[] childColliders =
                    data.clickableObject
                        .GetComponentsInChildren<Collider>(
                            true
                        );

                if (childColliders.Length > 0)
                {
                    data.clickCollider =
                        childColliders[0];
                }
            }

            if (data.clickCollider == null)
            {
                Debug.LogWarning(
                    "Element " +
                    i +
                    ": No Collider found for " +
                    data.clickableObject.name
                );
            }
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        if (raycastCamera == null)
        {
            FindCamera();

            if (raycastCamera == null)
                return;
        }

        Vector2 screenPosition;

        if (GetPointerDown(
            out screenPosition))
        {
            DetectObject(
                screenPosition
            );
        }
    }

    // ============================================================
    // INPUT
    // ============================================================

    private bool GetPointerDown(
        out Vector2 screenPosition)
    {
        screenPosition =
            Vector2.zero;

#if ENABLE_INPUT_SYSTEM

        // TOUCH
        if (Touchscreen.current != null)
        {
            var touch =
                Touchscreen.current
                    .primaryTouch;

            if (touch.press
                .wasPressedThisFrame)
            {
                screenPosition =
                    touch.position
                        .ReadValue();

                return true;
            }
        }

        // MOUSE
        if (Mouse.current != null)
        {
            if (Mouse.current
                .leftButton
                .wasPressedThisFrame)
            {
                screenPosition =
                    Mouse.current
                        .position
                        .ReadValue();

                return true;
            }
        }

#else

        // OLD INPUT SYSTEM
        if (Input.GetMouseButtonDown(0))
        {
            screenPosition =
                Input.mousePosition;

            return true;
        }

        if (Input.touchCount > 0)
        {
            Touch touch =
                Input.GetTouch(0);

            if (touch.phase ==
                TouchPhase.Began)
            {
                screenPosition =
                    touch.position;

                return true;
            }
        }

#endif

        return false;
    }

    // ============================================================
    // DETECT OBJECT
    // ============================================================

    private void DetectObject(
        Vector2 screenPosition)
    {
        if (raycastCamera == null)
            return;

        Ray ray =
            raycastCamera
                .ScreenPointToRay(
                    screenPosition
                );

        RaycastHit[] hits =
            Physics.RaycastAll(
                ray,
                raycastDistance,
                clickableLayerMask,
                QueryTriggerInteraction.Collide
            );

        if (hits == null ||
            hits.Length == 0)
        {
            return;
        }

        for (int h = 0;
             h < hits.Length;
             h++)
        {
            Collider hitCollider =
                hits[h].collider;

            if (hitCollider == null)
                continue;

            for (int i = 0;
                 i < objectUIList.Count;
                 i++)
            {
                ObjectUIData data =
                    objectUIList[i];

                if (data == null)
                    continue;

                if (data.clickableObject == null)
                    continue;

                // DIRECT COLLIDER MATCH
                if (data.clickCollider != null &&
                    hitCollider ==
                    data.clickCollider)
                {
                    ObjectClicked(i);
                    return;
                }

                // ROOT OBJECT MATCH
                if (hitCollider.transform ==
                    data.clickableObject.transform)
                {
                    ObjectClicked(i);
                    return;
                }

                // CHILD COLLIDER MATCH
                if (hitCollider.transform.IsChildOf(
                    data.clickableObject.transform))
                {
                    ObjectClicked(i);
                    return;
                }
            }
        }
    }

    // ============================================================
    // OBJECT CLICKED
    // ============================================================

    private void ObjectClicked(
        int index)
    {
        if (!IsValidIndex(index))
            return;

        if (enableDebugLog)
        {
            Debug.Log(
                "OBJECT CLICKED: Element " +
                index
            );
        }

        // IMPORTANT:
        // Previous UIs are NOT hidden.
        EnableUI(index);
    }

    // ============================================================
    // GET SELECTED UI
    // ============================================================

    private GameObject GetSelectedUI(
        ObjectUIData data)
    {
        if (data == null)
            return null;

        switch (data.uiType)
        {
            case UIDisplayType.Normal:
                return data.uiObject;

            case UIDisplayType.Correct:
                return data.correctUI;

            case UIDisplayType.Wrong:
                return data.wrongUI;
        }

        return null;
    }

    // ============================================================
    // ENABLE UI
    // ============================================================

    public void EnableUI(
        int index)
    {
        if (!IsValidIndex(index))
            return;

        ObjectUIData data =
            objectUIList[index];

        if (data == null)
            return;

        StopScaleAnimation(index);

        // Disable ONLY this object's variants
        if (data.uiObject != null)
            data.uiObject.SetActive(false);

        if (data.correctUI != null)
            data.correctUI.SetActive(false);

        if (data.wrongUI != null)
            data.wrongUI.SetActive(false);

        // Get selected UI
        GameObject selectedUI =
            GetSelectedUI(data);

        if (selectedUI == null)
        {
            Debug.LogWarning(
                "Element " +
                index +
                ": Selected UI is empty."
            );

            return;
        }

        // Enable UI
        selectedUI.SetActive(true);

        // Animate scale
        StartScaleAnimation(
            index,
            selectedUI.transform,
            data.uiScale
        );

        // Check required correct count
        CheckCorrectCount();
    }

    // ============================================================
    // CHECK CORRECT COUNT
    // ============================================================

    private void CheckCorrectCount()
    {
        // Event already triggered
        if (eventTriggered)
            return;

        int correctCount =
            0;

        for (int i = 0;
             i < objectUIList.Count;
             i++)
        {
            ObjectUIData data =
                objectUIList[i];

            if (data == null)
                continue;

            // Count active Correct UI
            if (data.uiType ==
                    UIDisplayType.Correct &&
                data.correctUI != null &&
                data.correctUI.activeSelf)
            {
                correctCount++;
            }
        }

        if (enableDebugLog)
        {
            Debug.Log(
                "Correct Count: " +
                correctCount +
                " / Required: " +
                correctCountRequired
            );
        }

        // Trigger Event
        if (correctCount >=
            correctCountRequired)
        {
            eventTriggered =
                true;

            if (enableDebugLog)
            {
                Debug.Log(
                    "CORRECT COUNT REACHED! " +
                    "EVENT TRIGGERED."
                );
            }

            onCorrectCountReached?.Invoke();
        }
    }

    // ============================================================
    // DISABLE UI
    // ============================================================

    public void DisableUI(
        int index)
    {
        if (!IsValidIndex(index))
            return;

        ObjectUIData data =
            objectUIList[index];

        if (data == null)
            return;

        StopScaleAnimation(index);

        if (data.uiObject != null)
            data.uiObject.SetActive(false);

        if (data.correctUI != null)
            data.correctUI.SetActive(false);

        if (data.wrongUI != null)
            data.wrongUI.SetActive(false);
    }

    // ============================================================
    // DISABLE ALL UI
    // ============================================================

    public void DisableAllUI()
    {
        if (objectUIList == null)
            return;

        for (int i = 0;
             i < objectUIList.Count;
             i++)
        {
            DisableUI(i);
        }

        // Reset event
        eventTriggered =
            false;
    }

    // ============================================================
    // DOTWEEN SCALE ANIMATION
    // ============================================================

    private void StartScaleAnimation(
        int index,
        Transform target,
        Vector3 finalScale)
    {
        if (target == null)
            return;

        StopScaleAnimation(index);

        // Start small
        target.localScale =
            uiStartScale;

        // Instant scale
        if (uiAnimationDuration <= 0f)
        {
            target.localScale =
                finalScale;

            return;
        }

        // DOTween DOScale
        Tween scaleTween =
            target.DOScale(
                finalScale,
                uiAnimationDuration
            )
            .SetEase(
                uiAnimationEase
            )
            .OnComplete(() =>
            {
                activeAnimations.Remove(
                    index
                );
            });

        activeAnimations[index] =
            scaleTween;
    }

    // ============================================================
    // STOP DOTWEEN ANIMATION
    // ============================================================

    private void StopScaleAnimation(
        int index)
    {
        if (activeAnimations.TryGetValue(
            index,
            out Tween tween))
        {
            if (tween != null &&
                tween.IsActive())
            {
                tween.Kill();
            }

            activeAnimations.Remove(
                index
            );
        }
    }

    // ============================================================
    // PUBLIC FUNCTIONS
    // ============================================================

    public void ShowNormalUI(
        int index)
    {
        if (!IsValidIndex(index))
            return;

        objectUIList[index].uiType =
            UIDisplayType.Normal;

        EnableUI(index);
    }

    public void ShowCorrectUI(
        int index)
    {
        if (!IsValidIndex(index))
            return;

        objectUIList[index].uiType =
            UIDisplayType.Correct;

        EnableUI(index);
    }

    public void ShowWrongUI(
        int index)
    {
        if (!IsValidIndex(index))
            return;

        objectUIList[index].uiType =
            UIDisplayType.Wrong;

        EnableUI(index);
    }

    // ============================================================
    // RESET CORRECT EVENT
    // ============================================================

    public void ResetCorrectEvent()
    {
        eventTriggered =
            false;

        if (enableDebugLog)
        {
            Debug.Log(
                "Correct Event Reset."
            );
        }
    }

    // ============================================================
    // VALIDATE INDEX
    // ============================================================

    private bool IsValidIndex(
        int index)
    {
        return objectUIList != null &&
               index >= 0 &&
               index < objectUIList.Count;
    }

    // ============================================================
    // REFRESH COLLIDERS
    // ============================================================

    public void RefreshColliders()
    {
        SetupColliders();

        if (enableDebugLog)
        {
            Debug.Log(
                "Colliders refreshed."
            );
        }
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        foreach (var animation
            in activeAnimations)
        {
            if (animation.Value != null &&
                animation.Value.IsActive())
            {
                animation.Value.Kill();
            }
        }

        activeAnimations.Clear();
    }
}