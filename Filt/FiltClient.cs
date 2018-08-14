using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Security;

namespace Filt
{
    /// <summary>
    /// The request data class of Filt Client.
    /// <summary>
    public class FiltRequest {
        public byte[] Target { get; set; }
        public Dictionary<string, string> Option { get; set; }

        public FiltRequest(byte[] target, Dictionary<string, string> option=null) {
            Target = target;

            if (option == null) {
                Option = new Dictionary<string, string>();
            } else{
                Option = option;
            }
        }
    }

    /// <summary>
    /// The json response class of Filt Client.
    /// <summary>
    [JsonObject]
    public class FiltJsonResponse {
        [JsonProperty("hit")]
        public bool Hit { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("messages")]
        public List<string> Messages { get; set; }

        public FiltResponse Deserialize() {
            /// <summary>
            /// Deserialize from FiltJsonResponse to FiltReponse.
            /// </summary>

            List<byte[]> messages = new List<byte[]>(); 

            foreach(var base64_message in Messages) {
                byte[] message = System.Convert.FromBase64String(base64_message);
                messages.Add(message);
            }
            
            return new FiltResponse(
                Hit,
                Success,
                messages
            );
        }
    }
    /// <summary>
    /// The response class of Filt Client.
    /// <summary>
    public class FiltResponse {
        public FiltResponse(bool hit, bool success, List<byte[]> messages)
        {
            Hit = hit;
            Success = success;
            Messages = messages;
        }

        public bool Hit { get; set; }
        public bool Success { get; set; }
        public List<byte[]> Messages { get; set; }
    }

    /// <summary>
    /// The client class of Filt.
    /// </summary>
    public class FiltClient
    {
        string Url;
        public FiltClient(string Url) {
            this.Url = Url;
        }

        /// <summary>
        /// Send data to Filt, and return result.
        /// </summary>
        public async Task<FiltResponse> Send(FiltRequest filt_request, bool verify=true) {
            // init handler that called checking certification
            var handler = new HttpClientHandler() {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    switch (sslPolicyErrors)
                    {
                        case SslPolicyErrors.None:
                        case SslPolicyErrors.RemoteCertificateNameMismatch:
                            return true;
                        default:
                            return !verify;
                    }
                }
            };
            
            // init request data
            Dictionary<string, string> dict_request;

            if (filt_request.Option == null) {
                dict_request = new Dictionary<string, string>();
            } else {
                dict_request = filt_request.Option;
            }

            // convert target to basa64
            string base64_target = Convert.ToBase64String(filt_request.Target);
            dict_request["target"] = base64_target;
            
            // convert dictionary to json string
            string json_request = JsonConvert.SerializeObject(dict_request);

            // send request
            var client = new HttpClient(handler);
            var content = new StringContent(json_request, Encoding.UTF8, "application/json");
        
            HttpResponseMessage response;
            try {
                response = await client.PostAsync(Url, content);
            } catch {
                 throw new Exception("Faild to communicate with Filt");
            }

            try {
                string body = await response.Content.ReadAsStringAsync();
                System.Console.WriteLine(body);
                FiltJsonResponse json_response = JsonConvert.DeserializeObject<FiltJsonResponse>(body);
                
                return json_response.Deserialize();

            } catch {
                throw new Exception("Faild to desirialize to FiltResponse");
            }
        }
    }
}
