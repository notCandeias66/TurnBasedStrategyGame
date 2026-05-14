using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class BaseUnit : MonoBehaviour
{
    //[SerializeField] protected internal SpriteRenderer sp;
    //[SerializeField] private GameObject hightlight;

    public UnitStatus unitStatus;

    public UnitTurnAction unitTurnAction;

    public int unitId { get; set; }

    public string unitRole { get; set; }

    public string unitType { get; set; }

    public virtual void Init(int x, int y)
    {

    }

    /*private void OnMouseEnter()
    {
        hightlight.SetActive(true);
    }*/

    /*private void OnMouseExit()
    {
        hightlight.SetActive(false);
    }*/
}

public enum UnitStatus
{
    Alive,
    Died,
    Dead,
    Ghost
}

public enum UnitTurnAction
{
    Spawned,
    Attacked,
    Holded,
    Moved
}
