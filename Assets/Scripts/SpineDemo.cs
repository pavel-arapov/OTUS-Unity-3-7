using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class SpineDemo : MonoBehaviour
{
    SkeletonAnimation anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<SkeletonAnimation>();
        anim.AnimationName = "run";

        anim.AnimationState.Complete += (TrackEntry entry) => {
            };

        anim.AnimationState.Event += (TrackEntry entry, Spine.Event e) => {
                Debug.Log($"Spine Event");
            };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
