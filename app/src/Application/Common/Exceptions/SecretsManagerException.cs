using System.Runtime.Serialization;

namespace Application.Common.Exceptions;
[Serializable]
public class SecretsManagerException : Exception
{
    public SecretsManagerException()
        : base()
    {
    }

    public SecretsManagerException(string message)
        : base(message)
    {
    }

    public SecretsManagerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public SecretsManagerException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
    protected SecretsManagerException(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {

    }
}
