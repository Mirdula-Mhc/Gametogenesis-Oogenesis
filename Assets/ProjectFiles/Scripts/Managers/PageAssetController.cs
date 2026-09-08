using UnityEngine;
using System.Collections.Generic;

public class PageAssetController : MonoBehaviour
{
    [System.Serializable]
    public class PageAssets
    {
        [Tooltip("The page index this list applies to (0-based, matches PageNavigationController's currentIndex)")]
        public int pageIndex;

        [Tooltip("Objects that should be SET ACTIVE on this page.")]
        public List<GameObject> activeObjects = new List<GameObject>();

        [Tooltip("Objects that should be SET INACTIVE on this page.")]
        public List<GameObject> inactiveObjects = new List<GameObject>();
    }

    [Header("One entry per page. Just drag objects into Active / Inactive lists.")]
    [SerializeField] private List<PageAssets> pageAssets = new List<PageAssets>();

    private void OnEnable()
    {
        PageNavigationController.OnPageChanged += HandlePageChanged;
    }

    private void OnDisable()
    {
        PageNavigationController.OnPageChanged -= HandlePageChanged;
    }

    private void HandlePageChanged(int pageIndex)
    {
        foreach (var page in pageAssets)
        {
            if (page == null)
                continue;
            if (page.pageIndex != pageIndex)
                continue; // not this page, don't touch these objects

            if (page.activeObjects != null)
            {
                foreach (var obj in page.activeObjects)
                {
                    if (obj == null)
                        continue;
                    obj.SetActive(true);
                }
            }

            if (page.inactiveObjects != null)
            {
                foreach (var obj in page.inactiveObjects)
                {
                    if (obj == null)
                        continue;
                    obj.SetActive(false);
                }
            }
        }
    }
}