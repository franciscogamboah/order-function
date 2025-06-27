using System.Runtime.Serialization;

namespace Application.Common.Exceptions;

[Serializable]
public class LambdaException : Exception
{
    public readonly int CodigoError;
    public LambdaException()
        : base()
    {

    }
    public LambdaException(string message)
        : base(message)
    {
    }
    public LambdaException(string message, int codigoError)
        : base(message)
    {
        CodigoError = codigoError;
    }
    protected LambdaException(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {

    }
}