using UnityEngine;

public enum AnimationArchitype
{
    Masculine,
    Feminine
}

[CreateAssetMenu(fileName = "CharacterAnimData", menuName = "Game/Characters/AnimationData")]
public class CharacterAnimationData : ScriptableObject
{
    public AnimationArchitype animationArchitype;
}