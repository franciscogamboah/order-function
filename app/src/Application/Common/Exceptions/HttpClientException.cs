using System.Runtime.Serialization;

namespace Application.Common.Exceptions;
[Serializable]
public class HttpClientException : Exception
{
    public HttpClientException() : base()
    {
    }

    public HttpClientException(string message) : base(message)
    {
    }

    public HttpClientException(string message, Exception innerException) : base(message, innerException)
    {
    }
    protected HttpClientException(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {

    }
}