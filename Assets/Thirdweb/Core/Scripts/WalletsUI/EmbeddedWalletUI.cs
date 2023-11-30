using UnityEngine;
using Thirdweb.EWS;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using Thirdweb.Redcode.Awaiting;
using Thirdweb.Browser;
using System.Web;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Thirdweb.Wallets
{
    public class EmbeddedWalletUI : MonoBehaviour
    {
        #region Variables

        public GameObject EmbeddedWalletCanvas;
        public TMP_InputField OTPInput;
        public TMP_InputField RecoveryInput;
        public Button SubmitButton;
        public GameObject RecoveryCodesCanvas;
        public TMP_Text RecoveryCodesText;
        public Button RecoveryCodesCopy;

        private EmbeddedWallet _embeddedWallet;
        private string _email;
        private User _user;
        private Exception _exception;
        private string _callbackUrl;
        private string _customScheme;

        #endregion

        #region Initialization

        public static EmbeddedWalletUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }
        }

        #endregion

        #region Connection Flow

        public async Task<User> Connect(EmbeddedWallet embeddedWallet, string email, AuthOptions authOptions)
        {
            var config = Resources.Load<ThirdwebConfig>("ThirdwebConfig");
            _customScheme = config != null ? config.customScheme : null;
            if (!string.IsNullOrEmpty(_customScheme))
                _customScheme = _customScheme.EndsWith("://") ? _customScheme : $"{_customScheme}://";
            _embeddedWallet = embeddedWallet;
            _email = email;
            _user = null;
            _exception = null;
            OTPInput.text = "";
            RecoveryInput.text = "";
            RecoveryInput.gameObject.SetActive(false);
            SubmitButton.onClick.RemoveAllListeners();
            RecoveryCodesCopy.onClick.RemoveAllListeners();
            EmbeddedWalletCanvas.SetActive(false);
            RecoveryCodesCanvas.SetActive(false);

            try
            {
                string authProvider = authOptions.authProvider switch
                {
                    AuthProvider.EmailOTP => "EmailOTP",
                    AuthProvider.Google => "Google",
                    AuthProvider.Apple => "Apple",
                    AuthProvider.Facebook => "Facebook",
                    AuthProvider.CustomAuth => "CustomAuth",
                    _ => throw new UnityException($"Unsupported auth provider: {authOptions.authProvider}"),
                };
                return await _embeddedWallet.GetUserAsync(_email, authProvider);
            }
            catch (Exception e)
            {
                ThirdwebDebug.Log($"Could not recreate user automatically, proceeding with auth: {e.Message}");
            }

            try
            {
                switch (authOptions.authProvider)
                {
                    case AuthProvider.EmailOTP:
                        await LoginWithOTP();
                        break;
                    case AuthProvider.Google:
                        await LoginWithOauth("Google");
                        break;
                    case AuthProvider.Apple:
                        await LoginWithOauth("Apple");
                        break;
                    case AuthProvider.Facebook:
                        await LoginWithOauth("Facebook");
                        break;
                    case AuthProvider.CustomAuth:
                        await LoginWithCustomJwt(authOptions.authToken, authOptions.recoveryCode);
                        break;
                    default:
                        throw new UnityException($"Unsupported auth provider: {authOptions.authProvider}");
                }
            }
            catch (Exception e)
            {
                _exception = e;
            }

            await new WaitUntil(() => _user != null || _exception != null);
            EmbeddedWalletCanvas.SetActive(false);
            if (_exception != null)
                throw _exception;
            return _user;
        }

        public void Cancel()
        {
            _exception = new UnityException("User cancelled");
        }

        #endregion

        #region Email OTP Flow

        private async Task LoginWithOTP()
        {
            if (_email == null)
                throw new UnityException("Email is required for OTP login");

            SubmitButton.onClick.AddListener(OnSubmitOTP);
            await OnSendOTP();
            EmbeddedWalletCanvas.SetActive(true);
        }

        private async Task OnSendOTP()
        {
            try
            {
                (bool isNewUser, bool isNewDevice, bool needsRecoveryCode) = await _embeddedWallet.SendOtpEmailAsync(_email);
                if (needsRecoveryCode && !isNewUser && isNewDevice)
                    DisplayRecoveryInput(false);
                ThirdwebDebug.Log($"finished sending OTP:  isNewUser {isNewUser}, isNewDevice {isNewDevice}");
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        private async void OnSubmitOTP()
        {
            OTPInput.interactable = false;
            RecoveryInput.interactable = false;
            SubmitButton.interactable = false;
            try
            {
                string otp = OTPInput.text;
                var res = await _embeddedWallet.VerifyOtpAsync(_email, otp, string.IsNullOrEmpty(RecoveryInput.text) ? null : RecoveryInput.text);
                _user = res.User;
                ShowRecoveryCodes(res);
            }
            catch (Exception e)
            {
                _exception = e;
            }
            finally
            {
                OTPInput.interactable = true;
                RecoveryInput.interactable = true;
                SubmitButton.interactable = true;
            }
        }

        #endregion

        #region OAuth2 Flow

        private async Task LoginWithOauth(string authProviderStr)
        {
            if (Application.isMobilePlatform && string.IsNullOrEmpty(_customScheme))
                throw new UnityException("No custom scheme provided for mobile deeplinks, please set one in your ThirdwebConfig (found in ThirdwebManager)");

            string loginUrl = await GetLoginLink(authProviderStr);

            string redirectUrl = Application.isMobilePlatform ? _customScheme : "http://localhost:8789/";
            CrossPlatformBrowser browser = new();
            var browserResult = await browser.Login(loginUrl, redirectUrl);
            if (browserResult.status != BrowserStatus.Success)
                _exception = new UnityException($"Failed to login with {authProviderStr}: {browserResult.status} | {browserResult.error}");
            else
                _callbackUrl = browserResult.callbackUrl;

            await new WaitUntil(() => _callbackUrl != null);

            string decodedUrl = HttpUtility.UrlDecode(_callbackUrl);
            Uri uri = new(decodedUrl);
            string queryString = uri.Query;
            var queryDict = HttpUtility.ParseQueryString(queryString);
            string authResultJson = queryDict["authResult"];

            bool needsRecoveryCode = await _embeddedWallet.IsRecoveryCodeNeededAsync(authResultJson);

            if (needsRecoveryCode)
            {
                SubmitButton.onClick.AddListener(() => OnSubmitRecoveryOauth(authProviderStr, authResultJson));
                DisplayRecoveryInput(true);
            }
            else
            {
                try
                {
                    var res = await _embeddedWallet.SignInWithOauth(authProviderStr, authResultJson, null);
                    _user = res.User;
                }
                catch (Exception e)
                {
                    _exception = e;
                }
            }
        }

        private async void OnSubmitRecoveryOauth(string authProviderStr, string authResult)
        {
            try
            {
                string recoveryCode = RecoveryInput.text;
                var res = await _embeddedWallet.SignInWithOauth(authProviderStr, authResult, recoveryCode);
                _user = res.User;
                ShowRecoveryCodes(res);
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        private async Task<string> GetLoginLink(string authProvider)
        {
            string loginUrl = await _embeddedWallet.FetchHeadlessOauthLoginLinkAsync(authProvider);
            string platform = "unity";
            string redirectUrl = UnityWebRequest.EscapeURL(Application.isMobilePlatform ? _customScheme : "http://localhost:8789/");
            string developerClientId = UnityWebRequest.EscapeURL(ThirdwebManager.Instance.SDK.session.Options.clientId);
            return $"{loginUrl}?platform={platform}&redirectUrl={redirectUrl}&developerClientId={developerClientId}&authOption={authProvider}";
        }

        #endregion

        #region Custom JWT Flow

        private async Task LoginWithCustomJwt(string jwtToken, string recoveryCode)
        {
            var res = await _embeddedWallet.SignInWithJwtAuthAsync(jwtToken, recoveryCode);
            _user = res.User;
            ShowRecoveryCodes(res);
        }

        #endregion

        #region Common

        private void DisplayRecoveryInput(bool hideOtpInput)
        {
            if (hideOtpInput)
                OTPInput.gameObject.SetActive(false);
            RecoveryInput.gameObject.SetActive(true);
            EmbeddedWalletCanvas.SetActive(true);
        }

        private void ShowRecoveryCodes(EmbeddedWallet.VerifyResult res)
        {
            if (res.MainRecoveryCode != null && res.WasEmailed.HasValue && res.WasEmailed.Value == false)
            {
                List<string> recoveryCodes = new() { res.MainRecoveryCode };
                if (res.BackupRecoveryCodes != null)
                    recoveryCodes.AddRange(res.BackupRecoveryCodes);
                string recoveryCodesString = string.Join("\n", recoveryCodes.Select((code, i) => $"{i + 1}. {code}"));
                string message = $"Please save the following recovery codes in a safe place:\n\n{recoveryCodesString}";
                ThirdwebDebug.Log(message);
                RecoveryCodesText.text = message;
                string messageToSave = JsonConvert.SerializeObject(recoveryCodes);
                RecoveryCodesCopy.onClick.AddListener(() => GUIUtility.systemCopyBuffer = messageToSave);
                RecoveryCodesCanvas.SetActive(true);
            }
        }

        #endregion
    }
}
