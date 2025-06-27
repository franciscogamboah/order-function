using System.Net;
using Amazon.SQS;
using Amazon.SQS.Model;
using FluentAssertions;
using Infrastructure.Services;
using Moq;
using Xunit;

namespace AWSLambda.Tests.infraestructure;
public class EventServiceSqsTest
{
    [Fact]
    public async Task SqsSserviceTest_GetMessageFromUrl_GetAllOK()
    {
        //arrange
        var messages = new List<Message>() {
            new Message (){ },
            new Message (){ }
        };
        var mockClient = new Mock<IAmazonSQS>();
        mockClient.Setup(client => client.ReceiveMessageAsync(
               It.IsAny<ReceiveMessageRequest>(),
               It.IsAny<CancellationToken>()))
           .Returns((ReceiveMessageRequest r,
               CancellationToken token) =>
           {
               return Task.FromResult(new ReceiveMessageResponse()
               {
                   Messages = messages,
                   HttpStatusCode = HttpStatusCode.OK,
               });
           });

        var calculoComisionSserviceSqs = new EventServiceSqs(mockClient.Object);


        // Act
        var httpWebObject = await calculoComisionSserviceSqs.GetMessage("url");

        //Assert
        Assert.Equal(messages, httpWebObject.Messages);
        httpWebObject.HttpStatusCode.Should().Be(HttpStatusCode.OK);
    }
    [Fact]
    public async Task DeleteMessageTest()
    {
        var mockClient = new Mock<IAmazonSQS>();
        mockClient.Setup(client => client.DeleteMessageAsync(
               It.IsAny<DeleteMessageRequest>(),
               It.IsAny<CancellationToken>()))
           .Returns((DeleteMessageRequest r,
               CancellationToken token) =>
           {
               return Task.FromResult(new DeleteMessageResponse()
               {
                   HttpStatusCode = HttpStatusCode.OK,
               });
           });

        var calculoComisionSserviceSqs = new EventServiceSqs(mockClient.Object);

        var deleteMessageRequest = new DeleteMessageRequest
        {
            QueueUrl = "_queueUrl",
            ReceiptHandle = "f1e931bc-92e0-415f-ab01-123456789abc"
        };

        // Act
        var httpWebObject = await calculoComisionSserviceSqs.DeleteMessage(deleteMessageRequest);

        //Assert
        httpWebObject.HttpStatusCode.Should().Be(HttpStatusCode.OK);
    }
}
