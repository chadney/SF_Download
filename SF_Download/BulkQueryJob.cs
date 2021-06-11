
namespace SF_Download
{
    public class BulkQueryJob
    {

        private string _JobId;
        public string JobId { get { return _JobId; } }

        //initial batch ID for bulk query
        public string BatchId { get; set; }
        public string BatchStatus { get; set; }
        public string BatchError { get; set; }

        public BulkQueryJob(string JobId)
        {
            _JobId = JobId;

        }
    }
}
