using Qdrant.Client;

namespace HRManagement.Services.Rag
{
    public interface IQdrantService
    {
        QdrantClient GetClient();
        Task InitializeCollectionAsync();

    }
}
