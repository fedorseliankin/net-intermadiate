using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MessageQueue
{

    public class ElasticEmailClient
    {
        private readonly string apiKey;
        private readonly HttpClient httpClient;

        public ElasticEmailClient(string apiKey)
        {
            this.apiKey = apiKey;
            this.httpClient = new HttpClient();
        }

        public async Task<string> SendEmailAsync(string to, string subject, string body)
        {
            var formData = new Dictionary<string, string>
        {
            { "apikey", apiKey },
            { "subject", subject },
            { "from", "anohermail@gmail.com" },
            { "fromName", "Your Name" },
            { "to", to },
            { "bodyHtml", body },
            { "isTransactional", "true" }
        };

            var content = new FormUrlEncodedContent(formData);
            var response = await httpClient.PostAsync("https://api.elasticemail.com/v2/email/send", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to send email. Status code: {response.StatusCode}, Message: {errorMessage}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
