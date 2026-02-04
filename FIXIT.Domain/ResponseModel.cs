namespace City_Bus_Management_System.DataLayer
{
    public class ResponseModel<T>
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; }
        public T Result { get; set; }
    }
}
