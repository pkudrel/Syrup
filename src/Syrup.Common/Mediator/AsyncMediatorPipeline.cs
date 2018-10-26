//using System.Threading.Tasks;
//using MediatR;
//using NLog;

//namespace Syrup.Common.Mediator
//{

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <typeparam name="TRequest"></typeparam>
//    /// <typeparam name="TResponse"></typeparam>
//    /// <remarks>
//    /// https://gist.github.com/NotMyself/579f94e1aad6a022ddb9
//    /// </remarks>
//    public class AsyncMediatorPipeline<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
//        where TRequest : IRequest<TResponse>
//    {
//        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
//        private readonly IRequestHandler<TRequest, TResponse> inner;
//        private readonly IAsyncPostRequestHandler<TRequest, TResponse>[] postRequestHandlers;
//        private readonly IAsyncPreRequestHandler<TRequest>[] preRequestHandlers;


//        public AsyncMediatorPipeline(IRequestHandler<TRequest, TResponse> inner,
//            IAsyncPreRequestHandler<TRequest>[] preRequestHandlers,
//            IAsyncPostRequestHandler<TRequest, TResponse>[] postRequestHandlers)
//        {
//            this.inner = inner;
//            this.preRequestHandlers = preRequestHandlers;
//            this.postRequestHandlers = postRequestHandlers;
//        }

//        public ILogger Logger { get; set; }

//        public async Task<TResponse> Handle(TRequest message)
//        {
//            _log.Trace("Handle");

//            foreach (var preRequestHandler in preRequestHandlers)
//            {
//                await preRequestHandler.Handle(message);
//            }

//            var result = await inner.Handle(message);

//            foreach (var postRequestHandler in postRequestHandlers)
//            {
//                await postRequestHandler.Handle(message, result);
//            }
//            return result;
//        }
//    }
//}