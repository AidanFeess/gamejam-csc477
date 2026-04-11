using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TowerPlacer : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject towerPrefab;

    [Header("Placement Rules")]
    public LayerMask trackLayer;
    public float towerCheckRadius = 0.5f;

    [Header("Ghost Visuals")]
    public Color validColor = new Color(1f, 1f, 1f, 0.5f);
    public Color invalidColor = new Color(1f, 0.3f, 0.3f, 0.5f);
    
    [Header("Towers")]
    public TowerData towerOne;
    public TowerData towerTwo;
    public TowerData towerThree;
    public TowerData towerFour;


    // runtime state
    public TowerData selectedTower;
    private GameObject currentGhost;
    private SpriteRenderer ghostSpriteRenderer;
    private Tower ghostTowerScript;
    private Collider2D ghostCollider;

    private static Tower currentlySelected;

    // Called by UI buttons to begin placing a tower
    public void SetSelectedTower(TowerData newTower)
    {
        selectedTower = newTower;
        BeginPlacing(newTower);
    }

    private void BeginPlacing(TowerData towerData)
    {
        if (currentGhost != null) Destroy(currentGhost);
        if (towerData == null) return;

        currentGhost = Instantiate(towerPrefab);

        ghostTowerScript = currentGhost.GetComponent<Tower>();
        ghostCollider = currentGhost.GetComponent<Collider2D>();
        ghostSpriteRenderer = currentGhost.GetComponent<SpriteRenderer>();

        // disable gameplay behavior while it's a ghost
        if (ghostTowerScript != null)
        {
            ghostTowerScript.enabled = false;
            ghostTowerScript.Initialize(towerData);
            ghostTowerScript.SetSelected(true); // show range indicator
        }

        if (ghostCollider != null) ghostCollider.enabled = false;

        if (ghostSpriteRenderer != null) ghostSpriteRenderer.color = validColor;
    }

    private void CancelPlacing()
    {
        if (currentGhost != null)
        {
            Destroy(currentGhost);
            currentGhost = null;
        }
        selectedTower = null;
        ghostTowerScript = null;
        ghostCollider = null;
        ghostSpriteRenderer = null;
    }

    private void SelectTower(Tower tower)
    {
        if (currentlySelected != null) currentlySelected.SetSelected(false);
        currentlySelected = tower;
        if (currentlySelected != null) currentlySelected.SetSelected(true);
    }

    void Update()
    {
        // selecting towers
        /*
        1: Medicine Tower
        2: Sleep Tower
        3: Priest Tower
        4: Meditation Tower
        */

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SetSelectedTower(towerOne);
        } 
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SetSelectedTower(towerTwo);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SetSelectedTower(towerThree);
        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            // SetSelectedTower(towerFour);
        }

        if (Mouse.current == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(screenPos);
        mouseWorld.z = 0;

        // move the ghost with the mouse and tint based on validity
        bool placementValid = true;
        if (currentGhost != null)
        {
            currentGhost.transform.position = mouseWorld;
            placementValid = IsPlacementValid(mouseWorld);

            if (ghostSpriteRenderer != null)
            {
                ghostSpriteRenderer.color = placementValid ? validColor : invalidColor;
            }
        }

        // right-click or escape cancels placement
        if (Mouse.current.rightButton.wasPressedThisFrame ||
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            CancelPlacing();
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        // ignore clicks on UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // raycast to see if we clicked an existing tower (for selection)
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);
        if (hit.collider != null)
        {
            Tower clickedTower = hit.collider.GetComponentInParent<Tower>();
            if (clickedTower != null && clickedTower != ghostTowerScript)
            {
                SelectTower(clickedTower);
                return;
            }
        }

        // clicked empty space — deselect any previously selected tower
        SelectTower(null);

        // try to place a tower if we're in placing mode
        if (selectedTower != null && currentGhost != null && placementValid)
        {
            if (GameController.Instance.TryTransaction(-selectedTower.cost))
            {
                // promote ghost to a real tower
                if (ghostTowerScript != null)
                {
                    ghostTowerScript.enabled = true;
                    ghostTowerScript.SetSelected(false);
                }
                if (ghostCollider != null) ghostCollider.enabled = true;
                if (ghostSpriteRenderer != null) ghostSpriteRenderer.color = Color.white;

                currentGhost.transform.position = mouseWorld;

                // release the ghost reference so it stays as a placed tower
                currentGhost = null;
                ghostTowerScript = null;
                ghostCollider = null;
                ghostSpriteRenderer = null;
                selectedTower = null;
            }
            else
            {
                // TODO: failed transaction, play failure audio or whatever
            }
        }
    }

    private bool IsPlacementValid(Vector2 position)
    {
        // check if the cursor is inside any track collider
        Collider2D trackHit = Physics2D.OverlapPoint(position, trackLayer);
        if (trackHit != null) return false;

        // check for nearby existing towers
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (GameObject tower in towers)
        {
            if (currentGhost != null && tower == currentGhost) continue;
            if (Vector2.Distance(position, tower.transform.position) < towerCheckRadius)
            {
                return false;
            }
        }

        return true;
    }
}