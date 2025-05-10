using Unity.Netcode.Components;
using UnityEngine;

public class OwnerNetworkAnimator : NetworkAnimator
{
    Animator m_Animator;
    private void Start()
    {
        m_Animator = Animator;
    }
    protected override bool OnIsServerAuthoritative()
    {
        return false; // Disable server authority
    }
    
    public void SetBool(string varName, bool value)
    {
        m_Animator.SetBool(varName, value);
    }
}
