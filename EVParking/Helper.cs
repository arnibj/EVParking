namespace EVParking
{
    public class Helper
    {
        public void LogTrace(string message)
        {
            Console.WriteLine(message);
        }

        public void LogException(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
}
