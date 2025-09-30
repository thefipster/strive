namespace TheFipster.ActivityAggregator.Domain;

public static class Const
{
    public static class Hubs
    {
        public static class Ingester
        {
            public const string UnzipFinishedMethod = "OnUnzipFinished";
            public const string WorkerInfoMethod = "OnWorkerStart";
            public const string FileScanFinished = "OnFileScanFinished";
            public const string FileScanProgress = "OnFileScanProgress";
            public const string Url = "/hubs/ingest";
        }
    }
}
