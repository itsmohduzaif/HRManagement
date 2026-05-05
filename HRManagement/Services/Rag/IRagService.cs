namespace HRManagement.Services.Rag
{
    public interface IRagService
    {
        //List<string> ChunkText(string text, int maxChunkLength = 200);
        Task AddDocumentsAsync(List<string> docs);
        Task<string> AskQuestion(string question);
    }
}
