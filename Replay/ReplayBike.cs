using UnityEngine;


namespace AlternativeCameraMod.Replay;

internal class ReplayBike
{
   private readonly GameObject _bike;
   private GameObject _playerBike;
   private readonly BikeReanimator _reanimator;


   private ReplayBike(GameObject bike)
   {
      _bike = bike;
      _reanimator = new BikeReanimator(_bike);
   }


   public BikeReanimator Reanimator
   {
      get { return _reanimator; }
   }

   
   public static ReplayBike Player()
   {
      var bike = GameObject.Find("Bike(Clone)");
      return new ReplayBike(bike);
   }


   public static ReplayBike Playback()
   {
      var bike = GameObject.Find("Bike(Clone)");
      var replayBike = GameObject.Instantiate(bike);
      bike.active = false;
      var rb = new ReplayBike(replayBike);
      rb._playerBike = bike;
      return rb;
   }

   
   public void ShowPlayerBike()
   {
      _playerBike.active = true;
   }


   public void Close()
   {
      if (_playerBike != null)
      {
         GameObject.Destroy(_bike);
         _playerBike.active = true;
      }
   }
}
