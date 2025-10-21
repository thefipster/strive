namespace TheFipster.ActivityAggregator.Domain;

public static class Const
{
    public static class Cors
    {
        public const string AllowAll = "AllowAll";
        public const string AllowOne = "AllowOne";
    }

    public static class Hubs
    {
        public static class Importer
        {
            public const string Url = "/hubs/import";

            public const string ReportAction = "ReportAction";
            public const string ReportProgress = "ReportProgress";
            public const string ReportQueue = "ReportQueue";

            public static class Actions
            {
                public const string Unzip = "Unzip";
                public const string Scan = "Scan";
                public const string Assimilate = "Assimilate";
                public const string Batch = "Batch";
                public const string Queue = "Queue";
            }
        }

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
