using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using System.Threading.Tasks;

public class BotController
{
    public PlayerInput playerInput;
    public Gamepad virtualGamepad;

    private Vector2 leftStickValue = Vector2.zero;
    private Vector2 rightStickValue = Vector2.zero;
    private HashSet<GamepadButton> pressedButtons = new HashSet<GamepadButton>();

    /// <param name="pi">Componente PlayerInput asignado al bot.</param>
    /// <param name="gamepad">Gamepad virtual que se utilizará para enviar eventos.</param>
    public BotController(PlayerInput pi, Gamepad gamepad)
    {
        playerInput = pi;
        virtualGamepad = gamepad;
    }

    /// <summary>
    /// Asigna una dirección al joystick especificado sin enviar el evento todavía.
    /// </summary>
    /// <param name="stickID">Valores válidos: "leftStick" o "rightStick".</param>
    /// <param name="direction">Dirección normalizada.</param>
    public void SetStick(string stickID, Vector2 direction)
    {
        if (stickID == "leftStick") leftStickValue = direction;
        else if (stickID == "rightStick") rightStickValue = direction;
    }

    /// <summary>
    /// Asigna el estado de un botón como presionado o liberado sin enviar el evento todavía.
    /// </summary>
    /// <param name="button">Botón a modificar.</param>
    /// <param name="pressed">True para presionar, false para soltar.</param>
    public void SetButton(GamepadButton button, bool pressed)
    {
        if (pressed) pressedButtons.Add(button);
        else pressedButtons.Remove(button);
    }

    /// <summary>
    /// Envía el evento combinado con todos los inputs asignados en este frame.
    /// </summary>
    public void ApplyInputs()
    {
        InputEventPtr eventPtr;
        using (StateEvent.From(virtualGamepad, out eventPtr))
        {
            virtualGamepad.leftStick.WriteValueIntoEvent(leftStickValue, eventPtr);
            virtualGamepad.rightStick.WriteValueIntoEvent(rightStickValue, eventPtr);

            foreach (GamepadButton button in System.Enum.GetValues(typeof(GamepadButton)))
            {
                float value = pressedButtons.Contains(button) ? 1f : 0f;
                virtualGamepad[button].WriteValueIntoEvent(value, eventPtr);
            }

            InputSystem.QueueEvent(eventPtr);
        }
    }

    /// <summary>
    /// Mueve el joystick en la dirección especificada y aplica el evento inmediatamente.
    /// </summary>
    /// <param name="direction">Dirección normalizada.</param>
    /// <param name="stickID">"leftStick" o "rightStick".</param>
    public void MoveJoystick(Vector2 direction, string stickID)
    {
        SetStick(stickID, direction);
        ApplyInputs();
    }

    /// <summary>
    /// Libera el joystick (valor cero) y aplica el evento inmediatamente.
    /// </summary>
    /// <param name="stickID">"leftStick" o "rightStick".</param>
    public void ReleaseJoystick(string stickID)
    {
        SetStick(stickID, Vector2.zero);
        ApplyInputs();
    }

    /// <summary>
    /// Presiona un botón y aplica el evento inmediatamente.
    /// </summary>
    /// <param name="buttonID">Valid values are: DpadUp, DpadDown, DpadLeft, DpadRight, North, East,
    /// South, West, LeftStick, RightStick, LeftShoulder, RightShoulder, Start, Select, LeftTrigger,
    /// RightTrigger.</param>
    public void PressButton(GamepadButton buttonID)
    {
        SetButton(buttonID, true);
        ApplyInputs();
    }

    /// <summary>
    /// Libera un botón y aplica el evento inmediatamente.
    /// </summary>
    /// <param name="buttonID">Valid values are: DpadUp, DpadDown, DpadLeft, DpadRight, North, East,
    /// South, West, LeftStick, RightStick, LeftShoulder, RightShoulder, Start, Select, LeftTrigger,
    /// RightTrigger.</param>
    public void ReleaseButton(GamepadButton buttonID)
    {
        SetButton(buttonID, false);
        ApplyInputs();
    }

    /// <summary>
    /// Presiona y luego libera un botón después de un intervalo determinado (forma asincrónica).
    /// </summary>
    /// <param name="buttonID">Valid values are: DpadUp, DpadDown, DpadLeft, DpadRight, North, East,
    /// South, West, LeftStick, RightStick, LeftShoulder, RightShoulder, Start, Select, LeftTrigger,
    /// RightTrigger.</param>
    /// <param name="duration">Duración en segundos del botón presionado.</param>
    public async Task PressAndReleaseButton(GamepadButton buttonID, float duration)
    {
        PressButton(buttonID);
        await Task.Delay((int)(duration * 1000));
        ReleaseButton(buttonID);
    }

    /// <summary>
    /// Presiona sticks y botones al mismo tiempo durante un intervalo, luego libera todos.
    /// Compatible con corutinas.
    /// </summary>
    /// <param name="leftStick">Dirección del stick izquierdo.</param>
    /// <param name="rightStick">Dirección del stick derecho.</param>
    /// <param name="buttons">Arreglo de botones a presionar.</param>
    /// <param name="duration">Duración en segundos.</param>
    public IEnumerator PressAndReleaseCombo(Vector2 leftStick, Vector2 rightStick, GamepadButton[] buttons, float duration)
    {
        SetStick("leftStick", leftStick);
        SetStick("rightStick", rightStick);

        foreach (var button in buttons)
            SetButton(button, true);

        ApplyInputs();
        yield return new WaitForSeconds(duration);

        SetStick("leftStick", Vector2.zero);
        SetStick("rightStick", Vector2.zero);

        foreach (var button in buttons)
            SetButton(button, false);

        ApplyInputs();
    }

    /// <summary>
    /// Reinicia todos los inputs del gamepad virtual a cero (sticks y botones).
    /// </summary>
    public void ResetInputs()
    {
        leftStickValue = Vector2.zero;
        rightStickValue = Vector2.zero;
        pressedButtons.Clear();
        ApplyInputs();
    }
}