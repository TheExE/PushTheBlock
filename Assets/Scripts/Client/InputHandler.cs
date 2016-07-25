using UnityEngine;
using System.Collections.Generic;

public class InputHandler
{
    private Rigidbody playerBody;
    private Vector2 touchStartPosition;

    public InputHandler(Rigidbody playerBody)
    {
        this.playerBody = playerBody;
    }


    public List<InputType> Update()
    {
        List<InputType> inputs = new List<InputType>();

        HandleDesktopControlls(inputs);
        HandleTouchControlls(inputs);


        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        return inputs;
    }

    private void HandleDesktopControlls(List<InputType> inputs)
    {

        if (Input.GetKey(KeyCode.UpArrow))
        {
            MoveFoward();
            inputs.Add(InputType.MoveForward);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            MoveBack();
            inputs.Add(InputType.MoveBack);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
            inputs.Add(InputType.MoveLeft);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight();
            inputs.Add(InputType.MoveRight);
        }
    }
    private void HandleTouchControlls(List<InputType> inputs)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPosition = touch.position;
                    break;

                case TouchPhase.Ended:
                    float swipeDistVertical = (new Vector3(0, touch.position.y, 0) - new Vector3(0, touchStartPosition.y, 0)).magnitude;
                    float swipeDistHorizontal = (new Vector3(touch.position.x, 0, 0) - new Vector3(touchStartPosition.x, 0, 0)).magnitude;
                    Debug.Log("vertical:" + swipeDistVertical);
                    Debug.Log("horizontal:" + swipeDistHorizontal);

                    if (swipeDistVertical > swipeDistHorizontal && swipeDistVertical > 0.1f)
                    {
                        float swipeValue = Mathf.Sign(touch.position.y - touchStartPosition.y);
                        if (swipeValue > 0)//up swipe
                        {
                            MoveFoward();
                            inputs.Add(InputType.MoveForward);
                        }
                        else if (swipeValue < 0)//down swipe
                        {
                            MoveBack();
                            inputs.Add(InputType.MoveBack);
                        }
                    }
                    else if (swipeDistHorizontal > 0.1f)
                    {
                        float swipeValue = Mathf.Sign(touch.position.x - touchStartPosition.x);
                        if (swipeValue > 0)//right swipe
                        {
                            MoveRight();
                            inputs.Add(InputType.MoveRight);
                        }
                        else if (swipeValue < 0)//left swipe
                        {
                            MoveLeft();
                            inputs.Add(InputType.MoveLeft);
                        }
                    }
                    break;
            }
        }
    }
    private void MoveLeft()
    {
        playerBody.AddForce(Vector3.left * GameConsts.MOVE_SPEED * Server.ServerTime);
    }
    private void MoveRight()
    {
        playerBody.AddForce(Vector3.right * GameConsts.MOVE_SPEED * Server.ServerTime);
    }
    public void MoveBack()
    {
        playerBody.AddForce(Vector3.back * GameConsts.MOVE_SPEED * Server.ServerTime);
    }
    public void MoveFoward()
    {
        playerBody.AddForce(Vector3.forward * GameConsts.MOVE_SPEED * Server.ServerTime);
    }
}
