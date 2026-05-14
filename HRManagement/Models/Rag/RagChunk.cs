namespace HRManagement.Models.Rag
{
    public class RagChunk
    {
        public Guid Id { get; set; }

        public string ChunkText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
