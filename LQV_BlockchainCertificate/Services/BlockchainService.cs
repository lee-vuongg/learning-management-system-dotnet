using System;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace LQV_BlockchainCertificate.Services
{
    public class BlockchainService
    {
        private readonly Web3 _web3;
        private readonly string _fromAddress;

        public BlockchainService(string rpcUrl, string fromAddress, string privateKey)
        {
            _web3 = new Web3(new Nethereum.Web3.Accounts.Account(privateKey), rpcUrl);
            _fromAddress = fromAddress;
        }

        public async Task<(string TxHash, long BlockNumber, string EtherscanUrl)> SendTransactionAsync(string data)
        {
            try
            {
                var txInput = new TransactionInput
                {
                    From = _fromAddress,
                    To = _fromAddress,
                    Gas = new HexBigInteger(21000),
                    GasPrice = new HexBigInteger(Web3.Convert.ToWei(5, Nethereum.Util.UnitConversion.EthUnit.Gwei)),
                    Value = new HexBigInteger(0),
                    Data = "0x" + Encoding.UTF8.GetBytes(data).ToHex()
                };

                var txHash = await _web3.Eth.Transactions.SendTransaction.SendRequestAsync(txInput);
                TransactionReceipt receipt = null;

                while (receipt == null)
                {
                    receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
                    await Task.Delay(3000);
                }

                long blockNumber = (long)receipt.BlockNumber.Value;
                string etherscanUrl = $"https://sepolia.etherscan.io/tx/{txHash}";

                return (txHash, blockNumber, etherscanUrl);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi gửi giao dịch blockchain: {ex.Message}");
            }
        }
    }

    public static class HexHelper
    {
        public static string ToHex(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
