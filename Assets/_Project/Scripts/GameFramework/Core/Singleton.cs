using Unity.Netcode;
using UnityEngine;
namespace InfimaGames.LowPolyShooterPack.Assets_ăn_trộm._External_Assets.Infima_Games.Low_Poly_Shooter_Pack.Code.GameFramework.Core
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
       private static T _instance;
       
       public static T Instance
       {
           get
           {
               if (_instance == null)
               {
                   T[] objs = FindObjectsOfType<T>();
                   if (objs.Length > 0)
                   {
                       T instance = objs[0];
                       _instance = instance;
                   }
                   else
                   {
                       GameObject go = new GameObject();
                       go.name = typeof(T).Name;
                       _instance = go.AddComponent<T>();
                       DontDestroyOnLoad(go);
                   }
               }
               return _instance;
           }
       }
        
    }
}