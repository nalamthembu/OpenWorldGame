using UnityEngine;

[CreateAssetMenu(fileName = "ControlSpriteData", menuName = "Game/Controls/Control Sprite Data")]
public class ControlSpriteData : ScriptableObject
{
    [SerializeField] ControlScheme[] controlSchemes;

    public Control GetControl(string controlName, string controlSchemeName)
    {
        for (int i = 0; i < controlSchemes.Length; i++)
        {
            if (controlSchemes[i].name.Equals(controlSchemeName))
            {
                for (int j = 0; j < controlSchemes[i].controls.Length; j++)
                {
                    if (controlSchemes[i].controls[j].name.Equals(controlName))
                        return controlSchemes[i].controls[j];
                }
            }
        }

        Debug.LogError("Could not find specified control, " +
            "please make sure the control scheme and control" +
            " name are spelt correctly.");

        return new Control() { name = "ERROR : COULD NOT FIND CONTROL" };
    }
}

//This describes the control scheme (example : PS Controller, XB Controller, Generic Gamepad, Keyboard & Mouse)
public struct ControlScheme
{
    public string name;
    public Control[] controls;
}

//A single control (Cross Button, Y Button or F-Key & LMB)
public struct Control
{
    public string name;
    public Sprite sprite;
}