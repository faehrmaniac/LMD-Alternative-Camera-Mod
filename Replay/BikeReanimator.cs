using UnityEngine;


namespace AlternativeCameraMod.Replay;

internal class BikeReanimator
{
   private readonly Dictionary<string, GameObject> _bikeObjects;
   private HashSet<string> _applyElements;


   public BikeReanimator(GameObject bike)
   {
      _bikeObjects = new Dictionary<string, GameObject>();
      _bikeObjects.Add("Bike(Clone)", bike);
      GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
      for (int i = 0; i < gameObjects.Length; i++)
      {
         // LMD 'BindJNT' objects represent the parts of the bike and the rider which all
         // need to be animated to reproduce the look and feel of a real ride
         if (gameObjects[i].name.EndsWith("BindJNT") 
             && !gameObjects[i].name.StartsWith("SoxAtk") 
             && gameObjects[i].transform.GetRoot().Equals(bike.transform)
             && gameObjects[i].active)
         {
            _bikeObjects.Add(gameObjects[i].name, gameObjects[i]);
         }
      }
   }
   

   public void Initialize(HashSet<string> elements)
   {
      _applyElements = elements;
   }


   public int GetLocationCount()
   {
      return _bikeObjects.Count;
   }
   

   public IEnumerable<string> GetObjectNames()
   {
      return _bikeObjects.Keys;
   }


   public IEnumerable<Tuple<string, Vector3, Quaternion>> GetLocations()
   {
      foreach (var go in _bikeObjects.Values)
      {
         yield return new Tuple<string, Vector3, Quaternion>(go.name, go.transform.localPosition, go.transform.localRotation);
      }
   }


   public void ApplyState(Snapshot s1, Snapshot s2, float interpolationFactor)
   {
      if (_applyElements == null) return;
      // 0 = Camera
      foreach (var name in _applyElements)
      {
         if (_bikeObjects.TryGetValue(name, out var go))
         {
            RecreateLocalPosition(go.transform, name, s1, s2, interpolationFactor);
         }
      }
   }

   
   private void RecreateLocalPosition(Transform target, string name, Snapshot s1, Snapshot s2, float interpolationFactor)
   {
      var pos1 = s1.LocationMap[name].Item1;
      var rot1 = s1.LocationMap[name].Item2;
      if (interpolationFactor < 0)
      {
         target.localPosition = pos1;
         target.localRotation = rot1;
      }
      else
      {
         var pos2 = s2.LocationMap[name].Item1;
         var rot2 = s2.LocationMap[name].Item2;
         target.localPosition = Vector3.Lerp(pos1, pos2, interpolationFactor);
         target.localRotation = Quaternion.Slerp(rot1, rot2, interpolationFactor);
      }
   }
}
