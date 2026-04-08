using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class MenuNavigation : MonoBehaviour
{
    public Selectable defaultSelection;

    private InputAction m_SubmitAction;
    private InputAction m_NavigateAction;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EventSystem.current.SetSelectedGameObject(null);

        m_SubmitAction = InputSystem.actions.FindAction("UI/Submit");
        m_NavigateAction = InputSystem.actions.FindAction("UI/Navigate");
        m_SubmitAction.Enable();
        m_NavigateAction.Enable();
    }

    void LateUpdate()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (m_SubmitAction.WasPressedThisFrame()
                || m_NavigateAction.ReadValue<Vector2>().sqrMagnitude != 0)
            {
                EventSystem.current.SetSelectedGameObject(defaultSelection.gameObject);
            }
        }
    }
}