using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerHandler : MonoBehaviour,

    IEventSystemHandler,
    IPointerClickHandler,
    IPointerUpHandler,
    IPointerDownHandler,
    IPointerMoveHandler,
    IPointerEnterHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public event Action<PointerEventData> Click;
    public event Action<PointerEventData> Up;
    public event Action<PointerEventData> Down;
    public event Action<PointerEventData> Move;
    public event Action<PointerEventData> Enter;
    public event Action<PointerEventData> Exit;
    public event Action<PointerEventData> BeginDrag;
    public event Action<PointerEventData> Drag;
    public event Action<PointerEventData> EndDrag;

    public void OnPointerClick(PointerEventData eventData) => Click?.Invoke(eventData);
    public void OnPointerUp(PointerEventData eventData) => Up?.Invoke(eventData);
    public void OnPointerDown(PointerEventData eventData) => Down?.Invoke(eventData);
    public void OnPointerMove(PointerEventData eventData) => Move?.Invoke(eventData);
    public void OnPointerEnter(PointerEventData eventData) => Enter?.Invoke(eventData);
    public void OnPointerExit(PointerEventData eventData) => Exit?.Invoke(eventData);
    public void OnBeginDrag(PointerEventData eventData) => BeginDrag?.Invoke(eventData);
    public void OnDrag(PointerEventData eventData) => Drag?.Invoke(eventData);
    public void OnEndDrag(PointerEventData eventData) => EndDrag?.Invoke(eventData);
}

