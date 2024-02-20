using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueNet.Demos
{
    public class BackgroundAnimation : MonoBehaviour
    {

        public SyncedAnimator SyncedAnimator;
        int state=0;
        private IEnumerator Start()
        {
            while (enabled)
            {
                yield return new WaitForSecondsRealtime(10);
                state++;
                if (state > 3)
                {
                    state = 1;
                }
                SyncedAnimator.SetInt("state", state);
                //state is needed to be received before trigger so force is needed
                SyncedAnimator.ForceUpdate();
                SyncedAnimator.SetTrigger("start");
            }
        }
    }
}