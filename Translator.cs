using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using MelonLoader;
using Newtonsoft.Json.Linq;

namespace DMPTranslator
{
    public class PapagoTranslator
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly HttpClient httpClient;
        private const string API_URL = "https://naveropenapi.apigw.ntruss.com/nmt/v1/translation";

        public PapagoTranslator(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.httpClient = new HttpClient();
            
            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
            {
                httpClient.DefaultRequestHeaders.Add("X-NCP-APIGW-API-KEY-ID", clientId);
                httpClient.DefaultRequestHeaders.Add("X-NCP-APIGW-API-KEY", clientSecret);
            }
        }

        public async Task<string> TranslateAsync(string text, string source = "ja", string target = "ko")
        {
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                MelonLogger.Warning("Papago API 키가 설정되지 않았습니다!");
                return text;
            }

            try
            {
                var values = new Dictionary<string, string>
                {
                    { "source", source },
                    { "target", target },
                    { "text", text }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await httpClient.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(responseBody);
                    return json["message"]["result"]["translatedText"].ToString();
                }
                else
                {
                    MelonLogger.Error($"Papago API 오류: {response.StatusCode}");
                    string errorBody = await response.Content.ReadAsStringAsync();
                    MelonLogger.Error($"오류 내용: {errorBody}");
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"번역 예외: {e.Message}");
            }

            return text; // 실패시 원문 반환
        }
    }

    // Google Translate (무료 대안)
    public class GoogleTranslator
    {
        private readonly HttpClient httpClient;

        public GoogleTranslator()
        {
            this.httpClient = new HttpClient();
        }

        public async Task<string> TranslateAsync(string text, string from = "ja", string to = "ko")
        {
            try
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={from}&tl={to}&dt=t&q={Uri.EscapeDataString(text)}";
                
                string response = await httpClient.GetStringAsync(url);
                var json = JArray.Parse(response);
                
                if (json.Count > 0 && json[0] is JArray translations)
                {
                    string result = "";
                    foreach (var item in translations)
                    {
                        if (item is JArray pair && pair.Count > 0)
                        {
                            result += pair[0].ToString();
                        }
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Google Translate 오류: {e.Message}");
            }

            return text;
        }
    }
}
