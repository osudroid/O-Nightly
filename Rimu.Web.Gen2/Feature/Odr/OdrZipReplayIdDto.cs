using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Rimu.Web.Gen2.Feature.Odr;

public sealed class OdrZipReplayIdDto {
    [Microsoft.AspNetCore.Mvc.FromQuery(Name = "replayId")]
    public long ReplayId { get; set; }
        
    public sealed class OdrZipRReplayIdValidator : Validator<OdrZipReplayIdDto> {
        public OdrZipRReplayIdValidator() {
            RuleFor(x => x.ReplayId)
                .GreaterThanOrEqualTo(0)
                ;
        }
    }
}