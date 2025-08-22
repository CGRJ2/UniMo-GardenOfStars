using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunTimeData : MonoBehaviour
{
    public Vector3 Direction;

    public Stack<IngrediantInstance> IngrediantStack = new();

    public ObservableProperty<bool> IsWork;
    public ObservableProperty<bool> IsMove;
}
