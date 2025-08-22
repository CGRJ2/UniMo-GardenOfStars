using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaterRuntimeData : MonoBehaviour
{
    public Stack<IngrediantInstance> IngrediantStack = new();
    public Transform ProdsAttachPoint;

    public ObservableProperty<bool> IsWork;
    public ObservableProperty<bool> IsMove;
}
