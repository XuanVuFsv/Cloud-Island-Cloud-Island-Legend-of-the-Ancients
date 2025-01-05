using Cysharp.Threading.Tasks;
using Nethereum.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Thirdweb;
using Thirdweb.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using VitsehLand.Scripts;
using VitsehLand.Scripts.Manager;
using VitsehLand.Scripts.Pattern.Singleton;
using VitsehLand.Scripts.Player;
using VitsehLand.Scripts.TimeManager;
using WalletConnectUnity.Modal;

namespace Web3Intergrating.Evm
{
    public class EvmBlockchainManager : Singleton<EvmBlockchainManager>
    {
        public string CitizenCardContractAddress;
        public string baseTokenURI = "";
        public string playerAddress = "";

        public UnityEvent<string> OnLoggedIn;
        public UnityEvent OnModalClosed;

        public GameObject claimPanel;
        public GameObject playPanel;
        public Button claimCitizenCardButton;
        public TextMeshProUGUI claimCitizenCardButtonText;
        public TextMeshProUGUI balanceText;
        public RawImage nftImage;

        public bool hasCitizenCard = false;
        object citizenCardData;
        public string citizenCardID = "";
        public CitizenCard citizenCard;

        public IThirdwebWallet Wallet { get; private set; }
        public string Address { get; private set; }

        WalletOptions connection = null;

        [System.Serializable]
        public class CitizenCard
        {
            public string name;
            public string description;
            public string image;
            public Properties properties;

            [System.Serializable]
            public class Properties
            {
                public int number;
                public string name;
            }
        }

        public GameObject Group_NFT;
        public GameObject Group_Main;
        public GameObject Game_UI;
        public TextMeshProUGUI Score_Updated;

        public TextMeshProUGUI savedScoreText, rankText;

        public bool isEnableUI = false;

        private void Start()
        {
            WalletConnectModal.ModalClosed += (_, _) => OnModalClosed?.Invoke();

            if (isEnableUI)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                Game_UI.SetActive(isEnableUI);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ShowGameUI(!isEnableUI);
            }
        }

        public async void Login(string authProvider)
        {
            AuthProvider provider = AuthProvider.Google;

            switch (authProvider)
            {
                case "google":
                    provider = AuthProvider.Google;
                    break;
                case "apple":
                    provider = AuthProvider.Apple;
                    break;
                case "facebook":
                    provider = AuthProvider.Facebook;
                    break;
                case "walletconnect":
                    connection = new WalletOptions(
                        provider: WalletProvider.WalletConnectWallet,
                        chainId: 28122024
                    );
                    break;
            }

            if (connection == null)
            {
                connection = new WalletOptions(
                    provider: WalletProvider.InAppWallet,
                    chainId: 28122024,
                    inAppWalletOptions: new InAppWalletOptions(authprovider: provider),
                    smartWalletOptions: new SmartWalletOptions(sponsorGas: true)
                );
            }

            Wallet = await ThirdwebManager.Instance.ConnectWallet(connection);
            Address = await Wallet.GetAddress();
            playerAddress = Address;

            UpdateBalance();

            var citizenCardContract = await ThirdwebManager.Instance.GetContract(CitizenCardContractAddress, connection.ChainId);
            var cardBalance = await citizenCardContract.ERC721_BalanceOf(Address);
            Debug.Log(cardBalance);

            OnLoggedIn?.Invoke(Address);
            if (cardBalance > 0)
            {
                hasCitizenCard = true;
                UpdateCitizenCard(citizenCardContract);

            }
            else
            {
                Debug.Log("Don't have card");
                playPanel.SetActive(false);
                claimPanel.SetActive(true);
            }
        }

        public async void UpdateBalance()
        {
            var balance = await Wallet.GetBalance(connection.ChainId);
            balanceText.text = "Balance: " + (Thirdweb.Utils.ToEth(balance.ToString())) + " ETH";

            GemManager.Instance.SavedGemCount = await GetScore();
            savedScoreText.text = "Onchain Coin: " + GemManager.Instance.SavedGemCount.ToString();
            rankText.text = "Rank: " + (await GetRank()).ToString();
        }

        public async void ClaimCitizenCard()
        {
            claimCitizenCardButtonText.text = "Claiming...";
            claimCitizenCardButton.interactable = false;

            var citizenCardContract = await ThirdwebManager.Instance.GetContract(CitizenCardContractAddress, connection.ChainId);
            var result = await citizenCardContract.DropERC721_Claim(Wallet, Address, 1);

            UpdateBalance();
            UpdateCitizenCard(citizenCardContract);

            claimCitizenCardButtonText.text = "Claimed Citizen Card";
            claimPanel.SetActive(false);
            playPanel.SetActive(true);
        }

        public async void UpdateCitizenCard(ThirdwebContract contract)
        {
            var ids = await contract.ERC721A_TokensOfOwner(Address);
            citizenCardID = ids[0].ToString();
            var base64JsonURI = await contract.ERC721_TokenURI(ids[0]);
            string citizenCardJsonData = DecodeBase64(GetBase64FromDataUrl(base64JsonURI));

            citizenCard = JsonUtility.FromJson<CitizenCard>(citizenCardJsonData);
            string imageUrl = citizenCard.image.Replace("ipfs://", baseTokenURI); // Convert IPFS URL

            StartCoroutine(LoadImageFromIPFS(imageUrl));
        }

        string GetBase64FromDataUrl(string dataUrl)
        {
            // Split the string and get the part after "base64,"
            string[] parts = dataUrl.Split(new[] { "base64," }, StringSplitOptions.None);
            return parts.Length > 1 ? parts[1] : null; // Return the Base64 part
        }

        string DecodeBase64(string base64)
        {
            byte[] jsonBytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(jsonBytes);
        }

        IEnumerator LoadImageFromIPFS(string url)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                Texture image = DownloadHandlerTexture.GetContent(webRequest);
                nftImage.texture = image;
            }
        }

        internal async Task SubmitScore(float distanceTravelled)
        {
            Debug.Log($"Submitting score of {distanceTravelled} to blockchain for address {Address}");
            var contract = await ThirdwebManager.Instance.GetContract(
                "0x5073dc7e624aed14f5641b115a2a0ab5758d9e2a",
                connection.ChainId,
"[{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"name\":\"ScoreAddedd\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"name\":\"_scores\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"getRank\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"rank\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"getScore\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"name\":\"submitScore\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]"
            );
            var result = await contract.Write(Wallet, "submitScore", 0, (int)distanceTravelled);
            Debug.Log(result.Status);
        }

        internal async Task<int> GetRank()
        {
            var contract = await ThirdwebManager.Instance.GetContract(
                "0x5073dc7e624aed14f5641b115a2a0ab5758d9e2a",
                connection.ChainId,
"[{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"name\":\"ScoreAddedd\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"name\":\"_scores\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"getRank\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"rank\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"getScore\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"name\":\"submitScore\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]");
            var rank = await contract.Read<int>("getRank", Address);
            Debug.Log($"Rank for address {Address} is {rank}");
            return rank;
        }

        internal async Task<int> GetScore()
        {
            var contract = await ThirdwebManager.Instance.GetContract(
                "0x5073dc7e624aed14f5641b115a2a0ab5758d9e2a",
                connection.ChainId,
"[{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"name\":\"ScoreAddedd\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"name\":\"_scores\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"getRank\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"rank\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"getScore\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"name\":\"submitScore\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]"
            );
            var score = await contract.Read<int>("getScore", Address);
            Debug.Log($"Score for address {Address} is {score}");
            return score;
        }

        public void SwitchGroupUI()
        {
            Group_NFT.SetActive(!Group_NFT.activeSelf);
            Group_Main.SetActive(!Group_Main.activeSelf);
        }

        public void PlayGame()
        {
            isEnableUI = false;
            Game_UI.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GameTimeManager.Instance.UnPause();
        }

        public void ShowGameUI(bool enabled)
        {
            Game_UI.SetActive(enabled);

            if (enabled)
            {
                //Set up cursor
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                GameTimeManager.Instance.Pause();
            }
            else
            {
                //Set up cursor
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                GameTimeManager.Instance.UnPause();
            }
            isEnableUI = !isEnableUI;
        }

        public async void Quit()
        {
            Score_Updated.gameObject.SetActive(true);
            if (Address != null && Address != "" && GemManager.Instance.GemCount > 2500)
            {
                await SubmitScore((float)(GemManager.Instance.GemCount + GemManager.Instance.SavedGemCount - 2500));
                Score_Updated.text = "Update Score Onchain Successfully!!!";
            }
            await UniTask.Delay(1000);
            Application.Quit();
        }

        public async void Restart()
        {
            if (Address != null || Address != "")
            {
                await SubmitScore((float)(GemManager.Instance.GemCount + GemManager.Instance.SavedGemCount - 2500));
                UpdateBalance();
            }
        }
    }
}