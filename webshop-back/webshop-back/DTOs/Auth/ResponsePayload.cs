namespace webshop_back.DTOs.Auth
{
    public class ResponsePayload<T>
    {
        public ResponseStatus Status { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public ResponsePayload()
        {
            Status = ResponseStatus.OK;
            Message = string.Empty;
        }

        public ResponsePayload(T data, ResponseStatus status = ResponseStatus.OK, string message = "")
        {
            Data = data;
            Status = status;
            Message = message;
        }

        public ResponsePayload(ResponseStatus status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}
