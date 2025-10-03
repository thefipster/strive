namespace TheFipster.ActivityAggregator.Domain;

public static class Const
{
    public static class Hubs
    {
        public static class Ingester
        {
            public const string Url = "/hubs/ingest";

            public const string UnzipFinished = "OnUnzipFinished";
            public const string WorkerInfo = "OnWorkerStart";
            public const string FileScanFinished = "OnFileScanFinished";
            public const string FileScanProgress = "OnFileScanProgress";
            public const string AssimilationFinished = "OnAssimilationFinished";
            public const string AssimilationProgress = "OnAssimilationProgress";
            public const string BatchFinished = "OnBatchFinished";
            public const string BatchProgress = "OnBatchProgress";
        }
    }
}
