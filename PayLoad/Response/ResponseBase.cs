namespace BigProject.Payload.Response
{
    public class ResponseBase
    {
        public int Status { get; set; }
        public string Message { get; set; }

        public ResponseBase() { }
        public ResponseBase(int status, string message)
        {
            Status = status;
            Message = message;
        }


        public ResponseBase ResponseBaseSuccess(string message)
        {
            return new ResponseBase(StatusCodes.Status200OK, message);
        }
        public ResponseBase ResponseBaseError(int status, string message)
        {
            return new ResponseBase(status, message);
        }
    }
}
