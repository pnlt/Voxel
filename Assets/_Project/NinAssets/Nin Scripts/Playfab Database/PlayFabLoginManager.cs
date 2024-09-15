using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class PlayFabLoginManager : MonoBehaviour
    {
        #region RegisterUser

        [SerializeField] private TMP_InputField registerEmail;
        [SerializeField] private TMP_InputField registerUserName;
        [SerializeField] private TMP_InputField registerPassword;

        public void OnRegister()
        {
            RegisterUser(registerEmail.text, registerUserName.text, registerPassword.text);
        }

        private void RegisterUser(string email, string userName, string password)
        {
            var request = new RegisterPlayFabUserRequest()
            {
                Email = email,
                Username = userName,
                Password = password,
            };
            PlayFabClientAPI.RegisterPlayFabUser(
                request,
                onSuccess =>
                {
                    Debug.Log("Enter game");
                },
                onFailure =>
                {
                    PlayFabFailure(onFailure);
                });
            
        }

        private void PlayFabFailure(PlayFab.PlayFabError obj)
        {
            Debug.Log(obj.Error + " " + obj.GenerateErrorReport());
        }

        #endregion


        #region LoginUser

        [SerializeField] private TMP_InputField userName;
        [SerializeField] private TMP_InputField password;

        public void OnLogin()
        {
            
        }

        private void LoginUser(string userName, string password)
        {
            var request = new LoginWithPlayFabRequest()
            {
                Username = userName,
                Password = password,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
                {
                    GetPlayerProfile = true
                }

            };
            PlayFabClientAPI.LoginWithPlayFab(request,
                onSuccess =>
                {
                    Debug.Log("Enter game");
                },
                onFailure =>
                {
                    
                }
                );
        }
        #endregion

    }
}
