using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Models;
using Fipster.TrackMe.Importer.Modules;
using Microsoft.AspNetCore.Mvc;

namespace Fipster.TrackMe.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExtractController : ControllerBase
    {
        private readonly ILogger<ExtractController> logger;
        private readonly DataIndexer indexer;

        public ExtractController(ILogger<ExtractController> logger, DataIndexer indexer)
        {
            this.logger = logger;
            this.indexer = indexer;
        }

        [HttpGet(Name = "GetOverview")]
        public Dictionary<DateTime, int> GetOverview(DateTime date)
        {
            var minDate = new DateTime(date.Year, date.Month, 1).AddDays(-7);
            var maxDate = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(6);

            var files = indexer.Index.Where(x => x.Date >= minDate && x.Date <= maxDate);
            var byDay = files.GroupBy(x => x.Date.Date);

            return byDay
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key,
                    y =>
                        (y.Any(z => z.Range == DateRanges.Day) ? 1 : 0)
                        + (y.Any(z => z.Range == DateRanges.Time) ? 2 : 0)
                );
        }

        [HttpGet("sport", Name = "GetSport")]
        public IEnumerable<FileExtraction> GetSport(DateTime date) =>
            getByDay(date, DateRanges.Time);

        [HttpGet("day", Name = "GetDay")]
        public IEnumerable<FileExtraction> GetDay(DateTime date) => getByDay(date, DateRanges.Day);

        private IEnumerable<FileExtraction> getByDay(DateTime date, DateRanges range) =>
            indexer
                .Index.Where(x => x.Range == range && x.Date.Date == date.Date)
                .Select(x => FileExtraction.FromFile(x.Filepath));
    }
}
