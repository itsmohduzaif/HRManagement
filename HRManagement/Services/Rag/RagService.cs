using HRManagement.Entities;
using OpenAI;
using OpenAI.Chat;
using Qdrant.Client;

namespace HRManagement.Services.Rag
{
    using DocumentFormat.OpenXml.Office2010.ExcelAc;
    using HRManagement.Models.Rag;
    using Microsoft.Graph.Models;
    using OpenAI.Chat;
    using OpenAI.Embeddings;
    using Qdrant.Client.Grpc;
    using System;
    using System.Diagnostics.Metrics;

    public class RagService
    {
        //private readonly List<RagDocument> _documents = new();
        private readonly string _apiKey;
        private readonly QdrantClient _client;

        public RagService(IConfiguration config, QdrantService qdrantService)
        {
            _apiKey = config["OpenAI:ApiKey"];
            _client = qdrantService.GetClient();
        }

        //................................................................................NEW CODE BELOW...............................................


        // Charecter based chunking
        //private List<string> ChunkText(string text, int size = 200)
        //{
        //    var chunks = new List<string>();

        //    for (int i = 0; i < text.Length; i += size)
        //    {
        //        chunks.Add(text.Substring(i, Math.Min(size, text.Length - i)));
        //    }

        //    return chunks;
        //}


        // Sentence based chunking
        private List<string> ChunkText(string text, int maxChunkLength = 200)
        {
            var sentences = text.Split(new[] { ". ", "? ", "! " }, StringSplitOptions.None);
            var chunks = new List<string>();
            var currentChunk = "";

            foreach (var sentence in sentences)
            {
                if ((currentChunk + sentence).Length > maxChunkLength)
                {
                    chunks.Add(currentChunk.Trim());
                    currentChunk = sentence;
                }
                else
                {
                    currentChunk += sentence + ". ";
                }
            }

            if (!string.IsNullOrWhiteSpace(currentChunk))
                chunks.Add(currentChunk.Trim());

            return chunks;
        }



        public async Task AddDocumentsAsync(List<string> docs)
        {
            List<string> ListOfChunks = new List<string>();

            foreach (var doc in docs)
            {
                Console.WriteLine($"Length: {doc.Length}");
                var chunks = ChunkText(doc);

                ListOfChunks.AddRange(chunks);

            }



            var collections = await _client.ListCollectionsAsync();

            bool exists = collections.Any(c => c == "ragCollection2");

            if (!exists)
            {
                await _client.CreateCollectionAsync(
                collectionName: "ragCollection2",
                vectorsConfig: new VectorParams
                {
                    Size = 384,
                    Distance = Distance.Cosine
                }
                );
            }


            //...............
            var points = new List<PointStruct>();
            for (int i = 0; i < ListOfChunks.Count(); i++)
            {
                var item = ListOfChunks[i];
                points.Add(new PointStruct
                {
                    Id = Guid.NewGuid(),
                    Vectors = new Document
                    {
                        Text = $"{item}",
                        Model = "sentence-transformers/all-MiniLM-L6-v2",
                    },
                    Payload =
                    {
                        ["ChunkString"] = item,
                        //["description"] = item.Item2,
                        //["price"] = item.Item3,
                        //["category"] = item.Item4,
                    },
                });
            }

            await _client.UpsertAsync("ragCollection2", points);

        }




        public async Task<string> AskQuestion(string question)
        {

            //................ Search the menu items........................

            // generate query embedding
            var queryText = question;

            // search for similar menu items
            var results = await _client.QueryAsync(
                collectionName: "ragCollection2",
                query: new Document
                {
                    Text = queryText,
                    Model = "sentence-transformers/all-MiniLM-L6-v2",
                },
                payloadSelector: true,
                limit: 5
            );


            List<string> strings = new List<string>();

            // print results
            foreach (var result in results)
            {
                strings.Add(result.Payload["ChunkString"].StringValue);

                Console.WriteLine($"\n\n\nSimilarity Result: {result.Payload["ChunkString"].StringValue}");
                Console.WriteLine($"\nScore: {result.Score}");

                //Console.WriteLine($"Item: {result.Payload["item_name"].StringValue}");
                //Console.WriteLine($"Score: {result.Score}");
                //Console.WriteLine($"Description: {result.Payload["description"].StringValue}");
                //Console.WriteLine($"Price: {result.Payload["price"].StringValue}");
                //Console.WriteLine("---");
            }
            // Each result contains:

            // Score → similarity(higher = better)
            // Payload → your stored data

            try
            {
                var client = new ChatClient(model: "gpt-4.1-mini", apiKey: _apiKey);

                var context = string.Join("\n", strings);

                var prompt = $@"
                                You are an HR assistant.

                                Answer ONLY from the provided context.
                                If the answer is not present, say ""I don't know"".

                                Context:
                                {context}

                                Question:
                                {question}
                            ";


                var response = await client.CompleteChatAsync(prompt);

                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("insufficient_quota"))
                {
                    return "OpenAI quota exceeded. Please check billing.";
                }

                return "Issue in server side!";
            }


        }









        //................................................................................OLD BELOW...............................................




        


        //public async Task AddDocumentsAsync(List<string> docs)
        //{
        //    foreach (var doc in docs)
        //    {
        //        Console.WriteLine($"Length: {doc.Length}");
        //        var chunks = ChunkText(doc);

        //        foreach (var chunk in chunks)
        //        {
        //            RagDocument x = new RagDocument  {
        //                Content = chunk,
        //                Embedding = await GetEmbedding(chunk)
        //            };

        //            _documents.Add(x);
        //        }
        //    }

        //    Console.WriteLine($"/n/n\n\n\n\n [DOCUMENTS Count]: {_documents.Count()}");
        //}




        //public async Task<float[]> GetEmbedding(string doc)
        //{
        //    EmbeddingClient client = new(model: "text-embedding-3-small", apiKey: _apiKey);
        //    EmbeddingGenerationOptions options = new() { Dimensions = 512 };
        //    OpenAIEmbedding embedding = await client.GenerateEmbeddingAsync(doc, options);
        //    var vector = embedding.ToFloats().ToArray();
            
        //    return vector;
        //}


        private float CosineSimilarity(float[] a, float[] b)
        {
            float dot = 0;
            float normA = 0;
            float normB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            return dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
        }
        // closer to 1 → very similar
        // closer to 0 → not related



        //public async Task<string> AskQuestion(string question)
        //{
        //    try
        //    {
        //        var client = new ChatClient(model: "gpt-4.1-mini", apiKey: _apiKey);


        //        // 1️) Get question embedding
        //        var questionEmbedding = await GetEmbedding(question);

        //        // 2️) Calculate similarity
        //        var scoredDocs = _documents.Select(d => new
        //        {
        //            Doc = d,
        //            Score = CosineSimilarity(questionEmbedding, d.Embedding)
        //        }).OrderByDescending(x => x.Score)
        //        .Take(2)
        //        .Select(x => x.Doc.Content);

        //        // 3️) Create context
        //        var context = string.Join("\n", scoredDocs);


        //        var prompt = $@"
        //                        You are an HR assistant.

        //                        Answer ONLY from the provided context.
        //                        If the answer is not present, say ""I don't know"".

        //                        Context:
        //                        {context}

        //                        Question:
        //                        {question}
        //                    ";


        //        var response = await client.CompleteChatAsync(prompt);

        //        return response.Value.Content[0].Text;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message.Contains("insufficient_quota"))
        //        {
        //            return "OpenAI quota exceeded. Please check billing.";
        //        }

        //        return "Issue in server side!";
        //    }
        //}








        //--------------- For Documentation Testing Purposes Only ------------------


        public async Task<ReadOnlyMemory<float>> Test()
        {
            EmbeddingClient client = new(model: "text-embedding-3-small", apiKey: _apiKey);

            string description = "Best hotel in town if you like luxury hotels. They have an amazing infinity pool, a spa,"
                                    + " and a really helpful concierge. The location is perfect -- right downtown, close to all the tourist"
                                    + " attractions. We highly recommend this hotel.";

            EmbeddingGenerationOptions options = new() { Dimensions = 512 };


            OpenAIEmbedding embedding = client.GenerateEmbedding(description, options);
            ReadOnlyMemory<float> vector = embedding.ToFloats();


            return vector;

            //----------------------------
            // Using Chat Client

            //ChatClient client = new(model: "gpt-4.1-mini", apiKey: _apiKey);

            //ChatCompletion completion = await client.CompleteChatAsync("Say 'this is a test.'");
            //Console.WriteLine($"[ASSISTANT]: {completion.Content[0].Text}");

            //return completion.Content[0].Text;
        }





        public async Task TestQdrant()
        {
            //..............

            //var collections = await _client.GetCollectionsAsync();
            var collections = await _client.ListCollectionsAsync();

            //bool exists = collections.Collections.Any(c => c.Name == "rag_docs");
            bool exists = collections.Any(c => c == "items");

            if (!exists)
            {
                await _client.CreateCollectionAsync(
                collectionName: "items",
                vectorsConfig: new VectorParams
                {
                    Size = 384,
                    Distance = Distance.Cosine
                }
            );
            }

            //...............



            var menuItems = new[] {
                            ("Pad Thai with Tofu", "Stir-fried rice noodles with tofu bean sprouts scallions and crushed peanuts in traditional tamarind sauce", "$13.95", "Noodles"),
                            ("Grilled Salmon Fillet", "Wild-caught Atlantic salmon grilled with lemon butter and fresh herbs served with seasonal vegetables", "$24.50", "Seafood Entrees"),
                            ("Mushroom Risotto", "Creamy arborio rice with mixed mushrooms parmesan truffle oil and fresh thyme", "$16.75", "Vegetarian"),
                            ("Bibimbap Bowl", "Korean rice bowl with seasoned vegetables fried egg gochujang sauce and choice of protein", "$14.50", "Korean Bowls"),
                            ("Falafel Wrap", "Crispy chickpea fritters with hummus tahini cucumber tomato and pickled vegetables in warm pita", "$11.25", "Mediterranean"),
                            ("Shrimp Tacos", "Three soft tacos with grilled shrimp cabbage slaw chipotle aioli and fresh lime", "$13.00", "Tacos"),
                            ("Vegetable Curry", "Mixed vegetables in aromatic coconut curry sauce with jasmine rice and naan bread", "$12.95", "Indian Curries"),
                            ("Tuna Poke Bowl", "Fresh ahi tuna with avocado edamame cucumber seaweed salad over sushi rice with spicy mayo", "$16.50", "Poke Bowls"),
                            ("Margherita Pizza", "Fresh mozzarella san marzano tomatoes basil and extra virgin olive oil on wood-fired crust", "$14.00", "Pizza"),
                            ("Chicken Tikka Masala", "Tandoori chicken in creamy tomato sauce with aromatic spices served with basmati rice", "$15.95", "Indian Entrees"),
                            ("Greek Salad", "Romaine lettuce tomatoes cucumbers kalamata olives feta cheese red onion with lemon oregano dressing", "$10.50", "Salads"),
                            ("Lobster Roll", "Fresh Maine lobster meat with light mayo on toasted buttery roll served with chips", "$22.00", "Seafood Sandwiches"),
                            ("Quinoa Buddha Bowl", "Organic quinoa with roasted chickpeas kale sweet potato tahini dressing and hemp seeds", "$13.50", "Healthy Bowls"),
                            ("Beef Pho", "Traditional Vietnamese beef noodle soup with rice noodles fresh herbs bean sprouts and lime", "$12.75", "Noodle Soups"),
                            ("Eggplant Parmesan", "Breaded eggplant layered with marinara mozzarella and parmesan served with pasta", "$15.25", "Italian Entrees"),
                            ("Crab Cakes", "Maryland-style lump crab cakes with remoulade sauce and mixed greens", "$18.50", "Seafood Appetizers"),
                            ("Tofu Stir Fry", "Crispy tofu with broccoli bell peppers snap peas in garlic ginger sauce over steamed rice", "$12.50", "Vegetarian Entrees"),
                            ("Salmon Sushi Platter", "12 pieces of fresh salmon nigiri and sashimi with wasabi pickled ginger and soy sauce", "$19.95", "Sushi"),
                            ("Caprese Sandwich", "Fresh mozzarella tomatoes basil pesto balsamic glaze on ciabatta bread", "$11.75", "Sandwiches"),
                            ("Tom Yum Soup", "Spicy and sour Thai soup with shrimp lemongrass galangal mushrooms and kaffir lime leaves", "$11.50", "Soups"),
                            ("Lentil Dal", "Red lentils simmered with turmeric cumin coriander served with rice and naan", "$11.95", "Vegan Entrees"),
                            ("Fish and Chips", "Beer-battered cod with crispy fries malt vinegar and tartar sauce", "$16.00", "British Classics"),
                            ("Veggie Burger", "House-made black bean and quinoa patty with avocado sprouts tomato on brioche bun", "$13.25", "Burgers"),
                            ("Miso Ramen", "Rich miso broth with ramen noodles soft-boiled egg bamboo shoots nori and scallions", "$14.50", "Ramen"),
                            ("Stuffed Bell Peppers", "Roasted bell peppers filled with rice vegetables herbs and melted cheese", "$13.75", "Vegetarian Entrees"),
                            ("Scallop Risotto", "Pan-seared sea scallops over creamy parmesan risotto with white wine and lemon", "$26.50", "Seafood Specials"),
                            ("Spring Rolls", "Fresh rice paper rolls with vegetables tofu rice noodles herbs and peanut dipping sauce", "$8.95", "Appetizers"),
                            ("Oyster Po Boy", "Fried oysters with lettuce tomato pickles and remoulade on french bread", "$15.50", "Sandwiches"),
                            ("Portobello Mushroom Steak", "Grilled portobello cap marinated in balsamic with roasted vegetables and quinoa", "$14.95", "Vegan Entrees"),
                            ("Coconut Shrimp", "Jumbo shrimp breaded in shredded coconut served with sweet chili sauce", "$14.25", "Seafood Appetizers"),
                        };

            var points = new List<PointStruct>();
            for (int i = 0; i < menuItems.Length; i++)
            {
                var item = menuItems[i];
                points.Add(new PointStruct
                {
                    Id = (ulong)i,
                    Vectors = new Document
                    {
                        Text = $"{item.Item1} {item.Item2}",
                        Model = "sentence-transformers/all-MiniLM-L6-v2",
                    },
                    Payload =
                    {
                        ["item_name"] = item.Item1,
                        ["description"] = item.Item2,
                        ["price"] = item.Item3,
                        ["category"] = item.Item4,
                    },
                });
            }

            await _client.UpsertAsync("items", points);



            //................ Search the menu items........................

            // generate query embedding
            var queryText = "vegetarian dishes";

            // search for similar menu items
            var results = await _client.QueryAsync(
                collectionName: "items",
                query: new Document
                {
                    Text = queryText,
                    Model = "sentence-transformers/all-MiniLM-L6-v2",
                },
                payloadSelector: true,
                limit: 5
            );

            // print results
            foreach (var result in results)
            {
                Console.WriteLine($"Item: {result.Payload["item_name"].StringValue}");
                Console.WriteLine($"Score: {result.Score}");
                Console.WriteLine($"Description: {result.Payload["description"].StringValue}");
                Console.WriteLine($"Price: {result.Payload["price"].StringValue}");
                Console.WriteLine("---");
            }
            // Each result contains:

            // Score → similarity(higher = better)
            // Payload → your stored data


        }













    }
}
