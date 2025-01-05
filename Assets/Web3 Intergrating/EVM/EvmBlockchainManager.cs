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
using VitsehLand.Scripts.Pattern.Singleton;
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

        private void Start()
        {
            WalletConnectModal.ModalClosed += (_, _) => OnModalClosed?.Invoke();
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
                "0x9d9a1f4c1a685857a5666db45588aa3d5643af9f",
                421614,
                "[{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"name\":\"ScoreAdded\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"getRank\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"rank\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"name\":\"submitScore\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]"
            );
            await contract.Write(Wallet, "submitScore", 0, (int)distanceTravelled);
        }

        internal async Task<int> GetRank()
        {
            var contract = await ThirdwebManager.Instance.GetContract(
                "0x9d9a1f4c1a685857a5666db45588aa3d5643af9f",
                421614,
                "[{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"name\":\"ScoreAdded\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"getRank\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"rank\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"score\",\"type\":\"uint256\"}],\"name\":\"submitScore\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]"
            );
            var rank = await contract.Read<int>("getRank", Address);
            Debug.Log($"Rank for address {Address} is {rank}");
            return rank;
        }
    
        public void SwitchGroupUI()
        {
            Group_NFT.SetActive(!Group_NFT.activeSelf);
            Group_Main.SetActive(!Group_Main.activeSelf);
        }
    }
}