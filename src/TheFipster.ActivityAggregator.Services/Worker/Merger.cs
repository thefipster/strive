using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Services.Worker;

public class Merger(IOptions<ApiConfig> config) { }
