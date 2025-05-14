using Microsoft.AspNetCore.Builder;

namespace Rimu.Web.Gen2.Interface;

public interface IEndpointBinder {
    public void Bind(WebApplication app);
}