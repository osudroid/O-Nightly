using System.Reflection;
using System.Text;
using JsonApiSerializer;
using JsonApiSerializer.JsonApi;
using Newtonsoft.Json;

namespace Patreon.NET; 

public sealed class PatreonClient : IDisposable {
    // ReSharper disable once InconsistentNaming
    private const string SAFE_ROOT = "https://www.patreon.com/api/oauth2/v2/";

    // ReSharper disable once InconsistentNaming
    private const string PUBLIC_ROOT = "https://www.patreon.com/api/";

    private readonly HttpClient HttpClient;


    public PatreonClient(string accessToken) {
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
    }

    public void Dispose() {
        HttpClient.Dispose();
    }

    private static string CampaignURL(string campaignId) {
        return SAFE_ROOT + $"campaigns/{campaignId}";
    }

    private static string PledgesURL(string campaignId) {
        return CampaignURL(campaignId) + "/pledges";
    }

    private static string MembersURL(string campaignId) {
        return CampaignURL(campaignId) + "/members";
    }

    private static string MemberURL(string memberId) {
        return SAFE_ROOT + $"members/{memberId}";
    }

    private static string UserURL(string userId) {
        return PUBLIC_ROOT + "user/" + userId;
    }

    private static string GenerateFieldsAndIncludes(Type includes, params Type[] fields) {
        var str = new StringBuilder();

        foreach (var field in fields) {
            GenerateFields(field, str);
            str.Append("&");
        }

        GenerateIncludes(includes, str);

        return str.ToString();
    }

    private static void GenerateFields(Type type, StringBuilder str) {
        str.Append("fields%5B");

        var name = type.Name.Replace("Attributes", "");

        for (var i = 0; i < name.Length; i++) {
            var ch = name[i];

            if (char.IsUpper(ch) && i != 0)
                str.Append("_");

            str.Append(char.ToLower(ch));
        }

        str.Append("%5D=");

        GenerateFieldList(type, str);
    }

    private static void GenerateIncludes(Type type, StringBuilder str) {
        str.Append("include=");

        GenerateFieldList(type, str);
    }

    private static void GenerateFieldList(Type type, StringBuilder str) {
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            var attributes = property.GetCustomAttributes(typeof(JsonPropertyAttribute), true);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (attributes is null || attributes.Length == 0)
                continue;

            foreach (var attribute in attributes) {
                str.Append(((JsonPropertyAttribute)attribute).PropertyName);
                str.Append(",");
            }
        }

        // remove the last comma
        str.Length -= 1;
    }

    private static string AppendQuery(string url, string query) {
        if (url.Contains("?"))
            url += "&" + query;
        else
            url += "?" + query;

        return url;
    }

    // ReSharper disable once InconsistentNaming
    private async Task<HttpResponseMessage> GET(string url) {
        return await HttpClient.GetAsync(url);
    }

    // ReSharper disable once InconsistentNaming
    public async Task<T?> GET<T>(string url)
        where T : class {
        var response = await GET(url);

        if (!response.IsSuccessStatusCode) return null;
        try {
            var json = await response.Content.ReadAsStringAsync();
            var settings = new JsonApiSerializerSettings();
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
        catch (Exception ex) {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
        }

        return null;
    }

    public async Task<Campaign?> GetCampaign(string campaignId) {
        var url = CampaignURL(campaignId);

        url = AppendQuery(url, GenerateFieldsAndIncludes(typeof(CampaignRelationships),
            typeof(CampaignAttributes), typeof(UserAttributes), typeof(TierAttributes)));

        var document = await GET<DocumentRoot<Campaign>>(url).ConfigureAwait(false);

        return document?.Data;
    }

    public async Task<List<Tier>> GetCampaignTiers(string campaignId) {
        var campaign = await GetCampaign(campaignId).ConfigureAwait(false);

        if (campaign is null)
            return new List<Tier>(0);
        return campaign!.Relationships!.Tiers!;
    }

    public async Task<List<Member>> GetCampaignMembers(string campaignId) {
        var list = new List<Member>();

        var next = MembersURL(campaignId);

        do {
            var url = next;

            url = AppendQuery(url, GenerateFieldsAndIncludes(typeof(MemberRelationships),
                typeof(MemberAttributes), typeof(UserAttributes)));

            var document = await GET<DocumentRoot<Member[]>>(url).ConfigureAwait(false);

            if (document is null)
                continue;

            list.AddRange(document.Data);

            if (document.Links != null && document.Links.ContainsKey("next"))
                next = document.Links["next"].Href;
            else
                next = null;
        } while (next != null);

        return list;
    }

    public async Task<User?> GetUser(string id) {
        return (await GET<UserData>(UserURL(id)).ConfigureAwait(false))?.User;
    }
}