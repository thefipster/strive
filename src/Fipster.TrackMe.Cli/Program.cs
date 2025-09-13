using Fipster.TrackMe.Cli;
using Fipster.TrackMe.Cli.Components;

Console.WriteLine("Running Track Me Cli");
Console.WriteLine();

var archive = @"E:\polar\archive";
var storage = @"E:\polar\data";

var setup = new Setup();

// var sources = setup.GetSources(archive);
// var importer = new Importer(archive, sources);
// importer.Run();

var extractors = setup.GetExtractors();
var assimilator = new Assimilator(archive, storage, extractors);
assimilator.Run();
