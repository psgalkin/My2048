using UnityEngine;

public class Control : MonoBehaviour
{
    [SerializeField] private InGameUi _ui;

    private Vector2 _startPos;
    private Vector2 _endPos;
    private Vector2 _direction;
    private bool _directionChosen;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            Field.MoveTilesInDirection(Direction.Up);
        else if (Input.GetKeyDown(KeyCode.S))
            Field.MoveTilesInDirection(Direction.Down);
        else if (Input.GetKeyDown(KeyCode.A))
            Field.MoveTilesInDirection(Direction.Left);
        else if (Input.GetKeyDown(KeyCode.D))
            Field.MoveTilesInDirection(Direction.Right);

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _startPos = touch.position;
                    _directionChosen = false;
                    break;

                case TouchPhase.Moved:
                    _direction = touch.position - _startPos;
                    break;

                case TouchPhase.Ended:
                    _endPos = touch.position;
                    _directionChosen = true;
                    break;
            }
        }
        if (_directionChosen)
        {
            _direction.Normalize();
            float angle = Angle(_direction);
            
            if (angle > 45 && angle <= 135)
                Field.MoveTilesInDirection(Direction.Up);
            else if (angle > 135 || angle < -135)
                Field.MoveTilesInDirection(Direction.Left);
            else if (angle < -45 && angle >= -135)
                Field.MoveTilesInDirection(Direction.Down);
            else 
                Field.MoveTilesInDirection(Direction.Right);

            _directionChosen = false;
        }
        
    }

    public static float Angle(Vector2 v)
    {
        return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
    }


}
