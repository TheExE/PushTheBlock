﻿using UnityEngine;
using System.Collections.Generic;

public class InputHandler
{
    private Transform playerTransf;
    private Vector2 touchStartPosition;
    private bool isInputHandlerInited = false;

    public List<InputType> Update(bool isClientCreated)
    {
        List<InputType> inputs = new List<InputType>();

        if (isClientCreated)
        {
            HandleDesktopControlls(inputs);
            HandleTouchControlls(inputs);
            ExecuteCharControlls(inputs);
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        return inputs;
    }
    public void InitInputHandler(Transform playerTransf)
    {
        if (!isInputHandlerInited)
        {
            this.playerTransf = playerTransf;
            isInputHandlerInited = true;
        }
    }
    public Vector3 GetPositionChangeBasedOnInput(InputMessage inputMsg)
    {
        Vector3 positionChange = Vector3.zero;

        foreach (InputType inputType in inputMsg.InputTypeMsg)
        {
            switch (inputType)
            {
                case InputType.MoveBack:
                    positionChange.z -= GameConsts.MOVE_SPEED * Time.deltaTime;
                    break;

                case InputType.MoveForward:
                    positionChange.z += GameConsts.MOVE_SPEED * Time.deltaTime;
                    break;

                case InputType.MoveLeft:
                    positionChange.x -= GameConsts.MOVE_SPEED * Time.deltaTime;
                    break;

                case InputType.MoveRight:
                    positionChange.x += GameConsts.MOVE_SPEED * Time.deltaTime;
                    break;
            }
        }

        return positionChange;
    }

    private void ExecuteCharControlls(List<InputType> inputs)
    {
        foreach (InputType inputType in inputs)
        {
            switch (inputType)
            {
                case InputType.MoveBack:

                    MoveCharacterBackward();
                    break;

                case InputType.MoveForward:

                    MoveCharacterForward();
                    break;

                case InputType.MoveLeft:

                    MoveCharacterLeft();
                    break;

                case InputType.MoveRight:

                    MoveCharacterRight();
                    break;
            }

        }
    }
    private void HandleDesktopControlls(List<InputType> inputs)
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            inputs.Add(InputType.MoveForward);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            inputs.Add(InputType.MoveBack);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            inputs.Add(InputType.MoveLeft);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
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
                        if (swipeValue > 0)     //Up swipe
                        {
                            inputs.Add(InputType.MoveForward);
                        }
                        else if (swipeValue < 0)        //Down swipe
                        {
                            inputs.Add(InputType.MoveBack);
                        }
                    }
                    else if (swipeDistHorizontal > 0.1f)
                    {
                        float swipeValue = Mathf.Sign(touch.position.x - touchStartPosition.x);
                        if (swipeValue > 0)     //Right swipe
                        {
                            inputs.Add(InputType.MoveRight);
                        }
                        else if (swipeValue < 0)    //Left swipe
                        {
                            inputs.Add(InputType.MoveLeft);
                        }
                    }
                    break;
            }
        }
    }
    private void MoveCharacterLeft()
    {
        playerTransf.position += Vector3.left * GameConsts.MOVE_SPEED * Time.deltaTime;
    }
    private void MoveCharacterRight()
    {
        playerTransf.position += Vector3.right * GameConsts.MOVE_SPEED * Time.deltaTime;
    }
    private void MoveCharacterBackward()
    {
        playerTransf.position += Vector3.back * GameConsts.MOVE_SPEED * Time.deltaTime;
    }
    private void MoveCharacterForward()
    {
        playerTransf.position += Vector3.forward * GameConsts.MOVE_SPEED * Time.deltaTime;
    }
}
