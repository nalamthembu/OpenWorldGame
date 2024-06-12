using System.Collections;
using UnityEngine;

public class CharacterSpeech : MonoBehaviour
{
    [SerializeField] CharacterSpeechData m_CharacterSpeechData;
    [SerializeField] AudioSource m_SpeechOrigin; //Where is the mouth?
    [SerializeField] float m_ReactionTime = 0.25F;
    BaseCharacter m_AttachedCharacter;

    //Flags
    bool m_IsFalling;
    bool m_HasStartedFallScream;

    private void Awake()
    {
        if (m_CharacterSpeechData == null)
        {
            Debug.LogError("There is not speech data assigned!");
            enabled = false;
            return;
        }

        if (m_SpeechOrigin == null)
        {
            Debug.LogError("There is no Speech origin assigned!");
            enabled = false; return;
        }
        else
        {
            m_SpeechOrigin.spatialBlend = 1;
            m_SpeechOrigin.minDistance = 3;
            m_SpeechOrigin.maxDistance = 50.0F;
            m_SpeechOrigin.playOnAwake = false;
            m_SpeechOrigin.outputAudioMixerGroup = m_CharacterSpeechData.GetMixerGroup();
        }

        m_AttachedCharacter = GetComponent<BaseCharacter>();
    }

    #region Event Subscription
    private void OnEnable() => SubscribeToEvents();
    private void SubscribeToEvents()
    {
        //Subscribe to events
        if (m_AttachedCharacter.TryGetComponent<HealthComponent>(out var health))
        {
            health.OnBeginFall += OnCharacterStartsFalling;
            health.OnEndFall += OnCharacterEndFall;
            health.OnEntityDeath += OnCharacterDead;
            health.OnEntityTakeDamage += OnCharacterTookDamage;
        }
    }
    private void UnSubscribeFromEvents()
    {
        //Unsubscribe to events
        if (m_AttachedCharacter.TryGetComponent<HealthComponent>(out var health))
        {
            health.OnBeginFall -= OnCharacterStartsFalling;
            health.OnEndFall -= OnCharacterEndFall;
            health.OnEntityDeath -= OnCharacterDead;
            health.OnEntityTakeDamage -= OnCharacterTookDamage;
        }
    }
    private void OnDisable() => UnSubscribeFromEvents();
    #endregion

    private void Start()
    {
        if (m_CharacterSpeechData == null)
        {
            Debug.LogError("Character Speech Data is null");
        }
    }
    private void OnCharacterEndFall(float initialHeight, Vector3 impactPoint)
    {
        m_HasStartedFallScream = false;
        m_IsFalling = false;
    }
    private void OnCharacterStartsFalling(float fallHeight)
    {
        if (m_HasStartedFallScream)
            return;

        m_IsFalling = true;

        Vocalisation vocalisation;

        //fall height is less than 10m
        if (fallHeight > 2 && fallHeight <= 10)
        {
            //Disrupt current speech
            vocalisation = m_CharacterSpeechData.GetVocalisation(VocalisationType.LowFall);

            if (vocalisation == null)
                return;

            m_SpeechOrigin.clip = vocalisation.GetRandomVocalisationClip();

            m_SpeechOrigin.pitch = Random.Range(0.95F, 1.025F);

            m_SpeechOrigin.minDistance = 5;

            m_SpeechOrigin.Play(); //WHOA!
        }
        else //That's kinda high, might wanna scream a little louder.
        {
            //Disrupt current speech
            vocalisation = m_CharacterSpeechData.GetVocalisation(VocalisationType.HighFall);

            if (vocalisation == null)
                return;

            m_SpeechOrigin.clip = vocalisation.GetRandomVocalisationClip();

            m_SpeechOrigin.pitch = Random.Range(0.95F, 1.025F);

            m_SpeechOrigin.minDistance = 5;

            m_SpeechOrigin.Play(); //AHHHH!!
        }

        m_HasStartedFallScream = true;
    }
    private void OnCharacterDead(bool fromHeadshot = false)
    {
        if (m_SpeechOrigin)
        {
            var waveTable = m_CharacterSpeechData.GetPainWaveTable(fromHeadshot ? PainLevel.Death_Headshot : PainLevel.Death);

            m_SpeechOrigin.clip = waveTable.GetRandomPainClip();

            m_SpeechOrigin.pitch = Random.Range(0.95f, 1.025f);

            m_SpeechOrigin.Play(); //BLOOD GURGLES

            Debug.Log("Character Speech Disabled [REASON : DEATH]");
            enabled = false;
        }
    }
    private void OnCharacterTookDamage(PainLevel painLevel)
    {
        if (m_SpeechOrigin)
        {
            if (!m_SpeechOrigin.isPlaying || m_IsFalling)
            {
                var waveTable = m_CharacterSpeechData.GetPainWaveTable(painLevel);

                m_SpeechOrigin.clip = waveTable.GetRandomPainClip();

                m_SpeechOrigin.pitch = Random.Range(0.95f, 1.05f);

                m_SpeechOrigin.Play(); //OUCH!

                m_IsFalling = false;
            }
        }
    }
    public void DoReaction(ReactionType reactionType) => StartCoroutine(ReactionSequence(reactionType));
    private IEnumerator ReactionSequence(ReactionType reactionType)
    {
        yield return new WaitForSeconds(m_ReactionTime);

        ReactionSpeech reaction = m_CharacterSpeechData.GetReactionSpeech(reactionType);

        if (reaction != null && m_SpeechOrigin != null)
        {
            m_SpeechOrigin.clip = reaction.GetRandomReactionClip();

            m_SpeechOrigin.pitch = Random.Range(0.95f, 1.025f);

            if (!m_SpeechOrigin.isPlaying)
                m_SpeechOrigin.Play();
        }
    }
}