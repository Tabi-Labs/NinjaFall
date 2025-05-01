using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class BotController
{
    public PlayerInput playerInput;
    public Gamepad virtualGamepad;

    private GamepadState currentState;

    public BotController(PlayerInput pi, Gamepad gamepad)
    {
        playerInput = pi;
        virtualGamepad = gamepad;
        currentState = new GamepadState();
    }

    /// <summary>
    /// Moves the joystick in the specified direction.
    /// </summary>
    /// <param name="direction">Normalized Vector 2 in a certain direction.</param>
    /// <param name="stickID"> Valid values are: "leftStick" and "rightStick".</param>
    public void MoveJoystick(Vector2 direction, string stickID)
    {
        InputEventPtr eventPtr;
        using (StateEvent.From(virtualGamepad, out eventPtr))
        {
            ((StickControl) virtualGamepad[stickID]).WriteValueIntoEvent(direction, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    /// <summary>
    /// Releases the joystick, setting its value to zero.
    /// </summary>
    /// <param name="stickID"> Valid values are: "leftStick" and "rightStick".</param>
    public void ReleaseJoystick(string stickID)
    {
        InputEventPtr eventPtr;
        using (StateEvent.From(virtualGamepad, out eventPtr))
        {
            ((StickControl) virtualGamepad[stickID]).WriteValueIntoEvent(Vector2.zero, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    /// <summary>
    /// Presses a button on the virtual gamepad.
    /// </summary>
    /// <param name="buttonID">Valid values are: DpadUp, DpadDown, DpadLeft, DpadRight, North, East, South, West, LeftStick, RightStick, LeftShoulder, RightShoulder, Start, Select, LeftTrigger, RightTrigger</param>
    public void PressButton(GamepadButton buttonID)
    {
        InputEventPtr eventPtr;
        using (StateEvent.From(virtualGamepad, out eventPtr))
        {
            virtualGamepad[buttonID].WriteValueIntoEvent<float>(1, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    /// <summary>
    /// Releases a button on the virtual gamepad.
    /// </summary>
    /// <param name="buttonID">Valid values are: DpadUp, DpadDown, DpadLeft, DpadRight, North, East, South, West, LeftStick, RightStick, LeftShoulder, RightShoulder, Start, Select, LeftTrigger, RightTrigger</param>
    public void ReleaseButton(GamepadButton buttonID)
    {
        InputEventPtr eventPtr;
        using (StateEvent.From(virtualGamepad, out eventPtr))
        {
            virtualGamepad[buttonID].WriteValueIntoEvent<float>(0, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    /// <summary>
    /// Presses and releases a button on the virtual gamepad for a specified duration.
    /// </summary>
    /// <param name="buttonID">Valid values are: DpadUp, DpadDown, DpadLeft, DpadRight, North, East, South, West, LeftStick, RightStick, LeftShoulder, RightShoulder, Start, Select, LeftTrigger, RightTrigger</param>
    /// <param name="duration">Duration in seconds for which the button will be pressed.</param>
    public async Task PressAndReleaseButton(GamepadButton buttonID, float duration)
    {
        PressButton(buttonID);
        await Task.Delay((int) (duration * 1000)); // Convertir a milisegundos
        ReleaseButton(buttonID);
    }

    /// <summary>
    /// Resets all inputs of the virtual gamepad to zero.
    /// </summary>
    public void ResetInputs()
    {
        InputEventPtr eventPtr;
        using (StateEvent.From(virtualGamepad, out eventPtr))
        {
            foreach (var control in virtualGamepad.allControls)
            {
                if (control is ButtonControl button)
                    button.WriteValueIntoEvent(0f, eventPtr);
                else if (control is StickControl stick)
                    stick.WriteValueIntoEvent(Vector2.zero, eventPtr);
            }

            InputSystem.QueueEvent(eventPtr);
        }
    }

}
