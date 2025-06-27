using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;
using Domain.Entities;
using Application;
using System.Text.Json;

namespace AWSLambda.Tests;

public class FunctionTest
{
    //[Fact]
    //public async Task TestSQSEventLambdaFunction()
    //{

    //    SQSEvent sqsEvent = new();
    //    sqsEvent.Records = new();

    //    List<SQSEvent.SQSMessage> records = new();
    //    records.Add(new SQSEvent.SQSMessage() { Body = CrearObjectoMessageSqs() });

    //    sqsEvent.Records = records;

    //    var logger = new TestLambdaLogger();
    //    var context = new TestLambdaContext
    //    {
    //        Logger = logger
    //    };

    //    var function = new Function();
    //    await function.FunctionHandler(sqsEvent, context);

    //    Assert.Contains("Processed message foobar", logger.Buffer.ToString());
    //}

    //public string CrearObjectoMessageSqs()
    //{
    //    var body = new MensajeCalculoComision()
    //    {
    //        Id = Guid.Parse("6eed71f9-51e9-47d8-85da-efd76da73024"),
    //        IdSolicitud = 1,
    //        Cuerpo = new MessageComisionSyndeo()
    //        {
    //            IdSolicitud = 56519,
    //            IdServicio = 1,
    //            TipoComision = 15,
    //            IdMoneda = 1,
    //            Monto = 35,
    //            ValorFijo = (decimal)7.00,
    //            ValorPorcentual = (decimal)15.00,
    //            ValorTope = (decimal)30.00
    //        }
    //    };
    //    return JsonSerializer.Serialize(body);
    //}
}