using UnityEngine;
using System;

public class PlayerInput
{
    public event Action<Vector2> movementChanged;
    private Vector2 _prevMoveVector;
    public void Update()
    {
        Vector2 moveVector = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));

        if (moveVector!=_prevMoveVector)
            movementChanged?.Invoke(moveVector);

        _prevMoveVector = moveVector;
    }
}
