using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using NLog;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Repository.Pp.Domain;

public class PpCalculatorProvider {
    private const string Path = "/calculate-replay";
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static PpCalculatorProvider? _provider;
    private static readonly object Lock = new();
    private readonly HttpClient _client;
    private readonly IEnvDb _settingsDb;

    private PpCalculatorProvider(IEnvProvider envProvider) {
        _settingsDb = envProvider.EnvDb;

        var http = new HttpClient(new HttpClientHandler { MaxConnectionsPerServer = 10 });
        http.BaseAddress = new Uri(_settingsDb.Pp_URL + Path);
        _client = http;
    }

    public static PpCalculatorProvider Self {
        get {
            lock (Lock) {
                var envProvider = Rimu.Repository.Dependency.Adapter.Injection.GlobalServiceProvider.GetEnvProvider();
                _provider ??= new PpCalculatorProvider(envProvider);
                return _provider;
            }
        }
    }

    public async Task<SResult<Option<double>>> CalculateReplayAsync(byte[] replayFileBytes, string filename) {
        try {
            var file = new ByteArrayContent(replayFileBytes);
            file.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(file, "replayfile", filename);
            using var response = await _client.PostAsync(_client.BaseAddress, multipartContent);
            if (response.StatusCode != HttpStatusCode.OK)
                return SResult<Option<double>>.Err($"Failed to calculate replay Status Code {response.StatusCode}");

            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var jObjectPp = jObject["pp"];
            if (jObjectPp is null or { Type: JTokenType.Null }) {
                Logger.Error("jObject[\"pp\"] is null");
                return SResult<Option<double>>.Err("Failed to parse jObject[\"pp\"]");
            }

            if (jObjectPp.Type != JTokenType.Float) {
                Logger.Error("jObject[\"pp\"] is not type of Float");
                return SResult<Option<double>>.Err("Failed to parse jObject[\"pp\"]");
            }
            
            float pp = jObjectPp.ToObject<float>();
            return SResult<Option<double>>.Ok(Option<double>.With(pp));
        }
        catch (Exception e) {
            Console.WriteLine(e);
            Logger.Error(e, "Failed to calculate replay file");
            return SResult<Option<double>>.Err(e);
        }
    }
}