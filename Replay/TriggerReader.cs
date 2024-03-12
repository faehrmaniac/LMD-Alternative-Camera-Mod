using MelonLoader;
using UnityEngine;


namespace AlternativeCameraMod;

internal enum TriggerEvent
{
   Start,
   Checkpoint,
   Finish
}


[RegisterTypeInIl2Cpp]
internal class TriggerReader : MonoBehaviour
{
   public State _state;
   private TriggerEvent _triggerEvent;


   public TriggerReader(IntPtr ptr)
      : base(ptr)
   {
   }


   public void Configure(State rp, TriggerEvent triggerEvent)
   {
      _state = rp;
      _triggerEvent = triggerEvent;
   }
   

   public void OnTriggerEnter(Collider other)
   {
      _state?.OnTrigger(_triggerEvent, gameObject, other);
   }
}
