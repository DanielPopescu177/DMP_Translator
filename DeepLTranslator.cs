using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DMPTranslator
{
    public class DeepLTranslator
    {
        private readonly string apiKey;
        private readonly HttpClient client;

        public DeepLTranslator(string apiKey)
        {
            this.apiKey = apiKey;
            this.client = new HttpClient();
            this.client.DefaultRequestHeaders.Add("Authorization", $"DeepL-Auth-Key {apiKey}");
        }

        public async Task<string> TranslateAsync(string text, string sourceLang = "JA", string targetLang = "KO")
        {
            try
            {
                var content = new StringContent(
                    $"text={Uri.EscapeDataString(text)}&source_lang={sourceLang}&target_lang={targetLang}",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded"
                );

                var response = await client.PostAsync("https://api-free.deepl.com/v2/translate", content);
                var result = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(result);
                var translations = json["translations"] as JArray;
                
                if (translations != null && translations.Count > 0)
                {
                    return translations[0]["text"]?.ToString() ?? text;
                }

                return text;
            }
            catch (Exception)
            {
                return text;
            }
        }
    }
}
