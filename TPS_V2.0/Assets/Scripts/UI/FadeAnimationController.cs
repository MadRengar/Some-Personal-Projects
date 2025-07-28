using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeAnimationController : MonoBehaviour
{
    public void OnFadeComplete()
    {
        UIManager.Instance.OnScreenFadeComplete();
    }
}
