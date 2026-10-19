using GCFoundation.Components.Models.Code;

namespace GCFoundation.Web.Models.Components
{
    /// <summary>
    /// Sample sources displayed by the code component documentation.
    /// </summary>
    public static class CodeDemoSamples
    {
        /// <summary>Sync group shared by the API request examples.</summary>
        public const string RequestSyncGroup = "request-examples";

        /// <summary>Gets a JSON response containing localized text.</summary>
        public static string ResponseJson => $$"""
            {
              "id": "3f2c9a1e-7b4d-4c1a-9a63-2f0d5e8b1c47",
              "title": "{{Resources.Components.Code_SampleRequestTitle}}",
              "status": "submitted",
              "pageCount": 12,
              "urgent": false
            }
            """;

        /// <summary>Gets examples that create a request.</summary>
        public static IReadOnlyList<CodeSample> CreateRequest =>
        [
            new() { Language = "csharp", Code = """
                using var client = new HttpClient { BaseAddress = new Uri("https://api.example.gc.ca/") };
                var response = await client.PostAsJsonAsync("v1/requests", new { title = "Access request" });
                response.EnsureSuccessStatusCode();
                """ },
            new() { Language = "javascript", Code = """
                const response = await fetch('https://api.example.gc.ca/v1/requests', {
                  method: 'POST',
                  headers: { 'Content-Type': 'application/json' },
                  body: JSON.stringify({ title: 'Access request' })
                });
                """ },
            new() { Language = "python", Code = """
                import requests

                response = requests.post(
                    "https://api.example.gc.ca/v1/requests",
                    json={"title": "Access request"},
                )
                response.raise_for_status()
                """ }
        ];

        /// <summary>Gets examples that read a request, including a Bash variant the create examples do not have.</summary>
        public static IReadOnlyList<CodeSample> ReadRequest =>
        [
            new() { Language = "csharp", Code = """
                using var client = new HttpClient { BaseAddress = new Uri("https://api.example.gc.ca/") };
                var request = await client.GetFromJsonAsync<JsonElement>($"v1/requests/{requestId}");
                """ },
            new() { Language = "javascript", Code = """
                const response = await fetch(`https://api.example.gc.ca/v1/requests/${requestId}`);
                const request = await response.json();
                """ },
            new() { Language = "python", Code = """
                response = requests.get(f"https://api.example.gc.ca/v1/requests/{request_id}")
                request = response.json()
                """ },
            new() { Language = "bash", Code = """
                curl --header "Authorization: Bearer YOUR_ACCESS_TOKEN" \
                  https://api.example.gc.ca/v1/requests/REQUEST_ID
                """ }
        ];

        /// <summary>Gets a plain text log with long lines.</summary>
        public static string Log => """
            2026-10-19T08:15:02Z INFO  Request 3f2c9a1e-7b4d-4c1a-9a63-2f0d5e8b1c47 received from https://api.example.gc.ca/v1/requests with correlation ID 7d9e2b40-1c4f-4e8a-b3a5-6f1d2c9e8a71
            2026-10-19T08:15:03Z WARN  Retry-After 30 returned by the downstream records service; the request will be retried automatically
            """;
    }
}
