using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharaterRuntimeData : MonoBehaviour
{
    public Stack<IngrediantInstance> IngrediantStack = new();
    public Transform ProdsAttachPoint;

    public ObservableProperty<bool> IsWork = new();
    public ObservableProperty<bool> IsMove = new();

    public abstract int GetMaxCapacity();
    public abstract float GetProductionSpeed();
}
