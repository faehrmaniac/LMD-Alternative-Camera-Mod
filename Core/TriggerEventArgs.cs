using UnityEngine;


namespace AlternativeCameraMod;

internal class TriggerEventArgs: EventArgs
{
   public TriggerEvent TriggerEvent { get; }
   public GameObject RelatedGameObject { get; }
   public Collider RelatedCollider { get; }


   public TriggerEventArgs(TriggerEvent triggerEvent, GameObject relatedGameObject, Collider relatedCollider)
   {
      TriggerEvent = triggerEvent;
      RelatedGameObject = relatedGameObject;
      RelatedCollider = relatedCollider;
   }
}
