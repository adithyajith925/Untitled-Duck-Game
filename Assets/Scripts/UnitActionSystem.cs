using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitActionSystem : MonoBehaviour
{

    public static UnitActionSystem Instance { get; private set; }
    public event EventHandler OnSelectedUnitChanged;
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<bool> OnBusyStatusChanged;
    public event EventHandler OnActionStarted;

    [SerializeField] private Unit selectedUnit;
    [SerializeField] private LayerMask unitLayerMask;

    private BaseAction selectedAction;
    private bool isBusy;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There's more than one UnitActionSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        SetSelectedUnit(selectedUnit);
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }
    private void Update() {
        if (isBusy) {
            return;
        }
        if (!TurnSystem.Instance.IsPlayerTurn()) {
            return;
        }
        if(TryHandleUnitSelection()) {
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject()) {
            return;
        }
        HandleSelectedAction();
    }

    private void HandleSelectedAction() {
        if (Input.GetMouseButtonDown(0)) {
            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());
            if (selectedAction.IsValidActionGridPosition(mouseGridPosition)) {
                if (selectedUnit.TrySpendActionPointsToTakeAction(selectedAction)) {
                    SetBusy();
                    selectedAction.TakeAction(mouseGridPosition, ClearBusy);
                    OnActionStarted?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    private bool TryHandleUnitSelection() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask)) {
                if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit)) {
                    if (unit == selectedUnit) {
                        return false;
                    }
                    if (unit.IsEnemy()) {
                        return false;
                    }
                    SetSelectedUnit(unit);
                    return true;
                }
            }
        }
        return false;
    }

    public void SetSelectedUnit(Unit unit) {
        selectedUnit = unit;
        SetSelectedAction(unit.GetAction<MoveAction>());
        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSelectedAction(BaseAction baseAction) {
        selectedAction = baseAction;
        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetSelectedUnit() {
        return selectedUnit;
    }

    public BaseAction GetSelectedAction() {
        return selectedAction;
    }

    private void SetBusy() {
        isBusy = true;
        OnBusyStatusChanged?.Invoke(this, isBusy);
    }

    private void ClearBusy() {
        isBusy = false;
        OnBusyStatusChanged?.Invoke(this, isBusy);
    }

    public bool IsBusy() {
        return isBusy;
    }

     private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            if (selectedUnit == null)
            {
                if (UnitManager.Instance.GetFriendlyUnitList().Count > 0)
                {
                    SetSelectedUnit(UnitManager.Instance.GetFriendlyUnitList()[0]);
                } else
                {
                    // SetSelectedUnit(null);
                    TurnSystem.Instance.NextTurn();
                }
            }
        }
    }
 }
