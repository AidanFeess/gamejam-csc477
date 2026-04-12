using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

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
    public TowerData selectedTower;

    [Header("Sell UI")]
    public GameObject sellCanvas;
    public TMPro.TextMeshProUGUI sellValueText;
    public Vector3 sellCanvasOffset = new Vector3(1f, 0f, 0f);

    // runtime state
    private GameObject currentGhost;
    private SpriteRenderer ghostSpriteRenderer;
    private Tower ghostTowerScript;
    private Collider2D ghostCollider;
    private Animator ghostAnimator;

    private static Tower currentlySelected;

    public void SetSelectedTower(TowerData newTower)
    {
        if (newTower == null || newTower.isLocked) return;
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
        ghostAnimator = currentGhost.GetComponent<Animator>();

        // disable animator on ghost so it can't override the transform
        if (ghostAnimator != null) ghostAnimator.enabled = false;

        if (ghostTowerScript != null)
        {
            ghostTowerScript.Initialize(towerData, isGhost: true);
            ghostTowerScript.SetSelected(true);
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
        ghostAnimator = null;
    }

    private void SelectTower(Tower tower)
    {
        if (currentlySelected != null) currentlySelected.SetSelected(false);
        currentlySelected = tower;

        if (currentlySelected != null)
        {
            currentlySelected.SetSelected(true);
            ShowSellCanvas(currentlySelected);
        }
        else
        {
            HideSellCanvas();
        }
    }

    private void ShowSellCanvas(Tower tower)
    {
        if (sellCanvas == null) return;
        sellCanvas.SetActive(true);
        sellCanvas.transform.position = tower.transform.position + sellCanvasOffset;

        if (sellValueText != null)
        {
            sellValueText.text = $"Sell: {tower.data.sellValue}";
        }
    }

    private void HideSellCanvas()
    {
        if (sellCanvas != null) sellCanvas.SetActive(false);
    }

    public void OnSellButtonClicked()
    {
        if (currentlySelected == null) return;
        currentlySelected.Sell();
        SelectTower(null);
    }

    void Update()
    {
        // tower hotkey selection (1-4)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SetSelectedTower(towerOne);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) SetSelectedTower(towerTwo);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) SetSelectedTower(towerThree);
            else if (Keyboard.current.digit4Key.wasPressedThisFrame) SetSelectedTower(towerFour);
        }

        if (Mouse.current == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(screenPos);
        mouseWorld.z = 0;

        // move ghost with mouse and tint based on validity
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

        // right-click or escape cancels
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

        SelectTower(null);

        // try to place a tower
        if (selectedTower != null && currentGhost != null && placementValid)
        {
            if (GameController.Instance.TryTransaction(-selectedTower.cost))
            {
                // promote ghost to a real tower
                if (ghostTowerScript != null)
                {
                    ghostTowerScript.SetSelected(false);
                    ghostTowerScript.UnghostTower();

                    // spawn persistent sleep effect for sleep tower
                    if (selectedTower.towerName == "Sleep Tower" && ghostTowerScript.sleepEffectPrefab != null)
                    {
                        Vector3 offset = new Vector3(0, 0.5f, 0);
                        GameObject fx = Instantiate(
                            ghostTowerScript.sleepEffectPrefab,
                            ghostTowerScript.transform.position + offset,
                            Quaternion.identity);
                        fx.transform.SetParent(ghostTowerScript.transform);
                    }

                    // spawn priest attack effect (hidden until tower fires)
                    if (selectedTower.towerName == "Priest Tower" && ghostTowerScript.priestAttackEffectPrefab != null)
                    {
                        GameObject fx = Instantiate(
                            ghostTowerScript.priestAttackEffectPrefab,
                            ghostTowerScript.transform.position,
                            Quaternion.identity);   
                        fx.transform.SetParent(ghostTowerScript.transform);
                        fx.SetActive(false);
                        ghostTowerScript.SetPriestEffect(fx);
                    }
                }

                if (ghostCollider != null) ghostCollider.enabled = true;
                if (ghostAnimator != null) ghostAnimator.enabled = true;
                if (ghostSpriteRenderer != null) ghostSpriteRenderer.color = Color.white;

                currentGhost.transform.position = mouseWorld;

                // release references — it's a real tower now
                currentGhost = null;
                ghostTowerScript = null;
                ghostCollider = null;
                ghostSpriteRenderer = null;
                ghostAnimator = null;
                selectedTower = null;
            }
            else
            {
                // TODO: failed transaction, play failure audio
            }
        }
    }

    private bool IsPlacementValid(Vector2 position)
    {
        if (selectedTower == null) return false;
        if (!GameController.Instance.CanAfford(selectedTower.cost)) return false;

        // can't place on the track
        Collider2D trackHit = Physics2D.OverlapPoint(position, trackLayer);
        if (trackHit != null) return false;

        // can't overlap other towers
        Collider2D[] overlappingColliders = Physics2D.OverlapCircleAll(position, towerCheckRadius);

        foreach (Collider2D col in overlappingColliders)
        {
            if (currentGhost != null && col.gameObject == currentGhost) continue;

            if (col.CompareTag("Tower"))
            {
                return false;
            }
        }

        return true;
    }
}