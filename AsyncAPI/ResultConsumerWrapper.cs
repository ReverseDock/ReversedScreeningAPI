using EasyNetQ;

using AsyncAPI.Consumers;

namespace AsyncAPI;

public class ResultConsumerWrapper
{
    public ResultConsumer _resultConsumer;

    public ResultConsumerWrapper(ResultConsumer resultConsumer)
    {
        _resultConsumer = resultConsumer;
    }


}