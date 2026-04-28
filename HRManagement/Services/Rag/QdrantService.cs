using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace HRManagement.Services.Rag
{
    public class QdrantService
    {
        private readonly QdrantClient _client;

        public QdrantService(IConfiguration config)
        {
            _client = new QdrantClient(
                host: config["Qdrant:Host"],
                https: true,
                apiKey: config["Qdrant:ApiKey"]
            );
        }

        public QdrantClient GetClient()
        {
            return _client;
        }

        public async Task InitializeCollectionAsync()
        {
            //var collections = await _client.GetCollectionsAsync();
            var collections = await _client.ListCollectionsAsync();

            //bool exists = collections.Collections.Any(c => c.Name == "rag_docs");
            bool exists = collections.Any(c => c == "rag_docs1");

            if (!exists)
            {
                await _client.CreateCollectionAsync(
                    collectionName: "rag_docs1",
                    vectorsConfig: new VectorParams
                    {
                        Size = 512,
                        Distance = Distance.Cosine
                    }
                );
            }
        }



    }
}



//--------------------

//using Qdrant.Client;
//using Qdrant.Client.Grpc;


//namespace HRManagement.Services.Rag
//{


//    public class QdrantService
//    {
//        private readonly QdrantClient _client;

//        public QdrantService()
//        {
//            _client = new QdrantClient(
//                host: "YOUR_QDRANT_URL",
//                https: true,
//                apiKey: "YOUR_API_KEY"
//            );
//        }


//        public async void x()
//        {
//            var client = new QdrantClient(
//            host: "xyz-example.qdrant.io",
//            port: 6334,
//            https: true,
//            apiKey: "<your-api-key>"
//            );


//            await client.CreateCollectionAsync(
//                collectionName: "items",
//                vectorsConfig: new VectorParams { Size = 384, Distance = Distance.Cosine }
//            );

//        }


//    }
//}
