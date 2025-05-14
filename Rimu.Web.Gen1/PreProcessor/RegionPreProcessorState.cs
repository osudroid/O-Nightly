using System.Net;
using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Region.Adapter.Interface;

namespace Rimu.Web.Gen1.PreProcessor;

public sealed class RegionPreProcessorState {
    public Option<ICountry> Country { get; set; } = default;
    public Option<IPAddress> IPAddress { get; set; } = default;

    public RegionPreProcessorState() {
    }
}