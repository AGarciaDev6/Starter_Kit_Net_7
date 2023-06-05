namespace Starter_NET_7.DTOs.Response.General
{
    public class ImportResponse
    {
        public int RecordOk { get; set; }
        public int RecordError { get; set; }

        public List<RecordErrorResponse> Errors { get; set; } = new List<RecordErrorResponse>();
    }

    public class RecordErrorResponse
    {
        public int Row { get; set; }
        public string Message { get; set; } = null!;
    }
}
