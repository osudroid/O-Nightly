using System.Net;
using FastEndpoints;
using FluentValidation.Results;
using LamLibAllOver.ErrorHandling;
using NLog;
using Rimu.Repository.Authentication.Adapter.Dto;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Region.Adapter.Interface;

namespace Rimu.Web.Gen1.PreProcessor;

public sealed class RegionPreProcessor<T>: PreProcessor<T, RegionPreProcessorState> {
    private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IIPAdressInfoProvider _ipAddressInfoProvider;

    public RegionPreProcessor(IIPAdressInfoProvider ipAddressInfoProvider) {
        _ipAddressInfoProvider = ipAddressInfoProvider;
    }

    public override async Task PreProcessAsync(IPreProcessorContext<T> context, RegionPreProcessorState state, CancellationToken ct) {
        var response = context.HttpContext.Response;
        
        try {
            if (!context.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var ip) || ip.Count == 0) {
                context.ValidationFailures.Add(new ValidationFailure("RegionPreProcessor", "X-Forwarded-For is missing"));
                return;
            }

            var ipAddress = IPAddress.Parse(ip.First()!);
            if (ipAddress.Equals(IPAddress.Any) || ipAddress.Equals(IPAddress.None)) {
                context.ValidationFailures.Add(new ValidationFailure("RegionPreProcessor", "IP address is not valid"));
                return;
            }

            var countyFromIpResult = await _ipAddressInfoProvider.GetCountyFromIPAdressAsync(ipAddress);

            if (countyFromIpResult == EResult.Err) {
                await response.SendErrorsAsync(context.ValidationFailures, 500, cancellation: ct);
                return;
            }
            if (countyFromIpResult.Ok().IsNotSet()) {
                context.ValidationFailures.Add(new ValidationFailure("RegionPreProcessor", "IP address is not valid"));
                return;
            }

            state.IPAddress = Option<IPAddress>.With(ipAddress);
            state.Country = countyFromIpResult.Ok();
        }
        catch (Exception e) {
            Logger.Error(e);
            await response.SendErrorsAsync(context.ValidationFailures, 500, cancellation: ct);
        }
        
    }
}