using System.Security.Cryptography;
using System.Text;
using System;

namespace LQV_BlockchainCertificate.Services
{
    /// <summary>
    /// Dịch vụ tiện ích để tính toán Hash SHA256 cho chuỗi dữ liệu.
    /// Hash này được sử dụng làm "dấu vân tay" để lưu trữ trên Blockchain.
    /// </summary>
    public class HashHelper
    {
        /// <summary>
        /// Tính toán Hash SHA256 từ chuỗi đầu vào và trả về chuỗi Hex Lowercase.
        /// </summary>
        /// <param name="input">Chuỗi dữ liệu cần băm (ví dụ: chuỗi kết hợp thông tin sinh viên).</param>
        /// <returns>Chuỗi SHA256 Hash (64 ký tự).</returns>
        public string CalculateSha256Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            try
            {
                // Khởi tạo thuật toán SHA256
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    // Băm dữ liệu
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                    // Chuyển mảng byte thành chuỗi hex
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        // Định dạng thành chuỗi hex hai ký tự (lowercase)
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                Console.WriteLine($"Lỗi khi tính SHA256 Hash: {ex.Message}");
                throw;
            }
        }
    }
}