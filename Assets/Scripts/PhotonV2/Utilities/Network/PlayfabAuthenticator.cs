using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.Assertions.Must;

public class PlayfabAuthenticator : MonoBehaviour
{

    //panel list
    [SerializeField]
    private GameObject LoginPanel;
    [SerializeField]
    private GameObject RegistrationPanel;
    [SerializeField]
    private GameObject FrontPanel;
    [SerializeField]
    private GameObject ConnectingPanel;
    [SerializeField]
    private GameObject AccountCreatedPanel;

    //input fields
    //login
    public InputField Login_User;
    public InputField Login_Pass;
    //registration
    public InputField Register_User;
    public InputField Register_Pass;
    public InputField Register_Email;

    private string PlayerIDCache = "";

    public void OnClick_LoginPanel()
    {
        FrontPanel.SetActive(false);
        RegistrationPanel.SetActive(false);
        LoginPanel.SetActive(true);
    }

    public void OnClick_RegistrationPanel()
    {
        FrontPanel.SetActive(false);
        RegistrationPanel.SetActive(true);
    }

    public void OnClick_BackButton()
    {
        FrontPanel.SetActive(true);
        LoginPanel.SetActive(false);
        RegistrationPanel.SetActive(false);
    }

    //Run the entire thing on awake
    public void Awake()
    {
        //AuthenticateWithPlayFab();
    }

    public void OnClick_AuthenticateWithPlayFabLogin()
    {
        Debug.Log("PlayFab authenticating using custom ID");

        //LoginWithCustomIDRequest request = new LoginWithCustomIDRequest();
        //request.CreateAccount = true;
        //request.CustomId = PlayFabSettings.DeviceUniqueIdentifier;

        //PlayFabClientAPI.LoginWithCustomID(request, RequestToken, OnError);

        LoginWithPlayFabRequest request = new LoginWithPlayFabRequest();
        request.Username = Login_User.text;
        request.Password = Login_Pass.text;

        PlayFabClientAPI.LoginWithPlayFab(request, RequestToken, OnError);
        LoginPanel.SetActive(false);
        ConnectingPanel.SetActive(true);
    }

    public void OnClick_PlayFabRegister()
    {
        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest();
        request.Username = Register_User.text;
        request.Password = Register_Pass.text;
        request.Email = Register_Email.text;
        PlayFabClientAPI.RegisterPlayFabUser(request, result => { Debug.Log("Account Made!"); }, OnError);

        AccountCreatedPanel.SetActive(true);
        GameObject.Find("RegisterUsername").SetActive(false);
        GameObject.Find("RegisterPassword").SetActive(false);
        GameObject.Find("RegisterEmail").SetActive(false);
        GameObject.Find("RegistrationButton").SetActive(false);
        GameObject.Find("RegisterBackButton").SetActive(false);

        //LoginWithPlayFabRequest LoginRequest = new LoginWithPlayFabRequest();
        //LoginRequest.Username = Register_User.text;
        //LoginRequest.Password = Register_Pass.text;

        //PlayFabClientAPI.LoginWithPlayFab(LoginRequest, RequestToken, OnError);

        //RegistrationPanel.SetActive(false);
        //ConnectingPanel.SetActive(true);
    }

    void RequestToken(LoginResult result)
    {
        Debug.Log("PlayFab authenticated. Requesting authentication token....");

        PlayerIDCache = result.PlayFabId;

        GetPhotonAuthenticationTokenRequest request = new GetPhotonAuthenticationTokenRequest();
        request.PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
        PlayFabClientAPI.GetPhotonAuthenticationToken(request, AuthenticateWithPhoton ,OnError);

    }

    void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult result)
    {
        Debug.Log("Photon token acquired: " + result.PhotonCustomAuthenticationToken + " Authentication complete.");

        var CustomAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };

        CustomAuth.AddAuthParameter("username", PlayerIDCache);
        CustomAuth.AddAuthParameter("token", result.PhotonCustomAuthenticationToken);
        PhotonNetwork.AuthValues = CustomAuth;

        TestConnect.ConnectToPhoton();
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError($"[ERROR] | {error.GenerateErrorReport()}");
    }
}
