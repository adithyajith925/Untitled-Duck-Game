using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SpinAction : BaseAction
{
    private float totalSpinAmount;
    private void Update() {
        if (!isActive) {
            return;
        }
        float spinAddAmount = 360f * Time.deltaTime;
        transform.eulerAngles += new Vector3(0, spinAddAmount, 0);
        totalSpinAmount += spinAddAmount;
        if(totalSpinAmount >= 360f) {
            isActive = false;
            onActionComplete();
        }
    }
    public override void TakeAction(GridPosition gridPosition, Action onSpinComplete) {
        this.onActionComplete = onSpinComplete;
        isActive = true;
        totalSpinAmount = 0f;
    }

    public override string GetActionName() {
        return "Spin";
    }

     public override List<GridPosition> GetValidActionGridPositionList() {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();
        return new List<GridPosition> {unitGridPosition};
     }
}
