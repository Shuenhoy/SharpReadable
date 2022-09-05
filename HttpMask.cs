using System;

namespace SharpReadable
{
    class HttpMask : IMask
    {
        HttpClient client = new HttpClient();
        bool refine;
        bool fill;
        int contextSize;
        public HttpMask(string api, bool _refine = true, bool _fill = true, int _contextSize = 49)
        {
            refine = _refine;
            fill = _fill;
            contextSize = _contextSize;

            client.BaseAddress = new Uri(api);
        }

        public bool[] GetMask(string page)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("text", page);
            dict.Add("refine", refine ? "1" : "0");
            dict.Add("fill", fill ? "1" : "0");
            dict.Add("context_size", contextSize.ToString());
            var task = client.PostAsync("/",
                new FormUrlEncodedContent(dict));
            task.Wait();
            var response = task.Result;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Server returned " + response.StatusCode);
            }
            var mask = response.Content.ReadAsStringAsync().Result;
            return mask.Select(x => x == '1').ToArray();
        }
    }
}