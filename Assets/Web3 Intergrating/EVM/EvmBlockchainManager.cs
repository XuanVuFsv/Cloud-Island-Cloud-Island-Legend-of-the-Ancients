using System;
using System.Threading.Tasks;
using Thirdweb;
using Thirdweb.Unity;
using UnityEngine;
using UnityEngine.Events;
using VitsehLand.Scripts.Pattern.Singleton;
using WalletConnectUnity.Modal;

namespace Web3Intergrating.Evm
{
    public class EvmBlockchainManager : Singleton<EvmBlockchainManager>
    {
        public UnityEvent<string> OnLoggedIn;
        public UnityEvent<string> OnModalClosed;

        public IThirdwebWallet Wallet { get; private set; }
        public string Address { get; private set; }

        private void Start()
        {
            WalletConnectModal.ModalClosed += (_, _) => OnModalClosed?.Invoke(null);
        }

        public async void Login(string authProvider)
        {
            WalletOptions connection = null;
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
                    chainId: 28122024
                );
            }

            Wallet = await ThirdwebManager.Instance.ConnectWallet(connection);
            Address = await Wallet.GetAddress();

            var balance = await Wallet.GetBalance(28122024);
            Debug.Log(balance);

            OnLoggedIn?.Invoke(Address);
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
    }
}