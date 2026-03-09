using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace LQV_BlockchainCertificate.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GeminiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Gemini:ApiKey"] ?? "";

            Console.WriteLine("===== GEMINI SERVICE INIT =====");
            Console.WriteLine($"API KEY LENGTH: {_apiKey.Length}");
            Console.WriteLine($"API KEY EMPTY?: {string.IsNullOrEmpty(_apiKey)}");
            Console.WriteLine("==============================");
        }
        // ======================================================
        // 🔥 PHÂN TÍCH GIAN LẬN THI BẰNG GEMINI
        // ======================================================
        public async Task<GeminiProctorResult> AnalyzeImageAsync(string base64Image, string prompt)
        {
            try
            {
                var model = "gemini-2.5-flash";
                var url =
                    $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

                var body = new
                {
                    contents = new[]
                    {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = prompt },
                        new
                        {
                            inlineData = new
                            {
                                mimeType = "image/jpeg",
                                data = base64Image.Replace("data:image/jpeg;base64,", "")
                            }
                        }
                    }
                }
            }
                };

                var json = JsonSerializer.Serialize(body);

                var response = await _http.PostAsync(
                    url,
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new GeminiProctorResult
                    {
                        Cheating = false,
                        Confidence = 0,
                        Reason = "Gemini API error"
                    };
                }

                using var doc = JsonDocument.Parse(raw);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrEmpty(text))
                {
                    return new GeminiProctorResult
                    {
                        Cheating = false,
                        Confidence = 0,
                        Reason = "Empty AI response"
                    };
                }

                // Gemini trả JSON text → ta parse lại
                try
                {
                    var aiResult = JsonSerializer.Deserialize<GeminiProctorResult>(text);
                    return aiResult ?? new GeminiProctorResult();
                }
                catch
                {
                    return new GeminiProctorResult
                    {
                        Cheating = false,
                        Confidence = 0,
                        Reason = text
                    };
                }
            }
            catch (Exception ex)
            {
                return new GeminiProctorResult
                {
                    Cheating = false,
                    Confidence = 0,
                    Reason = ex.Message
                };
            }
        }
        // ======================================================
        // 🔥 VERIFY KHUÔN MẶT BẰNG GEMINI VISION
        // ======================================================
        public async Task<bool> VerifyFaceAsync(byte[] imageBytes)
        {
            Console.WriteLine("===== GEMINI FACE VERIFY START =====");

            try
            {
                var base64 = Convert.ToBase64String(imageBytes);

                var model = "gemini-2.5-flash";
                var url =
                    $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

                var body = new
                {
                    contents = new[]
                    {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = "Ảnh này có khuôn mặt người đang nhìn vào camera để điểm danh không? Trả lời YES hoặc NO." },
                        new
                        {
                            inlineData = new
                            {
                                mimeType = "image/jpeg",
                                data = base64
                            }
                        }
                    }
                }
            }
                };

                var json = JsonSerializer.Serialize(body);

                Console.WriteLine("Sending image to Gemini...");
                Console.WriteLine($"Image size: {imageBytes.Length} bytes");

                var response = await _http.PostAsync(
                    url,
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                var raw = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine("Raw:");
                Console.WriteLine(raw);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("❌ Gemini API lỗi");
                    return false;
                }

                using var doc = JsonDocument.Parse(raw);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                Console.WriteLine($"Gemini reply: {text}");

                if (text == null) return false;

                bool ok = text?.ToLower().Contains("yes") == true;


                Console.WriteLine($"FACE RESULT: {ok}");

                Console.WriteLine("===== GEMINI FACE VERIFY END =====");

                return ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ FACE VERIFY ERROR:");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<string?> AskAsync(string prompt)
        {
            Console.WriteLine("===== GEMINI ASK START =====");
            Console.WriteLine("Prompt:");
            Console.WriteLine(prompt);

            // ✅ COMBO CHUẨN – KHÔNG 404
            var model = "gemini-2.5-flash"; // default
            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";


            Console.WriteLine("Request URL:");
            Console.WriteLine(url);

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);
            Console.WriteLine("Request JSON:");
            Console.WriteLine(json);

            HttpResponseMessage response;

            try
            {
                response = await _http.PostAsync(
                    url,
                    new StringContent(json, Encoding.UTF8, "application/json")
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ HTTP EXCEPTION:");
                Console.WriteLine(ex.Message);
                return "❌ Không kết nối được tới Gemini API";
            }

            Console.WriteLine($"Status Code: {response.StatusCode}");

            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Raw Response:");
            Console.WriteLine(raw);

            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 429)
                {
                    return "🤖 Trợ lý AI đang quá tải (hết lượt miễn phí). Vui lòng thử lại sau.";
                }

                return "❌ Hệ thống AI đang gặp sự cố, vui lòng thử lại sau.";
            }


            try
            {
                using var doc = JsonDocument.Parse(raw);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                Console.WriteLine("===== GEMINI ASK END =====");
                Console.WriteLine($"AI Reply: {text}");

                return text;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ PARSE JSON ERROR:");
                Console.WriteLine(ex.Message);
                return "❌ Lỗi xử lý dữ liệu phản hồi từ AI";
            }
        }
    }

    public class GeminiProctorResult
    {
        public bool Cheating { get; set; }
        public double Confidence { get; set; }
        public string Reason { get; set; } = "";
    }
}
