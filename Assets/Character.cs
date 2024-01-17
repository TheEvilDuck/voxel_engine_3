using UnityEngine;

public class Character : MonoBehaviour
{
    private float _moveSpeed = 0.3f;
    private Vector2 _moveVector;

    private void Start() 
    {
        transform.position = new Vector3(4.07501078f,19.6999969f,-37.4778595f);
        transform.Rotate(new Vector3(17.5000038f,0,0));
    }

    public void Move(Vector2 direction)
    {
        _moveVector = Vector2.ClampMagnitude(direction,1)*_moveSpeed;
    }

    private void Update() 
    {
        transform.position+=new Vector3(_moveVector.x,0,_moveVector.y);
    }
}
