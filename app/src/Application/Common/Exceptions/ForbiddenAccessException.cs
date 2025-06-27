using System.Runtime.Serialization;

namespace Application.Common.Exceptions;
[Serializable]
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base() { }
    protected ForbiddenAccessException(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {

    }
}
