using UnityEngine.Events;

namespace InfimaGames.LowPolyShooterPack
{
    public static class UnityEventUtility
    {
        public static void AddListenerOnce(this UnityEvent unityEvent, UnityAction call)
        {
            unityEvent.RemoveListener(call);
            unityEvent.AddListener(call);
        }
        public static void AddListenerOnce<T>(this UnityEvent<T> unityEvent, UnityAction<T> call)
        {
            unityEvent.RemoveListener(call);
            unityEvent.AddListener(call);
        }
    }
}