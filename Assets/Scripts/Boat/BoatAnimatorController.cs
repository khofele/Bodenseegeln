using System;
using System.Collections;
using UnityEngine;

public class BoatAnimatorController : MonoBehaviour
{
    // GENERAL FIELDS
    private Animator m_animator = null;
    private Rigidbody m_rigidbody = null;
    private float m_frontSailValue = 0.0f;
    private float m_mainSailValue = 0.0f;
    private float m_lerpSpeed = 2.0f;
    private bool m_isSailTransitioning = false;
    private bool m_isSailOpenTarget = false;
    private int m_finishedTransitions = 0;

    // ANIMATION EVENTS
    public event Action OnMainSailTransitionFinished;
    public event Action OnFrontSailTransitionFinished;

    [Header("General Fields")]
    [SerializeField] private WindController m_windController = null;

    [Header("Sails")]
    [SerializeField] private Sail m_frontSail = null;
    [SerializeField] private Sail m_mainSail = null;

    public bool IsSailTransitioning
    {
        get { return m_isSailTransitioning; }
    }

    private void ActivateSailAnimations()
    {
        m_animator.SetBool("IsSailActive", true);
    }

    private void DeactivateSailAnimations()
    {
        m_animator.SetBool("IsSailActive", false);
    }

    private void SetAnimatorSailValue()
    {
        if(m_windController == null || m_frontSail == null || m_mainSail == null)
        {
            return;
        }

        Vector3 apparentWind = m_windController.TrueWind - m_rigidbody.linearVelocity;

        // no wind speed --> move sails to idle pos
        if(apparentWind.magnitude < 0.01f)
        {
            LerpSailValues(0.0f, 0.0f);
            ApplyAnimationValues(m_frontSailValue, m_mainSailValue);

            return;
        }

        // sail angles
        // vector up as reference vector
        float angleFrontSailWind = Vector3.SignedAngle(m_frontSail.transform.forward,apparentWind.normalized,Vector3.up);
        float angleMainSailWind = Vector3.SignedAngle(m_mainSail.transform.forward,apparentWind.normalized,Vector3.up);

        // map sail angles to -1 to 1 --> sinus curve has values ranging from 1 to -1 depending on the angle 
        // -1 sail on left side
        // 0 idle, sail in middle
        // 1 sail on right side
        float frontSailSide = Mathf.Sin(angleFrontSailWind * Mathf.Deg2Rad);
        float mainSailSide = Mathf.Sin(angleMainSailWind * Mathf.Deg2Rad);

        LerpSailValues(frontSailSide, mainSailSide);
        ApplyAnimationValues(m_frontSailValue, m_mainSailValue);
    }

    private void ApplyAnimationValues(float frontSailValue, float mainSailValue)
    {
        m_animator.SetFloat("FrontSailValue", frontSailValue);
        m_animator.SetFloat("MainSailValue", mainSailValue);
    }

    private void LerpSailValues(float frontSailTargetValue, float mainSailTargetValue)
    {
        m_frontSailValue = Mathf.Lerp(m_frontSailValue, frontSailTargetValue, Time.deltaTime * m_lerpSpeed);
        m_mainSailValue = Mathf.Lerp(m_mainSailValue, mainSailTargetValue, Time.deltaTime * m_lerpSpeed);
    }

    private void HandleMainSailTransitionFinished()
    {
        // set is open according to target --> depending on sailmode/motormode
        m_mainSail.IsOpen = m_isSailOpenTarget;

        // trigger animator bool if both sail are ready
        if(m_mainSail.IsOpen == m_frontSail.IsOpen)
        {
            m_animator.SetBool("IsSailOpen", m_mainSail.IsOpen);
        }

        m_finishedTransitions++;
        CheckTransitionsFinished();
    }

    private void HandleFrontSailTransitionFinished()
    {
        m_frontSail.IsOpen = m_isSailOpenTarget;

        if (m_mainSail.IsOpen == m_frontSail.IsOpen)
        {
            m_animator.SetBool("IsSailOpen", m_frontSail.IsOpen);
        }

        m_finishedTransitions++;
        CheckTransitionsFinished();
    }

    private void CheckTransitionsFinished()
    {
        // check finished transition so nothing goes wrong
        if (m_finishedTransitions >= 2)
        {
            m_finishedTransitions = 0;

            // blocks keyboard input for mode change
            m_isSailTransitioning = false;
        }
    }

    private void Start()
    {
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody>();

        m_animator.SetBool("IsSailActive", true);
        m_animator.SetBool("IsSailOpen", true);

        OnMainSailTransitionFinished += HandleMainSailTransitionFinished;
        OnFrontSailTransitionFinished += HandleFrontSailTransitionFinished;
    }

    private void OnDestroy()
    {
        OnMainSailTransitionFinished -= HandleMainSailTransitionFinished;
        OnFrontSailTransitionFinished -= HandleFrontSailTransitionFinished;
    }

    private void Update()
    {
        if (m_animator.GetBool("IsSailOpen") == true && m_animator.GetBool("IsSailActive") == true)
        {
            SetAnimatorSailValue();
        }
    }

    public void AnimationEvent_MainSailTransitionFinished()
    {
        OnMainSailTransitionFinished?.Invoke();
    }

    public void AnimationEvent_FrontSailTransitionFinished()
    {
        OnFrontSailTransitionFinished?.Invoke();
    }

    public void ActivateMotormodeAnimations()
    {
        m_isSailTransitioning = true;
        m_isSailOpenTarget = false;

        DeactivateSailAnimations();
        ApplyAnimationValues(0.0f, 0.0f);
    }

    public void ActivateSailmodeAnimations()
    {
        m_isSailTransitioning = true;
        m_isSailOpenTarget = true;

        ActivateSailAnimations();
        ApplyAnimationValues(0.0f, 0.0f);
    }
}
