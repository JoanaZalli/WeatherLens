using Amazon.SQS.Model;

namespace WeatherLens.Application.Common.Interfaces;
public interface ISqsMessenger
{
    Task<SendMessageResponse> SendMessageAsync<T>(T message);
}
