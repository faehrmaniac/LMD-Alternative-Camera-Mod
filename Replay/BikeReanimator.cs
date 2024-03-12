using UnityEngine;


namespace AlternativeCameraMod.Replay;

internal class BikeReanimator
{
   private readonly List<GameObject> _bikeObjects;


   public BikeReanimator(GameObject bike)
   {
      _bikeObjects = new List<GameObject>();
      _bikeObjects.Add(bike);
      GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
      for (int i = 0; i < gameObjects.Length; i++)
      {
         // LMD 'BindJNT' objects represent the parts of the bike and the rider which all
         // need to be animated to reproduce the look and feel of a real ride
         if (gameObjects[i].name.EndsWith("BindJNT") && gameObjects[i].transform.GetRoot().Equals(bike.transform))
         {
            _bikeObjects.Add(gameObjects[i]);
         }
      }

      _bikeObjects.Sort((n, m) => String.Compare(n.name, m.name, StringComparison.OrdinalIgnoreCase));
   }


   public int GetLocationCount()
   {
      return _bikeObjects.Count;
   }


   public string GetLocationName(int i)
   {
      return _bikeObjects[i].name;
   }


   public IEnumerable<Tuple<string, Vector3, Quaternion>> GetLocations()
   {
      foreach (var go in _bikeObjects)
      {
         yield return new Tuple<string, Vector3, Quaternion>(go.name, go.transform.localPosition, go.transform.localRotation);
      }
   }


   public void ApplyState(Snapshot s1, Snapshot s2, float interpolationFactor)
   {
      // 0 = Camera
      int index = 1;
      foreach (var go in _bikeObjects)
      {
         RecreateLocalPosition(go.transform, index++, s1, s2, interpolationFactor);
      }
   }

   
   private void RecreateLocalPosition(Transform target, int index, Snapshot s1, Snapshot s2, float interpolationFactor)
   {
      var pos1 = s1.Locations[index].Item2;
      var rot1 = s1.Locations[index].Item3;
      if (interpolationFactor < 0)
      {
         target.localPosition = pos1;
         target.localRotation = rot1;
      }
      else
      {
         var pos2 = s2.Locations[index].Item2;
         var rot2 = s2.Locations[index].Item3;
         target.localPosition = Vector3.Lerp(pos1, pos2, interpolationFactor);
         target.localRotation = Quaternion.Slerp(rot1, rot2, interpolationFactor);
      }
   }
}
