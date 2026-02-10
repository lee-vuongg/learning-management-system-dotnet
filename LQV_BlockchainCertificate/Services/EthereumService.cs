using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace LQV_BlockchainCertificate.Services
{
    // DTO đọc dữ liệu
    public class CertificateDto
    {
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public string IssueDate { get; set; }
        public string CertHash { get; set; }
        public string IssuedByAddress { get; set; }
    }

    public class EthereumService
    {
        // ===============================
        // CONFIG (SEPOLIA)
        // ===============================
        private const string RpcUrl =
            "https://eth-sepolia.g.alchemy.com/v2/RzNVVCRNVkqLfmdSjrDIx";

        private const string PrivateKey =
            "0x5bf27f472e15a1ec400f786d8dfab019684b574451c5a7df1b86000e63fc36a3";

        private const string ContractAddress =
            "0xA63516F44B69286397745B937d280B6b8976ae3d";

        // ===============================
        // ABI
        // ===============================
        private const string Abi = @"[
            {
                ""inputs"": [
                    {""internalType"": ""string"", ""name"": ""_studentName"", ""type"": ""string""},
                    {""internalType"": ""string"", ""name"": ""_courseName"", ""type"": ""string""},
                    {""internalType"": ""string"", ""name"": ""_issueDate"", ""type"": ""string""},
                    {""internalType"": ""string"", ""name"": ""_certHash"", ""type"": ""string""}
                ],
                ""name"": ""issueCertificate"",
                ""outputs"": [],
                ""stateMutability"": ""nonpayable"",
                ""type"": ""function""
            },
            {
                ""inputs"": [
                    {""internalType"": ""uint256"", ""name"": ""_id"", ""type"": ""uint256""}
                ],
                ""name"": ""getCertificate"",
                ""outputs"": [
                    {""internalType"": ""string"", ""name"": """", ""type"": ""string""},
                    {""internalType"": ""string"", ""name"": """", ""type"": ""string""},
                    {""internalType"": ""string"", ""name"": """", ""type"": ""string""},
                    {""internalType"": ""string"", ""name"": """", ""type"": ""string""},
                    {""internalType"": ""address"", ""name"": """", ""type"": ""address""}
                ],
                ""stateMutability"": ""view"",
                ""type"": ""function""
            }
        ]";

        // ===============================
        // NETHEREUM OBJECTS
        // ===============================
        private readonly Account _account;
        private readonly Web3 _web3;
        private readonly Contract _contract;

        public EthereumService()
        {
            _account = new Account(PrivateKey, 11155111); // Sepolia chainId
            _web3 = new Web3(_account, RpcUrl);
            _contract = _web3.Eth.GetContract(Abi, ContractAddress);
        }

        // ===============================
        // GHI CHỨNG NHẬN
        // ===============================
        public async Task<string> IssueCertificateAsync(
            string studentName,
            string courseName,
            string issueDate,
            string certHash)
        {
            var function = _contract.GetFunction("issueCertificate");

            // Ước lượng gas
            var gasEstimate = await function.EstimateGasAsync(
                _account.Address,
                null,
                null,
                studentName,
                courseName,
                issueDate,
                certHash
            );

            var gasLimit = new HexBigInteger(gasEstimate.Value * 120 / 100);

            var receipt = await function.SendTransactionAndWaitForReceiptAsync(
                _account.Address,
                gasLimit,
                null,
                null,
                studentName,
                courseName,
                issueDate,
                certHash
            );

            if (receipt.Status.Value == 1)
                return receipt.TransactionHash;

            throw new Exception("Blockchain transaction failed.");
        }

        // ===============================
        // CHỜ RECEIPT (Controller dùng)
        // ===============================
        public async Task<TransactionReceipt> WaitForReceiptAsync(string txHash)
        {
            TransactionReceipt receipt = null;

            while (receipt == null)
            {
                receipt = await _web3.Eth.Transactions
                    .GetTransactionReceipt
                    .SendRequestAsync(txHash);

                await Task.Delay(3000);
            }

            return receipt;
        }

        // ===============================
        // ĐỌC CHỨNG NHẬN
        // ===============================
        public async Task<CertificateDto> GetCertificateAsync(BigInteger certificateId)
        {
            var function = _contract.GetFunction("getCertificate");

            var result = await function.CallAsync<object[]>(certificateId);

            if (result == null || result.Length < 5)
                return null;

            return new CertificateDto
            {
                StudentName = result[0]?.ToString(),
                CourseName = result[1]?.ToString(),
                IssueDate = result[2]?.ToString(),
                CertHash = result[3]?.ToString(),
                IssuedByAddress = result[4]?.ToString()
            };
        }
    }
}
