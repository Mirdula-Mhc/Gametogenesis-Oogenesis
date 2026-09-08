using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// -----------------------------------------------------------------
// One mechanism per page assumed. Page-indexed list of entries -
// each points at a Collider (the draggable) + a trigger Collider
// (the snap zone) + the AnimationSource to play on a valid drop.
// No separate "object" component needed - manager owns all drag
// state directly, keyed by page (only one entry is ever live at a
// time).
//
// Snap check happens ONCE, at the moment of release: the dragged
// object's bounds must overlap the snap zone's bounds, and rotation
// must be within snapAngle (if overlapOnly is false). This does NOT
// require a Rigidbody on the dragged object - Collider.bounds.Intersects()
// is a pure bounding-box test, no physics simulation involved.
//
// Input is routed through Pointer.current (new Input System) rather
// than OnMouseDown, so this behaves consistently for touch on WebGL
// builds, matching ClickAnimManager's raycast approach.
// -----------------------------------------------------------------
public class DragDropAnimManager : MonoBehaviour
{
    [System.Serializable]
    public class PageEntry
    {
        public int pageIndex;

        [Tooltip("The collider on the object the user drags.")]
        public Collider dragTarget;

        [Tooltip("Trigger collider marking the valid drop zone. Must have 'Is Trigger' checked.")]
        public Collider snapZone;

        [Tooltip("If true, only bounding-box overlap is required to snap — rotation is ignored entirely. If false, the object's rotation must also be within snapAngle of snapZone's rotation.")]
        public bool overlapOnly = true;

        [Tooltip("Max rotation difference (degrees) from snapZone's rotation to count as a valid drop. Only used when overlapOnly is false.")]
        public float snapAngle = 15f;

        [Tooltip("Renderers to highlight while this page's object is waiting to be dragged.")]
        public List<Renderer> targetRenderers = new List<Renderer>();
        public Material highlightMaterial;

        public GameObject dropTarget;

        public AnimationSource animation;

        [HideInInspector] public List<Material> originalMaterials;
    }

    [Header("Per-Page Drag-Drop Entries (one per page)")]
    public List<PageEntry> entries = new List<PageEntry>();

    [Header("Drag Detection")]
    public Camera raycastCamera;
    public LayerMask draggableLayers = ~0;

    private int currentPageIndex = -1;
    private readonly HashSet<int> finishedPages = new HashSet<int>();

    private bool dragging = false;
    private Transform draggedTransform;
    private Vector3 dragPlaneOffset;

    // Cached so we can re-enable it if a drop fails and the object
    // returns to its start position/rotation.
    private Animator draggedAnimator;

    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;

    private void OnEnable()
    {
        PageNavigationController.OnPageChanged += SetPageContext;
    }

    private void OnDisable()
    {
        PageNavigationController.OnPageChanged -= SetPageContext;
    }

    private void OnDestroy()
    {
        PageNavigationController.OnPageChanged -= SetPageContext;
    }

    private void Start()
    {
        if (raycastCamera == null)
            raycastCamera = Camera.main;

        SetPageContext(PageNavigationController.CurrentIndex);
    }

    private void Update()
    {
        if (Pointer.current == null) return;
        if (currentPageIndex < 0) return;
        if (finishedPages.Contains(currentPageIndex)) return;

        PageEntry entry = FindEntry(currentPageIndex);
        if (entry == null || entry.dragTarget == null || entry.snapZone == null) return;

        if (raycastCamera == null) raycastCamera = Camera.main;
        if (raycastCamera == null) return;

        if (!dragging && Pointer.current.press.wasPressedThisFrame)
        {
            TryBeginDrag(entry);
        }
        else if (dragging && Pointer.current.press.isPressed)
        {
            ContinueDrag();
        }
        else if (dragging && Pointer.current.press.wasReleasedThisFrame)
        {
            EndDrag(currentPageIndex, entry);
        }
    }

    private void TryBeginDrag(PageEntry entry)
    {
        if (entry.dropTarget != null)
            entry.dropTarget.SetActive(true);

        Ray ray = raycastCamera.ScreenPointToRay(Pointer.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, draggableLayers))
            return;

        if (hit.collider != entry.dragTarget &&
            hit.collider.transform.GetComponentInParent<Collider>() != entry.dragTarget)
            return;

        draggedTransform = entry.dragTarget.transform;
        dragStartPosition = draggedTransform.position;
        dragStartRotation = draggedTransform.rotation;
        dragging = true;

        // Disable Animator for the duration of the drag so it doesn't
        // overwrite our manual transform writes in ContinueDrag().
        draggedAnimator = draggedTransform.GetComponent<Animator>();
        if (draggedAnimator != null)
            draggedAnimator.enabled = false;

        Vector3 pointerWorld = ScreenToPlanePoint(Pointer.current.position.ReadValue());
        dragPlaneOffset = draggedTransform.position - pointerWorld;
    }

    private void ContinueDrag()
    {
        if (draggedTransform == null) return;
        Vector3 pointerWorld = ScreenToPlanePoint(Pointer.current.position.ReadValue());
        draggedTransform.position = pointerWorld + dragPlaneOffset;
    }

    private void EndDrag(int pageIndex, PageEntry entry)
    {
        dragging = false;
        EvaluateDrop(pageIndex, entry);

        // Hide the highlight mesh after evaluating - EvaluateDrop needs
        // entry.snapZone's collider active for a valid bounds check if
        // snapZone lives under dropTarget.
        if (entry.dropTarget != null)
            entry.dropTarget.SetActive(false);

        draggedTransform = null;
    }

    private Vector3 ScreenToPlanePoint(Vector2 screenPos)
    {
        Ray ray = raycastCamera.ScreenPointToRay(screenPos);

        // XY plane — allows movement on X and Y,
        // while keeping Z fixed.
        Plane plane = new Plane(
            Vector3.forward,
            new Vector3(0, 0, draggedTransform.position.z)
        );

        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return draggedTransform != null
            ? draggedTransform.position
            : Vector3.zero;
    }

    private void EvaluateDrop(int pageIndex, PageEntry entry)
    {
        Physics.SyncTransforms();
        Transform obj = entry.dragTarget.transform;

        bool overlapping = entry.dragTarget.bounds.Intersects(entry.snapZone.bounds);
        float angle = Quaternion.Angle(obj.rotation, entry.snapZone.transform.rotation);
        bool rotationOk = entry.overlapOnly || angle <= entry.snapAngle;

        Debug.Log($"[DragDrop] Page {pageIndex} release check — overlapping={overlapping}, angle={angle:F2} (overlapOnly={entry.overlapOnly}, max {entry.snapAngle})");

        if (overlapping && rotationOk)
        {
            Debug.Log($"[DragDrop] Page {pageIndex} — SNAP PASSED, snapping and triggering animation.");
            obj.position = entry.snapZone.transform.position;
            obj.rotation = entry.snapZone.transform.rotation;
            OnSnapped(pageIndex, entry); // re-enables draggedAnimator internally
        }
        else
        {
            Debug.Log($"[DragDrop] Page {pageIndex} — snap FAILED, out of tolerance. Returning to start position.");
            obj.position = dragStartPosition;
            obj.rotation = dragStartRotation;

            // Snap failed - object goes back to its start pose, so restore
            // its Animator so any idle/authored animation resumes normally.
            if (draggedAnimator != null)
            {
                draggedAnimator.enabled = true;
                draggedAnimator = null;
            }
        }
    }

    private void OnSnapped(int pageIndex, PageEntry entry)
    {
        finishedPages.Add(pageIndex);

        ClearHighlight(entry);

        // Snap succeeded — re-enable the Animator now that the drag is done.
        if (draggedAnimator != null)
        {
            draggedAnimator.enabled = true;
            draggedAnimator = null;
        }

        if (entry.animation != null && entry.animation.IsValid)
        {
            Debug.Log($"[DragDrop] Page {pageIndex} — OnSnapped: valid AnimationSource found (director={(entry.animation.director != null ? entry.animation.director.name : "none")}, animator={(entry.animation.animator != null ? entry.animation.animator.name : "none")}). Starting Play().");
            StartCoroutine(entry.animation.Play(this, () =>
            {
                Debug.Log($"[DragDrop] Page {pageIndex} — animation Play() completed, requesting navigation unlock.");
                PageNavigationController.RequestNavigationUnlock();
            }));
        }
        else
        {
            Debug.LogWarning($"[DragDropAnimManager] Page {pageIndex}: no valid AnimationSource - unlocking immediately.");
            PageNavigationController.RequestNavigationUnlock();
        }
    }

    private void SetPageContext(int pageIndex)
    {
        // Disarm/clear whatever was active on the page we're leaving.
        PageEntry previousEntry = FindEntry(currentPageIndex);
        if (previousEntry != null)
            ClearHighlight(previousEntry);

        currentPageIndex = pageIndex;
        dragging = false;
        draggedTransform = null;

        PageEntry entry = FindEntry(pageIndex);
        if (entry == null || finishedPages.Contains(pageIndex))
            return;

        ApplyHighlight(entry);
    }

    private void ApplyHighlight(PageEntry entry)
    {
        if (entry.targetRenderers == null || entry.targetRenderers.Count == 0 || entry.highlightMaterial == null)
            return;

        // Rebuild the cache if it's missing or out of sync with the current
        // renderer list (e.g. the list was edited in the Inspector after a
        // previous highlight pass already cached it).
        if (entry.originalMaterials == null || entry.originalMaterials.Count != entry.targetRenderers.Count)
        {
            entry.originalMaterials = new List<Material>();
            foreach (var r in entry.targetRenderers)
                entry.originalMaterials.Add(r != null ? r.material : null);
        }

        foreach (var r in entry.targetRenderers)
            if (r != null) r.material = entry.highlightMaterial;
    }

    private void ClearHighlight(PageEntry entry)
    {
        if (entry.targetRenderers == null || entry.originalMaterials == null) return;

        int count = Mathf.Min(entry.targetRenderers.Count, entry.originalMaterials.Count);
        for (int i = 0; i < count; i++)
            if (entry.targetRenderers[i] != null && entry.originalMaterials[i] != null)
                entry.targetRenderers[i].material = entry.originalMaterials[i];
    }

    private PageEntry FindEntry(int pageIndex)
    {
        return entries.Find(e => e != null && e.pageIndex == pageIndex);
    }

    public bool OwnsPage(int pageIndex) => FindEntry(pageIndex) != null;
}